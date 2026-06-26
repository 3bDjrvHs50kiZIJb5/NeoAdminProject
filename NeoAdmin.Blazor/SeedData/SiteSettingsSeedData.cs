using FreeSql;
using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Blazor.SeedData;

public static class SiteSettingsSeedData
{
    public static void Ensure(IFreeSql freeSql)
    {
        if (freeSql.Select<SysSiteSettings>().Any())
        {
            return;
        }

        freeSql.Insert(new SysSiteSettings
        {
            Title = "NeoAdmin",
            Host = "localhost",
            Host2 = "127.0.0.1",
            Description = "NeoAdmin 管理后台",
            IsEnabled = true
        }).ExecuteAffrows();
    }
}
