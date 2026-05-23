using System;
using Newtonsoft.Json;

namespace NoAdmin.Blazor.Mvc;

public class LongJsonConverter : JsonConverter
{
	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.Value == null)
		{
			return null;
		}
		return reader.Value.ConvertTo<long>();
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(long) || objectType == typeof(long?);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteValue(value);
		}
		else
		{
			writer.WriteValue(value?.ToString() ?? "");
		}
	}
}
