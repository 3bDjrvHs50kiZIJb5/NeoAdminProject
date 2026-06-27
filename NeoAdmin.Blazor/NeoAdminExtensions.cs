using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using NeoAdmin.Blazor.Api;
using NeoAdmin.Blazor.Attributes;
using FreeSql;
using NeoAdmin.Blazor.Core.Identity;
using NeoAdmin.Blazor.Data;
using NeoAdmin.Blazor.Entities;
using NeoAdmin.Blazor.Core.Navigation;
using NeoAdmin.Blazor.Middlewares;
using NeoAdmin.Blazor.Core.Authorization;
using NeoAdmin.Blazor.Core.Scheduling;
using NeoAdmin.Blazor.SeedData;
using NeoAdmin.Blazor.Services;
using NeoAdmin.Blazor.Swagger;
using NeoUI.Blazor;
using FreeScheduler;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NeoAdmin.Blazor.Extensions;

public static class NeoAdminExtensions
{
    public static IServiceCollection AddNeoAdmin(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        Action<NeoAdminOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddDataProtection();
        services.AddHttpContextAccessor();
        services.AddOptions<NeoAdminOptions>();
        if (configuration is not null)
        {
            services.Configure<NeoAdminOptions>(configuration.GetSection("NeoAdmin"));
        }

        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }

        // NeoUI 默认 Singleton，多用户会互相看到 Toast；按 Blazor Circuit 隔离为当前客户端
        services.RemoveAll<IToastService>();
        services.AddScoped<IToastService, ToastService>();

        services.AddSingleton(serviceProvider =>
        {
            NeoAdminOptions options = serviceProvider.GetRequiredService<IOptions<NeoAdminOptions>>().Value;
            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            ILogger freeSqlLogger = loggerFactory.CreateLogger("FreeSql");
            SQLitePCL.Batteries.Init();
            FreeSqlSnowflakeSetup.SetIdGenerator(options.WorkId);

            var builder = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(options.DataType, options.ConnectionString)
                .UseAutoSyncStructure(options.AutoSyncStructure);

            IFreeSql freeSql = builder.Build();
            FreeSqlSnowflakeSetup.Configure(freeSql);
            if (options.MonitorCommand)
            {
                FreeSqlMonitorSetup.Configure(freeSql, freeSqlLogger);
            }
            if (options.AutoSyncStructure)
            {
                FreeSqlSchedulerSetup.ConfigureEntities(freeSql);
                FreeSqlSchedulerSetup.SyncStructure(freeSql);
                freeSql.CodeFirst.SyncStructure<SysUser>();
                freeSql.CodeFirst.SyncStructure<SysUserLoginLog>();
                freeSql.CodeFirst.SyncStructure<SysMenu>();
                freeSql.CodeFirst.SyncStructure<SysDict>();
                freeSql.CodeFirst.SyncStructure<SysParam>();
                freeSql.CodeFirst.SyncStructure<SysIpWhitelist>();
                freeSql.CodeFirst.SyncStructure<SysFile>();
                freeSql.CodeFirst.SyncStructure<SysOrg>();
                freeSql.CodeFirst.SyncStructure<SysRole>();
                freeSql.CodeFirst.SyncStructure<SysRoleUser>();
                freeSql.CodeFirst.SyncStructure<SysRoleMenu>();
                freeSql.CodeFirst.SyncStructure<SysSiteSettings>();
                freeSql.CodeFirst.SyncStructure<SysAuditLog>();
                freeSql.CodeFirst.SyncStructure<SysAuditEntityLog>();
            }

            EnsureSeedData(freeSql, options);
            return freeSql;
        });

        services.AddScoped<UnitOfWorkManager>();
        services.AddSingleton<LoginRateLimiter>();
        services.AddScoped<NeoAdminAuthService>();
        services.AddScoped<MenuService>();
        services.AddSingleton<AliyunOssStorageService>();
        services.AddScoped<FileService>();
        services.AddScoped<OrgService>();
        services.AddScoped<RoleService>();
        services.AddScoped<TaskSchedulerService>();
        services.AddScoped<SiteSettingsService>();
        services.AddSingleton<SerilogLogService>();
        services.AddScoped<MenuPermissionService>();
        services.AddScoped<NeoAdminScopeState>();
        services.AddScoped<AuditWorkflowService>();
        services.AddScoped<NeoPickerOverlayService>();
        services.AddNeoAdminScheduler();
        services.AddNeoAdminVoice(configuration);
        return services;
    }

    /// <summary>
    /// 注册 NeoAdmin REST API 控制器（含 NeoAdmin.Blazor 与宿主程序集）。
    /// </summary>
    public static IServiceCollection AddNeoAdminApi(
        this IServiceCollection services,
        params Assembly[] hostAssemblies)
    {
        ArgumentNullException.ThrowIfNull(services);

        List<Assembly> apiAssemblies =
        [
            typeof(BaseApiController).Assembly,
            ..hostAssemblies.Where(assembly => assembly is not null)
        ];

        services.AddScoped<NeoAdminPermissionFilter>();
        IMvcBuilder mvcBuilder = services.AddControllers(options =>
            options.Filters.AddService<NeoAdminPermissionFilter>());
        foreach (Assembly assembly in apiAssemblies)
        {
            mvcBuilder.AddApplicationPart(assembly);
        }

        services.AddSwaggerGen();
        services.AddOptions<SwaggerGenOptions>()
            .Configure<IOptions<NeoAdminOptions>>((swaggerOptions, _) =>
                ConfigureSwaggerGenOptions(swaggerOptions, apiAssemblies));

        services.AddCors(corsOptions =>
        {
            corsOptions.AddPolicy("cors_all", policy =>
                policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
        });

        return services;
    }

    public static WebApplication UseNeoAdmin(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.Services.GetRequiredService<IFreeSql>();
        app.UseRouting();

        NeoAdminOptions options = app.Services.GetRequiredService<IOptions<NeoAdminOptions>>().Value;
        if (options.EnableIpWhitelist)
        {
            app.UseIpWhitelist();
        }

        if (IsSwaggerEnabled(app.Environment, options))
        {
            app.UseCors("cors_all");
            app.UseSwagger();
            app.UseSwaggerUI(swaggerUiOptions =>
            {
                swaggerUiOptions.RoutePrefix = "api";
                swaggerUiOptions.DocumentTitle = "NeoAdmin Swagger UI";
                swaggerUiOptions.SwaggerEndpoint("/v1/api-docs", "V1 Docs");
            });
            app.MapSwagger("{documentName}/api-docs");
        }

        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                TransactionalAttribute.SetServiceProvider(context.RequestServices);
            }

            await next();
        });

        app.MapControllers();
        app.MapNeoAdminVoiceHub();
        return app;
    }

    private static bool IsSwaggerEnabled(IHostEnvironment environment, NeoAdminOptions options) =>
        options.IsSwagger ?? environment.IsDevelopment();

    private static void ConfigureSwaggerGenOptions(
        SwaggerGenOptions swaggerOptions,
        IReadOnlyList<Assembly> apiAssemblies)
    {
        swaggerOptions.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "NeoAdmin WebAPI"
        });

        foreach (Assembly assembly in apiAssemblies)
        {
            string xmlPath = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
            if (File.Exists(xmlPath))
            {
                swaggerOptions.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }
        }

        swaggerOptions.SchemaFilter<SwaggerSchemaFilter>();
        swaggerOptions.DocumentFilter<HideControllerFilter>();

        swaggerOptions.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            Description = "Token 令牌",
            Name = "Authorization",
            In = ParameterLocation.Header
        });
    }

    private static void EnsureSeedData(IFreeSql freeSql, NeoAdminOptions options)
    {
        if (!freeSql.Select<SysUser>().Any(a => a.Username == options.SeedAdminUserName))
        {
            freeSql.Insert(new SysUser
            {
                Username = options.SeedAdminUserName,
                Nickname = "管理员",
                Password = options.SeedAdminPassword,
                Description = "系统初始化管理员",
                IsEnabled = true,
                IsSystem = true,
                LoginTime = DateTime.Now
            }).ExecuteAffrows();
        }

        MenuSeedData.Ensure(freeSql);
        FileApiMenuSeedData.Ensure(freeSql);
        RoleSeedData.Ensure(freeSql, options);
        DictSeedData.Ensure(freeSql);
        ParamSeedData.Ensure(freeSql);
        SiteSettingsSeedData.Ensure(freeSql);

        if (options.EnableSeedData)
        {
            OrgSeedData.Ensure(freeSql);
            UserSeedData.Ensure(freeSql);
        }
    }
}
