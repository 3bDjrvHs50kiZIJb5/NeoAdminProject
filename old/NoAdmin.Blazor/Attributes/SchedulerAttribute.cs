using System;
using FreeScheduler;

namespace NoAdmin.Blazor.Attributes;


[AttributeUsage(AttributeTargets.Method)]
public class SchedulerAttribute : Attribute
{
	public string Name { get; set; }

	public TaskInterval Interval { get; set; }

	public string Argument { get; set; }

	public int Round { get; set; } = -1;

	public TaskStatus Status { get; set; }

	public SchedulerAttribute(string name)
	{
		Name = name;
	}

	public SchedulerAttribute(string name, string cron)
	{
		Name = name;
		Interval = (TaskInterval)21;
		Argument = cron;
	}
}