using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NeoAdmin.Blazor.Services.Voice;

namespace NeoAdmin.Blazor.Hubs;

/// <summary>实时语音识别 SignalR Hub（代理 DashScope WebSocket）。</summary>
[AllowAnonymous]
public sealed class VoiceRealtimeHub : Hub
{
    private readonly VoiceRealtimeSessionManager _sessionManager;
    private readonly ILogger<VoiceRealtimeHub> _logger;

    public VoiceRealtimeHub(VoiceRealtimeSessionManager sessionManager, ILogger<VoiceRealtimeHub> logger)
    {
        _sessionManager = sessionManager;
        _logger = logger;
    }

    public async Task StartSession()
    {
        _logger.LogInformation("实时语音 Hub 会话开始，ConnectionId={ConnectionId}", Context.ConnectionId);

        try
        {
            await _sessionManager.StartSessionAsync(Context.ConnectionId, Context.ConnectionAborted);
            await Clients.Caller.SendAsync("SessionReady");
            _logger.LogInformation("实时语音 Hub 会话就绪，ConnectionId={ConnectionId}", Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "实时语音 Hub 启动失败，ConnectionId={ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("RecognitionError", ex.Message);
            throw;
        }
    }

    public Task SendAudio(int[] audio)
    {
        if (audio is null or { Length: 0 })
        {
            return Task.CompletedTask;
        }

        var bytes = new byte[audio.Length];
        for (var i = 0; i < audio.Length; i++)
        {
            bytes[i] = (byte)audio[i];
        }

        return _sessionManager.SendAudioAsync(Context.ConnectionId, bytes, Context.ConnectionAborted);
    }

    public Task StopSession()
    {
        _logger.LogInformation("实时语音 Hub 停止会话，ConnectionId={ConnectionId}", Context.ConnectionId);
        return _sessionManager.StopSessionAsync(Context.ConnectionId);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is not null)
        {
            _logger.LogWarning(exception, "实时语音 Hub 连接断开，ConnectionId={ConnectionId}", Context.ConnectionId);
        }

        return _sessionManager.RemoveSessionAsync(Context.ConnectionId);
    }
}
