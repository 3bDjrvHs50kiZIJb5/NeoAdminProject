using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using NoAdmin.Blazor.Components;
using FreeScheduler;
using FreeSql;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OtpNet;
using Rougamo;
using Rougamo.Context;
using NoAdmin.Blazor.Utils;

namespace NoAdmin.Blazor.Pages;

[Layout(typeof(LayoutEmpty))]
[Route("/Login")]
public class Login : ComponentBase
{
	private enum LoginStep
	{
		Login,
		Bind2FA,
		Verify2FA
	}

	private string username;

	private string password;

	private bool remember = true;

	private string submitText = "安 全 登 录";

	private SysUserLoginLog loginLog;

	private LoginStep currentStep = LoginStep.Login;

	private string inputCode2FA;

	private string tempSecretKey;

	private string qrCodeUrl;

	private SysUser tempUser;

	private string removeLimit;

	private static ConcurrentDictionary<string, int> limit = new ConcurrentDictionary<string, int>();

	[Inject]
	private NovaAdminContext admin { get; set; }

	[Inject]
	private Scheduler scheduler { get; set; }

	[Inject]
	private WebClientService webClientService { get; set; }

	[Inject]
	private ToastService ToastService { get; set; } = null;

	[Inject]
	private MessageService MessageService { get; set; } = null;

	[Inject]
	private NavigationManager Nav { get; set; } = null;

	[Inject]
	private IJSRuntime JS { get; set; } = null;

	[Inject]
	private IServiceProvider ServiceProvider { get; set; } = null;

	protected override void BuildRenderTree(RenderTreeBuilder __builder)
	{
		__builder.OpenComponent<PageTitle>(0);
		__builder.AddAttribute(1, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(2, "登录 - ");
			renderTreeBuilder.AddContent(3, admin.Tenant?.Title);
		});
		__builder.CloseComponent();
		__builder.AddMarkupContent(4, "\r\n\r\n");
		__builder.OpenElement(5, "style");
		__builder.AddMarkupContent(6, """
body,html{height:100%}
body{-moz-osx-font-smoothing:grayscale;-webkit-font-smoothing:antialiased;text-rendering:optimizeLegibility;font-family:Helvetica Neue,Helvetica,PingFang SC,Hiragino Sans GB,Microsoft YaHei,Arial,sans-serif;background:linear-gradient(180deg,#f4f7f5 0%,#eef3f1 100%)}
html,body,#app{height:100%;margin:0;padding:0}
.login1{display:flex;flex-direction:column;justify-content:center;align-items:center;height:100%;min-width:1200px;min-height:600px;background:radial-gradient(circle at 12% 12%, rgba(204, 223, 223, 0.55), transparent 24%),radial-gradient(circle at 82% 18%, rgba(32, 86, 86, 0.08), transparent 24%),linear-gradient(180deg, #f4f7f5 0%, #eef3f1 100%)}
.form-title,.login1-title{display:flex;flex-direction:row;align-items:center}
.form-title{margin:30px 0;color:var(--main-color-primary);font-size:20px}
.login1-title{margin:0 0 30px;color:#173433;font-size:38px}
.login1-main{display:flex;flex-direction:row;justify-content:center;background-color:#fff;border-radius:12px;border:1px solid rgba(32, 86, 86, 0.08);box-shadow:0 0 10px 0 rgb(32 86 86 / 20%)}
.login1-left-img{border-radius:12px 0 0 12px;height:400px}
.login1-form{padding:20px 40px;display:flex;flex-direction:column;align-items:center;}
.login1-form .input-group{width:300px}
.login1-form .form-control{border-color:rgba(32, 86, 86, 0.14)}
.login1-form .form-control:focus{border-color:var(--main-color-primary);box-shadow:0 0 0 .2rem rgba(32, 86, 86, 0.12)}
.login1-form .input-group-text{background:#fbfcfc;border-color:rgba(32, 86, 86, 0.14);color:#243f3f}
.login1-form .btn-primary{width:300px;margin-top:40px;height:36px;background:linear-gradient(135deg,#205656 0%,#163b3a 100%);border-color:var(--main-color-primary)}
.login1-form .btn-primary:hover{background:linear-gradient(135deg,#2d6967 0%,#0f2f2d 100%);border-color:var(--main-color-primary)}
.login1-form .btn-success{background:linear-gradient(135deg,#205656 0%,#163b3a 100%);border-color:var(--main-color-primary)}
.login1-form .btn-success:hover{background:linear-gradient(135deg,#2d6967 0%,#0f2f2d 100%);border-color:var(--main-color-primary)}
.login1-form .btn-link{color:var(--main-color-primary)}
.login1-register{position:fixed;bottom:8px;left:8px;color:#5a6c6b;font-size:14px}
""");
		__builder.CloseElement();
		__builder.AddMarkupContent(9, "\r\n\r\n");
		__builder.OpenElement(10, "div");
		__builder.AddAttribute(11, "style", "height: 100%; width: 100%;");
		__builder.OpenElement(12, "div");
		__builder.AddAttribute(13, "class", "login1");
		__builder.OpenElement(14, "div");
		__builder.AddAttribute(15, "class", "login1-main");
		__builder.OpenElement(16, "img");
		__builder.AddAttribute(17, "src", admin.Tenant?.LoginImage ?? "_content/AntDesign.Blazor/login/login-left.jpeg");
		__builder.AddAttribute(18, "class", "login1-left-img");
		__builder.CloseElement();
		__builder.AddMarkupContent(19, "\r\n            ");
		__builder.OpenElement(20, "div");
		__builder.AddAttribute(21, "class", "login1-form");
		if (currentStep == LoginStep.Login)
		{
			__builder.AddMarkupContent(22, "<div class=\"form-title\"><span>欢迎登录</span></div>\r\n                    ");
			__builder.OpenElement(23, "div");
			__builder.AddAttribute(24, "class", "input-group mb-3");
			__builder.OpenElement(25, "div");
			__builder.AddAttribute(26, "class", "form-floating");
			__builder.OpenElement(27, "input");
			__builder.AddAttribute(28, "onkeydown", "if(event.code=='Enter'&&this.value.length>0)$('#loginPassword')[0].focus()");
			__builder.AddAttribute(29, "id", "loginUser");
			__builder.AddAttribute(30, "type", "text");
			__builder.AddAttribute(31, "class", "form-control");
			__builder.AddAttribute(32, "placeholder", "");
			__builder.AddAttribute(33, "value", BindConverter.FormatValue(username));
			__builder.AddAttribute(34, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				username = __value;
			}, username));
			__builder.SetUpdatesAttributeName("value");
			__builder.CloseElement();
			__builder.AddMarkupContent(35, "<label for=\"loginUser\" style=\"font-weight:300\">用户名</label>");
			__builder.CloseElement();
			__builder.AddMarkupContent(36, "\r\n                        ");
			__builder.AddMarkupContent(37, "<div class=\"input-group-text\"><span class=\"fa fa-user\"></span></div>");
			__builder.CloseElement();
			__builder.AddMarkupContent(38, "\r\n                    ");
			__builder.OpenElement(39, "div");
			__builder.AddAttribute(40, "class", "input-group mb-3");
			__builder.OpenElement(41, "div");
			__builder.AddAttribute(42, "class", "form-floating");
			__builder.OpenElement(43, "input");
			__builder.AddAttribute(44, "onkeydown", "if(event.code=='Enter'&&this.value.length>0)$('#loginSubmit').click()");
			__builder.AddAttribute(45, "id", "loginPassword");
			__builder.AddAttribute(46, "type", "password");
			__builder.AddAttribute(47, "class", "form-control");
			__builder.AddAttribute(48, "placeholder", "");
			__builder.AddAttribute(49, "value", BindConverter.FormatValue(password));
			__builder.AddAttribute(50, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				password = __value;
			}, password));
			__builder.SetUpdatesAttributeName("value");
			__builder.CloseElement();
			__builder.AddMarkupContent(51, "<label for=\"loginPassword\" style=\"font-weight:300\">密码</label>");
			__builder.CloseElement();
			__builder.AddMarkupContent(52, "\r\n                        ");
			__builder.AddMarkupContent(53, "<div class=\"input-group-text\"><span class=\"fas fa-lock\"></span></div>");
			__builder.CloseElement();
			__builder.AddMarkupContent(54, "\r\n                    ");
			__builder.OpenElement(55, "div");
			__builder.OpenElement(56, "div");
			__builder.OpenElement(57, "button");
			__builder.AddAttribute(58, "type", "button");
			__builder.AddAttribute(59, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)Submit));
			__builder.AddAttribute(60, "id", "loginSubmit");
			__builder.AddAttribute(61, "class", "btn btn-primary");
			__builder.AddAttribute(62, "style", "width: 300px; margin-top: 40px; height: 36px;");
			__builder.AddContent(63, submitText);
			__builder.CloseElement();
			__builder.CloseElement();
			__builder.CloseElement();
		}
		else if (currentStep == LoginStep.Bind2FA)
		{
			__builder.AddMarkupContent(69, "<div class=\"form-title\" style=\"color: #d9534f; margin: 10px 0 10px 0; font-size: 18px;\"><span>使用 Google Authenticator 扫码</span></div>\r\n                    ");
			__builder.OpenComponent<QRCode>(70);
			__builder.AddComponentParameter(71, "Content", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(qrCodeUrl));
			__builder.AddComponentParameter(72, "Width", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(150));
			__builder.CloseComponent();
			__builder.AddMarkupContent(73, "\r\n                    ");
			__builder.OpenElement(74, "div");
			__builder.AddAttribute(75, "class", "input-group mb-2");
			__builder.AddAttribute(76, "style", "flex-shrink: 0;margin-top:18px");
			__builder.OpenElement(77, "div");
			__builder.AddAttribute(78, "class", "form-floating");
			__builder.OpenElement(79, "input");
			__builder.AddAttribute(80, "type", "text");
			__builder.AddAttribute(81, "class", "form-control");
			__builder.AddAttribute(82, "placeholder", "输入6位验证码");
			__builder.AddAttribute(83, "maxlength", "6");
			__builder.AddAttribute(84, "style", "text-align: center; font-size: 16px; font-weight: bold;");
			__builder.AddAttribute(85, "value", BindConverter.FormatValue(inputCode2FA));
			__builder.AddAttribute(86, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				inputCode2FA = __value;
			}, inputCode2FA));
			__builder.SetUpdatesAttributeName("value");
			__builder.CloseElement();
			__builder.AddMarkupContent(87, "\r\n                            ");
			__builder.AddMarkupContent(88, "<label style=\"font-weight:300; font-size:14px;\">输入 6 位验证码</label>");
			__builder.CloseElement();
			__builder.AddMarkupContent(89, "\r\n                        ");
			__builder.AddMarkupContent(90, "<div class=\"input-group-text\"><span class=\"fas fa-key\"></span></div>");
			__builder.CloseElement();
			__builder.AddMarkupContent(91, "\r\n                    ");
			__builder.OpenElement(92, "button");
			__builder.AddAttribute(93, "type", "button");
			__builder.AddAttribute(94, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)Submit2FA));
			__builder.AddAttribute(95, "class", "btn btn-success");
			__builder.AddAttribute(96, "style", "width: 300px; margin-top: 5px; font-size: 16px; height: 32px; flex-shrink: 0;");
			__builder.AddMarkupContent(97, "确认绑定并登录");
			__builder.CloseElement();
			__builder.AddMarkupContent(98, "\r\n                    ");
			__builder.OpenElement(99, "button");
			__builder.AddAttribute(100, "type", "button");
			__builder.AddAttribute(101, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
			{
				currentStep = LoginStep.Login;
			}));
			__builder.AddAttribute(102, "class", "btn btn-link");
			__builder.AddAttribute(103, "style", "margin-top: 12px; font-size: 12px; padding: 0; flex-shrink: 0;");
			__builder.AddMarkupContent(104, "取消并返回登录");
			__builder.CloseElement();
		}
		else if (currentStep == LoginStep.Verify2FA)
		{
			__builder.AddMarkupContent(105, "<div class=\"form-title\" style=\"color: #0b49d8; margin: 30px 0;\"><span>谷歌验证码</span></div>\r\n                    ");
			__builder.AddMarkupContent(106, "<div style=\"text-align: center; margin-bottom: 30px; color: #666; width: 300px;\"><i class=\"fas fa-lock\" style=\"font-size: 24px; color: #0b49d8;\"></i>\r\n                        <div style=\"margin-top: 15px;\">\r\n                            您正在使用非信任网络登录。\r\n                        </div></div>\r\n                    ");
			__builder.OpenElement(107, "div");
			__builder.AddAttribute(108, "class", "input-group mb-3");
			__builder.AddAttribute(109, "style", "width: 300px;");
			__builder.OpenElement(110, "div");
			__builder.AddAttribute(111, "class", "form-floating");
			__builder.OpenElement(112, "input");
			__builder.AddAttribute(113, "onkeydown", "if(event.code=='Enter'&&this.value.length==6)$('#btnVerify2fa').click()");
			__builder.AddAttribute(114, "id", "code2fa");
			__builder.AddAttribute(115, "type", "text");
			__builder.AddAttribute(116, "class", "form-control");
			__builder.AddAttribute(117, "placeholder", "输入6位验证码");
			__builder.AddAttribute(118, "maxlength", "6");
			__builder.AddAttribute(119, "style", "text-align: center; font-size: 20px; font-weight: bold;");
			__builder.AddAttribute(120, "value", BindConverter.FormatValue(inputCode2FA));
			__builder.AddAttribute(121, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
			{
				inputCode2FA = __value;
			}, inputCode2FA));
			__builder.SetUpdatesAttributeName("value");
			__builder.CloseElement();
			__builder.AddMarkupContent(122, "\r\n                            ");
			__builder.AddMarkupContent(123, "<label for=\"code2fa\" style=\"font-weight:300\">输入6位验证码</label>");
			__builder.CloseElement();
			__builder.AddMarkupContent(124, "\r\n                        ");
			__builder.AddMarkupContent(125, "<div class=\"input-group-text\"><span class=\"fas fa-shield-alt\"></span></div>");
			__builder.CloseElement();
			__builder.AddMarkupContent(126, "\r\n                    ");
			__builder.OpenElement(127, "button");
			__builder.AddAttribute(128, "id", "btnVerify2fa");
			__builder.AddAttribute(129, "type", "button");
			__builder.AddAttribute(130, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)Submit2FA));
			__builder.AddAttribute(131, "class", "btn btn-primary");
			__builder.AddAttribute(132, "style", "width: 300px; margin-top: 20px; font-size: 20px; height: 36px;");
			__builder.AddMarkupContent(133, "验证登录");
			__builder.CloseElement();
			__builder.AddMarkupContent(134, "\r\n                    ");
			__builder.OpenElement(135, "button");
			__builder.AddAttribute(136, "type", "button");
			__builder.AddAttribute(137, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Action)delegate
			{
				currentStep = LoginStep.Login;
			}));
			__builder.AddAttribute(138, "class", "btn btn-link");
			__builder.AddAttribute(139, "style", "margin-top:10px;");
			__builder.AddMarkupContent(140, "返回账号/密码输入");
			__builder.CloseElement();
		}
		__builder.CloseElement();
		__builder.CloseElement();
		__builder.AddMarkupContent(141, "\r\n        ");
		__builder.OpenElement(142, "div");
		__builder.AddAttribute(143, "class", "login1-register");
		__builder.OpenElement(144, "div");
			if (NovaAdminExtensions.Options.IsDevelopment)
			{
				__builder.AddMarkupContent(147, "<a href=\"/api\" target=\"_blank\" style=\"color:#c5c5c5;\">API</a>");
			}
		__builder.CloseElement();
		__builder.CloseElement();
		__builder.CloseElement();
		__builder.CloseElement();
	}

	[AntiConcurrency(100)]
	private async Task Submit()
	{
		await _0024Rougamo_Submit();
	}

	private async Task Submit2FA()
	{
		if (string.IsNullOrWhiteSpace(inputCode2FA) || inputCode2FA.Length != 6)
		{
			await MessageService.Error("请输入6位验证码");
			return;
		}
		string secretToValidate = ((currentStep == LoginStep.Bind2FA) ? tempSecretKey : tempUser.GoogleSecretKey);
		bool isValid = !string.IsNullOrEmpty(secretToValidate);
		if (isValid)
		{
			byte[] bytes = Base32Encoding.ToBytes(secretToValidate);
			Totp totp = new Totp(bytes, 30, (OtpHashMode)0, 6, (TimeCorrection)null);
			long timeStepMatched = default(long);
				isValid = totp.VerifyTotp(inputCode2FA, out timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay);
		}
		if (!isValid)
		{
			await MessageService.Error("验证码错误，请重试");
			return;
		}
		if (currentStep == LoginStep.Bind2FA)
		{
			tempUser.GoogleSecretKey = tempSecretKey;
			await admin.Orm.Update<SysUser>().SetSource(tempUser).UpdateColumns((Expression<Func<SysUser, object>>)((SysUser a) => a.GoogleSecretKey))
				.ExecuteAffrowsAsync(default(CancellationToken));
		}
		await PerformFinalLogin(tempUser);
	}

	private async Task PerformFinalLogin(SysUser user)
	{
		await admin.SignIn(user, remember);
		loginLog.Type = SysUserLoginLog.LogType.登陆成功;
		await admin.Orm.Insert<SysUserLoginLog>(loginLog).ExecuteAffrowsAsync(default(CancellationToken));
		string redirect = Nav.GetQueryStringValue("Redirect");
		if (redirect.IsNull())
		{
			redirect = "/Admin";
		}
		admin.Redirect(redirect);
	}

	protected override async Task OnInitializedAsync()
	{
		await admin.Init();
		if (NovaAdminExtensions.Options.IsDevelopment)
		{
			username ??= "admin";
			password ??= "admin";
		}
	}

	private async Task _0024Rougamo_Submit()
	{
		submitText = "验 证 中 ...";
		StateHasChanged();
		await Task.Delay(500);
		if (username.IsNull() || password.IsNull())
		{
			await MessageService.Error("用户名或密码不能为空");
			submitText = "安 全 登 录";
			StateHasChanged();
			return;
		}
		string ip = IpHelper.GetClientIpAddress(admin.HttpContext);
		if (limit.TryGetValue(ip, out var count) && count >= 5)
		{
			await MessageService.Error("频率过高！请过一会再试...");
			submitText = "安 全 登 录";
			StateHasChanged();
			return;
		}
		ClientInfo clientInfo = await webClientService.GetClientInfo();
		loginLog = new SysUserLoginLog
		{
			LoginTime = DateTime.Now,
			Username = username,
			Browser = clientInfo.Browser,
			City = clientInfo.City,
			Device = clientInfo.Device,
			Engine = clientInfo.Engine,
			Ip = ip,
			Language = clientInfo.Language,
			OS = clientInfo.OS,
			UserAgent = clientInfo.UserAgent
		};
		SysUser user = await ((ISelect0<ISelect<SysUser>, SysUser>)(object)admin.Orm.Select<SysUser>().IncludeMany<SysRole>((Expression<Func<SysUser, IEnumerable<SysRole>>>)((SysUser a) => a.Roles), (Action<ISelect<SysRole>>)null).Where((Expression<Func<SysUser, bool>>)((SysUser a) => a.Username == username))).FirstAsync(default(CancellationToken));
		if (user == null || user.Password != password)
		{
			limit.AddOrUpdate(ip, ++count, (string _, int __) => count);
			if (removeLimit.IsNull())
			{
				removeLimit = scheduler.AddTempTask(TimeSpan.FromSeconds(60.0), (Action)delegate
				{
					removeLimit = null;
					limit.TryRemove(ip, out var _);
				});
			}
			loginLog.Type = SysUserLoginLog.LogType.登陆失败;
			await admin.Orm.Insert<SysUserLoginLog>(loginLog).ExecuteAffrowsAsync(default(CancellationToken));
			await MessageService.Error("用户名或密码不正确");
			submitText = "安 全 登 录";
			StateHasChanged();
		}
		else if (user.Roles.All((SysRole a) => string.IsNullOrWhiteSpace(a.IpWhiteList)) || user.Roles.Any((SysRole a) => !string.IsNullOrWhiteSpace(a.IpWhiteList) && ("; " + a.IpWhiteList + ";").Contains("; " + ip + ";")))
		{
			await PerformFinalLogin(user);
		}
		else
		{
			tempUser = user;
			if (string.IsNullOrEmpty(user.GoogleSecretKey))
			{
				tempSecretKey = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
				qrCodeUrl = ((object)new OtpUri((OtpType)0, Base32Encoding.ToBytes(tempSecretKey), user.Username, admin.Tenant?.Title ?? "AdminSystem", (OtpHashMode)0, 6, 30, 0L)).ToString();
				currentStep = LoginStep.Bind2FA;
				submitText = "请 绑 定";
			}
			else
			{
				currentStep = LoginStep.Verify2FA;
				submitText = "验 证 码";
			}
			StateHasChanged();
		}
	}
}
