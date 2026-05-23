using NovaAdmin.Components;
using NovaAdmin.SeedData;
using BootstrapBlazor.Components;
using FreeSql;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

SQLitePCL.Batteries.Init();
builder.AddNovaAdminSerilog();

builder.AddNovaAdmin(new NovaAdminOptionsItem
{
    IsSampleLayout = false,
    Assemblies = [typeof(Program).Assembly],
    FreeSqlBuilder = freeSqlBuilder =>
        freeSqlBuilder
            .UseConnectionString(DataType.Sqlite, "Data Source=novaadmin.db")
            .UseMonitorCommand(cmd => System.Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {cmd.CommandText}\r\n"))
            .UseAutoSyncStructure(true),
    IsSwagger = true
});
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.Logger.LogInformation("NovaAdmin starting. Environment={Environment}", app.Environment.EnvironmentName);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseNovaAdminSerilogRequestLogging();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseBootstrapBlazor();
app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(typeof(NovaAdminOptionsItem).Assembly)
    .AddInteractiveServerRenderMode();
app.UseAdminOmniApi();

#region 博客示例测试数据
app.Logger.LogInformation("Preparing blog and menu seed data.");

var fsql = app.Services.GetRequiredService<FreeSqlCloud>();
var adminUser = fsql.Select<SysUser>().Where(a => a.Roles.Any(b => b.IsAdministrator)).First();

if (adminUser != null)
{
    app.Logger.LogInformation("Seed data initialization started for admin user {Username} ({UserId}).", adminUser.Username, adminUser.Id);

    // 菜单种子独立执行，避免被博客示例数据初始化条件挡住。
    MenuSeedData.Initialize(fsql, adminUser.Id, adminUser.Username);
    BlogSeedData.Initialize(fsql, adminUser.Id, adminUser.Username);
    DictSeedData.Initialize(fsql, adminUser.Id, adminUser.Username);
    ParamSeedData.Initialize(fsql, adminUser.Id, adminUser.Username);

    app.Logger.LogInformation("Seed data initialization completed for admin user {Username}.", adminUser.Username);
}
else
{
    app.Logger.LogWarning("Seed data skipped because no administrator user was found.");
}
#endregion

app.Logger.LogInformation("NovaAdmin startup pipeline configured. Application is ready to run.");

app.Run();
