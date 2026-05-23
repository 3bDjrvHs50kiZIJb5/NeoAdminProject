using BootstrapBlazor.Components;
using FreeSql;

namespace NovaAdmin.SeedData;

/// <summary>
/// 参数配置示例种子 - 用于初始化首页演示所需的参数数据
/// </summary>
public static class ParamSeedData
{
    /// <summary>
    /// 初始化参数配置示例数据
    /// </summary>
    public static void Initialize(FreeSqlCloud fsql, long adminUserId, string adminUsername)
    {
        var exists = fsql.Select<SysParam>()
            .Any(a => a.Id == "Home_ContactCard");

        if (exists)
        {
            return;
        }

        fsql.Insert(new SysParam
        {
            Id = "Home_ContactCard",
            Title = "首页联系卡片",
            Enabled = true,
            Sort = 10,
            Value = "首页联系卡片",
            Value2 = "如需开通新模块或初始化演示数据，请联系实施同事处理。",
            Value3 = "400-800-1234",
            Description = "首页参数演示：Value=标题，Value2=正文，Value3=联系电话",
            CreatedUserId = adminUserId,
            CreatedUserName = adminUsername
        }).ExecuteAffrows();
    }
}
