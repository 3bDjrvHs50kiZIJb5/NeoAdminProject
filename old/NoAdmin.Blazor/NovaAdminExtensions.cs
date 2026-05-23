using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using NoAdmin.Blazor.Infrastructure.File;
using NoAdmin.Blazor.Services;
using NoAdmin.Blazor.Mvc;
using NoAdmin.Blazor.Components;
using NoAdmin.Blazor.Middlewares;
using FreeScheduler;
using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Internal.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerUI;
using NCrontab;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OnceMi.AspNetCore.OSS;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Yitter.IdGenerator;

public static class NovaAdminExtensions
{
	private record ConfirmResult(bool isConfirmed);

	public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
	{
		NullValueHandling = (NullValueHandling)1,
		Formatting = (Formatting)0,
		ContractResolver = (IContractResolver)new CamelCasePropertyNamesContractResolver(),
			Converters =
			{
				(JsonConverter)new StringEnumConverter(),
				(JsonConverter)new IsoDateTimeConverter
				{
					DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
				},
				(JsonConverter)(object)new LongJsonConverter()
			}
	};

	internal static NovaAdminOptionsItem Options { get; private set; }

	public static WebApplicationBuilder AddNovaAdminSerilog(this WebApplicationBuilder builder)
	{
		string logsPath = Path.Combine(AppContext.BaseDirectory, "Logs");
		Directory.CreateDirectory(logsPath);
		IConfigurationSection serilogSection = builder.Configuration.GetSection("Serilog");
		builder.Host.UseSerilog(delegate(HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration)
		{
			loggerConfiguration.ReadFrom.Services(services).Enrich.FromLogContext();
			if (serilogSection.Exists())
			{
				loggerConfiguration.ReadFrom.Configuration(context.Configuration);
			}
			else
			{
				loggerConfiguration.MinimumLevel.Information().MinimumLevel.Override("Microsoft", LogEventLevel.Warning).MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
					.WriteTo.Console()
					.WriteTo.File(Path.Combine(logsPath, "admin-.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30, shared: true, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");
			}
		});
		return builder;
	}

	public static WebApplication UseNovaAdminSerilogRequestLogging(this WebApplication app)
	{
		app.UseSerilogRequestLogging(delegate(Serilog.AspNetCore.RequestLoggingOptions options)
		{
			options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
		});
		return app;
	}

	/// <summary>
	/// 开发环境<para></para>
	/// - 开启Swagger<para></para>
	/// - 关闭【定时任务】持久化<para></para>
	/// 正式环境<para></para>
	/// - 关闭Swagger<para></para>
	/// - 开启【定时任务】持久化<para></para>
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public static IServiceCollection AddNovaAdmin(this WebApplicationBuilder builder, NovaAdminOptionsItem options)
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		IWebHostEnvironment env = builder.Environment;
		ConfigurationManager configuration = builder.Configuration;
		IServiceCollection services = builder.Services;
		if (options == null)
		{
			options = new NovaAdminOptionsItem();
		}
		Options = options;
		Options.IsDevelopment = env.IsDevelopment();
		if (!Options._IsSwagger.HasValue)
		{
			Options._IsSwagger = Options.IsDevelopment;
		}
		NovaAdminOptionsItem.Global_CookieName = options.CookieName;
		if (options.Assemblies == null)
		{
			options.Assemblies = new Assembly[1] { typeof(NovaAdminExtensions).Assembly };
		}
		else
		{
			options.Assemblies = options.Assemblies.Concat(new Assembly[1] { typeof(NovaAdminExtensions).Assembly }).Distinct().ToArray();
		}
		YitIdHelper.SetIdGenerator(new IdGeneratorOptions(options.WorkId)
		{
			WorkerIdBitLength = 6
		});
		string path = Path.Combine(AppContext.BaseDirectory, options.ConfigsPath, "ossconfig.json").ToPath();
		if (!File.Exists(path))
		{
			File.WriteAllText(path, "{\r\n  \"OssConfig\": {\r\n    //本地上传配置\r\n    \"LocalUploadConfig\": {\r\n      //上传目录\r\n      \"Directory\": \"uploads\",\r\n      //日期目录\r\n      \"DateTimeDirectory\": \"yyyyMMdd\",\r\n      \"Md5\": false,\r\n      //文件最大大小\r\n      \"MaxSize\": 104857600,\r\n      //包含文件拓展名列表\r\n      \"IncludeExtension\": [],\r\n      //排除文件拓展名列表\r\n      \"ExcludeExtension\": [ \".exe\", \".dll\", \".jar\" ]\r\n    },\r\n    //文件存储供应商\r\n    \"Provider\": \"Minio\",\r\n    //OSS配置列表\r\n    \"OSSConfigs\": [\r\n      //Minio\r\n      {\r\n        \"Provider\": \"Minio\",\r\n        \"Endpoint\": \"127.0.0.1:9000\",\r\n        \"Region\": \"\",\r\n        \"AccessKey\": \"ypMNCiFma9n1Ce9swm99\",\r\n        \"SecretKey\": \"JrSYawzEcsCCMmk3GBdAbYqSEd4FwM0FVn4zHO3w\",\r\n        \"IsEnableHttps\": false,\r\n        \"IsEnableCache\": true,\r\n        \"BucketName\": \"admin\",\r\n        \"Url\": \"\", //文件外链\r\n        \"Md5\": false,\r\n        \"Enable\": false\r\n      },\r\n      //阿里云\r\n      {\r\n        \"Provider\": \"Aliyun\",\r\n        \"Endpoint\": \"oss-cn-shenzhen.aliyuncs.com\",\r\n        \"Region\": \"\",\r\n        \"AccessKey\": \"\",\r\n        \"SecretKey\": \"\",\r\n        \"IsEnableHttps\": true,\r\n        \"IsEnableCache\": true,\r\n        \"BucketName\": \"admin\",\r\n        \"Url\": \"\",\r\n        \"Md5\": false,\r\n        \"Enable\": false\r\n      },\r\n      //腾讯云\r\n      {\r\n        \"Provider\": \"QCloud\",\r\n        \"Endpoint\": \"\", //AppId\r\n        \"Region\": \"\",\r\n        \"AccessKey\": \"\",\r\n        \"SecretKey\": \"\",\r\n        \"IsEnableHttps\": true,\r\n        \"IsEnableCache\": true,\r\n        \"BucketName\": \"admin\",\r\n        \"Url\": \"\",\r\n        \"Md5\": false,\r\n        \"Enable\": false\r\n      },\r\n      //七牛\r\n      {\r\n        \"Provider\": \"Qiniu\",\r\n        \"Endpoint\": \"\",\r\n        \"Region\": \"\",\r\n        \"AccessKey\": \"\",\r\n        \"SecretKey\": \"\",\r\n        \"IsEnableHttps\": true,\r\n        \"IsEnableCache\": true,\r\n        \"BucketName\": \"admin\",\r\n        \"Url\": \"\",\r\n        \"Md5\": false,\r\n        \"Enable\": false\r\n      },\r\n      //华为云\r\n      {\r\n        \"Provider\": \"HuaweiCloud\",\r\n        \"Endpoint\": \"\",\r\n        \"Region\": \"\",\r\n        \"AccessKey\": \"\",\r\n        \"SecretKey\": \"\",\r\n        \"IsEnableHttps\": true,\r\n        \"IsEnableCache\": true,\r\n        \"BucketName\": \"admin\",\r\n        \"Url\": \"\",\r\n        \"Md5\": false,\r\n        \"Enable\": false\r\n      }\r\n    ]\r\n  }\r\n}");
		}
		AddJsonFilesFromDirectory(configuration, env.EnvironmentName, options.ConfigsPath);
		services.Configure<OSSConfig>(configuration.GetSection("OssConfig"));
		List<NoAdmin.Blazor.Infrastructure.File.OSSOptions> list = configuration.GetSection("OssConfig:OSSConfigs").Get<List<NoAdmin.Blazor.Infrastructure.File.OSSOptions>>() ?? new List<NoAdmin.Blazor.Infrastructure.File.OSSOptions>();
		foreach (NoAdmin.Blazor.Infrastructure.File.OSSOptions ossOptions in list)
		{
			if (!ossOptions.Enable)
			{
				continue;
			}
			string providerName = ossOptions.Provider.ToString();
			int num = list.IndexOf(ossOptions);
			if (!services.Any((ServiceDescriptor p) => p.ServiceType == typeof(IOSSServiceFactory)))
			{
				if (!services.Any((ServiceDescriptor p) => p.ServiceType == typeof(ICacheProvider)))
				{
					services.AddMemoryCache();
						services.TryAddSingleton<ICacheProvider, AdminMemoryCacheProvider>();
				}
				services.TryAddSingleton<IOSSServiceFactory, OSSServiceFactory>();
			}
			services.Configure(providerName, delegate(OnceMi.AspNetCore.OSS.OSSOptions o)
			{
				o.AccessKey = ossOptions.AccessKey;
				o.Endpoint = ossOptions.Endpoint;
				o.IsEnableCache = ossOptions.IsEnableCache;
				o.IsEnableHttps = ossOptions.IsEnableHttps;
				o.Provider = ossOptions.Provider;
				o.Region = ossOptions.Region;
				o.SecretKey = ossOptions.SecretKey;
			});
			services.TryAddScoped((IServiceProvider sp) => sp.GetRequiredService<IOSSServiceFactory>().Create(providerName));
		}
		FreeSqlCloud cloud = new FreeSqlCloud();
		ConcurrentDictionary<Type, string> entitySteeringsMap = new ConcurrentDictionary<Type, string>
		{
			[typeof(SysTenant)] = "main",
			[typeof(SysTenantDatabase)] = "main",
			[typeof(SysTenantMenu)] = "main",
			[typeof(SysDict)] = "main",
			[typeof(SysParam)] = "main",
			[typeof(TaskInfo)] = "main_task",
			[typeof(TaskLog)] = "main_task"
		};
			((FreeSqlCloud<string>)(object)cloud).EntitySteering = delegate(object _, FreeSqlCloud.EntitySteeringEventArgs e)
		{
			if (entitySteeringsMap.TryGetValue(e.EntityType, out string value))
			{
				e.DBKey = value;
			}
		};
		((FreeSqlCloud<string>)(object)cloud).Register("main", (Func<IFreeSql>)delegate
		{
			IFreeSql val2 = GetMainFreeSql();
			NovaAdminContext.ConfigFreeSql(val2);
			val2.CodeFirst.ConfigEntity<TaskInfo>((Action<TableFluent<TaskInfo>>)delegate(TableFluent<TaskInfo> a)
			{
				a.Name("SysTask");
				a.Property<string>((Expression<Func<TaskInfo, string>>)((TaskInfo val3) => val3.Id)).StringLength(30);
				a.Property<string>((Expression<Func<TaskInfo, string>>)((TaskInfo val3) => val3.Topic)).StringLength(100);
				a.Property<string>((Expression<Func<TaskInfo, string>>)((TaskInfo val3) => val3.Body)).StringLength(-1);
				a.Property<string>((Expression<Func<TaskInfo, string>>)((TaskInfo val3) => val3.IntervalArgument)).StringLength(50);
			});
			val2.CodeFirst.ConfigEntity<TaskLog>((Action<TableFluent<TaskLog>>)delegate(TableFluent<TaskLog> a)
			{
				a.Name("SysTaskLog");
				a.Property<string>((Expression<Func<TaskLog, string>>)((TaskLog val3) => val3.TaskId)).StringLength(30);
				a.Property<string>((Expression<Func<TaskLog, string>>)((TaskLog val3) => val3.Exception)).StringLength(-1);
				a.Property<string>((Expression<Func<TaskLog, string>>)((TaskLog val3) => val3.Remark)).StringLength(-1);
			});
			return val2;
		}, (TimeSpan?)TimeSpan.FromDays(36500.0));
		IFreeSql fsql = ((FreeSqlCloud<string>)(object)cloud).Use("main");
		NovaAdminOptionsItem.Global_FixedTenant = ((ISelect0<ISelect<SysTenant>, SysTenant>)(object)fsql.Select<SysTenant>().Where((Expression<Func<SysTenant, bool>>)((SysTenant a) => a.Id == options.FixedTenantId))).First();
		((FreeSqlCloud<string>)(object)cloud).Register("main_task", (Func<IFreeSql>)delegate
		{
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			var anon = fsql.Select<SysTenant>().Where((Expression<Func<SysTenant, bool>>)((SysTenant a) => a.Id == "main")).First((SysTenant a) => new { a.TaskDatabaseId, a.TaskDatabase });
			if (anon.TaskDatabaseId == 0L || anon.TaskDatabase == null)
			{
				entitySteeringsMap[typeof(TaskInfo)] = "main";
				entitySteeringsMap[typeof(TaskLog)] = "main";
				return fsql;
			}
			entitySteeringsMap[typeof(TaskInfo)] = "main_task";
			entitySteeringsMap[typeof(TaskLog)] = "main_task";
			IFreeSql val2 = new FreeSqlBuilder().UseConnectionString(anon.TaskDatabase.DataType, anon.TaskDatabase.ConenctionString, (Type)null).UseAdoConnectionPool(true).UseNoneCommandParameter(true)
				.UseAutoSyncStructure(true)
				.Build();
			val2.CodeFirst.ConfigEntity<TaskInfo>((Action<TableFluent<TaskInfo>>)delegate(TableFluent<TaskInfo> a)
			{
				a.Name("SysTask");
				a.Property<string>((Expression<Func<TaskInfo, string>>)((TaskInfo val3) => val3.Id)).StringLength(30);
				a.Property<string>((Expression<Func<TaskInfo, string>>)((TaskInfo val3) => val3.Topic)).StringLength(100);
				a.Property<string>((Expression<Func<TaskInfo, string>>)((TaskInfo val3) => val3.Body)).StringLength(-1);
				a.Property<string>((Expression<Func<TaskInfo, string>>)((TaskInfo val3) => val3.IntervalArgument)).StringLength(50);
			});
			val2.CodeFirst.ConfigEntity<TaskLog>((Action<TableFluent<TaskLog>>)delegate(TableFluent<TaskLog> a)
			{
				a.Name("SysTaskLog");
				a.Property<string>((Expression<Func<TaskLog, string>>)((TaskLog val3) => val3.TaskId)).StringLength(30);
				a.Property<string>((Expression<Func<TaskLog, string>>)((TaskLog val3) => val3.Exception)).StringLength(-1);
				a.Property<string>((Expression<Func<TaskLog, string>>)((TaskLog val3) => val3.Remark)).StringLength(-1);
			});
			return val2;
		}, (TimeSpan?)TimeSpan.FromDays(36500.0));
		fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysAuditEntityLog) });
		fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysAuditLog) });
		fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysDict) });
		fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysParam) });
		fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysOrg) });
		fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysUser) });
		fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysUserLoginLog) });
		fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysRole) });
		fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysIpWhitelist) });
		fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysRoleUser) });
		fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysMenu) });
		fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysRoleMenu) });
		fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysTenant) });
		fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysTenantDatabase) });
		fsql.CodeFirst.SyncStructure(new Type[1] { typeof(SysTenantMenu) });
		if (!((ISelect0<ISelect<SysMenu>, SysMenu>)(object)fsql.Select<SysMenu>()).Any())
		{
			IBaseRepository<SysMenu> aggregateRootRepository = FreeSqlAggregateRootRepositoryGlobalExtensions.GetAggregateRootRepository<SysMenu>(fsql);
			aggregateRootRepository.Insert((IEnumerable<SysMenu>)new SysMenu[2]
			{
				new SysMenu
				{
					Id = 100000000000001L,
					Label = "后台首页",
					Icon = "fa-home",
					Path = "Admin",
					Sort = -1000,
					Type = SysMenuType.菜单,
					IsSystem = true,
					IsHidden = false
				},
				new SysMenu
				{
					Id = 100000000000002L,
					Label = "系统管理",
					Icon = "fas fa-gears",
					Path = "",
					Sort = 101,
					Type = SysMenuType.菜单,
					IsSystem = true,
					Children = new List<SysMenu>
					{
						new SysMenu
						{
							Id = 100000000000003L,
							Label = "组织",
							Path = "Admin/Org",
							Sort = 201,
							Icon = "fas fa-sitemap",
							Type = SysMenuType.菜单,
							IsSystem = true,
							Children = getCudButtons(Array.Empty<SysMenu>())
						},
						new SysMenu
						{
							Id = 100000000000004L,
							Label = "用户资料",
							Path = "Admin/UserProfile",
							Sort = 202,
							Icon = "fas fa-address-card",
							Type = SysMenuType.菜单,
							IsSystem = true,
							IsHidden = true
						},
						new SysMenu
						{
							Id = 100000000000005L,
							Label = "用户",
							Path = "Admin/User",
							Sort = 203,
							Icon = "fas fa-user",
							Type = SysMenuType.菜单,
							IsSystem = true,
							Children = getCudButtons(new SysMenu[2]
							{
								new SysMenu
								{
									Label = "分配角色",
									Path = "alloc_roles",
									Sort = 301,
									Type = SysMenuType.按钮
								},
								new SysMenu
								{
									Label = "登陆日志",
									Path = "loginlog",
									Sort = 302,
									Type = SysMenuType.按钮
								}
							})
						},
						new SysMenu
						{
							Id = 100000000000006L,
							Label = "角色",
							Path = "Admin/Role",
							Sort = 204,
							Icon = "fas fa-user-tag",
							Type = SysMenuType.菜单,
							IsSystem = true,
							Children = getCudButtons(new SysMenu[2]
							{
								new SysMenu
								{
									Label = "分配用户",
									Path = "alloc_users",
									Sort = 301,
									Type = SysMenuType.按钮,
									IsSystem = true
								},
								new SysMenu
								{
									Label = "分配菜单",
									Path = "alloc_menus",
									Sort = 302,
									Type = SysMenuType.按钮,
									IsSystem = true
								}
							})
						},
						new SysMenu
						{
							Id = 100000000000007L,
							Label = "菜单",
							Path = "Admin/Menu",
							Sort = 205,
							Icon = "fas fa-list",
							Type = SysMenuType.菜单,
							IsSystem = true,
							Children = getCudButtons(Array.Empty<SysMenu>())
						},
						new SysMenu
						{
							Id = 100000000000008L,
							Label = "定时任务",
							Path = "Admin/TaskScheduler",
							Sort = 206,
							Icon = "fas fa-clock",
							Type = SysMenuType.菜单,
							IsSystem = true,
							Children = new List<SysMenu>
							{
								new SysMenu
								{
									Label = "添加",
									Path = "add",
									Sort = 301,
									Type = SysMenuType.按钮,
									IsSystem = true
								},
								new SysMenu
								{
									Label = "删除",
									Path = "remove",
									Sort = 302,
									Type = SysMenuType.按钮,
									IsSystem = true
								},
								new SysMenu
								{
									Label = "暂停",
									Path = "pause",
									Sort = 303,
									Type = SysMenuType.按钮,
									IsSystem = true
								},
								new SysMenu
								{
									Label = "恢复",
									Path = "resume",
									Sort = 304,
									Type = SysMenuType.按钮,
									IsSystem = true
								},
								new SysMenu
								{
									Label = "立即触发",
									Path = "runnow",
									Sort = 305,
									Type = SysMenuType.按钮,
									IsSystem = true
								},
								new SysMenu
								{
									Label = "查看日志",
									Path = "tasklog",
									Sort = 306,
									Type = SysMenuType.按钮,
									IsSystem = true
								},
								new SysMenu
								{
									Label = "集群日志",
									Path = "clusterlog",
									Sort = 307,
									Type = SysMenuType.按钮,
									IsSystem = true
								}
							}
						},
						new SysMenu
						{
							Id = 100000000000009L,
							Label = "数据字典",
							Path = "Admin/Dict",
							Sort = 207,
							Icon = "fas fa-book-bookmark",
							Type = SysMenuType.菜单,
							IsSystem = true,
							Children = getCudButtons(Array.Empty<SysMenu>())
						},
						new SysMenu
						{
							Id = 100000000000010L,
							Label = "参数配置",
							Path = "Admin/Param",
							Sort = 208,
							Icon = "fas fa-gears",
							Type = SysMenuType.菜单,
							IsSystem = true,
							Children = getCudButtons(Array.Empty<SysMenu>())
						},
						new SysMenu
						{
							Id = 100000000000011L,
							Label = "租户",
							Path = "Admin/Tenant",
							Sort = 209,
							Icon = "fas fa-users-viewfinder",
							Type = SysMenuType.菜单,
							IsSystem = true,
							Children = getCudButtons(Array.Empty<SysMenu>())
						},
						new SysMenu
						{
							Id = 100000000000012L,
							Label = "数据库",
							Path = "Admin/TenantDatabase",
							Sort = 210,
							Icon = "fas fa-database",
							Type = SysMenuType.菜单,
							IsSystem = true,
							Children = getCudButtons(Array.Empty<SysMenu>())
						},
						new SysMenu
						{
							Id = 100000000000013L,
							Label = "文件",
							Path = "Admin/File",
							Sort = 211,
							Icon = "fas fa-folder-open",
							Type = SysMenuType.菜单,
							IsSystem = true,
							Children = getCudButtons(Array.Empty<SysMenu>())
						},
						new SysMenu
						{
							Id = 100000000000014L,
							Label = "IP白名单",
							Path = "Admin/IpWhitelist",
							Sort = 212,
							Icon = "fas fa-shield-alt",
							Type = SysMenuType.菜单,
							IsSystem = true,
							Children = getCudButtons(Array.Empty<SysMenu>())
						}
					}
				}
			});
		}
		SysTenantDatabase sysTenantDatabase = ((ISelect0<ISelect<SysTenantDatabase>, SysTenantDatabase>)(object)fsql.Select<SysTenantDatabase>()).First();
		if (sysTenantDatabase == null)
		{
			IFreeSql obj = fsql;
			SysTenantDatabase obj2 = new SysTenantDatabase
			{
				Label = "体验数据库",
				DataType = (FreeSql.DataType)4,
				ConenctionString = "data source={database}.db"
			};
			sysTenantDatabase = obj2;
			obj.Insert<SysTenantDatabase>(obj2).ExecuteAffrows();
		}
		MenuService.InitMenuData(fsql, new SysMenu
		{
			Label = "系统管理",
			Path = "",
			Type = SysMenuType.菜单,
			IsSystem = true,
			Children = new List<SysMenu>
			{
				new SysMenu
				{
					Label = "IP白名单",
					Path = "Admin/IpWhitelist",
					Icon = "fas fa-shield-alt",
					Type = SysMenuType.菜单,
					IsSystem = true,
					Children = getCudButtons(Array.Empty<SysMenu>())
				}
			}
		});
		if (!fsql.Select<SysTenant>().Any((Expression<Func<SysTenant, bool>>)((SysTenant a) => a.Id == "main")))
		{
			fsql.Insert<SysTenant>(new SysTenant
			{
				Id = "main",
				Host = "localhost",
				Host2 = "127.0.0.1",
				Title = "NoAdmin",
				Description = "NoAdmin SaaS 系统主库(租户管理)",
				IsEnabled = true
			}).ExecuteAffrows();
		}
		if (!((ISelect0<ISelect<SysOrg>, SysOrg>)(object)fsql.Select<SysOrg>()).Any())
		{
			SysOrg[] array = new SysOrg[3]
			{
				new SysOrg
				{
					Label = "集团",
					Type = SysOrg.OrgType.集团,
					Description = "集团",
					IsEnabled = true,
					Sort = 101,
					Children = new List<SysOrg>
					{
						new SysOrg
						{
							Label = "xx公司",
							Type = SysOrg.OrgType.公司,
							Description = "公司",
							IsEnabled = true,
							Sort = 201,
							Children = new List<SysOrg>
							{
								new SysOrg
								{
									Label = "IT部",
									Type = SysOrg.OrgType.部门,
									Description = "",
									IsEnabled = true,
									Sort = 301
								},
								new SysOrg
								{
									Label = "财务部",
									Type = SysOrg.OrgType.部门,
									Description = "",
									IsEnabled = true,
									Sort = 302
								},
								new SysOrg
								{
									Label = "人事部",
									Type = SysOrg.OrgType.部门,
									Description = "",
									IsEnabled = true,
									Sort = 303
								},
								new SysOrg
								{
									Label = "运营部",
									Type = SysOrg.OrgType.部门,
									Description = "",
									IsEnabled = true,
									Sort = 304
								},
								new SysOrg
								{
									Label = "设计部",
									Type = SysOrg.OrgType.部门,
									Description = "",
									IsEnabled = true,
									Sort = 305
								},
								new SysOrg
								{
									Label = "技术部",
									Type = SysOrg.OrgType.部门,
									Description = "",
									IsEnabled = true,
									Sort = 306
								},
								new SysOrg
								{
									Label = "生产部",
									Type = SysOrg.OrgType.部门,
									Description = "",
									IsEnabled = true,
									Sort = 307
								}
							}
						}
					}
				},
				new SysOrg
				{
					Label = "供应商",
					Type = SysOrg.OrgType.供应商,
					Description = "供应商",
					IsEnabled = true,
					Sort = 102
				},
				new SysOrg
				{
					Label = "客户",
					Type = SysOrg.OrgType.客户,
					Description = "客户",
					IsEnabled = true,
					Sort = 103
				}
			};
			FreeSqlAggregateRootRepositoryGlobalExtensions.GetAggregateRootRepository<SysOrg>(fsql).Insert((IEnumerable<SysOrg>)array);
		}
		if (!((ISelect0<ISelect<SysRole>, SysRole>)(object)fsql.Select<SysRole>().Where((Expression<Func<SysRole, bool>>)((SysRole a) => a.IsAdministrator))).Any())
		{
			fsql.Insert<SysRole>(new SysRole[2]
			{
				new SysRole
				{
					Name = "管理员",
					Description = "管理员角色",
					IsAdministrator = true
				},
				new SysRole
				{
					Name = "普通用户",
					Description = "普通用户",
					IsAdministrator = false
				}
			}).ExecuteAffrows();
		}
		if (!((ISelect0<ISelect<SysUser>, SysUser>)(object)fsql.Select<SysUser>().Where((Expression<Func<SysUser, bool>>)((SysUser a) => a.Roles.Any((SysRole b) => b.IsAdministrator)))).Any())
		{
			SysUser sysUser = new SysUser();
			sysUser.Username = "admin";
			sysUser.Password = "admin";
			sysUser.Nickname = "管理员";
			sysUser.IsEnabled = true;
			sysUser.IsSystem = true;
			sysUser.OrgId = fsql.Select<SysOrg>().First<long>((Expression<Func<SysOrg, long>>)((SysOrg a) => a.Id));
			SysUser sysUser2 = sysUser;
			int num2 = 1;
			List<SysRole> list2 = new List<SysRole>(num2);
			CollectionsMarshal.SetCount(list2, num2);
			Span<SysRole> span = CollectionsMarshal.AsSpan(list2);
			int index = 0;
			span[index] = ((ISelect0<ISelect<SysRole>, SysRole>)(object)fsql.Select<SysRole>().Where((Expression<Func<SysRole, bool>>)((SysRole a) => a.IsAdministrator))).First();
			sysUser2.Roles = list2;
			FreeSqlAggregateRootRepositoryGlobalExtensions.GetAggregateRootRepository<SysUser>(fsql).Insert(sysUser2);
		}
		Func<IServiceProvider, IFreeSql> implementationFactory = delegate(IServiceProvider r)
		{
			NovaAdminContext service = r.GetService<NovaAdminContext>();
			return service.Orm;
		};
		services.AddSingleton((IServiceProvider r) => cloud);
		services.AddScoped<NovaAdminContext>();
		services.AddCascadingValue(delegate(IServiceProvider sp)
		{
			NovaAdminContext service = sp.GetService<NovaAdminContext>();
			return service.CascadeSource = new CascadingValueSource<NovaAdminContext>(service, isFixed: false);
		});
		services.AddScoped(implementationFactory);
		services.AddScoped<UnitOfWorkManager>();
		services.AddScoped((IServiceProvider r) => new RepositoryOptions
		{
				AuditValue = async delegate(DbContextAuditValueEventArgs e)
				{
					NovaAdminContext admin = r.GetService<NovaAdminContext>();
					if (Options.AuditValue != null)
					{
						await Options.AuditValue(admin, e);
					}
					SysUser user = admin?.User;
					if (user == null)
					{
						return;
					}

					if ((int)e.AuditValueType == 1 && e.Object is IEntityCreated created)
					{
						created.CreatedUserId = user.Id;
						created.CreatedUserName = user.Username;
					}

					if ((int)e.AuditValueType == 0 && e.Object is IEntityModified modified)
					{
						modified.ModifiedUserId = user.Id;
						modified.ModifiedUserName = user.Username;
					}
				}
			});
		services.AddScoped(typeof(IBaseRepository<>), typeof(BasicRepository<>));
		services.AddScoped(typeof(BaseRepository<>), typeof(BasicRepository<>));
		services.AddScoped(typeof(IBaseRepository<, >), typeof(BasicRepository<, >));
		services.AddScoped(typeof(BaseRepository<, >), typeof(BasicRepository<, >));
		services.AddScoped(typeof(IAggregateRootRepository<>), typeof(DddRepository<>));
		services.AddTransient(typeof(IAggregateRootRepositoryTransient<>), typeof(DddRepository<>));
		services.AddScoped(typeof(FileService));
		IFreeSql taskdb = ((FreeSqlCloud<string>)(object)cloud).Use("main_task");
		Dictionary<string, Action<IServiceProvider, TaskInfo>> schedulerAttributeTriggers = new Dictionary<string, Action<IServiceProvider, TaskInfo>>();
		Func<IServiceProvider, Scheduler> implementationFactory2 = (IServiceProvider r) => new FreeSchedulerBuilder().OnExecuting((Action<TaskInfo>)delegate(TaskInfo val2)
		{
			using IServiceScope serviceScope = r.CreateScope();
			if (schedulerAttributeTriggers.TryGetValue(val2.Topic, out Action<IServiceProvider, TaskInfo> value))
			{
				value(serviceScope.ServiceProvider, val2);
			}
			else
			{
				options.SchedulerExecuting?.Invoke(serviceScope.ServiceProvider, val2);
			}
		}).UseTimeZone(TimeSpan.FromHours(8.0)).UseStorage(taskdb, !env.IsDevelopment(), (Func<TaskInfo, bool>)null)
			.UseCustomInterval((Func<TaskInfo, TimeSpan?>)delegate(TaskInfo val2)
			{
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_001f: Expected O, but got Unknown
				DateTime utcNow = DateTime.UtcNow;
				DateTime nextOccurrence = CrontabSchedule.Parse(val2.IntervalArgument, new CrontabSchedule.ParseOptions
				{
					IncludingSeconds = true
				}).GetNextOccurrence(utcNow);
				return (nextOccurrence < utcNow) ? new TimeSpan?(TimeSpan.FromSeconds(5.0)) : new TimeSpan?(nextOccurrence.Subtract(utcNow));
			})
			.Build();
		if (options.Assemblies.Any())
		{
			Assembly[] assemblies = options.Assemblies;
			foreach (Assembly assembly in assemblies)
			{
				var list3 = (from a in assembly.GetTypes().SelectMany((Type a) => a.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Select(delegate(MethodInfo b)
					{
						//IL_0036: Unknown result type (might be due to invalid IL or missing references)
						//IL_003b: Unknown result type (might be due to invalid IL or missing references)
						//IL_0052: Unknown result type (might be due to invalid IL or missing references)
						//IL_0054: Unknown result type (might be due to invalid IL or missing references)
						//IL_005f: Unknown result type (might be due to invalid IL or missing references)
						//IL_006c: Unknown result type (might be due to invalid IL or missing references)
						//IL_0079: Unknown result type (might be due to invalid IL or missing references)
						//IL_007b: Unknown result type (might be due to invalid IL or missing references)
						//IL_0086: Unknown result type (might be due to invalid IL or missing references)
						//IL_0092: Unknown result type (might be due to invalid IL or missing references)
						//IL_009e: Unknown result type (might be due to invalid IL or missing references)
						//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
						//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
						//IL_00c6: Expected O, but got Unknown
						SchedulerAttribute customAttribute = b.GetCustomAttribute<SchedulerAttribute>();
						return (customAttribute == null || customAttribute.Name.IsNull()) ? null : new
						{
							Class = a,
							Method = b,
							IsLazyTask = customAttribute.Argument.IsNull(),
							TaskInfo = new TaskInfo
							{
								Topic = "[SchedulerAttribute]" + customAttribute.Name,
								Interval = customAttribute.Interval,
								IntervalArgument = customAttribute.Argument,
								Round = customAttribute.Round,
								Status = customAttribute.Status,
								Body = "",
								CreateTime = DateTime.Now,
								CurrentRound = 0,
								ErrorTimes = 0,
								LastRunTime = new DateTime(1970, 1, 1)
							}
						};
					}))
					where a != null
					select a).ToList();
				if (!list3.Any())
				{
					continue;
				}
				List<TaskInfo> list4 = ((ISelect0<ISelect<TaskInfo>, TaskInfo>)(object)taskdb.Select<TaskInfo>().Where((Expression<Func<TaskInfo, bool>>)((TaskInfo a) => a.Topic.StartsWith("[SchedulerAttribute]")))).ToList();
				List<TaskInfo> list5 = (from a in list3
					where !a.IsLazyTask
					select a.TaskInfo).ToList();
				foreach (TaskInfo task in list5)
				{
					TaskInfo val = list4.Find((TaskInfo a) => a.Topic == task.Topic);
					if (val != null)
					{
						task.Id = val.Id;
						task.Body = val.Body;
						task.CreateTime = val.CreateTime;
						task.CurrentRound = val.CurrentRound;
						task.ErrorTimes = val.ErrorTimes;
						task.LastRunTime = val.LastRunTime;
					}
					else
					{
						task.Id = $"{DateTime.Now.ToString("yyyyMMdd")}.{YitIdHelper.NextId()}";
					}
				}
				IBaseRepository<TaskInfo> repository = FreeSqlDbContextExtensions.GetRepository<TaskInfo>(taskdb);
				repository.BeginEdit(list4);
				repository.EndEdit(list5);
				foreach (var item in list3)
				{
					MethodInfo method = item.Method;
					string text = item.TaskInfo.Topic;
					if (item.IsLazyTask)
					{
						text = text.Replace("[SchedulerAttribute]", "");
					}
					schedulerAttributeTriggers[text] = delegate(IServiceProvider r, TaskInfo val2)
					{
						object obj3 = method.Invoke(null, ((IEnumerable<ParameterInfo>)method.GetParameters()).Select((Func<ParameterInfo, object>)((ParameterInfo a) => (a.ParameterType == typeof(IServiceProvider)) ? r : ((IServiceProvider)((a.ParameterType == typeof(TaskInfo)) ? val2 : null)))).ToArray());
						if (obj3 != null && typeof(Task).IsAssignableFrom(method.ReturnType))
						{
							((Task)obj3)?.Wait();
						}
					};
				}
			}
		}
		services.AddSingleton(implementationFactory2);
		services.AddHttpContextAccessor();
		BootstrapBlazorServiceCollectionExtensions.AddBootstrapBlazor(services, (Action<BootstrapBlazorOptions>)null, (Action<JsonLocalizationOptions>)null);
		BootstrapBlazor.Components.ServiceCollectionExtension.AddBootstrapBlazorRegionService(services);
		builder.Services.AddRequestLocalization(delegate(RequestLocalizationOptions localizerOption, IOptions<BootstrapBlazorOptions> blazorOption)
		{
			IList<CultureInfo> supportedUICultures = (localizerOption.SupportedCultures = blazorOption.Value.GetSupportedCultures());
			localizerOption.SupportedUICultures = supportedUICultures;
		});
		NewtonsoftJsonMvcBuilderExtensions.AddNewtonsoftJson(services.AddControllers(delegate(MvcOptions opt)
		{
			opt.Filters.Add<AdminOmniFilter>();
		}), (Action<MvcNewtonsoftJsonOptions>)delegate(MvcNewtonsoftJsonOptions opt)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Expected O, but got Unknown
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Expected O, but got Unknown
			opt.SerializerSettings.Formatting = (Formatting)0;
			opt.SerializerSettings.ContractResolver = (IContractResolver)new CamelCasePropertyNamesContractResolver();
			opt.SerializerSettings.Converters.Add((JsonConverter)new StringEnumConverter());
			opt.SerializerSettings.Converters.Add((JsonConverter)new IsoDateTimeConverter
			{
				DateTimeFormat = "yyyy-MM-dd HH:mm:ss"
			});
			opt.SerializerSettings.Converters.Add((JsonConverter)(object)new LongJsonConverter());
		});
		if (Options.IsSwagger)
		{
			services.AddCors(delegate(CorsOptions corsOptions)
			{
				corsOptions.AddPolicy("cors_all", delegate(CorsPolicyBuilder corsPolicyBuilder)
				{
					corsPolicyBuilder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
				});
			});
			SwaggerGenServiceCollectionExtensions.AddSwaggerGen(services, (Action<SwaggerGenOptions>)delegate(SwaggerGenOptions opt)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0029: Expected O, but got Unknown
				//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00df: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f1: Expected O, but got Unknown
				SwaggerGenOptionsExtensions.SwaggerDoc(opt, "v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "NoAdmin WebAPI"
				});
				Assembly[] assemblies2 = options.Assemblies;
				foreach (Assembly assembly2 in assemblies2)
				{
					string text2 = Path.Combine(AppContext.BaseDirectory, assembly2.GetName().Name + ".xml");
					if (File.Exists(text2))
					{
						SwaggerGenOptionsExtensions.IncludeXmlComments(opt, text2, true);
					}
				}
				SwaggerGenOptionsExtensions.SchemaFilter<SwaggerSchemaFilter>(opt, Array.Empty<object>());
				SwaggerGenOptionsExtensions.DocumentFilter<SwaggerDocumentFilter>(opt, Array.Empty<object>());
				SwaggerGenOptionsExtensions.AddDocumentFilterInstance<HideControllerFilter>(opt, new HideControllerFilter(options.SwaggerHides));
				SwaggerGenOptionsExtensions.AddSecurityDefinition(opt, "Bearer", (IOpenApiSecurityScheme)new OpenApiSecurityScheme
				{
					Type = (SecuritySchemeType)0,
					Description = "Token令牌",
					Name = "Authorization",
					In = (ParameterLocation)1
				});
			});
		}
		return services;
		IFreeSql GetMainFreeSql()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			FreeSqlBuilder val2 = new FreeSqlBuilder().UseNoneCommandParameter(true);
			if (options.FreeSqlBuilder != null)
			{
				options.FreeSqlBuilder?.Invoke(val2);
			}
			else
			{
				val2.UseConnectionString((FreeSql.DataType)4, "Data Source=master.db", (Type)null).UseMonitorCommand((Action<DbCommand>)delegate(DbCommand cmd)
				{
					System.Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] {cmd.CommandText}\r\n");
				}, (Action<DbCommand, string>)null).UseAutoSyncStructure(true);
			}
			IFreeSql val3 = val2.Build();
			NovaAdminContext.ConfigFreeSql(val3);
			return val3;
		}
		static List<SysMenu> getCudButtons(params SysMenu[] btns)
		{
			return new SysMenu[3]
			{
				new SysMenu
				{
					Label = "添加",
					Path = "add",
					Sort = 301,
					Type = SysMenuType.按钮,
					IsSystem = true
				},
				new SysMenu
				{
					Label = "编辑",
					Path = "edit",
					Sort = 302,
					Type = SysMenuType.按钮,
					IsSystem = true
				},
				new SysMenu
				{
					Label = "删除",
					Path = "remove",
					Sort = 303,
					Type = SysMenuType.按钮,
					IsSystem = true
				}
			}.Concat(btns ?? new SysMenu[0]).ToList();
		}
	}

	public static WebApplication UseAdminOmniApi(this WebApplication app)
	{
		app.UseIpWhitelist();
		app.UseHttpMethodOverride(new HttpMethodOverrideOptions
		{
			FormFieldName = "X-Http-Method-Override"
		});
		if (app.Environment.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}
			if (Options.IsSwagger)
			{
				app.UseCors("cors_all");
				SwaggerBuilderExtensions.UseSwagger((IApplicationBuilder)app, (Action<SwaggerOptions>)null);
				app.UseSwaggerUI(delegate(SwaggerUIOptions c)
				{
					c.RoutePrefix = "api";
					c.DocumentTitle = "NoAdmin Swagger UI";
					c.SwaggerEndpoint("/v1/api-docs", "V1 Docs");
				});
				SwaggerBuilderExtensions.MapSwagger((IEndpointRouteBuilder)app, "{documentName}/api-docs", (Action<SwaggerEndpointOptions>)null);
			}
		app.Use(async delegate(HttpContext context, Func<Task> next)
		{
			if (!context.Request.Path.StartsWithSegments("/api"))
			{
				await next();
			}
			else
			{
				NovaAdminContext admin = context.RequestServices.GetService<NovaAdminContext>();
				await admin.Init(isApi: true);
				TransactionalAttribute.SetServiceProvider(context.RequestServices);
				if (app.Environment.IsDevelopment())
				{
					context.RequestServices.GetService<ILogger<NovaAdminContext>>();
					await next();
				}
				else
				{
					await next();
				}
			}
		});
		app.MapControllers();
		return app;
	}

	public static string GetQueryStringValue(this NavigationManager nav, string name, string rawUrl = null)
	{
		Uri uri = (string.IsNullOrWhiteSpace(rawUrl) ? new Uri(nav.Uri) : nav.ToAbsoluteUri(rawUrl));
		NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(uri.Query);
		return nameValueCollection[name] ?? "";
	}

	public static string[] GetQueryStringValues(this NavigationManager nav, string name, string rawUrl = null)
	{
		Uri uri = (string.IsNullOrWhiteSpace(rawUrl) ? new Uri(nav.Uri) : nav.ToAbsoluteUri(rawUrl));
		NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(uri.Query);
		return nameValueCollection.GetValues(name) ?? Array.Empty<string>();
	}

	public static string UrlEncode(this string str)
	{
		return str.IsNull() ? "" : HttpUtility.UrlEncode(str);
	}

	public static async Task<bool> Confirm(this IJSRuntime JS, string title, string text = "")
	{
		return (await JSRuntimeExtensions.InvokeAsync<ConfirmResult>(JS, "novaAdminJS.confirm", new object[2] { title, text })).isConfirmed;
	}

	public static async Task Success(this MessageService messageService, string message)
	{
		await messageService.Show(new MessageOption
		{
			Color = (Color)4,
			Icon = "fa-solid fa-circle-info",
			Content = message,
			Delay = 1500
		}, (Message)null);
	}

	public static async Task Error(this MessageService messageService, string message)
	{
		await messageService.Show(new MessageOption
		{
			Color = (Color)5,
			Icon = "fa-solid fa-triangle-exclamation",
			Content = message,
			Delay = 1500
		}, (Message)null);
	}

	public static async Task Warning(this MessageService messageService, string message)
	{
		await messageService.Show(new MessageOption
		{
			Color = (Color)6,
			Icon = "fa-solid fa-circle-exclamation",
			Content = message,
			Delay = 1500
		}, (Message)null);
	}

	public static List<NovaAdminItem<TItem>> ToNovaAdminItemList<TItem>(this List<TItem> list, IFreeSql fsql)
	{
		TableInfo meta = fsql.CodeFirst.GetTableByEntity(typeof(TItem));
		KeyValuePair<string, TableRef> treeNav = (from a in meta.GetAllTableRef()
			where a.Value.Exception == null && (int)a.Value.RefType == 2 && a.Value.RefEntityType == typeof(TItem) && a.Value.Columns.Count == 1 && a.Value.Columns[0].Attribute.IsPrimary && a.Value.RefColumns.Count == 1
			select a).FirstOrDefault();
		if (!treeNav.Key.IsNull())
		{
			List<NovaAdminItem<TItem>> result = eachListParentId(FreeSqlGlobalExtensions.CreateInstanceGetDefaultValue(treeNav.Value.Columns[0].CsType), 1);
			list.Clear();
			return result;
		}
		return list.Select((TItem a) => new NovaAdminItem<TItem>(a)).ToList();
		object GetItemPrimaryValue(TItem item)
		{
			return meta.Primarys[0].GetValue((object)item);
		}
		List<NovaAdminItem<TItem>> eachListParentId(object id, int level)
		{
			List<NovaAdminItem<TItem>> list2 = new List<NovaAdminItem<TItem>>();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				object value = treeNav.Value.RefColumns[0].GetValue((object)list[num]);
				if (object.Equals(value, id) || (level == 1 && object.Equals(value, null)))
				{
					list2.Insert(0, new NovaAdminItem<TItem>(list[num])
					{
						Level = level
					});
					list.RemoveAt(num);
				}
			}
			if (list.Any())
			{
				List<List<NovaAdminItem<TItem>>> list3 = new List<List<NovaAdminItem<TItem>>>();
				for (int i = 0; i < list2.Count; i++)
				{
					list3.Add(eachListParentId(GetItemPrimaryValue(list2[i].Value), level + 1));
				}
				int num2 = 0;
				int num3 = 0;
				while (num2 < list3.Count)
				{
					if (list3[num2].Any())
					{
						list2.InsertRange(num3 + 1, list3[num2]);
						num3 += list3[num2].Count;
					}
					num2++;
					num3++;
				}
				list3.Clear();
			}
			return list2;
		}
	}

	/// <summary>
	/// 添加配置文件
	/// </summary>
	/// <param name="configuration">配置</param>
	/// <param name="environmentName">环境名</param>
	/// <param name="directory">目录</param>
	/// <param name="optional">可选</param>
	/// <param name="reloadOnChange">热更新</param>
	private static void AddJsonFilesFromDirectory(ConfigurationManager configuration, string environmentName, string directory = "Configs", bool optional = true, bool reloadOnChange = true)
	{
		string text = Path.Combine(AppContext.BaseDirectory, directory).ToPath();
		System.Console.WriteLine(text);
		Directory.CreateDirectory(text);
		IEnumerable<string> enumerable = from p in Directory.GetFiles(text)
			where p.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
			select p;
		IEnumerable<string> second = enumerable.Where((string p) => p.EndsWith("." + environmentName + ".json", StringComparison.OrdinalIgnoreCase));
		IEnumerable<string> first = enumerable.Except(second);
		IEnumerable<string> enumerable2 = first.Concat(second);
		foreach (string item in enumerable2)
		{
			System.Console.WriteLine(item);
			configuration.AddJsonFile(item, optional, reloadOnChange);
		}
	}

	public static string ToJson(this object obj)
	{
		return JsonConvert.SerializeObject(obj, JsonSerializerSettings);
	}

	public static T Deserialize<T>(this string json)
	{
		return JsonConvert.DeserializeObject<T>(json, JsonSerializerSettings);
	}
}
