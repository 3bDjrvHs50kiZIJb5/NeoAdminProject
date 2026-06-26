using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeoAdmin.Blazor.Hubs;
using NeoAdmin.Blazor.Services.Voice;

namespace NeoAdmin.Blazor.Extensions;

public static class NeoAdminVoiceExtensions
{
    /// <summary>
    /// 注册实时语音识别服务（DashScope + SignalR Hub 依赖）。
    /// 宿主 appsettings 需配置 <c>DashScope:ApiKey</c>。
    /// </summary>
    public static IServiceCollection AddNeoAdminVoice(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSignalR();
        if (configuration is not null)
        {
            services.Configure<DashScopeOptions>(configuration.GetSection(DashScopeOptions.SectionName));
        }

        services.AddSingleton<VoiceRealtimeSessionManager>();
        return services;
    }

    /// <summary>
    /// 映射 NeoAdmin 实时语音识别 Hub。默认路径 <see cref="NeoAdminVoiceDefaults.HubPath"/>。
    /// </summary>
    public static WebApplication MapNeoAdminVoiceHub(
        this WebApplication app,
        string path = NeoAdminVoiceDefaults.HubPath)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.MapHub<VoiceRealtimeHub>(path);
        return app;
    }
}
