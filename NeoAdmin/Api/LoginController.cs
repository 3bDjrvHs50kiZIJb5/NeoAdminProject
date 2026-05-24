using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeoAdmin.Blazor.Api;
using NeoAdmin.Blazor.Api.Dto;
using NeoAdmin.Blazor.Auth;
using NeoAdmin.Blazor.Entities;

namespace NeoAdmin.Api;

/// <summary>
/// 账号相关 API（参照 NovaAdmin LoginService）。
/// </summary>
[Route("api/login")]
[Tags("账号接口")]
public sealed class LoginController : BaseApiController
{
    private static readonly ConcurrentDictionary<string, int> LoginLimit = new();

    public LoginController(
        IFreeSql freeSql,
        NeoAdminAuthService auth,
        ILogger<LoginController> logger)
        : base(freeSql, auth, logger)
    {
    }

    /// <summary>
    /// 注册新账号。
    /// </summary>
    [HttpPost($"@{nameof(Register)}")]
    [AllowAnonymous]
    public async Task<ApiResult<UserSummaryResponse>> Register([FromBody] RegisterRequest request)
    {
        ApiResult? validationError = ValidateRequest(request);
        if (validationError is not null)
        {
            return ApiResult<UserSummaryResponse>.Error(validationError.Message, validationError.Code);
        }

        string username = request.Username.Trim();
        if (await FreeSql.Select<SysUser>().AnyAsync(a => a.Username == username))
        {
            return ApiResult<UserSummaryResponse>.Error("用户名已存在");
        }

        SysRole? role = await FreeSql.Select<SysRole>()
            .Where(a => !a.IsAdministrator)
            .OrderBy(a => a.Id)
            .FirstAsync();

        var user = new SysUser
        {
            Username = username,
            Nickname = string.IsNullOrWhiteSpace(request.Nickname) ? username : request.Nickname.Trim(),
            Password = request.Password,
            Description = request.Description?.Trim() ?? string.Empty,
            IsEnabled = true,
            LoginTime = DateTime.Now
        };

        await FreeSql.Insert(user).ExecuteAffrowsAsync();

        if (role is not null)
        {
            await FreeSql.Insert(new SysRoleUser { RoleId = role.Id, UserId = user.Id }).ExecuteAffrowsAsync();
        }

        await WriteLoginLogAsync(user.Username, SysUserLoginLog.LogType.登陆成功, "API register");

        List<string> roleNames = await GetRoleNamesAsync(user.Id);

        return ApiResult<UserSummaryResponse>.Success(new UserSummaryResponse
        {
            Id = user.Id,
            Username = user.Username,
            Nickname = user.Nickname,
            IsEnabled = user.IsEnabled,
            LoginTime = user.LoginTime,
            Roles = roleNames
        }, "注册成功");
    }

    /// <summary>
    /// 获取最近登录用户列表。
    /// </summary>
    [HttpGet($"@{nameof(GetWhoIsUsingList)}")]
    public async Task<ApiResult<UserCardListResponse>> GetWhoIsUsingList([FromQuery, Range(1, 24)] int limit = 12)
    {
        List<SysUser> users = await FreeSql.Select<SysUser>()
            .Where(a => a.IsEnabled)
            .OrderByDescending(a => a.LoginTime)
            .Limit(limit)
            .ToListAsync();

        List<UserCardResponse> items = users.Select(a => new UserCardResponse
        {
            Id = a.Id,
            Username = a.Username,
            DisplayName = DisplayName(a),
            LastLoginTime = a.LoginTime,
            Description = a.Description
        }).ToList();

        return ApiResult<UserCardListResponse>.Success(new UserCardListResponse
        {
            TotalCount = items.Count,
            Items = items
        });
    }

    /// <summary>
    /// 用户登录。
    /// </summary>
    [HttpPost($"@{nameof(Login)}")]
    [AllowAnonymous]
    public async Task<ApiResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        ApiResult? validationError = ValidateRequest(request);
        if (validationError is not null)
        {
            return ApiResult<LoginResponse>.Error(validationError.Message, validationError.Code);
        }

        string ip = GetClientIpAddress();
        if (LoginLimit.TryGetValue(ip, out int count) && count >= 5)
        {
            return ApiResult<LoginResponse>.Error($"{ip} 操作频率过高，请稍后再试...");
        }

        SysUser? user = await FreeSql.Select<SysUser>()
            .IncludeMany(a => a.Roles)
            .Where(a => a.Username == request.Username.Trim())
            .FirstAsync();

        if (user is null || !string.Equals(user.Password, request.Password, StringComparison.Ordinal))
        {
            count = LoginLimit.AddOrUpdate(ip, 1, (_, oldValue) => oldValue + 1);
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(60));
                LoginLimit.TryRemove(ip, out _);
            });
            await WriteLoginLogAsync(request.Username, SysUserLoginLog.LogType.登陆失败, $"failed:{count}");
            return ApiResult<LoginResponse>.Error($"用户名或密码错误，当前限制次数：{count}");
        }

        if (!user.IsEnabled)
        {
            await WriteLoginLogAsync(user.Username, SysUserLoginLog.LogType.登陆失败, "disabled");
            return ApiResult<LoginResponse>.Error("账户已被禁用");
        }

        user.LoginTime = DateTime.Now;
        await FreeSql.Update<SysUser>()
            .Where(a => a.Id == user.Id)
            .Set(a => a.LoginTime, user.LoginTime)
            .ExecuteAffrowsAsync();

        await WriteLoginLogAsync(user.Username, SysUserLoginLog.LogType.登陆成功);

        List<string> roleNames = user.Roles?.Select(a => a.Name).ToList()
            ?? await GetRoleNamesAsync(user.Id);

        return ApiResult<LoginResponse>.Success(new LoginResponse
        {
            Token = BuildToken(user),
            User = new UserSummaryResponse
            {
                Id = user.Id,
                Username = user.Username,
                Nickname = user.Nickname,
                IsEnabled = user.IsEnabled,
                LoginTime = user.LoginTime,
                Roles = roleNames
            }
        }, "登录成功");
    }

    /// <summary>
    /// 退出登录。
    /// </summary>
    [HttpGet($"@{nameof(Logout)}")]
    public Task<ApiResult<bool>> Logout()
    {
        HttpContext.Response.Cookies.Delete("NeoAdmin.Auth");
        return Task.FromResult(ApiResult<bool>.Success(true, "已退出登录"));
    }

    /// <summary>
    /// 校验登录态。
    /// </summary>
    [HttpGet($"@{nameof(Check)}")]
    public async Task<ApiResult<UserDetailResponse>> Check()
    {
        SysUser? user = await GetCurrentUserWithRolesAsync();
        if (user is null)
        {
            return ApiResult<UserDetailResponse>.Error("未登录或登录已过期", 401);
        }

        return ApiResult<UserDetailResponse>.Success(new UserDetailResponse
        {
            Id = user.Id,
            Username = user.Username,
            Nickname = user.Nickname,
            IsEnabled = user.IsEnabled,
            LoginTime = user.LoginTime,
            Description = user.Description,
            Roles = user.Roles?.Select(a => a.Name).ToList() ?? []
        });
    }

    /// <summary>
    /// 更新用户信息。
    /// </summary>
    [HttpPost($"@{nameof(UpdateMemberInfo)}")]
    public async Task<ApiResult> UpdateMemberInfo([FromBody] UpdateMemberInfoRequest request)
    {
        SysUser? user = await GetCurrentUserAsync();
        if (user is null)
        {
            return ApiResult.Error("未登录或登录已过期", 401);
        }

        if (!string.IsNullOrWhiteSpace(request.Nickname))
        {
            user.Nickname = request.Nickname.Trim();
        }

        if (request.Description is not null)
        {
            user.Description = request.Description.Trim();
        }

        await FreeSql.Update<SysUser>()
            .Where(a => a.Id == user.Id)
            .Set(a => a.Nickname, user.Nickname)
            .Set(a => a.Description, user.Description)
            .ExecuteAffrowsAsync();

        return ApiResult.Success("用户信息更新成功");
    }

    /// <summary>
    /// 修改登录密码。
    /// </summary>
    [HttpPost($"@{nameof(ChangePassword)}")]
    public async Task<ApiResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        ApiResult? validationError = ValidateRequest(request);
        if (validationError is not null)
        {
            return validationError;
        }

        SysUser? user = await GetCurrentUserAsync();
        if (user is null)
        {
            return ApiResult.Error("未登录或登录已过期", 401);
        }

        if (!string.Equals(user.Password, request.OldPassword, StringComparison.Ordinal))
        {
            return ApiResult.Error("旧密码错误，请重新输入");
        }

        if (string.Equals(request.OldPassword, request.NewPassword, StringComparison.Ordinal))
        {
            return ApiResult.Error("新密码不能与旧密码相同");
        }

        await FreeSql.Update<SysUser>()
            .Where(a => a.Id == user.Id)
            .Set(a => a.Password, request.NewPassword)
            .Set(a => a.LoginTime, DateTime.Now)
            .ExecuteAffrowsAsync();

        return ApiResult.Success("密码修改成功");
    }

    /// <summary>
    /// 注销当前账户（停用）。
    /// </summary>
    [HttpPost($"@{nameof(DeleteAccount)}")]
    public async Task<ApiResult> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        ApiResult? validationError = ValidateRequest(request);
        if (validationError is not null)
        {
            return validationError;
        }

        SysUser? user = await GetCurrentUserAsync();
        if (user is null)
        {
            return ApiResult.Error("未登录或登录已过期", 401);
        }

        if (!string.Equals(user.Password, request.Password, StringComparison.Ordinal))
        {
            return ApiResult.Error("密码错误，无法注销账户");
        }

        if (user.IsSystem)
        {
            return ApiResult.Error("系统用户不允许注销账户");
        }

        await FreeSql.Update<SysUser>()
            .Where(a => a.Id == user.Id)
            .Set(a => a.IsEnabled, false)
            .ExecuteAffrowsAsync();

        await WriteLoginLogAsync(user.Username, SysUserLoginLog.LogType.登陆失败, "account-disabled");
        return ApiResult.Success("账户已停用");
    }

    [HttpPost($"@{nameof(UploadAvatar)}")]
    public Task<ApiResult> UploadAvatar([FromBody] UploadAvatarRequest request) =>
        Task.FromResult(ApiResult.Error("当前项目的 SysUser 未提供头像字段，接口未启用", 501));

    [HttpPost($"@{nameof(UploadBadgePhoto)}")]
    public Task<ApiResult> UploadBadgePhoto([FromBody] UploadBadgePhotoRequest request) =>
        Task.FromResult(ApiResult.Error("当前项目未提供胸卡照片字段，接口未启用", 501));

    [HttpPost($"@{nameof(SendResetPasswordCode)}")]
    [AllowAnonymous]
    public Task<ApiResult> SendResetPasswordCode([FromBody] SendResetPasswordCodeRequest request) =>
        Task.FromResult(ApiResult.Error("当前项目未集成短信验证码能力", 501));

    [HttpPost($"@{nameof(ResetPassword)}")]
    [AllowAnonymous]
    public Task<ApiResult> ResetPassword([FromBody] ResetPasswordRequest request) =>
        Task.FromResult(ApiResult.Error("当前项目未启用短信找回密码流程", 501));

    [HttpPost($"@{nameof(SetAIAlarmLevel)}")]
    public Task<ApiResult> SetAIAlarmLevel([FromBody] SetAIAlarmLevelRequest request) =>
        Task.FromResult(ApiResult.Error("当前项目没有 AI 报警等级业务", 501));

    private string BuildToken(SysUser user) => Auth.CreateToken(user);

    private async Task<SysUser?> GetCurrentUserWithRolesAsync()
    {
        ApiResult<UserSummaryResponse> check = await CheckCurrentUserAsync();
        if (!check.Succeeded || check.Data is null)
        {
            return null;
        }

        return await FreeSql.Select<SysUser>()
            .IncludeMany(a => a.Roles)
            .Where(a => a.Id == check.Data.Id)
            .FirstAsync();
    }

    private async Task<List<string>> GetRoleNamesAsync(long userId)
    {
        List<SysRoleUser> links = await FreeSql.Select<SysRoleUser>()
            .Include(a => a.Role)
            .Where(a => a.UserId == userId)
            .ToListAsync();

        return links
            .Where(a => a.Role is not null)
            .Select(a => a.Role!.Name)
            .ToList();
    }
}
