using FreeSql;

internal class BasicRepository<TEntity, TKey> : BaseRepository<TEntity, TKey> where TEntity : class
{
	public BasicRepository(IFreeSql fsql)
		: base(fsql)
	{
	}

	public BasicRepository(IFreeSql fsql, UnitOfWorkManager uowManager)
		: base(((uowManager != null) ? uowManager.Orm : null) ?? fsql)
	{
		if (uowManager != null)
		{
			uowManager.Binding((IBaseRepository)(object)this);
		}
	}

	public BasicRepository(IFreeSql fsql, UnitOfWorkManager uowManager, RepositoryOptions options)
		: base(((uowManager != null) ? uowManager.Orm : null) ?? fsql)
	{
		if (uowManager != null)
		{
			uowManager.Binding((IBaseRepository)(object)this);
		}
		if (options != null)
		{
			((BaseRepository<TEntity>)(object)this).DbContextOptions.NoneParameter = options.NoneParameter;
			((BaseRepository<TEntity>)(object)this).DbContextOptions.EnableGlobalFilter = options.EnableGlobalFilter;
			((BaseRepository<TEntity>)(object)this).DbContextOptions.AuditValue = options.AuditValue;
		}
	}
}
internal class BasicRepository<TEntity> : BasicRepository<TEntity, long> where TEntity : class
{
	public BasicRepository(IFreeSql fsql)
		: base(fsql)
	{
	}

	public BasicRepository(IFreeSql fsql, UnitOfWorkManager uowManager)
		: base(fsql, uowManager)
	{
	}

	public BasicRepository(IFreeSql fsql, UnitOfWorkManager uowManager, RepositoryOptions options)
		: base(fsql, uowManager, options)
	{
	}
}
