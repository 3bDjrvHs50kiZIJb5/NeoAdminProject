using FreeSql;
using NeoAdmin.Blazor.Data.Entities;

namespace NeoAdmin.Blazor.SeedData;

public static class ParamSeedData
{
    public static void Ensure(IFreeSql freeSql)
    {
        if (freeSql.Select<SysParam>().Any())
        {
            return;
        }

        freeSql.Insert(new SysParam
        {
            Key = "Site_Title",
            Title = "站点标题",
            Enabled = true,
            Sort = 10,
            Value = "NeoAdmin",
            Value2 = "后台管理系统",
            Description = "Value=站点名称，Value2=副标题"
        }).ExecuteAffrows();

        freeSql.Insert(new SysParam
        {
            Key = "Home_ContactCard",
            Title = "首页联系卡片",
            Enabled = true,
            Sort = 20,
            Value = "联系我们",
            Value2 = "如需开通新模块或初始化演示数据，请联系管理员。",
            Value3 = "400-800-1234",
            Description = "Value=标题，Value2=正文，Value3=联系电话"
        }).ExecuteAffrows();
    }
}
