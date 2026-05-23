using System;

namespace NoAdmin.Blazor.Extensions;

public static class DateExtension
{
	/// <summary>
	/// 时间戳起始日期
	/// </summary>
	public static readonly DateTime TimestampStart = new DateTime(1970, 1, 1, 0, 0, 0, 0);

	public static string GetTimeAgo(DateTime targetTime)
	{
		TimeSpan timeSpan = DateTime.Now - targetTime;
		if (timeSpan.TotalDays >= 1.0)
		{
			return $"{(int)timeSpan.TotalDays}天前";
		}
		if (timeSpan.TotalHours >= 1.0)
		{
			return $"{(int)timeSpan.TotalHours}小时前";
		}
		if (timeSpan.TotalMinutes >= 1.0)
		{
			return $"{(int)timeSpan.TotalMinutes}分钟前";
		}
		if (timeSpan.TotalSeconds >= 10.0)
		{
			return $"{(int)timeSpan.TotalMinutes}分钟前";
		}
		return "刚刚";
	}

	/// <summary>
	/// 转换为时间戳
	/// </summary>
	/// <param name="dateTime"></param>
	/// <param name="milliseconds">是否使用毫秒</param>
	/// <returns></returns>
	public static long ToTimestamp(this DateTime dateTime, bool milliseconds = false)
	{
		TimeSpan timeSpan = dateTime.ToUniversalTime() - TimestampStart;
		return (long)(milliseconds ? timeSpan.TotalMilliseconds : timeSpan.TotalSeconds);
	}

	/// <summary>
	/// 获取周几
	/// </summary>
	/// <param name="datetime"></param>
	/// <returns></returns>
	public static string GetWeekName(this DateTime datetime)
	{
		int dayOfWeek = (int)datetime.DayOfWeek;
		string[] array = new string[7] { "周日", "周一", "周二", "周三", "周四", "周五", "周六" };
		return array[dayOfWeek];
	}

	public static decimal GetUniqueScore(this DateTime createTime, long targetId)
	{
		decimal num = new DateTimeOffset(createTime).ToUnixTimeMilliseconds();
		decimal num2 = (decimal)((double)(targetId % 1000000) / 1000000.0);
		return num + num2;
	}

	/// <summary>
	/// 根据生日计算年龄（周岁）
	/// </summary>
	/// <param name="birthday">生日</param>
	/// <returns>年龄，如果生日为 null 则返回 null</returns>
	public static int? GetAge(this DateTime? birthday)
	{
		if (!birthday.HasValue)
		{
			return null;
		}
		DateTime today = DateTime.Today;
		return (today.Year * 10000 + today.Month * 100 + today.Day - (birthday.Value.Year * 10000 + birthday.Value.Month * 100 + birthday.Value.Day)) / 10000;
	}

	/// <summary>
	/// (可选) 如果你想指定特定日期作为参考点（比如计算某个时间点的年龄）
	/// </summary>
	public static int? GetAgeAt(this DateTime? birthday, DateTime referenceDate)
	{
		if (!birthday.HasValue)
		{
			return null;
		}
		return (referenceDate.Year * 10000 + referenceDate.Month * 100 + referenceDate.Day - (birthday.Value.Year * 10000 + birthday.Value.Month * 100 + birthday.Value.Day)) / 10000;
	}
}
