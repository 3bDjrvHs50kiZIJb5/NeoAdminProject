using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace NoAdmin.Blazor.Mvc;

public static class EnumExtensions
{
	/// <summary>
	/// 获取枚举信息(枚举名称、描述、值)
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static string GetEnumDesc(this Enum value)
	{
		Type type = value.GetType();
		List<string> list = Enum.GetNames(type).ToList();
		FieldInfo[] fields = type.GetFields();
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (list.Contains(fieldInfo.Name) && !(value.ToString() != fieldInfo.Name))
			{
				DescriptionAttribute[] array2 = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);
				if (array2.Length != 0)
				{
					return array2[0].Description;
				}
				return "";
			}
		}
		return "";
	}
}
