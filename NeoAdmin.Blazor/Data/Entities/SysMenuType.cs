namespace NeoAdmin.Blazor.Data.Entities;

public enum SysMenuType
{
    菜单,
    按钮,
    外部连接,
    增删改查,
    接口
}

public static class SysMenuTypeExtensions
{
    /// <summary>按钮、接口等权限点，不在侧边栏展示。</summary>
    public static bool IsPermissionNode(this SysMenuType type) =>
        type is SysMenuType.按钮 or SysMenuType.接口;
}
