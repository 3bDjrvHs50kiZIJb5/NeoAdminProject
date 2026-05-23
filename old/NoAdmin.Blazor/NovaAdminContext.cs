using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NoAdmin.Blazor.Infrastructure.Encrypt;
using NoAdmin.Blazor.Components;
using FreeSql;
using FreeSql.Aop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Yitter.IdGenerator;
using NoAdmin.Blazor.Utils;

public sealed class NovaAdminContext : IDisposable
{
	public class TabInfo
	{
		public string Key { get; set; }

		public string Title { get; set; }

		public string Url { get; set; }

		public bool IsActive { get; set; }

		[JsonIgnore]
		public int ComponentKey { get; set; }

		[JsonIgnore]
		public int Sort { get; set; }

		[JsonIgnore]
		public bool IsLoad { get; set; }

		[JsonIgnore]
		public bool IsClosed { get; set; }

		[JsonIgnore]
		public string Exception { get; set; }

		[JsonIgnore]
		public Type PageType { get; set; }

		[JsonIgnore]
		public SysMenu Menu { get; set; }

		[JsonIgnore]
		public List<SysMenu> AuditButtons { get; set; }

		[JsonIgnore]
		public List<SysMenu> AuditAllButtons { get; set; }
	}

	private NavigationManager Nav;

	private IJSRuntime JS;

	private FreeSqlCloud Cloud;

	private readonly bool IsWebSocketRequest;

	private readonly string RequestPath;

	private static object BlazorNovaAdminContextsLock = new object();

	private static ConcurrentDictionary<long, ConcurrentDictionary<string, NovaAdminContext>> BlazorNovaAdminContexts = new ConcurrentDictionary<long, ConcurrentDictionary<string, NovaAdminContext>>();

	private ConcurrentDictionary<string, ConcurrentBag<Action<string, object>>> NotifyChangeds = new ConcurrentDictionary<string, ConcurrentBag<Action<string, object>>>();

	private bool RolesExpired = false;

	internal static ConcurrentDictionary<string, List<SysMenu>> _TenantMenusDict = new ConcurrentDictionary<string, List<SysMenu>>();

	private static string[] _sysButtonNames = new string[11]
	{
		"add", "edit", "remove", "audit_00", "audit_01", "audit_02", "audit_03", "audit_04", "audit_05", "audit_98",
		"audit_99"
	};

	private EventHandler<LocationChangedEventArgs> locationChangedEvent;

	private bool firstActiveTab = true;

	public static NovaAdminOptionsItem SharedOptions => NovaAdminExtensions.Options;

	public IServiceProvider Service { get; private set; }

	private ILogger<NovaAdminContext> Logger => Service?.GetService<ILogger<NovaAdminContext>>();

	public HttpContext HttpContext { get; }

	public ConcurrentDictionary<string, object> Bags { get; } = new ConcurrentDictionary<string, object>();

	public string RemoteIp { get; private set; }

	public string Fingerprint { get; private set; }

	public SysTenant Tenant { get; private set; }

	public IFreeSql Orm { get; private set; }

	public string CookieName => (Tenant == null || Tenant.Id == "main") ? (global::NovaAdminOptionsItem.Global_CookieName + "_login") : (global::NovaAdminOptionsItem.Global_CookieName + "_login_" + Tenant.Id);

	public string BlazorId { get; private set; }

	public ConcurrentBag<NovaAdminMessageInfo> Messages { get; } = new ConcurrentBag<NovaAdminMessageInfo>();

	public ConcurrentDictionary<long, NovaAdminLockResourceInfo> LockResources { get; } = new ConcurrentDictionary<long, NovaAdminLockResourceInfo>();

	public SysUser User { get; private set; }

	public List<SysRole> Roles { get; private set; } = new List<SysRole>();

	public List<SysMenu> RoleMenus { get; private set; } = new List<SysMenu>();

	public List<SysMenu> TenantMenus
	{
		get
		{
			List<SysMenu> value;
			return (Tenant != null && _TenantMenusDict.TryGetValue(Tenant.Id, out value)) ? value : null;
		}
	}

	internal CascadingValueSource<NovaAdminContext> CascadeSource { get; set; }

	internal List<TabInfo> Tabs { get; } = new List<TabInfo>(50);

	internal List<NovaModal> Modals { get; } = new List<NovaModal>(20);

	public NovaAdminContext(FreeSqlCloud cloud, IHttpContextAccessor httpContextAccessor, NavigationManager nav, IJSRuntime js)
	{
		Cloud = cloud;
		HttpContext = httpContextAccessor?.HttpContext;
		RequestPath = HttpContext?.Request?.Path.Value;
		IsWebSocketRequest = HttpContext?.WebSockets?.IsWebSocketRequest ?? false;
		Service = HttpContext?.RequestServices;
		Nav = nav;
		JS = js;
	}

	public async Task Init(bool isApi = false)
	{
		var request = HttpContext?.Request;
		Logger?.LogInformation("NovaAdminContext.Init 开始，isApi={IsApi}，HttpContextNull={HttpContextNull}，Path={Path}，Host={Host}，RemoteIp={RemoteIp}", isApi, HttpContext == null, request?.Path.Value, request?.Host.Value, IpHelper.GetClientIpAddress(HttpContext));
		RemoteIp = IpHelper.GetClientIpAddress(HttpContext);
		string fingerprint = ((!isApi) ? (await JSRuntimeExtensions.InvokeAsync<string>(JS, "novaAdminJS.getBrowserFingerprint", Array.Empty<object>())) : request?.Headers["Fingerprint"].FirstOrDefault());
		Fingerprint = fingerprint;
		Tenant = global::NovaAdminOptionsItem.Global_FixedTenant;
		if (Tenant == null)
		{
			string tenantHost = HttpContext?.Request?.Host.Host;
			if (!string.IsNullOrWhiteSpace(tenantHost))
			{
				tenantHost = tenantHost.ToLower();
				Tenant = await ((ISelect0<ISelect<SysTenant>, SysTenant>)(object)((FreeSqlCloud<string>)(object)Cloud).Use("main").Select<SysTenant>().Where((Expression<Func<SysTenant, bool>>)((SysTenant a) => (a.Host == tenantHost || a.Host2 == tenantHost || a.Host3 == tenantHost) && a.IsEnabled))).FirstAsync(default(CancellationToken));
			}
		}
		if (Tenant == null)
		{
			Orm = ((FreeSqlCloud<string>)(object)Cloud).Use("main");
		}
		else
		{
			Orm = GetTenantFreeSql(Tenant.Id);
		}
		string token = request?.Headers["Authorization"].FirstOrDefault() ?? request?.Query[CookieName].FirstOrDefault() ?? request?.Cookies[CookieName];
		long userId = 0L;
		if (!token.IsNull())
		{
			if (isApi)
			{
				if (token.StartsWith("Bearer "))
				{
					token = token.Substring(7);
				}
				if (NovaAdminExtensions.Options.TokenParse != null)
				{
					userId = await NovaAdminExtensions.Options.TokenParse(this, token);
					if (userId > 0)
					{
						User = await ((ISelect0<ISelect<SysUser>, SysUser>)(object)Orm.Select<SysUser>().Where((Expression<Func<SysUser, bool>>)((SysUser a) => a.Id == userId))).FirstAsync(default(CancellationToken));
					}
				}
				if (userId == 0L && TryParseCookie(token, out userId, out DateTime loginTime, out string fp) && userId > 0)
				{
					User = await ((ISelect0<ISelect<SysUser>, SysUser>)(object)Orm.Select<SysUser>().Where((Expression<Func<SysUser, bool>>)((SysUser a) => a.Id == userId))).FirstAsync(default(CancellationToken));
					if (User != null && ((NovaAdminExtensions.Options.TokenCheckLoginTime && loginTime > new DateTime(2000, 1, 1) && Math.Abs(User.LoginTime.Subtract(loginTime).TotalSeconds) > 1.0) || (NovaAdminExtensions.Options.TokenCheckApiFingerprint && Fingerprint != fp)))
					{
						User = null;
						return;
					}
				}
			}
			else
			{
				if (NovaAdminExtensions.Options.TokenParse != null)
				{
					userId = await NovaAdminExtensions.Options.TokenParse(this, token);
					if (userId > 0)
					{
						User = await ((ISelect0<ISelect<SysUser>, SysUser>)(object)Orm.Select<SysUser>().Where((Expression<Func<SysUser, bool>>)((SysUser a) => a.Id == userId))).FirstAsync(default(CancellationToken));
					}
				}
				if (userId == 0L && TryParseCookie(token, out userId, out DateTime loginTime2, out string fp2) && userId > 0)
				{
					User = await ((ISelect0<ISelect<SysUser>, SysUser>)(object)Orm.Select<SysUser>().Where((Expression<Func<SysUser, bool>>)((SysUser a) => a.Id == userId))).FirstAsync(default(CancellationToken));
					if (User != null && ((NovaAdminExtensions.Options.TokenCheckLoginTime && loginTime2 > new DateTime(2000, 1, 1) && Math.Abs(User.LoginTime.Subtract(loginTime2).TotalSeconds) > 1.0) || Fingerprint != fp2))
					{
						User = null;
						await SignOut();
						RedirectLogin();
						return;
					}
				}
				if (User == null)
				{
					await SignOut();
				}
				if (User != null && IsWebSocketRequest && RequestPath == "/_blazor")
				{
					BlazorId = HttpContext.Request.Query["id"];
					lock (BlazorNovaAdminContextsLock)
					{
						ConcurrentDictionary<string, NovaAdminContext> ctxs = BlazorNovaAdminContexts.GetOrAdd(User.Id, (long _) => new ConcurrentDictionary<string, NovaAdminContext>());
						ctxs.AddOrUpdate(BlazorId, this, (string k, NovaAdminContext v) => this);
					}
					if (Tenant != null)
					{
						RegisterNotifyChanged(Tenant.Id + "/" + Orm.CodeFirst.GetTableByEntity(typeof(SysUser)).DbName + "/OnEdit", delegate(string blazorId, object arg)
						{
							if (arg is SysUser sysUser && sysUser.Id == User.Id)
							{
								RolesExpired = true;
							}
						});
						RegisterNotifyChanged(Tenant.Id + "/" + Orm.CodeFirst.GetTableByEntity(typeof(SysRole)).DbName + "/OnEdit", delegate(string blazorId, object arg)
						{
							SysRole editItem = arg as SysRole;
							if (editItem != null && (editItem.IsAdministrator || !Roles.Any((SysRole role) => role.IsAdministrator)))
							{
								if (!Roles.Any((SysRole role) => role.Id == editItem.Id))
								{
									List<SysUser> users = editItem.Users;
									if (users == null || !users.Any((SysUser user) => user.Id == User.Id))
									{
										return;
									}
								}
								RolesExpired = true;
							}
						});
					}
				}
			}
		}
		if (Tenant != null && !_TenantMenusDict.ContainsKey(Tenant.Id))
		{
			List<SysMenu> menus = new List<SysMenu>();
			if (_TenantMenusDict.TryAdd(Tenant.Id, menus))
			{
				List<SysMenu> list = menus;
				list.AddRange(await ((ISelect0<ISelect<SysMenu>, SysMenu>)(object)Orm.Select<SysMenu>()).ToListAsync(default(CancellationToken)));
			}
		}
		if (User == null)
		{
			Logger?.LogInformation("NovaAdminContext.Init 结束，未识别到登录用户，Tenant={TenantId}，CookieName={CookieName}，IsApi={IsApi}", Tenant?.Id, CookieName, isApi);
			return;
		}
			List<long> userRoleIds = await Orm.Select<SysRoleUser>().Where((Expression<Func<SysRoleUser, bool>>)((SysRoleUser a) => a.UserId == User.Id)).ToListAsync<long>((Expression<Func<SysRoleUser, long>>)((SysRoleUser a) => a.RoleId), default(CancellationToken));
			Roles = await ((ISelect0<ISelect<SysRole>, SysRole>)(object)Orm.Select<SysRole>().Where((Expression<Func<SysRole, bool>>)((SysRole a) => userRoleIds.Contains(a.Id)))).ToListAsync(default(CancellationToken));
		if (Roles.Any((SysRole a) => a.IsAdministrator))
		{
			RoleMenus = TenantMenus;
			return;
		}
		IEnumerable<long> roleIds = Roles.Select((SysRole a) => a.Id);
		List<long> menuIds = await Orm.Select<SysRoleMenu>().Where((Expression<Func<SysRoleMenu, bool>>)((SysRoleMenu b) => roleIds.Contains(b.RoleId))).ToListAsync<long>((Expression<Func<SysRoleMenu, long>>)((SysRoleMenu a) => a.MenuId), default(CancellationToken));
		RoleMenus = TenantMenus.Where((SysMenu a) => menuIds.Contains(a.Id)).ToList();
		Logger?.LogInformation("NovaAdminContext.Init 结束，Tenant={TenantId}，User={UserId}，Roles={RoleCount}，Menus={MenuCount}，IsApi={IsApi}", Tenant?.Id, User?.Id, Roles?.Count ?? 0, RoleMenus?.Count ?? 0, isApi);
	}

	public void Dispose()
	{
		if (User != null && BlazorId != null)
		{
			lock (BlazorNovaAdminContextsLock)
			{
				ConcurrentDictionary<string, NovaAdminContext> orAdd = BlazorNovaAdminContexts.GetOrAdd(User.Id, (long _) => new ConcurrentDictionary<string, NovaAdminContext>());
				orAdd.TryRemove(BlazorId, out var _);
				if (!orAdd.Any())
				{
					BlazorNovaAdminContexts.TryRemove(User.Id, out ConcurrentDictionary<string, NovaAdminContext> _);
				}
			}
		}
		Bags.Clear();
		if (locationChangedEvent != null)
		{
			Nav.LocationChanged -= locationChangedEvent;
		}
		Messages.Clear();
		LockResources.Clear();
		NotifyChangeds.Clear();
	}

	public async Task SendMessage(long receiveUserId, string content)
	{
		DateTime now = DateTime.Now;
		ICollection<ConcurrentDictionary<string, NovaAdminContext>> ctxss = ((receiveUserId == 0L) ? BlazorNovaAdminContexts.Values : null);
		if (receiveUserId > 0 && BlazorNovaAdminContexts.TryGetValue(receiveUserId, out ConcurrentDictionary<string, NovaAdminContext> tmp))
		{
			ctxss = new List<ConcurrentDictionary<string, NovaAdminContext>> { tmp };
		}
		if (ctxss == null)
		{
			return;
		}
		foreach (ConcurrentDictionary<string, NovaAdminContext> ctxs in ctxss)
		{
			foreach (NovaAdminContext ctx in ctxs.Values)
			{
				ctx.Messages.Add(new NovaAdminMessageInfo
				{
					SendTime = now,
					SendUserId = (User?.Id ?? 0),
					SendUsername = User?.Username,
					Content = content
				});
				if (ctx.Messages.Count > 99)
				{
					ctx.Messages.TryTake(out NovaAdminMessageInfo _);
				}
				await ctx.CascadeSource.NotifyChangedAsync();
			}
		}
	}

	public async ValueTask LockResource(long id)
	{
		DateTime now = DateTime.Now;
		ICollection<ConcurrentDictionary<string, NovaAdminContext>> ctxss = BlazorNovaAdminContexts.Values;
		NovaAdminLockResourceInfo lockinfo = null;
		if (ctxss.Any((ConcurrentDictionary<string, NovaAdminContext> ctxs) => ctxs.Values.Any((NovaAdminContext ctx) => ctx.BlazorId != BlazorId && ctx.LockResources.TryGetValue(id, out lockinfo) && lockinfo.BlazorId != null)))
		{
			LockResources.TryAdd(id, new NovaAdminLockResourceInfo
			{
				LockTime = now,
				BlazorId = lockinfo.BlazorId,
				LockUserId = lockinfo.LockUserId,
				LockUsername = lockinfo.LockUsername
			});
		}
		else
		{
			LockResources.TryAdd(id, new NovaAdminLockResourceInfo
			{
				LockTime = now,
				BlazorId = BlazorId,
				LockUserId = (User?.Id ?? 0),
				LockUsername = User?.Username
			});
		}
		await Task.Yield();
	}

	public async ValueTask<bool> UnlockResource(long id)
	{
		ICollection<ConcurrentDictionary<string, NovaAdminContext>> ctxss = BlazorNovaAdminContexts.Values;
		if (LockResources.TryRemove(id, out NovaAdminLockResourceInfo lockinfo) && lockinfo.BlazorId == BlazorId)
		{
			foreach (ConcurrentDictionary<string, NovaAdminContext> ctxs in ctxss)
			{
				foreach (NovaAdminContext ctx in ctxs.Values)
				{
					if (ctx.BlazorId != BlazorId && ctx.LockResources.TryGetValue(id, out lockinfo))
					{
						lockinfo.BlazorId = null;
					}
				}
			}
			return true;
		}
		await Task.Yield();
		return false;
	}

	public void RegisterNotifyChanged(string key, Action<string, object> handler)
	{
		ConcurrentBag<Action<string, object>> orAdd = NotifyChangeds.GetOrAdd(key, (string t1) => new ConcurrentBag<Action<string, object>>());
		orAdd.Add(handler);
	}

	public void TriggerNotifyChanged(string key, object arg)
	{
		foreach (ConcurrentDictionary<string, NovaAdminContext> value2 in BlazorNovaAdminContexts.Values)
		{
			foreach (NovaAdminContext value3 in value2.Values)
			{
				if (!value3.NotifyChangeds.TryGetValue(key, out ConcurrentBag<Action<string, object>> value))
				{
					continue;
				}
				foreach (Action<string, object> item in value)
				{
					item(BlazorId, arg);
				}
			}
		}
	}

	public async Task SignIn(SysUser user, bool remember)
	{
		user.LoginTime = DateTime.Now;
		await Orm.Update<SysUser>().Where((Expression<Func<SysUser, bool>>)((SysUser a) => a.Id == user.Id)).Set<DateTime>((Expression<Func<SysUser, DateTime>>)((SysUser a) => a.LoginTime), user.LoginTime)
			.ExecuteAffrowsAsync(default(CancellationToken));
		string token = DesEncrypt.Encrypt(user.Id + "|" + user.LoginTime.ToString("yyyy-MM-dd HH:mm:ss") + "|" + Fingerprint);
		if (IsWebSocketRequest)
		{
			await JS.InvokeVoidAsync("novaAdminJS.setCookie", CookieName, token, remember ? 15 : (-1));
		}
		else
		{
			HttpContext.Response.Cookies.Append(CookieName, token, new CookieOptions
			{
				Path = "/",
				Expires = (remember ? new DateTimeOffset?(DateTimeOffset.UtcNow.AddDays(15.0)) : ((DateTimeOffset?)null))
			});
		}
		await Task.Yield();
	}

	public async Task SignOut()
	{
		if (IsWebSocketRequest)
		{
			await JS.InvokeVoidAsync("novaAdminJS.setCookie", CookieName, "");
		}
		else
		{
			HttpContext.Response.Cookies.Delete(CookieName);
		}
		await Task.Yield();
	}

	private static bool TryParseCookie(string cookie, out long userId, out DateTime loginTime, out string fingerprint)
	{
		try
		{
			if (!cookie.IsNull())
			{
				string[] array = DesEncrypt.Decrypt(cookie).Split('|');
				if (array.Length >= 3)
				{
					userId = array[0].ConvertTo<long>();
					loginTime = array[1].ConvertTo<DateTime>();
					fingerprint = array[2].ConvertTo<string>();
					return true;
				}
			}
		}
		catch
		{
		}
		userId = 0L;
		loginTime = DateTime.MinValue;
		fingerprint = null;
		return false;
	}

	public void Redirect(string url)
	{
		if (IsWebSocketRequest)
		{
			Nav.NavigateTo(url, forceLoad: true);
		}
		else
		{
			HttpContext.Response.Redirect(url);
		}
	}

	public void RedirectLogin()
	{
		if (IsWebSocketRequest)
		{
			Nav.NavigateTo("/Login?Redirect=" + new Uri(Nav.Uri).PathAndQuery.UrlEncode(), forceLoad: true);
		}
		else
		{
			HttpContext.Response.Redirect("/Login?Redirect=" + HttpContext.Request.GetEncodedPathAndQuery().UrlEncode());
		}
	}

	public bool AuthPath(string path)
	{
		if (path != "/")
		{
			path = path?.ToLower().Trim('/');
		}
		if (new string[1] { "login" }.Contains(path))
		{
			return true;
		}
		if (User == null)
		{
			return false;
		}
		if (RolesExpired)
		{
				List<long> userRoleIds = Orm.Select<SysRoleUser>().Where((Expression<Func<SysRoleUser, bool>>)((SysRoleUser a) => a.UserId == User.Id)).ToList<long>((Expression<Func<SysRoleUser, long>>)((SysRoleUser a) => a.RoleId));
				Roles = ((ISelect0<ISelect<SysRole>, SysRole>)(object)Orm.Select<SysRole>().Where((Expression<Func<SysRole, bool>>)((SysRole a) => userRoleIds.Contains(a.Id)))).ToList();
			if (Roles.Any((SysRole a) => a.IsAdministrator))
			{
				RoleMenus = TenantMenus;
			}
			else
			{
				IEnumerable<long> roleIds = Roles.Select((SysRole a) => a.Id);
				List<long> menuIds = Orm.Select<SysRoleMenu>().Where((Expression<Func<SysRoleMenu, bool>>)((SysRoleMenu b) => roleIds.Contains(b.RoleId))).ToList<long>((Expression<Func<SysRoleMenu, long>>)((SysRoleMenu a) => a.MenuId));
				RoleMenus = TenantMenus.Where((SysMenu a) => menuIds.Contains(a.Id)).ToList();
			}
			RolesExpired = false;
		}
		if (!Roles.Any())
		{
			return false;
		}
		SysMenu menu = RoleMenus.Where((SysMenu a) => string.Compare(a.Path, path, ignoreCase: true) == 0).FirstOrDefault();
		if (menu != null)
		{
			menu.Parent = RoleMenus.Where((SysMenu a) => a.Id == menu.ParentId)?.FirstOrDefault();
		}
		return menu != null;
	}

	public bool AuthButton(SysMenu menu, string name)
	{
		if (User == null)
		{
			return false;
		}
		if (menu == null)
		{
			return false;
		}
		SysMenu button = null;
		FindButton(new List<SysMenu> { menu });
		if (button == null)
		{
			if (Tenant.Id == "main" && !_sysButtonNames.Contains(name))
			{
				FindButtonFromDB(new List<SysMenu> { menu });
				if (button == null)
				{
					Orm.Insert<SysMenu>(new SysMenu
					{
						ParentId = menu.Id,
						Label = name,
						Path = name,
						Sort = 10031,
						Type = SysMenuType.按钮
					}).ExecuteAffrows();
					List<SysMenu> menus = ((ISelect0<ISelect<SysMenu>, SysMenu>)(object)Orm.Select<SysMenu>()).ToList();
					_TenantMenusDict.AddOrUpdate(Tenant.Id, (string k1) => menus, (string k1, List<SysMenu> k2) => menus);
				}
				if (Roles.Any((SysRole a) => a.IsAdministrator))
				{
					return true;
				}
			}
			return false;
		}
		return true;
		void FindButton(List<SysMenu> findMenus)
		{
			long[] array = findMenus.Select((SysMenu a) => a.Id).ToArray();
			List<SysMenu> list = RoleMenus.Where((SysMenu a) => findMenus.Select((SysMenu sysMenu) => sysMenu.Id).Contains(a.ParentId)).ToList();
			if (list.Any())
			{
				button = list.Where((SysMenu a) => a.Type == SysMenuType.按钮 && string.Compare(a.Path, name, ignoreCase: true) == 0).FirstOrDefault();
				if (button == null)
				{
					FindButton(list);
				}
			}
		}
		void FindButtonFromDB(List<SysMenu> findMenus)
		{
			long[] parentIds = findMenus.Select((SysMenu a) => a.Id).ToArray();
			List<SysMenu> list = TenantMenus.Where((SysMenu a) => parentIds.Contains(a.ParentId)).ToList();
			if (list.Any())
			{
				button = list.Where((SysMenu a) => a.Type == SysMenuType.按钮 && string.Compare(a.Path, name, ignoreCase: true) == 0).FirstOrDefault();
				if (button == null)
				{
					FindButtonFromDB(list);
				}
			}
		}
	}

	internal async Task<List<SysMenu>> GenerateTenantMenus(string tenantId, bool isAdministrator = false)
	{
		IFreeSql main = ((FreeSqlCloud<string>)(object)Cloud).Use("main");
		List<long> list = ((!isAdministrator) ? (await main.Select<SysTenantMenu>().Where((Expression<Func<SysTenantMenu, bool>>)((SysTenantMenu sysTenantMenu) => sysTenantMenu.TenantId == tenantId)).ToListAsync<long>((Expression<Func<SysTenantMenu, long>>)((SysTenantMenu sysTenantMenu) => sysTenantMenu.MenuId), default(CancellationToken))) : null);
		List<long> tenantMenuIds = list;
		List<NovaAdminItem<SysMenu>> allMenus = (await ((ISelect0<ISelect<SysMenu>, SysMenu>)(object)main.Select<SysMenu>()).ToListAsync(default(CancellationToken))).ToNovaAdminItemList(main);
		List<SysMenu> menus = new List<SysMenu>();
		string[] canSyncMenus = new string[4] { "admin/org", "admin/userprofile", "admin/user", "admin/role" };
		for (int a = 0; a < allMenus.Count; a++)
		{
			if (allMenus[a].Level == 1 && allMenus[a].Value.Label == "系统管理")
			{
				menus.Add(allMenus[a].Value);
				for (a++; a < allMenus.Count && allMenus[a].Level > 1; a++)
				{
					if (!canSyncMenus.Contains<string>(allMenus[a].Value.Path, StringComparer.OrdinalIgnoreCase))
					{
						continue;
					}
					if (isAdministrator || tenantMenuIds.Contains(allMenus[a].Value.Id))
					{
						menus.Add(allMenus[a].Value);
					}
					int level = allMenus[a].Level;
					for (a++; a < allMenus.Count && allMenus[a].Level > level; a++)
					{
						if (isAdministrator || tenantMenuIds.Contains(allMenus[a].Value.Id))
						{
							menus.Add(allMenus[a].Value);
						}
					}
					a--;
				}
				a--;
			}
			else if (isAdministrator || tenantMenuIds.Contains(allMenus[a].Value.Id))
			{
				menus.Add(allMenus[a].Value);
			}
		}
		return menus;
	}

	public IFreeSql GetTenantFreeSql(string tenantId)
	{
		((FreeSqlCloud<string>)(object)Cloud).Register(tenantId, (Func<IFreeSql>)delegate
		{
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			SysTenantDatabase sysTenantDatabase = ((FreeSqlCloud<string>)(object)Cloud).Use("main").Select<SysTenant>().Where((Expression<Func<SysTenant, bool>>)((SysTenant a) => a.Id == tenantId))
				.First<SysTenantDatabase>((Expression<Func<SysTenant, SysTenantDatabase>>)((SysTenant a) => a.Database));
			if (sysTenantDatabase == null)
			{
				throw new Exception("租户数据库错误");
			}
			IFreeSql val = new FreeSqlBuilder().UseConnectionString(sysTenantDatabase.DataType, sysTenantDatabase.ConenctionString.Replace("{database}", tenantId), (Type)null).UseAdoConnectionPool(true).UseNoneCommandParameter(true)
				.UseAutoSyncStructure(true)
				.Build();
			ConfigFreeSql(val);
			return val;
		}, (TimeSpan?)null);
		return ((FreeSqlCloud<string>)(object)Cloud).Use(tenantId);
	}

	internal static void ConfigFreeSql(IFreeSql fsql)
	{
		fsql.Aop.ConfigEntityProperty += delegate(object? s, ConfigEntityPropertyEventArgs e)
		{
			if (e.Property.PropertyType.IsEnum)
			{
				e.ModifyResult.MapType = typeof(int);
			}
			else if (FreeSqlGlobalExtensions.NullableTypeOrThis(e.Property.PropertyType).IsEnum)
			{
				e.ModifyResult.MapType = typeof(int?);
			}
		};
		DateTime value = FreeSqlGlobalExtensions.QuerySingle<DateTime>(fsql.Ado, (Expression<Func<DateTime>>)(() => DateTime.UtcNow));
		TimeSpan timeOffset = DateTime.UtcNow.Subtract(value);
		fsql.Aop.AuditValue += delegate(object? _, AuditValueEventArgs e)
		{
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			if (e.Column.Table.Type == typeof(SysTenant) && e.Column.CsName == "Id")
			{
				e.Value = e.Column.Table.ColumnsByCs["Id"].GetValue(e.Object).ConvertTo<string>()?.ToLower();
			}
			else if ((e.Column.CsType == typeof(DateTime) || e.Column.CsType == typeof(DateTime?)) && e.Column.Attribute.ServerTime != DateTimeKind.Unspecified)
			{
				if (((int)e.AuditValueType == 0 && e.Column.Attribute.CanUpdate) || e.Value == null || (DateTime)e.Value == default(DateTime) || !((DateTime?)e.Value).HasValue)
				{
					e.Value = ((e.Column.Attribute.ServerTime == DateTimeKind.Utc) ? DateTime.UtcNow : DateTime.Now).Subtract(timeOffset);
				}
			}
			else if (e.Column.CsType == typeof(long) && e.Property.GetCustomAttribute<SnowflakeAttribute>(inherit: false) != null && (e.Value == null || (long)e.Value == 0L || !((long?)e.Value).HasValue))
			{
				e.Value = YitIdHelper.NextId();
			}
			else if (e.Column.CsType == typeof(Guid) && e.Property.GetCustomAttribute<UuidV7Attribute>(inherit: false) != null && (e.Value == null || (Guid)e.Value == default(Guid) || !((Guid?)e.Value).HasValue))
			{
				e.Value = GuidGenerator.CreateVersion7();
			}
		};
		if (!fsql.DbFirst.ExistsTable(fsql.CodeFirst.GetTableByEntity(typeof(SysAuditLog)).DbName, true))
		{
			fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysAuditLog) });
		}
		if (!fsql.DbFirst.ExistsTable(fsql.CodeFirst.GetTableByEntity(typeof(SysAuditEntityLog)).DbName, true))
		{
			fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysAuditEntityLog) });
		}
	}

	public async Task InitTabRoute()
	{
		string openedTabJson = await JSRuntimeExtensions.InvokeAsync<string>(JS, "novaAdminJS.dockViewInit", new object[2]
		{
			DotNetObjectReference.Create(this),
			Tenant?.Title
		});
		if (string.IsNullOrWhiteSpace(openedTabJson))
		{
			openedTabJson = "[]";
		}
		TabInfo[] newTabs = JsonConvert.DeserializeObject<TabInfo[]>(openedTabJson);
		IEnumerable<TabInfo> uniqueNewTabs = from tab in newTabs
			group tab by tab.Key into @group
			select @group.First();
		Tabs.AddRange(uniqueNewTabs);
		int sort = 0;
		Tabs.ForEach(delegate(TabInfo a)
		{
			a.Sort = sort++;
		});
		if (!Tabs.Any())
		{
			await InitTabRouteAfter();
		}
	}

	[JSInvokable]
	public async Task InitTabRouteAfter()
	{
		if (locationChangedEvent == null)
		{
			locationChangedEvent = async delegate(object? s, LocationChangedEventArgs e)
			{
				await LocationChangedHandler(e.Location);
			};
			await LocationChangedHandler(Nav.Uri);
			Nav.LocationChanged += locationChangedEvent;
		}
	}

	private async ValueTask LocationChangedHandler(string location)
	{
		Uri uri = Nav.ToAbsoluteUri(location);
		if (uri.AbsolutePath.ToLower() == "/login")
		{
			return;
		}
		TabInfo newtab = Tabs.FirstOrDefault((TabInfo a) => Nav.ToAbsoluteUri(a.Url).AbsolutePath == uri.AbsolutePath);
		SysMenu findMenu = null;
		string oldUrl = "";
		List<SysMenu> tenantMenus = TenantMenus;
		if (newtab == null)
		{
			findMenu = tenantMenus.Find((SysMenu a) => !string.IsNullOrWhiteSpace(a.Path) && Nav.ToAbsoluteUri(a.Path).AbsolutePath == uri.AbsolutePath);
			if (findMenu != null)
			{
				newtab = Tabs.FirstOrDefault((TabInfo a) => a.Key == findMenu.Id.ToString());
				if (newtab == null)
				{
					List<TabInfo> tabs = Tabs;
					TabInfo obj = new TabInfo
					{
						Key = findMenu.Id.ToString(),
						Title = findMenu.Label,
						Url = uri.PathAndQuery,
						Menu = findMenu,
						Sort = Tabs.Count
					};
					TabInfo item = obj;
					newtab = obj;
					tabs.Add(item);
				}
				else
				{
					oldUrl = newtab.Url;
					newtab.Title = findMenu.Label;
					newtab.Url = uri.PathAndQuery;
					newtab.Menu = findMenu;
				}
			}
		}
		else
		{
			if (newtab.Menu == null)
			{
				newtab.Menu = tenantMenus.Find((SysMenu a) => !string.IsNullOrWhiteSpace(a.Path) && Nav.ToAbsoluteUri(a.Path).AbsolutePath == uri.AbsolutePath);
			}
			oldUrl = newtab.Url;
			newtab.Url = uri.PathAndQuery;
		}
		if (newtab != null)
		{
			TabInfo oldtab = Tabs.FirstOrDefault((TabInfo a) => a.IsActive);
			if (oldtab != null)
			{
				if (oldtab.Key != newtab.Key && oldUrl != newtab.Url)
				{
					newtab.ComponentKey++;
				}
				if (oldtab.Key == newtab.Key && newtab.PageType != null)
				{
					return;
				}
				if (oldtab.Key != newtab.Key)
				{
					oldtab.IsActive = false;
				}
			}
			if (findMenu == null)
			{
				findMenu = tenantMenus.Find((SysMenu a) => a.Id.ToString() == newtab.Key);
			}
			string title = findMenu?.Label;
			while (true)
			{
				SysMenu sysMenu = findMenu;
				if (sysMenu == null || sysMenu.ParentId <= 0)
				{
					break;
				}
				findMenu = RoleMenus.Find((SysMenu a) => a.Id == findMenu.ParentId);
				title = title + " - " + findMenu?.Label;
			}
			title = ((!string.IsNullOrWhiteSpace(title)) ? (title + " - " + Tenant?.Title) : Tenant?.Title);
			await JS.InvokeVoidAsync("eval", "\r\nvar title = '" + title.Replace("'", "\\'").Replace("\n", "") + "';\r\nvar setTitle = null;\r\nsetTitle = function(tryTimes) {\r\n    if (tryTimes > 5) return;\r\n    document.title = title;\r\n    setTimeout(function() { setTitle(tryTimes + 1); }, 100);\r\n};\r\nsetTitle(1);");
		}
		if (!AuthPath(uri.AbsolutePath))
		{
			if (User == null)
			{
				RedirectLogin();
				return;
			}
			if (newtab != null)
			{
				List<SysRole> roles = Roles;
				if (roles != null && !roles.Any())
				{
					newtab.Exception = "没有访问权限: 未分配角色.";
				}
				else
				{
					newtab.Exception = "没有访问权限.";
				}
				await CascadeSource.NotifyChangedAsync();
			}
		}
		else if (newtab != null)
		{
			if (newtab.PageType == null)
			{
				newtab.PageType = SharedOptions.Assemblies.Select((Assembly a) => a.GetTypes().FirstOrDefault((Type b) => typeof(ComponentBase).IsAssignableFrom(b) && b.GetCustomAttribute<RouteAttribute>()?.Template == uri.AbsolutePath)).FirstOrDefault((Type a) => a != null);
			}
			newtab.IsLoad = true;
			newtab.IsActive = true;
			newtab.Exception = null;
			await CascadeSource.NotifyChangedAsync();
		}
		await TryInvokeVoidAsync("novaAdminJS.dockViewOpenTab", JsonConvert.SerializeObject((object)(from a in Tabs
			where !a.IsClosed
			orderby a.Sort
			select a)), newtab?.Key, newtab?.Title, true);
	}

	[JSInvokable]
	public async Task ActiveTab(string key)
	{
		await InitTabRouteAfter();
		TabInfo tab = Tabs.FirstOrDefault((TabInfo a) => a.Key == key);
		if (firstActiveTab)
		{
			firstActiveTab = false;
			Uri startUri = new Uri(Nav.Uri);
			if (startUri.AbsolutePath != "/")
			{
				SysMenu startTab = RoleMenus.Find((SysMenu a) => !string.IsNullOrWhiteSpace(a.Path) && Nav.ToAbsoluteUri(a.Path).AbsolutePath == startUri.AbsolutePath);
				if (tab != null && startTab != null && tab.Key != startTab.Id.ToString())
				{
					await LocationChangedHandler(Nav.Uri);
					return;
				}
			}
			if (tab == null)
			{
				Nav.NavigateTo("/");
			}
			else
			{
				Nav.NavigateTo(tab.Url);
			}
		}
		else if (tab == null)
		{
			Nav.NavigateTo("/");
		}
		else
		{
			if (tab.IsActive)
			{
				return;
			}
			if (tab.PageType == null)
			{
				Uri uri = Nav.ToAbsoluteUri(tab.Url);
				if (SharedOptions.Assemblies.Select((Assembly a) => a.GetTypes().FirstOrDefault((Type b) => typeof(ComponentBase).IsAssignableFrom(b) && b.GetCustomAttribute<RouteAttribute>()?.Template == uri.AbsolutePath)).FirstOrDefault((Type a) => a != null) == null)
				{
					await JS.InvokeVoidAsync("Swal.fire", new
					{
						position = "top",
						title = "404",
						text = tab.Url + " 页面未找到",
						icon = "error",
						showConfirmButton = true
					});
					TabInfo oldtab = Tabs.FirstOrDefault((TabInfo a) => a.IsActive);
					await TryInvokeVoidAsync("novaAdminJS.dockViewOpenTab", JsonConvert.SerializeObject((object)(from a in Tabs
						where !a.IsClosed
						orderby a.Sort
						select a)), oldtab?.Key, oldtab?.Title, true);
					return;
				}
			}
			Nav.NavigateTo(tab.Url);
		}
	}

	[JSInvokable]
	public async Task JsLoadTab(string key)
	{
		await InitTabRouteAfter();
		TabInfo tab = Tabs.FirstOrDefault((TabInfo a) => a.Key == key);
		if (tab == null)
		{
			await LocationChangedHandler("/");
		}
		else
		{
			await LocationChangedHandler(tab.Url);
		}
	}

	[JSInvokable]
	public async Task JsMoveTab(string[] keys)
	{
		int sort;
		for (sort = 0; sort < keys.Length; sort++)
		{
			TabInfo tab = Tabs.FirstOrDefault((TabInfo b) => b.Key == keys[sort]);
			if (tab != null)
			{
				tab.Sort = sort;
			}
		}
		await TryInvokeVoidAsync("novaAdminJS.dockViewOpenTab", JsonConvert.SerializeObject((object)(from a in Tabs
			where !a.IsClosed
			orderby a.Sort
			select a)));
	}

	[JSInvokable]
	public async Task JsCloseTab(string[] keys)
	{
		bool needSave = false;
		foreach (string key in keys)
		{
			int oldtabIndex = Tabs.FindIndex((TabInfo a) => a.Key == key);
			if (oldtabIndex != -1)
			{
				TabInfo oldtab = Tabs[oldtabIndex];
				oldtab.PageType = null;
				oldtab.IsLoad = false;
				oldtab.IsClosed = true;
				if (!oldtab.IsActive)
				{
					needSave = true;
				}
			}
		}
		if (needSave)
		{
			await TryInvokeVoidAsync("novaAdminJS.dockViewOpenTab", JsonConvert.SerializeObject((object)(from a in Tabs
				where !a.IsClosed
				orderby a.Sort
				select a)));
		}
	}

	private async Task TryInvokeVoidAsync(string identifier, params object?[] args)
	{
		try
		{
			await JS.InvokeVoidAsync(identifier, args);
		}
		catch (TaskCanceledException)
		{
			// 页面切换或连接重建时，JS 调用可能被取消，忽略即可。
		}
		catch (JSDisconnectedException)
		{
			// Blazor circuit 已断开时不再继续调用 JS。
		}
	}

	public async Task OpenModal(NovaModal modal)
	{
		Modals.Add(modal);
		await CascadeSource.NotifyChangedAsync();
	}

	public async Task CloseModal(NovaModal modal)
	{
		Modals.Remove(modal);
		await CascadeSource.NotifyChangedAsync();
	}
}
