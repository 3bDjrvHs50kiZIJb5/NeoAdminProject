using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Blazor.Audit;

public static class AuditMenuDefinitions
{
    public static readonly string[] AllButtonPaths =
    [
        "audit_00", "audit_01", "audit_98", "audit_99", "edit_version"
    ];

    public static readonly string[] StepButtonPaths = ["audit_01"];

    public static readonly string[] ObsoleteStepButtonPaths =
    [
        "audit_02", "audit_03", "audit_04", "audit_05"
    ];

    public static IReadOnlyList<SysMenu> CreateAuditButtons() =>
    [
        new SysMenu { Label = "提交", Path = "audit_00", Sort = 400, Type = SysMenuType.按钮, IsSystem = true },
        new SysMenu { Label = "一审", Path = "audit_01", Sort = 401, Type = SysMenuType.按钮, IsSystem = true },
        new SysMenu { Label = "拒绝", Path = "audit_98", Sort = 431, Type = SysMenuType.按钮, IsSystem = true },
        new SysMenu { Label = "反审", Path = "audit_99", Sort = 432, Type = SysMenuType.按钮, IsSystem = true },
        new SysMenu { Label = "历史版本", Path = "edit_version", Sort = 304, Type = SysMenuType.按钮, IsSystem = true }
    ];
}
