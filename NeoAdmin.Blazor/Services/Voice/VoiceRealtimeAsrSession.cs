using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NeoAdmin.Blazor.Services.Voice;

/// <summary>DashScope Paraformer 实时语音识别 WebSocket 会话。</summary>
public sealed class VoiceRealtimeAsrSession : IAsyncDisposable
{
    private readonly DashScopeOptions _options;
    private readonly ILogger<VoiceRealtimeAsrSession> _logger;
    private readonly ClientWebSocket _webSocket = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly TaskCompletionSource _taskStartedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _taskFinishedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    private string _taskId = string.Empty;
    private Task? _receiveLoop;
    private bool _taskStarted;
    private bool _disposed;
    private long _audioBytesSent;

    public VoiceRealtimeAsrSession(DashScopeOptions options, ILogger<VoiceRealtimeAsrSession> logger)
    {
        _options = options;
        _logger = logger;
    }

    public Func<string, bool, Task>? OnResult { get; set; }

    public Func<string, Task>? OnError { get; set; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("请先在 appsettings 中配置 DashScope:ApiKey。");
        }

        _taskId = Guid.NewGuid().ToString("N");
        _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {_options.ApiKey}");

        _logger.LogInformation("实时语音识别连接开始，TaskId={TaskId}, Model={Model}", _taskId, _options.RealtimeModel);

        await _webSocket.ConnectAsync(new Uri(DashScopeOptions.DefaultWebSocketUrl), cancellationToken);
        await SendJsonAsync(CreateRunTaskMessage(), cancellationToken);

        _receiveLoop = ReceiveLoopAsync(_cts.Token);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
        await _taskStartedTcs.Task.WaitAsync(TimeSpan.FromSeconds(10), linkedCts.Token);

        _logger.LogInformation("实时语音识别任务已启动，TaskId={TaskId}", _taskId);
    }

    public async Task SendAudioAsync(byte[] audio, CancellationToken cancellationToken = default)
    {
        if (!_taskStarted || _webSocket.State != WebSocketState.Open || audio.Length == 0)
        {
            return;
        }

        Interlocked.Add(ref _audioBytesSent, audio.Length);

        await _sendLock.WaitAsync(cancellationToken);
        try
        {
            await _webSocket.SendAsync(
                audio,
                WebSocketMessageType.Binary,
                endOfMessage: true,
                cancellationToken);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_webSocket.State != WebSocketState.Open)
        {
            return;
        }

        if (_audioBytesSent == 0)
        {
            _logger.LogWarning("实时语音识别未收到有效音频，跳过结束任务，TaskId={TaskId}", _taskId);
            return;
        }

        _logger.LogInformation("实时语音识别结束任务，TaskId={TaskId}, AudioBytes={AudioBytes}", _taskId, _audioBytesSent);
        await SendJsonAsync(CreateFinishTaskMessage(), cancellationToken);

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            await _taskFinishedTcs.Task.WaitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("等待 task-finished 超时，TaskId={TaskId}", _taskId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await _cts.CancelAsync();

        if (_receiveLoop is not null)
        {
            try
            {
                await _receiveLoop.WaitAsync(TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "实时语音识别接收循环结束异常，TaskId={TaskId}", _taskId);
            }
        }

        if (_webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            try
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "关闭 DashScope WebSocket 失败，TaskId={TaskId}", _taskId);
            }
        }

        _webSocket.Dispose();
        _sendLock.Dispose();
        _cts.Dispose();
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var message = await ReceiveTextMessageAsync(cancellationToken);
                if (message is null)
                {
                    break;
                }

                await HandleMessageAsync(message);
            }
        }
        catch (OperationCanceledException)
        {
            // 正常取消
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "实时语音识别接收循环失败，TaskId={TaskId}", _taskId);
            await RaiseErrorAsync(ex.Message);
        }
    }

    private async Task HandleMessageAsync(string json)
    {
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(json);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "无法解析 DashScope 消息，TaskId={TaskId}", _taskId);
            return;
        }

        var eventName = root?["header"]?["event"]?.GetValue<string>();
        switch (eventName)
        {
            case "task-started":
                _taskStarted = true;
                _taskStartedTcs.TrySetResult();
                break;

            case "result-generated":
                await HandleRecognitionResultAsync(root);
                break;

            case "task-finished":
                _taskFinishedTcs.TrySetResult();
                _logger.LogInformation("实时语音识别任务完成，TaskId={TaskId}", _taskId);
                break;

            case "task-failed":
                var errorMessage = root?["header"]?["error_message"]?.GetValue<string>() ?? "语音识别任务失败";
                _logger.LogError("实时语音识别任务失败，TaskId={TaskId}, Message={Message}", _taskId, errorMessage);
                _taskStartedTcs.TrySetException(new InvalidOperationException(errorMessage));
                _taskFinishedTcs.TrySetResult();
                await RaiseErrorAsync(errorMessage);
                break;
        }
    }

    private async Task HandleRecognitionResultAsync(JsonNode? root)
    {
        var sentence = root?["payload"]?["output"]?["sentence"];
        if (sentence is null)
        {
            return;
        }

        var text = sentence["text"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var isFinal = sentence["sentence_end"]?.GetValue<bool>() ?? false;
        if (OnResult is not null)
        {
            await OnResult.Invoke(text, isFinal);
        }
    }

    private async Task RaiseErrorAsync(string message)
    {
        if (OnError is null || _disposed)
        {
            return;
        }

        try
        {
            await OnError.Invoke(message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "实时语音识别错误回调失败，TaskId={TaskId}", _taskId);
        }
    }

    private async Task SendJsonAsync(string json, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        await _webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken);
    }

    private async Task<string?> ReceiveTextMessageAsync(CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        WebSocketReceiveResult result;

        do
        {
            var buffer = new byte[8192];
            result = await _webSocket.ReceiveAsync(buffer, cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                return null;
            }

            if (result.MessageType != WebSocketMessageType.Text)
            {
                continue;
            }

            ms.Write(buffer, 0, result.Count);
        }
        while (!result.EndOfMessage);

        return ms.Length == 0 ? null : Encoding.UTF8.GetString(ms.ToArray());
    }

    private string CreateRunTaskMessage()
    {
        var message = new JsonObject
        {
            ["header"] = new JsonObject
            {
                ["action"] = "run-task",
                ["task_id"] = _taskId,
                ["streaming"] = "duplex",
            },
            ["payload"] = new JsonObject
            {
                ["task_group"] = "audio",
                ["task"] = "asr",
                ["function"] = "recognition",
                ["model"] = _options.RealtimeModel,
                ["parameters"] = new JsonObject
                {
                    ["format"] = "pcm",
                    ["sample_rate"] = _options.SampleRate,
                },
                ["input"] = new JsonObject(),
            },
        };

        return message.ToJsonString();
    }

    private string CreateFinishTaskMessage()
    {
        var message = new JsonObject
        {
            ["header"] = new JsonObject
            {
                ["action"] = "finish-task",
                ["task_id"] = _taskId,
                ["streaming"] = "duplex",
            },
            ["payload"] = new JsonObject
            {
                ["input"] = new JsonObject(),
            },
        };

        return message.ToJsonString();
    }
}
