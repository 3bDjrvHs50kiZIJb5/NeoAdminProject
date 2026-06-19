using FreeSql;
using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Blazor.SeedData;

/// <summary>
/// 启动种子版本标记：已应用当前版本时跳过重型检查，避免远程库每次启动大量往返。
/// 版本号持久化在 <see cref="SysParam"/> 中；迁移逻辑变更时递增 <c>currentVersion</c> 即可触发一次全量同步。
/// </summary>
public static class StartupSeedVersion
{
    public static bool IsApplied(IFreeSql freeSql, string key, string currentVersion)
    {
        string? value = freeSql.Select<SysParam>()
            .Where(p => p.Key == key)
            .First(p => p.Value);
        return string.Equals(value, currentVersion, StringComparison.Ordinal);
    }

    public static void MarkApplied(IFreeSql freeSql, string key, string currentVersion)
    {
        SysParam? param = freeSql.Select<SysParam>()
            .Where(p => p.Key == key)
            .First();
        if (param is null)
        {
            freeSql.Insert(new SysParam
            {
                Key = key,
                Title = key,
                Enabled = true,
                Sort = 0,
                Value = currentVersion,
            }).ExecuteAffrows();
            return;
        }

        if (string.Equals(param.Value, currentVersion, StringComparison.Ordinal))
        {
            return;
        }

        freeSql.Update<SysParam>()
            .Set(p => p.Value, currentVersion)
            .Where(p => p.Id == param.Id)
            .ExecuteAffrows();
    }
}
