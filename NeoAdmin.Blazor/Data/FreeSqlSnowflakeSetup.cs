using System.Reflection;
using FreeSql;
using NeoAdmin.Blazor.Data.Attributes;
using Yitter.IdGenerator;

namespace NeoAdmin.Blazor.Data;

internal static class FreeSqlSnowflakeSetup
{
    public static void Configure(IFreeSql freeSql)
    {
        freeSql.Aop.AuditValue += (_, e) =>
        {
            if (e.Column.CsType != typeof(long))
            {
                return;
            }

            if (e.Property.GetCustomAttribute<SnowflakeAttribute>(inherit: false) is null)
            {
                return;
            }

            if (e.Value is long id && id != 0)
            {
                return;
            }

            e.Value = YitIdHelper.NextId();
        };
    }

    public static void SetIdGenerator(ushort workId)
    {
        YitIdHelper.SetIdGenerator(new IdGeneratorOptions(workId)
        {
            WorkerIdBitLength = 6
        });
    }
}
