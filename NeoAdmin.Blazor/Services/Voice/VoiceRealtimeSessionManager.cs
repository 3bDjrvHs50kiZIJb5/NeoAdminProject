using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoAdmin.Blazor.Hubs;

namespace NeoAdmin.Blazor.Services.Voice;

/// <summary>按 SignalR 连接管理实时语音识别会话。</summary>
public sealed class VoiceRealtimeSessionManager
{
    private readonly ConcurrentDictionary<string, VoiceRealtimeAsrSession> _sessions = new();
    private readonly IHubContext<VoiceRealtimeHub> _hubContext;
    private readonly IOptions<DashScopeOptions> _options;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<VoiceRealtimeSessionManager> _logger;

    public VoiceRealtimeSessionManager(
        IHubContext<VoiceRealtimeHub> hubContext,
        IOptions<DashScopeOptions> options,
        ILoggerFactory loggerFactory,
        ILogger<VoiceRealtimeSessionManager> logger)
    {
        _hubContext = hubContext;
        _options = options;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task StartSessionAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        await RemoveSessionAsync(connectionId);

        var session = new VoiceRealtimeAsrSession(_options.Value, _loggerFactory.CreateLogger<VoiceRealtimeAsrSession>());
        session.OnResult += async (text, isFinal) =>
        {
            try
            {
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("RecognitionResult", text, isFinal, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "推送识别结果失败，ConnectionId={ConnectionId}", connectionId);
            }
        };
        session.OnError += async message =>
        {
            try
            {
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("RecognitionError", message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "推送识别错误失败，ConnectionId={ConnectionId}", connectionId);
            }
        };

        if (!_sessions.TryAdd(connectionId, session))
        {
            await session.DisposeAsync();
            throw new InvalidOperationException("无法创建语音识别会话。");
        }

        try
        {
            await session.StartAsync(cancellationToken);
            _logger.LogInformation("实时语音会话已创建，ConnectionId={ConnectionId}", connectionId);
        }
        catch
        {
            await RemoveSessionAsync(connectionId);
            throw;
        }
    }

    public Task SendAudioAsync(string connectionId, byte[] audio, CancellationToken cancellationToken = default)
    {
        if (_sessions.TryGetValue(connectionId, out var session))
        {
            return session.SendAudioAsync(audio, cancellationToken);
        }

        return Task.CompletedTask;
    }

    public async Task StopSessionAsync(string connectionId)
    {
        if (_sessions.TryRemove(connectionId, out var session))
        {
            try
            {
                await session.StopAsync();
            }
            finally
            {
                await session.DisposeAsync();
            }

            _logger.LogInformation("实时语音会话已结束，ConnectionId={ConnectionId}", connectionId);
        }
    }

    public Task RemoveSessionAsync(string connectionId) => StopSessionAsync(connectionId);
}
