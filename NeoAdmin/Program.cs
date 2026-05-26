using System.Reflection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using NeoAdmin.SeedData;
using NeoAdmin.Blazor.Extensions;
using NeoAdmin.Blazor.Components;
using NeoAdmin.Jobs;
using NeoUI.Blazor.Extensions;
using NeoUI.Blazor.Primitives.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.AddNeoAdminSerilog();

builder.Services.AddNeoUIPrimitives();
builder.Services.AddNeoUIComponents();
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddNeoAdmin(builder.Configuration, options =>
{
    options.SchedulerAssemblies = [Assembly.GetExecutingAssembly()];
});
builder.Services.AddNeoAdminApi(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.Logger.LogInformation("NeoAdmin 启动。Environment={Environment}", app.Environment.EnvironmentName);

DataSetup.Initialize(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseNeoAdminSerilogRequestLogging();
app.UseHttpsRedirection();
// 运行时上传的文件不在 StaticAssets 清单内，需用 StaticFiles 提供
app.UseStaticFiles();
app.MapStaticAssets();
app.UseNeoAdmin();
app.UseAntiforgery();
app.MapRazorPages();
app.MapRazorComponents<NeoAdmin.App>()
    .AddAdditionalAssemblies(typeof(LayoutAdmin).Assembly)
    .AddInteractiveServerRenderMode();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var addresses = app.Services.GetRequiredService<IServer>()
        .Features.Get<IServerAddressesFeature>()?.Addresses;
    if (addresses is null || addresses.Count == 0)
        return;

    foreach (var address in addresses)
        app.Logger.LogInformation("请在浏览器中访问: {Address}", address);
});

try
{
    app.Logger.LogInformation("NeoAdmin 启动完成，开始监听请求。");
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
