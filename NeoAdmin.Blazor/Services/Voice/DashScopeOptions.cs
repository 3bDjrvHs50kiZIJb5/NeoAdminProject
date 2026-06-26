namespace NeoAdmin.Blazor.Services.Voice;

/// <summary>阿里云百炼 DashScope 配置（Paraformer 实时语音识别）。</summary>
public sealed class DashScopeOptions
{
    public const string SectionName = "DashScope";

    public const string DefaultWebSocketUrl = "wss://dashscope.aliyuncs.com/api-ws/v1/inference";

    public string ApiKey { get; set; } = string.Empty;

    public string RealtimeModel { get; set; } = "paraformer-realtime-v2";

    public int SampleRate { get; set; } = 16000;
}
