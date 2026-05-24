using FreeSql;
using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Blazor.SeedData;

public static class DictSeedData
{
    public static void Ensure(IFreeSql freeSql)
    {
        if (freeSql.Select<SysDict>().Any())
        {
            return;
        }

        SysDict gender = Insert(freeSql, parentId: 0, name: "Gender", value: string.Empty,
            description: "性别字典分类", sort: 10);

        Insert(freeSql, parentId: gender.Id, name: "男", value: "M",
            description: "男性", sort: 1);
        Insert(freeSql, parentId: gender.Id, name: "女", value: "F",
            description: "女性", sort: 2);

        SysDict status = Insert(freeSql, parentId: 0, name: "Status", value: string.Empty,
            description: "状态字典分类", sort: 20);

        Insert(freeSql, parentId: status.Id, name: "启用", value: "1",
            description: "启用状态", sort: 1);
        Insert(freeSql, parentId: status.Id, name: "禁用", value: "0",
            description: "禁用状态", sort: 2);
    }

    private static SysDict Insert(IFreeSql freeSql, long parentId, string name, string value, string description, int sort)
    {
        SysDict dict = new()
        {
            ParentId = parentId,
            Name = name,
            Value = value,
            Description = description,
            Sort = sort,
            Enabled = true
        };
        freeSql.Insert(dict).ExecuteAffrows();
        return dict;
    }
}
