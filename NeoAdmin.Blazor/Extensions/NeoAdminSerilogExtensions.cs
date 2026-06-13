using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace NeoAdmin.Blazor.Extensions;

public static class NeoAdminSerilogExtensions
{
    private const string OutputTemplate =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}";

    /// <summary>注册 Serilog：控制台 + 按日滚动的 Logs/admin-.log。</summary>
    /// <example>
    /// <code>
    /// var builder = WebApplication.CreateBuilder(args);
    /// builder.AddNeoAdminSerilog();
    /// </code>
    /// </example>
    public static WebApplicationBuilder AddNeoAdminSerilog(this WebApplicationBuilder builder)
    {
        string logsPath = Path.Combine(AppContext.BaseDirectory, "Logs");
        Directory.CreateDirectory(logsPath);

        builder.Host.UseSerilog((_, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Services(services)
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Filter.ByExcluding(IsBlazorCircuitDisconnectNoise)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "NeoAdmin")
                .WriteTo.Console()
                .WriteTo.File(
                    Path.Combine(logsPath, "admin-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    shared: true,
                    outputTemplate: OutputTemplate);
        });

        return builder;
    }

    /// <summary>启用 HTTP 请求耗时日志。</summary>
    /// <example>
    /// <code>
    /// var app = builder.Build();
    /// app.UseNeoAdminSerilogRequestLogging();
    /// // 日志示例：HTTP GET /api/menu responded 200 in 12.3456 ms
    /// </code>
    /// </example>
    public static WebApplication UseNeoAdminSerilogRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });

        return app;
    }

    /// <summary>Blazor Server 页面关闭/刷新时 Circuit 断开产生的无害异常，不写入日志文件。</summary>
    private static bool IsBlazorCircuitDisconnectNoise(LogEvent logEvent)
    {
        if (logEvent.Exception?.GetType().Name == "JSDisconnectedException")
        {
            return true;
        }

        if (!logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue? sourceContext))
        {
            return false;
        }

        string source = sourceContext.ToString().Trim('"');
        return source.Contains("CircuitHost", StringComparison.Ordinal)
            && logEvent.MessageTemplate.Text.Contains("Unhandled exception in circuit", StringComparison.Ordinal);
    }
}
