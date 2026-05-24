using FreeSql;
using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Blazor.SeedData;

public static class OrgSeedData
{
    public static void Ensure(IFreeSql freeSql)
    {
        if (freeSql.Select<SysOrg>().Any())
        {
            return;
        }

        foreach (SysOrg root in CreateOrgs())
        {
            InsertRecursive(freeSql, root, 0);
        }
    }

    private static void InsertRecursive(IFreeSql freeSql, SysOrg org, long parentId)
    {
        org.ParentId = parentId;
        List<SysOrg> children = org.Children;
        org.Children = new List<SysOrg>();
        freeSql.Insert(org).ExecuteAffrows();

        foreach (SysOrg child in children)
        {
            InsertRecursive(freeSql, child, org.Id);
        }
    }

    private static List<SysOrg> CreateOrgs() =>
    [
        new SysOrg
        {
            Label = "集团",
            Type = SysOrgType.集团,
            Description = "集团",
            IsEnabled = true,
            Sort = 101,
            Children =
            [
                new SysOrg
                {
                    Label = "xx公司",
                    Type = SysOrgType.公司,
                    Description = "公司",
                    IsEnabled = true,
                    Sort = 201,
                    Children =
                    [
                        OrgDept("IT部", 301),
                        OrgDept("财务部", 302),
                        OrgDept("人事部", 303),
                        OrgDept("运营部", 304),
                        OrgDept("设计部", 305),
                        OrgDept("技术部", 306),
                        OrgDept("生产部", 307)
                    ]
                }
            ]
        },
        new SysOrg
        {
            Label = "供应商",
            Type = SysOrgType.供应商,
            Description = "供应商",
            IsEnabled = true,
            Sort = 102
        },
        new SysOrg
        {
            Label = "客户",
            Type = SysOrgType.客户,
            Description = "客户",
            IsEnabled = true,
            Sort = 103
        }
    ];

    private static SysOrg OrgDept(string label, int sort) => new()
    {
        Label = label,
        Type = SysOrgType.部门,
        Description = string.Empty,
        IsEnabled = true,
        Sort = sort
    };
}
