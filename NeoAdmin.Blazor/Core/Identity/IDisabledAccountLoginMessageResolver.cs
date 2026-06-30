using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Blazor.Core.Identity;

/// <summary>
/// 宿主应用可注册此接口，自定义未开通账号登录时的提示文案。
/// </summary>
public interface IDisabledAccountLoginMessageResolver
{
    Task<string?> TryResolveAsync(SysUser user, CancellationToken cancellationToken = default);
}
