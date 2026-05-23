using NeoAdmin;
using NeoAdmin.Blazor.Extensions;
using NeoAdmin.Blazor.Components;
using NeoUI.Blazor.Extensions;
using NeoUI.Blazor.Primitives.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNeoUIPrimitives();
builder.Services.AddNeoUIComponents();
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddNeoAdmin(builder.Configuration);

var app = builder.Build();

BlogDataSetup.Initialize(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

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

app.Run();
