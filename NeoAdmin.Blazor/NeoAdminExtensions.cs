using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FreeSql;
using NeoAdmin.Blazor.Auth;
using NeoAdmin.Blazor.Data;
using NeoAdmin.Blazor.Data.Entities;
using NeoAdmin.Blazor.Menus;
using NeoAdmin.Blazor.Middlewares;
using NeoAdmin.Blazor.Scheduling;
using NeoAdmin.Blazor.SeedData;
using NeoAdmin.Blazor.Services;
using FreeScheduler;

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

        services.AddSingleton(serviceProvider =>
        {
            NeoAdminOptions options = serviceProvider.GetRequiredService<IOptions<NeoAdminOptions>>().Value;
            SQLitePCL.Batteries.Init();
            FreeSqlSnowflakeSetup.SetIdGenerator(options.WorkId);

            var builder = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(options.DataType, options.ConnectionString)
                .UseAutoSyncStructure(options.AutoSyncStructure);

            if (options.MonitorCommand)
            {
                builder.UseMonitorCommand(command => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {command.CommandText}"));
            }

            IFreeSql freeSql = builder.Build();
            FreeSqlSnowflakeSetup.Configure(freeSql);
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
            }

            EnsureSeedData(freeSql, options);
            return freeSql;
        });

        services.AddScoped<NeoAdminAuthService>();
        services.AddScoped<MenuService>();
        services.AddScoped<FileService>();
        services.AddScoped<OrgService>();
        services.AddScoped<RoleService>();
        services.AddScoped<TaskSchedulerService>();
        services.AddScoped<SiteSettingsService>();
        services.AddNeoAdminScheduler();
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

        return app;
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
        OrgSeedData.Ensure(freeSql);
        RoleSeedData.Ensure(freeSql, options);
        DictSeedData.Ensure(freeSql);
        ParamSeedData.Ensure(freeSql);
        SiteSettingsSeedData.Ensure(freeSql);
        UserSeedData.Ensure(freeSql);
    }
}
