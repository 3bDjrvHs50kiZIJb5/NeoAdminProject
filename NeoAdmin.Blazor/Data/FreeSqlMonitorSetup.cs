using FreeSql;
using Microsoft.Extensions.Logging;

namespace NeoAdmin.Blazor.Data;

/// <summary>
/// 开发调试：通过 FreeSql AOP 记录失败或慢 SQL（超过 2 秒记为 Error，便于在系统日志中筛选）。
/// </summary>
internal static class FreeSqlMonitorSetup
{
    private const int SlowSqlThresholdMs = 2000;

    public static void Configure(IFreeSql freeSql, ILogger logger)
    {
        freeSql.Aop.CommandAfter += (_, e) =>
        {
            string sql = e.Command.CommandText;
            if (e.Exception is not null)
            {
                logger.LogError(
                    e.Exception,
                    "SQL 执行失败，耗时 {ElapsedMs}ms，SQL={Sql}",
                    e.ElapsedMilliseconds,
                    sql);
                return;
            }

            if (e.ElapsedMilliseconds <= SlowSqlThresholdMs)
            {
                return;
            }

            logger.LogError(
                "SQL 执行过慢，耗时 {ElapsedMs}ms，SQL={Sql}",
                e.ElapsedMilliseconds,
                sql);
        };
    }
}
