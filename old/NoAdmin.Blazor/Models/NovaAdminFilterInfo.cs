using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace NoAdmin.Blazor.Models;

public class NovaAdminFilterInfo
{
	public string Label { get; set; }

	public string QueryStringName { get; set; }

	public NovaAdminFilterType Type { get; set; }

	public Dictionary<string, object> ExtraData { get; set; } = new Dictionary<string, object>();

	public bool Visible { get; set; } = true;

	public int Order { get; set; }

	public NovaAdminItem<NovaAdminOptionsItem>[] Options { get; set; }

	public int Col { get; set; } = 12;

	public bool HasValue => Options.Where((NovaAdminItem<NovaAdminOptionsItem> a) => a.Selected).Any();

	public T[] Values<T>()
	{
		return (from a in Options
			where a.Selected
			select a.Value.Value.ConvertTo<T>()).ToArray();
	}

	public T Value<T>()
	{
		return Values<T>().FirstOrDefault();
	}

	public NovaAdminFilterInfo(string label, string queryStringName, string texts, string values)
		: this(label, queryStringName, multiple: false, 12, texts, values)
	{
	}

	public NovaAdminFilterInfo(string label, string queryStringName, bool multiple, int col, string texts, string values)
	{
		Label = label;
		QueryStringName = queryStringName;
		Type = (multiple ? NovaAdminFilterType.TagsMultiple : NovaAdminFilterType.Tags);
		string[] array = texts.Split(',');
		string[] vals = values.Split(",");
		if (array.Length != vals.Length)
		{
			throw new Exception("texts.Split(',').Length != values.Split(',').Length");
		}
		Options = (from a in texts.Split(',').Select((string a, int b) => new NovaAdminItem<NovaAdminOptionsItem>(new NovaAdminOptionsItem(a.Trim(), vals[b].Trim())))
			where !a.Value.Label.IsNull()
			select a).ToArray();
		Col = col;
	}

	/// <summary>
	/// 从枚举类型自动生成筛选选项
	/// </summary>
	/// <param name="label">筛选器标签</param>
	/// <param name="queryStringName">查询参数名</param>
	/// <param name="enumType">枚举类型</param>
	/// <param name="multiple">是否多选</param>
	/// <param name="col">栅格布局列数</param>
	public NovaAdminFilterInfo(string label, string queryStringName, Type enumType, bool multiple = false, int col = 12)
	{
		if (enumType == null || !enumType.IsEnum)
		{
			throw new ArgumentException("The provided type must be an Enum.", "enumType");
		}
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		string[] names = Enum.GetNames(enumType);
		foreach (string text in names)
		{
			MemberInfo memberInfo = enumType.GetMember(text).FirstOrDefault();
			if (!(memberInfo == null))
			{
				DisplayAttribute customAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
				DescriptionAttribute customAttribute2 = memberInfo.GetCustomAttribute<DescriptionAttribute>();
				string item = customAttribute?.GetName() ?? customAttribute2?.Description ?? text;
				object value = Enum.Parse(enumType, text);
				string item2 = Convert.ToInt32(value).ToString();
				list.Add(item);
				list2.Add(item2);
			}
		}
		string text2 = string.Join(",", list);
		string text3 = string.Join(",", list2);
		Label = label;
		QueryStringName = queryStringName;
		Type = (multiple ? NovaAdminFilterType.TagsMultiple : NovaAdminFilterType.Tags);
		Col = col;
		string[] source = text2.Split(',');
		string[] vals = text3.Split(",");
		Options = source.Select((string a, int b) => new NovaAdminItem<NovaAdminOptionsItem>(new NovaAdminOptionsItem(a.Trim(), vals[b].Trim()))).ToArray();
	}

	public NovaAdminFilterInfo(string label, string queryStringName, NovaAdminFilterType type, int col)
	{
		Label = label;
		QueryStringName = queryStringName;
		Type = type;
		switch (type)
		{
		case NovaAdminFilterType.DateRange:
			Options = new NovaAdminItem<NovaAdminOptionsItem>[2]
			{
				new NovaAdminItem<NovaAdminOptionsItem>(new NovaAdminOptionsItem("Date1", "")),
				new NovaAdminItem<NovaAdminOptionsItem>(new NovaAdminOptionsItem("Date2", ""))
			};
			break;
		case NovaAdminFilterType.Text:
			Options = new NovaAdminItem<NovaAdminOptionsItem>[1]
			{
				new NovaAdminItem<NovaAdminOptionsItem>(new NovaAdminOptionsItem("Text", ""))
			};
			break;
		}
		Col = col;
	}
}
