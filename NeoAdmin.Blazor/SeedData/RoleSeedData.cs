using FreeSql;
using NeoAdmin.Blazor.Data;
using NeoAdmin.Blazor.Data.Entities;
using Microsoft.Extensions.Options;

namespace NeoAdmin.Blazor.SeedData;

public static class RoleSeedData
{
    public static void Ensure(IFreeSql freeSql, NeoAdminOptions options)
    {
        long adminRoleId = EnsureRole(
            freeSql,
            "管理员",
            "管理员角色",
            isAdministrator: true);

        EnsureRole(
            freeSql,
            "普通用户",
            "普通用户",
            isAdministrator: false);

        SysUser? adminUser = freeSql.Select<SysUser>()
            .Where(a => a.Username == options.SeedAdminUserName)
            .First();

        if (adminUser is null)
        {
            return;
        }

        bool linked = freeSql.Select<SysRoleUser>()
            .Any(a => a.RoleId == adminRoleId && a.UserId == adminUser.Id);

        if (!linked)
        {
            freeSql.Insert(new SysRoleUser
            {
                RoleId = adminRoleId,
                UserId = adminUser.Id
            }).ExecuteAffrows();
        }
    }

    private static long EnsureRole(
        IFreeSql freeSql,
        string name,
        string description,
        bool isAdministrator)
    {
        SysRole? role = freeSql.Select<SysRole>()
            .Where(a => a.Name == name)
            .First();

        if (role is not null)
        {
            return role.Id;
        }

        SysRole newRole = new()
        {
            Name = name,
            Description = description,
            IsAdministrator = isAdministrator
        };
        newRole.Id = freeSql.Insert(newRole).ExecuteIdentity();
        return newRole.Id;
    }
}
