using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NeoAdmin.Blazor.Data;
using NeoAdminApp.SeedData;

namespace NeoAdminApp.Api;

/// <summary>E2E 测试基线数据接口，仅 Development 环境可用。</summary>
[ApiController]
[Route("api/e2e/seed")]
[AllowAnonymous]
public sealed class E2eSeedController(
    IWebHostEnvironment environment,
    IFreeSql freeSql,
    IOptions<NeoAdminOptions> neoAdminOptions) : ControllerBase
{
    [HttpPost("ensure")]
    public IActionResult Ensure()
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        DataSetup.EnsureSeedData(freeSql, neoAdminOptions.Value);
        return Ok();
    }
}
