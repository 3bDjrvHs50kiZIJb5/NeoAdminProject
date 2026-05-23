using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NoAdmin.Blazor.Infrastructure.Encrypt;
using NoAdmin.Blazor.Components;
using FreeScheduler;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using OtpNet;
using NoAdmin.Blazor.Utils;

namespace NoAdmin.Blazor.AdminOmni.Controllers;

/// <summary>
/// 登陆控制器
/// </summary>
[Route("api/Login")]
[Tags(new string[] { "系统" })]
[AllowAnonymous]
public class LoginController : ControllerBase
{
	private readonly IHttpContextAccessor httpContextAccessor;

	private readonly IFreeSql fsql;

	private readonly Scheduler scheduler;

	private readonly NovaAdminContext _adminContext;

	private static ConcurrentDictionary<string, int> limit = new ConcurrentDictionary<string, int>();

	public LoginController(IHttpContextAccessor httpContextAccessor, IFreeSql fsql, Scheduler scheduler, NovaAdminContext adminContext)
	{
		this.httpContextAccessor = httpContextAccessor;
		this.fsql = fsql;
		this.scheduler = scheduler;
		_adminContext = adminContext;
	}

	/// <summary>
	/// 登录提交接口
	/// </summary>
	/// <param name="username">用户名</param>
	/// <param name="password">密码</param>
	/// <param name="code">2FA验证码（可选）</param>
	/// <param name="tempSecret">绑定时的临时密钥（可选）</param>
	/// <returns></returns>
	[HttpPost("@submit")]
	public async Task<ApiResult> Submit([FromForm] string username, [FromForm] string password, [FromForm] string? code = null, [FromForm] string? tempSecret = null, [FromHeader] string? fingerprint = null)
	{
		if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
		{
			return ApiResult.Error.SetMessage("用户名或密码不能为空");
		}
		string ip = IpHelper.GetClientIpAddress(httpContextAccessor.HttpContext);
		if (limit.TryGetValue(ip, out var count) && count >= 5)
		{
			return ApiResult.Error.SetMessage("频率过高！请过一会再试...");
		}
		SysUserLoginLog obj = new SysUserLoginLog
		{
			LoginTime = DateTime.Now,
			Username = username,
			Ip = ip,
			Browser = "API/Web"
		};
		StringValues? stringValues = httpContextAccessor.HttpContext?.Request.Headers["User-Agent"];
		obj.UserAgent = (stringValues.HasValue ? ((string?)stringValues.GetValueOrDefault()) : null);
		obj.City = "未知";
		SysUserLoginLog log = obj;
		SysUser user = await ((ISelect0<ISelect<SysUser>, SysUser>)(object)fsql.Select<SysUser>().IncludeMany<SysRole>((Expression<Func<SysUser, IEnumerable<SysRole>>>)((SysUser a) => a.Roles), (Action<ISelect<SysRole>>)null).Where((Expression<Func<SysUser, bool>>)((SysUser a) => a.Username == username))).FirstAsync(default(CancellationToken));
		if (user == null || user.Password != password)
		{
			if (limit.AddOrUpdate(ip, ++count, (string _, int __) => count) == 1)
			{
				scheduler.AddTempTask(TimeSpan.FromSeconds(60.0), (Action)delegate
				{
					limit.TryRemove(ip, out var _);
				});
			}
			log.Type = SysUserLoginLog.LogType.登陆失败;
			await fsql.Insert<SysUserLoginLog>(log).ExecuteAffrowsAsync(default(CancellationToken));
			return ApiResult.Error.SetMessage("用户名或密码不正确");
		}
		if (user.Roles.All((SysRole a) => string.IsNullOrWhiteSpace(a.IpWhiteList)) || user.Roles.Any((SysRole a) => !string.IsNullOrWhiteSpace(a.IpWhiteList) && ("; " + a.IpWhiteList + ";").Contains("; " + ip + ";")))
		{
			return await PerformLogin(user, log, fingerprint);
		}
		long num = default(long);
		if (string.IsNullOrEmpty(user.GoogleSecretKey))
		{
			if (string.IsNullOrEmpty(tempSecret) || string.IsNullOrEmpty(code))
			{
				string newSecretKey = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
				string otpUri = ((object)new OtpUri((OtpType)0, Base32Encoding.ToBytes(newSecretKey), user.Username, _adminContext.Tenant?.Title ?? "AdminSystem", (OtpHashMode)0, 6, 30, 0L)).ToString();
				return ApiResult.Success.SetData(new
				{
					action = "Bind2FA",
					qrCodeUrl = otpUri,
					secret = newSecretKey,
					message = "请绑定谷歌验证器"
				});
			}
			byte[] bindBytes = Base32Encoding.ToBytes(tempSecret);
			Totp bindTotp = new Totp(bindBytes, 30, (OtpHashMode)0, 6, (TimeCorrection)null);
				if (!bindTotp.VerifyTotp(code, out num, VerificationWindow.RfcSpecifiedNetworkDelay))
			{
				return ApiResult.Error.SetMessage("验证码错误，绑定失败");
			}
			user.GoogleSecretKey = tempSecret;
			await fsql.Update<SysUser>().SetSource(user).UpdateColumns((Expression<Func<SysUser, object>>)((SysUser a) => a.GoogleSecretKey))
				.ExecuteAffrowsAsync(default(CancellationToken));
			return await PerformLogin(user, log, fingerprint);
		}
		if (string.IsNullOrEmpty(code))
		{
			return ApiResult.Success.SetData(new
			{
				action = "Verify2FA",
				message = "请输入验证码"
			});
		}
		byte[] bytes = Base32Encoding.ToBytes(user.GoogleSecretKey);
		Totp totp = new Totp(bytes, 30, (OtpHashMode)0, 6, (TimeCorrection)null);
			if (!totp.VerifyTotp(code, out num, VerificationWindow.RfcSpecifiedNetworkDelay))
		{
			return ApiResult.Error.SetMessage("验证码错误");
		}
		return await PerformLogin(user, log, fingerprint);
	}

	private async Task<ApiResult> PerformLogin(SysUser user, SysUserLoginLog log, string? fingerprint)
	{
		user.LoginTime = DateTime.Now;
		await fsql.Update<SysUser>().Where((Expression<Func<SysUser, bool>>)((SysUser a) => a.Id == user.Id)).Set<DateTime>((Expression<Func<SysUser, DateTime>>)((SysUser a) => a.LoginTime), user.LoginTime)
			.ExecuteAffrowsAsync(default(CancellationToken));
		string token = DesEncrypt.Encrypt(user.Id + "|" + user.LoginTime.ToString("yyyy-MM-dd HH:mm:ss") + "|" + fingerprint);
		log.Type = SysUserLoginLog.LogType.登陆成功;
		await fsql.Insert<SysUserLoginLog>(log).ExecuteAffrowsAsync(default(CancellationToken));
		return ApiResult.Success.SetData(new
		{
			action = "Success",
			token = token,
			message = "登录成功"
		});
	}

	[HttpGet("redirect")]
	public async Task<IActionResult> RedirectWithToken([FromServices] NovaAdminContext admin, [FromQuery] string token, [FromQuery] string redirect)
	{
		await admin.Init(isApi: true);
		if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(redirect))
		{
			return BadRequest("Token and redirect URL are required.");
		}
		if (!Uri.TryCreate(redirect, UriKind.RelativeOrAbsolute, out Uri redirectUri))
		{
			return BadRequest("Invalid redirect URL.");
		}
		if (redirectUri.IsAbsoluteUri && (!string.Equals(b: base.Request.Host.Host, a: redirectUri.Host, comparisonType: StringComparison.OrdinalIgnoreCase) || (!(redirectUri.Scheme == Uri.UriSchemeHttp) && !(redirectUri.Scheme == Uri.UriSchemeHttps))))
		{
			return BadRequest("Redirect URL must be on the same host and use HTTP or HTTPS.");
		}
		base.Response.Cookies.Append(admin.CookieName, token, new CookieOptions
		{
			Path = "/",
			Expires = DateTimeOffset.UtcNow.AddHours(7.0)
		});
		return Redirect(redirectUri.ToString());
	}
}
