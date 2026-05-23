using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading;

namespace NoAdmin.Blazor.Extensions;

public static class UtilConvertExtension
{
	private static int SetSetPropertyOrFieldValueSupportExpressionTreeFlag = 1;

	private static ConcurrentDictionary<Type, ConcurrentDictionary<string, Action<object, string, object>>> _dicSetPropertyOrFieldValue = new ConcurrentDictionary<Type, ConcurrentDictionary<string, Action<object, string, object>>>();

	private static ConcurrentDictionary<Type, ConcurrentDictionary<string, Func<object, string, object>>> _dicGetPropertyOrFieldValue = new ConcurrentDictionary<Type, ConcurrentDictionary<string, Func<object, string, object>>>();

	private static ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _dicGetPropertiesDictIgnoreCase = new ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>>();

	private static ConcurrentDictionary<Type, Dictionary<string, FieldInfo>> _dicGetFieldsDictIgnoreCase = new ConcurrentDictionary<Type, Dictionary<string, FieldInfo>>();

	private static ConcurrentDictionary<Type, Func<string, object>> _dicFromObject = new ConcurrentDictionary<Type, Func<string, object>>();

	/// <summary>
	/// 将序列分割成指定大小的块（Chunk）。
	/// 这是一个兼容旧版 .NET 的实现，它模拟了 .NET 6+ 中的 Enumerable.Chunk。
	/// </summary>
	/// <typeparam name="T">序列中的元素类型。</typeparam>
	/// <param name="source">源序列。</param>
	/// <param name="size">每个块的最大元素数量。</param>
	/// <returns>包含各个块的序列。</returns>
	public static IEnumerable<T[]> ChunkEnumerable<T>(this IEnumerable<T> source, int size)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (size <= 0)
		{
			throw new ArgumentOutOfRangeException("size");
		}
		using IEnumerator<T> enumerator = source.GetEnumerator();
		while (enumerator.MoveNext())
		{
			T[] chunk = new T[size];
			chunk[0] = enumerator.Current;
			int count = 1;
			while (count < size && enumerator.MoveNext())
			{
				chunk[count++] = enumerator.Current;
			}
			if (count == size)
			{
				yield return chunk;
				continue;
			}
			Array.Resize(ref chunk, count);
			yield return chunk;
		}
	}

	public static int ToInt(this object s, bool round = false)
	{
		if (s == null || s == DBNull.Value)
		{
			return 0;
		}
		if (s is bool flag)
		{
			return flag ? 1 : 0;
		}
		if (int.TryParse(s.ToString(), out var result))
		{
			return result;
		}
		if (s.GetType().IsEnum)
		{
			return (int)s;
		}
		float num = s.ToFloat();
		return round ? Convert.ToInt32(num) : ((int)num);
	}

	public static long ToLong(this object s)
	{
		if (s == null || s == DBNull.Value)
		{
			return 0L;
		}
		long.TryParse(s.ToString(), out var result);
		return result;
	}

	public static double ToMoney(this object thisValue)
	{
		if (thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out var result))
		{
			return result;
		}
		return 0.0;
	}

	public static double ToMoney(this object thisValue, double errorValue)
	{
		if (thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out var result))
		{
			return result;
		}
		return errorValue;
	}

	public static string ToString(this object thisValue)
	{
		if (thisValue != null)
		{
			return thisValue.ToString().Trim();
		}
		return "";
	}

	public static string ToString(this object thisValue, string errorValue)
	{
		if (thisValue != null)
		{
			return thisValue.ToString().Trim();
		}
		return errorValue;
	}

	public static float ToFloat(this object s, int? digits = null)
	{
		if (s == null || s == DBNull.Value)
		{
			return 0f;
		}
		float.TryParse(s.ToString(), out var result);
		if (!digits.HasValue)
		{
			return result;
		}
		return (float)Math.Round(result, digits.Value);
	}

	public static double ToDouble(this object s, int? digits = null)
	{
		if (s == null || s == DBNull.Value)
		{
			return 0.0;
		}
		double.TryParse(s.ToString(), out var result);
		if (!digits.HasValue)
		{
			return result;
		}
		return Math.Round(result, digits.Value);
	}

	public static decimal ToDecimal(this object thisValue)
	{
		if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out var result))
		{
			return result;
		}
		return 0m;
	}

	public static decimal ToDecimal(this object thisValue, decimal errorValue)
	{
		if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out var result))
		{
			return result;
		}
		return errorValue;
	}

	public static DateTime ToDateTime(this object thisValue)
	{
		DateTime result = DateTime.MinValue;
		if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out result))
		{
			result = Convert.ToDateTime(thisValue);
		}
		return result;
	}

	public static DateTime ToDateTime(this object thisValue, DateTime errorValue)
	{
		if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out var result))
		{
			return result;
		}
		return errorValue;
	}

	public static DateTime ToDateTime(this long milliseconds)
	{
		return DateExtension.TimestampStart.AddMilliseconds(milliseconds);
	}

	public static bool ToBool(this object thisValue)
	{
		bool result = false;
		if (thisValue != null && thisValue != DBNull.Value && bool.TryParse(thisValue.ToString(), out result))
		{
			return result;
		}
		return result;
	}

	public static byte ToByte(this object s)
	{
		if (s == null || s == DBNull.Value)
		{
			return 0;
		}
		byte.TryParse(s.ToString(), out var result);
		return result;
	}

	public static void SetPropertyOrFieldValue(this Type entityType, object entity, string propertyName, object value)
	{
		if (entity == null)
		{
			return;
		}
		if (entityType == null)
		{
			entityType = entity.GetType();
		}
		if (SetSetPropertyOrFieldValueSupportExpressionTreeFlag == 0)
		{
			if (UtilConvertExtension.GetPropertiesDictIgnoreCase(entityType).TryGetValue(propertyName, out PropertyInfo value2))
			{
				value2.SetValue(entity, value, null);
				return;
			}
			if (UtilConvertExtension.GetFieldsDictIgnoreCase(entityType).TryGetValue(propertyName, out FieldInfo value3))
			{
				value3.SetValue(entity, value);
				return;
			}
			throw new Exception($"The property({propertyName}) was not found in the type({FreeSqlGlobalExtensions.DisplayCsharp(entityType, true)})");
		}
		Action<object, string, object> action = null;
		try
		{
			action = _dicSetPropertyOrFieldValue.GetOrAdd(entityType, (Type et) => new ConcurrentDictionary<string, Action<object, string, object>>()).GetOrAdd(propertyName, delegate(string pn)
			{
				Type type = entityType;
				MemberInfo propertyOrFieldIgnoreCase = entityType.GetPropertyOrFieldIgnoreCase(pn);
				ParameterExpression parameterExpression = Expression.Parameter(typeof(object));
				ParameterExpression parameterExpression2 = Expression.Parameter(typeof(string));
				ParameterExpression parameterExpression3 = Expression.Parameter(typeof(object));
				ParameterExpression parameterExpression4 = Expression.Variable(type);
				List<Expression> list = new List<Expression>(new Expression[1] { Expression.Assign(parameterExpression4, Expression.TypeAs(parameterExpression, type)) });
				if (propertyOrFieldIgnoreCase != null)
				{
					list.Add(Expression.Assign(Expression.MakeMemberAccess(parameterExpression4, propertyOrFieldIgnoreCase), Expression.Convert(parameterExpression3, propertyOrFieldIgnoreCase.GetPropertyOrFieldType())));
				}
				return Expression.Lambda<Action<object, string, object>>(Expression.Block(new ParameterExpression[1] { parameterExpression4 }, list), new ParameterExpression[3] { parameterExpression, parameterExpression2, parameterExpression3 }).Compile();
			});
		}
		catch
		{
			Interlocked.Exchange(ref SetSetPropertyOrFieldValueSupportExpressionTreeFlag, 0);
			entityType.SetPropertyOrFieldValue(entity, propertyName, value);
			return;
		}
		action(entity, propertyName, value);
	}

	public static object GetPropertyOrFieldValue(this Type entityType, object entity, string propertyName)
	{
		if (entity == null)
		{
			return null;
		}
		if (entityType == null)
		{
			entityType = entity.GetType();
		}
		if (SetSetPropertyOrFieldValueSupportExpressionTreeFlag == 0)
		{
			if (UtilConvertExtension.GetPropertiesDictIgnoreCase(entityType).TryGetValue(propertyName, out PropertyInfo value))
			{
				return value.GetValue(entity);
			}
			if (UtilConvertExtension.GetFieldsDictIgnoreCase(entityType).TryGetValue(propertyName, out FieldInfo value2))
			{
				return value2.GetValue(entity);
			}
			throw new Exception($"The property({propertyName}) was not found in the type({FreeSqlGlobalExtensions.DisplayCsharp(entityType, true)})");
		}
		Func<object, string, object> func = null;
		try
		{
			func = _dicGetPropertyOrFieldValue.GetOrAdd(entityType, (Type et) => new ConcurrentDictionary<string, Func<object, string, object>>()).GetOrAdd(propertyName, delegate(string pn)
			{
				LabelTarget target = Expression.Label(typeof(object));
				Type type = entityType;
				MemberInfo propertyOrFieldIgnoreCase = entityType.GetPropertyOrFieldIgnoreCase(pn);
				ParameterExpression parameterExpression = Expression.Parameter(typeof(object));
				ParameterExpression parameterExpression2 = Expression.Parameter(typeof(string));
				ParameterExpression parameterExpression3 = Expression.Variable(type);
				List<Expression> list = new List<Expression>(new Expression[1] { Expression.Assign(parameterExpression3, Expression.TypeAs(parameterExpression, type)) });
				if (propertyOrFieldIgnoreCase == null)
				{
					list.AddRange(new Expression[2]
					{
						Expression.Return(target, Expression.Constant(null, typeof(object))),
						Expression.Label(target, Expression.Default(typeof(object)))
					});
				}
				else
				{
					list.AddRange(new Expression[2]
					{
						Expression.Return(target, Expression.MakeMemberAccess(parameterExpression3, propertyOrFieldIgnoreCase)),
						Expression.Label(target, Expression.Default(typeof(object)))
					});
				}
				return Expression.Lambda<Func<object, string, object>>(Expression.Block(new ParameterExpression[1] { parameterExpression3 }, list), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
			});
		}
		catch
		{
			Interlocked.Exchange(ref SetSetPropertyOrFieldValueSupportExpressionTreeFlag, 0);
			return entityType.GetPropertyOrFieldValue(entity, propertyName);
		}
		return func(entity, propertyName);
	}

	internal static Dictionary<string, PropertyInfo> GetPropertiesDictIgnoreCase(this Type that)
	{
		return (that == null) ? null : _dicGetPropertiesDictIgnoreCase.GetOrAdd(that, delegate
		{
			IEnumerable<PropertyInfo> enumerable = (from p in that.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				group p by p.DeclaringType).Reverse().SelectMany((IGrouping<Type, PropertyInfo> p) => p);
			Dictionary<string, PropertyInfo> dictionary = new Dictionary<string, PropertyInfo>(StringComparer.CurrentCultureIgnoreCase);
			foreach (PropertyInfo item in enumerable)
			{
				if (dictionary.TryGetValue(item.Name, out var value))
				{
					if (value.DeclaringType != item)
					{
						dictionary[item.Name] = item;
					}
				}
				else
				{
					dictionary.Add(item.Name, item);
				}
			}
			return dictionary;
		});
	}

	internal static Dictionary<string, FieldInfo> GetFieldsDictIgnoreCase(this Type that)
	{
		return (that == null) ? null : _dicGetFieldsDictIgnoreCase.GetOrAdd(that, delegate
		{
			IEnumerable<FieldInfo> enumerable = (from p in that.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				group p by p.DeclaringType).Reverse().SelectMany((IGrouping<Type, FieldInfo> p) => p);
			Dictionary<string, FieldInfo> dictionary = new Dictionary<string, FieldInfo>(StringComparer.CurrentCultureIgnoreCase);
			foreach (FieldInfo item in enumerable)
			{
				if (dictionary.ContainsKey(item.Name))
				{
					dictionary[item.Name] = item;
				}
				else
				{
					dictionary.Add(item.Name, item);
				}
			}
			return dictionary;
		});
	}

	internal static MemberInfo GetPropertyOrFieldIgnoreCase(this Type that, string name)
	{
		if (UtilConvertExtension.GetPropertiesDictIgnoreCase(that).TryGetValue(name, out PropertyInfo value))
		{
			return value;
		}
		if (UtilConvertExtension.GetFieldsDictIgnoreCase(that).TryGetValue(name, out FieldInfo value2))
		{
			return value2;
		}
		return null;
	}

	internal static Type GetPropertyOrFieldType(this MemberInfo that)
	{
		if (that is PropertyInfo propertyInfo)
		{
			return propertyInfo.PropertyType;
		}
		if (that is FieldInfo fieldInfo)
		{
			return fieldInfo.FieldType;
		}
		return null;
	}

	internal static string ToInvariantCultureToString(this object obj)
	{
		return (obj is string text) ? text : string.Format(CultureInfo.InvariantCulture, "{0}", obj);
	}

	internal static void MapSetListValue(this object[] list, Dictionary<string, Func<object[], object>> valueHandlers)
	{
		if (list == null)
		{
			return;
		}
		int num = list.Length - 2;
		int num2 = 0;
		while (num >= 0 && num2 < 2)
		{
			if (valueHandlers.TryGetValue(list[num]?.ToString(), out Func<object[], object> value))
			{
				num2++;
				if (list[num + 1] is object[] arg)
				{
					list[num + 1] = value(arg);
				}
			}
			num -= 2;
		}
	}

	internal static T MapToClass<T>(this object[] list, Encoding encoding)
	{
		if (list == null)
		{
			return default(T);
		}
		if (list.Length % 2 != 0)
		{
			throw new ArgumentException("list");
		}
		Type typeFromHandle = typeof(T);
		T val = (T)FreeSqlGlobalExtensions.CreateInstanceGetDefaultValue(typeFromHandle);
		for (int i = 0; i < list.Length; i += 2)
		{
			string name = list[i].ToString().Replace("-", "_");
			MemberInfo propertyOrFieldIgnoreCase = typeFromHandle.GetPropertyOrFieldIgnoreCase(name);
			if (!(propertyOrFieldIgnoreCase == null) && list[i + 1] != null)
			{
				typeFromHandle.SetPropertyOrFieldValue(val, propertyOrFieldIgnoreCase.Name, propertyOrFieldIgnoreCase.GetPropertyOrFieldType().FromObject(list[i + 1], encoding));
			}
		}
		return val;
	}

	internal static Dictionary<string, T> MapToHash<T>(this object[] list, Encoding encoding)
	{
		if (list == null)
		{
			return null;
		}
		if (list.Length % 2 != 0)
		{
			throw new ArgumentException("Array list length is not even");
		}
		Dictionary<string, T> dictionary = new Dictionary<string, T>();
		for (int i = 0; i < list.Length; i += 2)
		{
			string key = list[i].ToInvariantCultureToString();
			if (!dictionary.ContainsKey(key))
			{
				object obj = list[i + 1];
				if (obj == null)
				{
					dictionary.Add(key, default(T));
				}
				else
				{
					dictionary.Add(key, (obj is T val) ? val : ((T)typeof(T).FromObject(obj, encoding)));
				}
			}
		}
		return dictionary;
	}

	internal static List<KeyValuePair<string, T>> MapToKvList<T>(this object[] list, Encoding encoding)
	{
		if (list == null)
		{
			return null;
		}
		if (list.Length % 2 != 0)
		{
			throw new ArgumentException("Array list length is not even");
		}
		List<KeyValuePair<string, T>> list2 = new List<KeyValuePair<string, T>>();
		for (int i = 0; i < list.Length; i += 2)
		{
			string key = list[i].ToInvariantCultureToString();
			object obj = list[i + 1];
			if (obj == null)
			{
				list2.Add(new KeyValuePair<string, T>(key, default(T)));
			}
			else
			{
				list2.Add(new KeyValuePair<string, T>(key, (obj is T val) ? val : ((T)typeof(T).FromObject(obj, encoding))));
			}
		}
		return list2;
	}

	internal static List<T> MapToList<T>(this object[] list, Func<object, object, T> selector)
	{
		if (list == null)
		{
			return null;
		}
		if (list.Length % 2 != 0)
		{
			throw new ArgumentException("Array list length is not even");
		}
		List<T> list2 = new List<T>();
		for (int i = 0; i < list.Length; i += 2)
		{
			T val = selector(list[i], list[i + 1]);
			if (val != null)
			{
				list2.Add(val);
			}
		}
		return list2;
	}

	public static T ConvertTo<T>(this object value)
	{
		return (T)typeof(T).FromObject(value);
	}

	private static object FromObject(this Type targetType, object value, Encoding encoding = null)
	{
		if (targetType == typeof(object))
		{
			return value;
		}
		if (encoding == null)
		{
			encoding = Encoding.UTF8;
		}
		bool flag = value == null;
		Type valueType = (flag ? typeof(string) : value.GetType());
		if (valueType == targetType)
		{
			return value;
		}
		if (valueType == typeof(byte[]))
		{
			if (targetType == typeof(Guid))
			{
				byte[] array = value as byte[];
				Guid result;
				return Guid.TryParse(BitConverter.ToString(array, 0, Math.Min(array.Length, 36)).Replace("-", ""), out result) ? result : Guid.Empty;
			}
			if (targetType == typeof(Guid?))
			{
				byte[] array2 = value as byte[];
				Guid result2;
				return Guid.TryParse(BitConverter.ToString(array2, 0, Math.Min(array2.Length, 36)).Replace("-", ""), out result2) ? new Guid?(result2) : ((Guid?)null);
			}
		}
		if (targetType == typeof(string))
		{
			if (flag)
			{
				return null;
			}
			if (valueType == typeof(byte[]))
			{
				return encoding.GetString(value as byte[]);
			}
			return value.ToInvariantCultureToString();
		}
		if (targetType == typeof(byte[]))
		{
			if (flag)
			{
				return null;
			}
			if (valueType == typeof(Guid) || valueType == typeof(Guid?))
			{
				byte[] array3 = new byte[16];
				string text = ((Guid)value).ToString("N");
				for (int i = 0; i < text.Length; i += 2)
				{
					array3[i / 2] = byte.Parse($"{text[i]}{text[i + 1]}", NumberStyles.HexNumber);
				}
				return array3;
			}
			return encoding.GetBytes(value.ToInvariantCultureToString());
		}
		if (targetType.IsArray && value is Array array4)
		{
			Type elementType = targetType.GetElementType();
			int length = array4.Length;
			Array array5 = Array.CreateInstance(elementType, length);
			for (int j = 0; j < length; j++)
			{
				array5.SetValue(elementType.FromObject(array4.GetValue(j), encoding), j);
			}
			return array5;
		}
		Func<string, object> orAdd = _dicFromObject.GetOrAdd(targetType, delegate(Type tt)
		{
			if (tt == typeof(object))
			{
				return (string vs) => vs;
			}
			if (tt == typeof(string))
			{
				return (string vs) => vs;
			}
			if (tt == typeof(char[]))
			{
				return (string vs) => vs?.ToCharArray();
			}
			if (tt == typeof(char))
			{
				return (string vs) => vs?.ToCharArray(0, 1).FirstOrDefault() ?? '\0';
			}
			if (tt == typeof(bool))
			{
				return delegate(string vs)
				{
					if (vs == null)
					{
						return false;
					}
					string text2 = vs.ToLower();
					string text3 = text2;
					return (text3 == "true" || text3 == "1") ? ((object)true) : ((object)false);
				};
			}
			if (tt == typeof(bool?))
			{
				return delegate(string vs)
				{
					if (vs != null)
					{
						switch (vs.ToLower())
						{
						case "true":
						case "1":
							return true;
						case "false":
						case "0":
							return false;
						default:
							return (object)null;
						}
					}
					return false;
				};
			}
						if (tt == typeof(byte))
			{
				return (string vs) => (vs != null) ? (byte.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? result : 0) : 0;
			}
			if (tt == typeof(byte?))
			{
				return (string vs) => (vs == null) ? ((byte?)null) : (byte.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? new byte?(result) : ((byte?)null));
			}
			if (tt == typeof(decimal))
			{
				return (string vs) => (vs == null) ? 0m : (decimal.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? result : 0m);
			}
			if (tt == typeof(decimal?))
			{
				return (string vs) => (vs == null) ? ((decimal?)null) : (decimal.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? new decimal?(result) : ((decimal?)null));
			}
			if (tt == typeof(double))
			{
				return (string vs) => (vs == null) ? 0.0 : (double.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? result : 0.0);
			}
			if (tt == typeof(double?))
			{
				return (string vs) => (vs == null) ? ((double?)null) : (double.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? new double?(result) : ((double?)null));
			}
			if (tt == typeof(float))
			{
				return (string vs) => (vs == null) ? 0f : (float.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? result : 0f);
			}
			if (tt == typeof(float?))
			{
				return (string vs) => (vs == null) ? ((float?)null) : (float.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? new float?(result) : ((float?)null));
			}
			if (tt == typeof(int))
			{
				return (string vs) => (vs != null) ? (int.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? result : 0) : 0;
			}
			if (tt == typeof(int?))
			{
				return (string vs) => (vs == null) ? ((int?)null) : (int.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? new int?(result) : ((int?)null));
			}
			if (tt == typeof(long))
			{
				return (string vs) => (vs == null) ? 0 : (long.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? result : 0);
			}
			if (tt == typeof(long?))
			{
				return (string vs) => (vs == null) ? ((long?)null) : (long.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? new long?(result) : ((long?)null));
			}
			if (tt == typeof(sbyte))
			{
				return (string vs) => (vs != null) ? (sbyte.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? result : 0) : 0;
			}
			if (tt == typeof(sbyte?))
			{
				return (string vs) => (vs == null) ? ((sbyte?)null) : (sbyte.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? new sbyte?(result) : ((sbyte?)null));
			}
			if (tt == typeof(short))
			{
				return (string vs) => (vs != null) ? (short.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? result : 0) : 0;
			}
			if (tt == typeof(short?))
			{
				return (string vs) => (vs == null) ? ((short?)null) : (short.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? new short?(result) : ((short?)null));
			}
			if (tt == typeof(uint))
			{
				return (string vs) => (vs != null) ? (uint.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? result : 0u) : 0u;
			}
			if (tt == typeof(uint?))
			{
				return (string vs) => (vs == null) ? ((uint?)null) : (uint.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? new uint?(result) : ((uint?)null));
			}
			if (tt == typeof(ulong))
			{
				return (string vs) => (vs == null) ? 0 : (ulong.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? result : 0);
			}
			if (tt == typeof(ulong?))
			{
				return (string vs) => (vs == null) ? ((ulong?)null) : (ulong.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? new ulong?(result) : ((ulong?)null));
			}
			if (tt == typeof(ushort))
			{
				return (string vs) => (vs != null) ? (ushort.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? result : 0) : 0;
			}
			if (tt == typeof(ushort?))
			{
				return (string vs) => (vs == null) ? ((ushort?)null) : (ushort.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? new ushort?(result) : ((ushort?)null));
			}
			if (tt == typeof(DateTime))
			{
				return (string vs) => (vs == null) ? DateTime.MinValue : (DateTime.TryParse(vs, out var result) ? result : DateTime.MinValue);
			}
			if (tt == typeof(DateTime?))
			{
				return (string vs) => (vs == null) ? ((DateTime?)null) : (DateTime.TryParse(vs, out var result) ? new DateTime?(result) : ((DateTime?)null));
			}
			if (tt == typeof(DateTimeOffset))
			{
				return (string vs) => (vs == null) ? DateTimeOffset.MinValue : (DateTimeOffset.TryParse(vs, out var result) ? result : DateTimeOffset.MinValue);
			}
			if (tt == typeof(DateTimeOffset?))
			{
				return (string vs) => (vs == null) ? ((DateTimeOffset?)null) : (DateTimeOffset.TryParse(vs, out var result) ? new DateTimeOffset?(result) : ((DateTimeOffset?)null));
			}
			if (tt == typeof(TimeSpan))
			{
				return (string vs) => (vs == null) ? TimeSpan.Zero : (TimeSpan.TryParse(vs, out var result) ? result : TimeSpan.Zero);
			}
			if (tt == typeof(TimeSpan?))
			{
				return (string vs) => (vs == null) ? ((TimeSpan?)null) : (TimeSpan.TryParse(vs, out var result) ? new TimeSpan?(result) : ((TimeSpan?)null));
			}
			if (tt == typeof(Guid))
			{
				return (string vs) => (vs == null) ? Guid.Empty : (Guid.TryParse(vs, out var result) ? result : Guid.Empty);
			}
			if (tt == typeof(Guid?))
			{
				return (string vs) => (vs == null) ? ((Guid?)null) : (Guid.TryParse(vs, out var result) ? new Guid?(result) : ((Guid?)null));
			}
			if (tt == typeof(BigInteger))
			{
				return (string vs) => (vs == null) ? ((BigInteger)0) : (BigInteger.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? result : ((BigInteger)0));
			}
			if (tt == typeof(BigInteger?))
			{
				return (string vs) => (vs == null) ? ((BigInteger?)null) : (BigInteger.TryParse(vs, NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat, out var result) ? new BigInteger?(result) : ((BigInteger?)null));
			}
			if (FreeSqlGlobalExtensions.NullableTypeOrThis(tt).IsEnum)
			{
				Type tttype = FreeSqlGlobalExtensions.NullableTypeOrThis(tt);
				object ttdefval = FreeSqlGlobalExtensions.CreateInstanceGetDefaultValue(tt);
				return (string vs) => string.IsNullOrWhiteSpace(vs) ? ttdefval : Enum.Parse(tttype, vs, ignoreCase: true);
			}
			Type localTargetType = targetType;
			Type localValueType = valueType;
			return delegate(string vs)
			{
				if (vs != null)
				{
					throw new NotSupportedException("convert failed " + FreeSqlGlobalExtensions.DisplayCsharp(localValueType, true) + " -> " + FreeSqlGlobalExtensions.DisplayCsharp(localTargetType, true));
				}
				return (object)null;
			};
		});
		if (flag)
		{
			return orAdd(null);
		}
		if (valueType == typeof(byte[]))
		{
			return orAdd(encoding.GetString(value as byte[]));
		}
		Type type = FreeSqlGlobalExtensions.NullableTypeOrThis(valueType);
		if (type.IsEnum && FreeSqlGlobalExtensions.IsNumberType(targetType))
		{
			return orAdd(Convert.ChangeType(value, type.GetEnumUnderlyingType()).ToInvariantCultureToString());
		}
		return orAdd(value.ToInvariantCultureToString());
	}
}
