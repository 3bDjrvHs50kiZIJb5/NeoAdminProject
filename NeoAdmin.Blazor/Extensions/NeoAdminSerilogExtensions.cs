using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace NeoAdmin.Blazor.Extensions;

public static class NeoAdminSerilogExtensions
{
    public static WebApplicationBuilder AddNeoAdminSerilog(this WebApplicationBuilder builder)
    {
        string logsPath = Path.Combine(AppContext.BaseDirectory, "Logs");
        Directory.CreateDirectory(logsPath);

        IConfigurationSection serilogSection = builder.Configuration.GetSection("Serilog");
        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "NeoAdmin");

            if (serilogSection.Exists())
            {
                loggerConfiguration.ReadFrom.Configuration(context.Configuration);
            }
            else
            {
                loggerConfiguration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .WriteTo.Console()
                    .WriteTo.File(
                        Path.Combine(logsPath, "admin-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        shared: true,
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");
            }
        });

        return builder;
    }

    public static WebApplication UseNeoAdminSerilogRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });

        return app;
    }
}
