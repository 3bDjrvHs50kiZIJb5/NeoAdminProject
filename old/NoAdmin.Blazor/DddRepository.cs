using System;
using FreeSql;

internal class DddRepository<TEntity> : AggregateRootRepository<TEntity>, IAggregateRootRepository<TEntity>, IBaseRepository<TEntity>, IBaseRepository, IDisposable, IAggregateRootRepositoryTransient<TEntity> where TEntity : class
{
	public override ISelect<TEntity> Select => base.SelectDiy;

	public DddRepository(IFreeSql fsql)
		: base(fsql)
	{
	}

	public DddRepository(IFreeSql fsql, UnitOfWorkManager uowManager)
		: base(fsql, uowManager)
	{
	}

	public DddRepository(IFreeSql fsql, UnitOfWorkManager uowManager, RepositoryOptions options)
		: base(fsql, uowManager)
	{
		if (options != null)
		{
			base.DbContextOptions.NoneParameter = options.NoneParameter;
			base.DbContextOptions.EnableGlobalFilter = options.EnableGlobalFilter;
			base.DbContextOptions.AuditValue = options.AuditValue;
		}
	}
}
