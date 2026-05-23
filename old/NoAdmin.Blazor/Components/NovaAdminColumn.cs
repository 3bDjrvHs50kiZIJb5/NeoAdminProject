using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace NoAdmin.Blazor.Components;

public class NovaAdminColumn<TType> : ComponentBase, INovaAdminColumn, IDisposable
{
	private Func<object, object> _fieldAccessor;

	private static ConcurrentDictionary<string, Func<object, object>> _accessorCache = new ConcurrentDictionary<string, Func<object, object>>();

	[CascadingParameter]
	public object Table { get; set; }

	/// <summary>
	/// 绑定字段值
	/// </summary>
	[Parameter]
	public TType Field { get; set; }

	[Parameter]
	public EventCallback<TType> FieldChanged { get; set; }

	/// <summary>
	/// 绑定字段表达式，用于自动推断 Title、Sort、FilterKey，以及构建值访问器
	/// </summary>
	[Parameter]
	public Expression<Func<TType>> FieldExpression { get; set; }

	/// <summary>标题</summary>
	[Parameter]
	public string Title { get; set; }

	/// <summary>宽度 (px)</summary>
	[Parameter]
	public int Width { get; set; }

	/// <summary>
	/// 排序字段名。
	/// </summary>
	[Parameter]
	public string Sort { get; set; }

	/// <summary>
	/// 对应的筛选Key。
	/// </summary>
	[Parameter]
	public string FilterKey { get; set; }

	/// <summary>自定义列内容模板 (Row)</summary>
	[Parameter]
	public RenderFragment<object>? Template { get; set; }

	/// <summary>自定义列内容模板 (Value)</summary>
	[Parameter]
	public RenderFragment<TType> ChildContent { get; set; }

	/// <summary>是否在界面上固定 (Left/Right)</summary>
	[Parameter]
	public NovaAdminColumnFixed Fixed { get; set; } = NovaAdminColumnFixed.None;

	/// <summary>是否为主列（显示树形展开图标、多选框）</summary>
	[Parameter]
	public bool Primary { get; set; }

	/// <summary>是否为操作列（显示编辑、删除按钮）</summary>
	[Parameter]
	public bool IsOperation { get; set; }

	/// <summary>是否显示 (控制整列：表头、筛选、数据)</summary>
	[Parameter]
	public bool Visible { get; set; } = true;

	/// <summary>格式化字符串 (例如 "yyyy-MM-dd" 或 "C2")，仅当使用了 Field 且未设置 Template 时有效</summary>
	[Parameter]
	public string FormatString { get; set; }

	/// <summary>内部使用，用于排序</summary>
	public int Order { get; set; }

	/// <summary>内部使用，用于计算样式</summary>
	public string CalculatedStyle { get; set; }

	protected override void OnInitialized()
	{
		if (FieldExpression != null)
		{
			MemberInfo memberInfo = GetMemberInfo(FieldExpression.Body);
			if (memberInfo != null)
			{
				if (string.IsNullOrEmpty(Title))
				{
					DisplayAttribute customAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
					DisplayNameAttribute customAttribute2 = memberInfo.GetCustomAttribute<DisplayNameAttribute>();
					Title = customAttribute?.Name ?? customAttribute2?.DisplayName ?? memberInfo.Name;
				}
				InitializeAccessor(memberInfo.Name);
			}
		}
		if (Template == null && ChildContent != null)
		{
			Template = (object obj) => delegate(RenderTreeBuilder builder)
			{
				object value = GetValue(obj);
				TType value2 = default(TType);
				if (value != null)
				{
					value2 = (TType)value;
				}
				builder.AddContent(0, ChildContent(value2));
			};
		}
		if (Table != null)
		{
			MethodInfo method = Table.GetType().GetMethod("AddColumn", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				method.Invoke(Table, new object[1] { this });
			}
		}
	}

	private void InitializeAccessor(string propertyName)
	{
		if (Table == null)
		{
			return;
		}
		Type type = Table.GetType();
		if (!type.IsGenericType)
		{
			return;
		}
		Type tItem = type.GetGenericArguments()[0];
		string key = tItem.FullName + "." + propertyName;
		_fieldAccessor = _accessorCache.GetOrAdd(key, (Func<string, Func<object, object>>)delegate
		{
			try
			{
				ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "obj");
				UnaryExpression expression = Expression.Convert(parameterExpression, tItem);
				MemberExpression expression2 = Expression.Property(expression, propertyName);
				UnaryExpression body = Expression.Convert(expression2, typeof(object));
				Expression<Func<object, object>> expression3 = Expression.Lambda<Func<object, object>>(body, new ParameterExpression[1] { parameterExpression });
				return expression3.Compile();
			}
			catch
			{
				return (Func<object, object>)null;
			}
		});
	}

	public object GetValue(object item)
	{
		if (_fieldAccessor != null)
		{
			object obj = _fieldAccessor(item);
			if (obj != null && !string.IsNullOrEmpty(FormatString) && obj is IFormattable formattable)
			{
				return formattable.ToString(FormatString, null);
			}
			return obj;
		}
		return null;
	}

	private MemberInfo GetMemberInfo(Expression expression)
	{
		if (expression is MemberExpression memberExpression)
		{
			return memberExpression.Member;
		}
		if (expression is UnaryExpression { Operand: MemberExpression operand })
		{
			return operand.Member;
		}
		return null;
	}

	public void Dispose()
	{
		if (Table != null)
		{
			MethodInfo method = Table.GetType().GetMethod("RemoveColumn", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				method.Invoke(Table, new object[1] { this });
			}
		}
	}
}
