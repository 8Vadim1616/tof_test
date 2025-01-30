using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Utils
{
	public class NoLogsConverter : JsonConverter
	{
		public static bool UseForConvert = true;
		
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, UseForConvert ? value : null);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return JObject.Load(reader).ToObject(objectType);
		}

		public override bool CanConvert(Type objectType)
		{
			return true;
		}
	}
}