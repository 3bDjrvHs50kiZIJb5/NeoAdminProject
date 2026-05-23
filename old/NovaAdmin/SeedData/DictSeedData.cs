using BootstrapBlazor.Components;
using FreeSql;

namespace NovaAdmin.SeedData;

/// <summary>
/// 数据字典示例种子 - 用于初始化首页演示所需的字典数据
/// </summary>
public static class DictSeedData
{
    private const long GenderCategoryId = 620000000000001;
    private const long GenderMaleId = 620000000000002;
    private const long GenderFemaleId = 620000000000003;

    /// <summary>
    /// 初始化数据字典示例数据
    /// </summary>
    public static void Initialize(FreeSqlCloud fsql, long adminUserId, string adminUsername)
    {
        var genderCategory = EnsureDict(
            fsql,
            id: GenderCategoryId,
            parentId: 0,
            name: "Gender",
            value: string.Empty,
            description: "首页字典演示：性别字典分类",
            sort: 10,
            enabled: true);

        EnsureDict(
            fsql,
            id: GenderMaleId,
            parentId: genderCategory.Id,
            name: "男",
            value: "M",
            description: "首页字典演示：男性",
            sort: 1,
            enabled: true);

        EnsureDict(
            fsql,
            id: GenderFemaleId,
            parentId: genderCategory.Id,
            name: "女",
            value: "F",
            description: "首页字典演示：女性",
            sort: 2,
            enabled: true);
    }

    private static SysDict EnsureDict(
        FreeSqlCloud fsql,
        long id,
        long parentId,
        string name,
        string value,
        string description,
        int sort,
        bool enabled)
    {
        var dict = fsql.Select<SysDict>()
            .Where(a => a.ParentId == parentId && a.Name == name)
            .First();

        if (dict != null)
        {
            return dict;
        }

        dict = new SysDict
        {
            Id = id,
            ParentId = parentId,
            Name = name,
            Value = value,
            Description = description,
            Sort = sort,
            Enabled = enabled
        };

        fsql.Insert(dict).ExecuteAffrows();
        return dict;
    }
}
