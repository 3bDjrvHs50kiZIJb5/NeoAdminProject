using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NoAdmin.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace NoAdmin.Blazor.Pages;

[Route("/Admin/UserProfile")]
public class UserProfile : ComponentBase
{
	private string _password;

	private string _newPassword;

	private string _newPassword2;

	[Inject]
	private IFreeSql fsql { get; set; }

	[Inject]
	private NovaAdminContext admin { get; set; }

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
			renderTreeBuilder.AddMarkupContent(2, "用户资料 - 修改密码");
		});
		__builder.CloseComponent();
		__builder.AddMarkupContent(3, "\r\n\r\n");
		__builder.OpenElement(4, "div");
		__builder.AddAttribute(5, "class", "container-fluid p-5");
		__builder.AddAttribute(6, "style", "width:450px;");
		__builder.AddMarkupContent(7, "<h4 class=\"mb-4 fw-normal\"><i class=\"fas fa-shield-alt me-2\"></i>安全设置\r\n    </h4>\r\n    ");
		__builder.OpenElement(8, "div");
		__builder.AddAttribute(9, "class", "row form-inline form-inline-end");
		__builder.OpenElement(10, "div");
		__builder.AddAttribute(11, "class", "form-group col-12 mb-2");
		__builder.AddMarkupContent(12, "<label for=\"oldPassword\" class=\"form-label\">旧密码</label>\r\n            ");
		__builder.OpenElement(13, "div");
		__builder.AddAttribute(14, "class", "input-group");
		__builder.AddMarkupContent(15, "<span class=\"input-group-text\"><i class=\"fas fa-key fa-fw\"></i></span>\r\n                ");
		__builder.OpenElement(16, "input");
		__builder.AddAttribute(17, "type", "password");
		__builder.AddAttribute(18, "class", "form-control");
		__builder.AddAttribute(19, "maxlength", "50");
		__builder.AddAttribute(20, "required");
		__builder.AddAttribute(21, "value", BindConverter.FormatValue(_password));
		__builder.AddAttribute(22, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
		{
			_password = __value;
		}, _password));
		__builder.SetUpdatesAttributeName("value");
		__builder.CloseElement();
		__builder.CloseElement();
		__builder.CloseElement();
		__builder.AddMarkupContent(23, "\r\n        ");
		__builder.OpenElement(24, "div");
		__builder.AddAttribute(25, "class", "form-group col-12 mb-2");
		__builder.AddMarkupContent(26, "<label for=\"newPassword\" class=\"form-label\">新密码</label>\r\n            ");
		__builder.OpenElement(27, "div");
		__builder.AddAttribute(28, "class", "input-group");
		__builder.AddMarkupContent(29, "<span class=\"input-group-text\"><i class=\"fas fa-lock fa-fw\"></i></span>\r\n                ");
		__builder.OpenElement(30, "input");
		__builder.AddAttribute(31, "type", "password");
		__builder.AddAttribute(32, "class", "form-control");
		__builder.AddAttribute(33, "minlength", "3");
		__builder.AddAttribute(34, "maxlength", "50");
		__builder.AddAttribute(35, "required");
		__builder.AddAttribute(36, "value", BindConverter.FormatValue(_newPassword));
		__builder.AddAttribute(37, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
		{
			_newPassword = __value;
		}, _newPassword));
		__builder.SetUpdatesAttributeName("value");
		__builder.CloseElement();
		__builder.CloseElement();
		__builder.CloseElement();
		__builder.AddMarkupContent(38, "\r\n        ");
		__builder.OpenElement(39, "div");
		__builder.AddAttribute(40, "class", "form-group col-12 mb-3");
		__builder.AddMarkupContent(41, "<label for=\"confirmPassword\" class=\"form-label\">确认新密码</label>\r\n            ");
		__builder.OpenElement(42, "div");
		__builder.AddAttribute(43, "class", "input-group");
		__builder.AddMarkupContent(44, "<span class=\"input-group-text\"><i class=\"fas fa-check-circle fa-fw\"></i></span>\r\n                ");
		__builder.OpenElement(45, "input");
		__builder.AddAttribute(46, "type", "password");
		__builder.AddAttribute(47, "class", "form-control");
		__builder.AddAttribute(48, "maxlength", "50");
		__builder.AddAttribute(49, "required");
		__builder.AddAttribute(50, "value", BindConverter.FormatValue(_newPassword2));
		__builder.AddAttribute(51, "onchange", EventCallback.Factory.CreateBinder(this, delegate(string? __value)
		{
			_newPassword2 = __value;
		}, _newPassword2));
		__builder.SetUpdatesAttributeName("value");
		__builder.CloseElement();
		__builder.CloseElement();
		__builder.CloseElement();
		__builder.AddMarkupContent(52, "\r\n        ");
		__builder.OpenElement(53, "div");
		__builder.AddAttribute(54, "class", "form-group col-12");
		__builder.AddMarkupContent(55, "<label class=\"form-label\"></label>\r\n            ");
		__builder.OpenComponent<Button>(56);
		__builder.AddComponentParameter(57, "class", "btn btn-primary");
		__builder.AddComponentParameter(58, "onclick", EventCallback.Factory.Create<MouseEventArgs>((object)this, (Func<Task>)ChangePassword));
		__builder.AddAttribute(59, "ChildContent", (RenderFragment)delegate(RenderTreeBuilder renderTreeBuilder)
		{
			renderTreeBuilder.AddMarkupContent(60, "修改密码");
		});
		__builder.CloseComponent();
		__builder.CloseElement();
		__builder.AddMarkupContent(61, "\r\n        ");
		__builder.AddMarkupContent(62, "<div class=\"form-group col-12\"><label class=\"form-label\"></label>\r\n            <div class=\"text-center text-muted\"><small>为了您的账户安全，请定期修改密码。</small></div></div>");
		__builder.CloseElement();
		__builder.CloseElement();
	}

	private async Task ChangePassword()
	{
		if (admin.User.Password != _password)
		{
			await MessageService.Show(new MessageOption
			{
				Color = (Color)5,
				Content = "旧密码错误"
			}, (Message)null);
			return;
		}
		if (_newPassword.Length < 5)
		{
			await MessageService.Show(new MessageOption
			{
				Color = (Color)5,
				Content = "新密码长度不能小于5"
			}, (Message)null);
			return;
		}
		if (_newPassword != _newPassword2)
		{
			await MessageService.Show(new MessageOption
			{
				Color = (Color)5,
				Content = "两次输入的新密码不一致"
			}, (Message)null);
			return;
		}
		admin.User.Password = _newPassword;
		await fsql.Update<SysUser>().Where((Expression<Func<SysUser, bool>>)((SysUser a) => a.Id == admin.User.Id)).Set<string>((Expression<Func<SysUser, string>>)((SysUser a) => a.Password), _newPassword)
			.ExecuteAffrowsAsync(default(CancellationToken));
		await MessageService.Show(new MessageOption
		{
			Color = (Color)4,
			Content = "密码修改成功，即将重新登陆！"
		}, (Message)null);
		await Task.Delay(2000);
		await admin.SignOut();
		admin.RedirectLogin();
	}
}
