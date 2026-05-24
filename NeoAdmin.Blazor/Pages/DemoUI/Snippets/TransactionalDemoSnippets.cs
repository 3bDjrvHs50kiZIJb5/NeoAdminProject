namespace NeoAdmin.Blazor.Pages.DemoUI;

internal static class TransactionalDemoSnippets
{
    public const string Usage = """
        [Transactional]
        private async Task CommitAsync()
        {
            await UowManager.Orm.Insert(new SysParam
            {
                Key = "tx.demo.batch.a",
                Title = "步骤 A"
            }).ExecuteAffrowsAsync();

            await UowManager.Orm.Insert(new SysParam
            {
                Key = "tx.demo.batch.b",
                Title = "步骤 B"
            }).ExecuteAffrowsAsync();
        }

        [Transactional]
        private async Task RollbackAsync()
        {
            await UowManager.Orm.Insert(new SysParam
            {
                Key = "tx.demo.batch.a",
                Title = "将被回滚"
            }).ExecuteAffrowsAsync();

            throw new InvalidOperationException("模拟失败，触发 Rollback");
        }
        """;

    public const string Behavior = """
        方法进入时自动 Begin 工作单元
        方法正常结束（Task 成功）时 Commit
        抛出异常或 Task 失败时 Rollback
        事务内请使用 UowManager.Orm，而不是单独注入的 IFreeSql
        Blazor 页面需注入 IServiceProvider 与 UnitOfWorkManager
        API 控制器依赖 /api 中间件设置 ServiceProvider
        """;
}
