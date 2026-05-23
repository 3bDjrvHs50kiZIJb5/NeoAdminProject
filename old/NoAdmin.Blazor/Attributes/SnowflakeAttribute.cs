using System;

namespace NoAdmin.Blazor.Attributes;


[AttributeUsage(AttributeTargets.Property)]
public class SnowflakeAttribute : Attribute
{
	public bool Enable { get; set; } = true;
}