using System;

namespace NoAdmin.Blazor.Attributes;


[AttributeUsage(AttributeTargets.Property)]
public class UuidV7Attribute : Attribute
{
	public bool Enable { get; set; } = true;
}