using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.User.MetaPayments
{
	public class MetaPaymentConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var jObj = JObject.Load(reader);
			var type = MetaPayment.GetPaymentType(jObj["type"].ToString());
			if (type != null)
				return jObj.ToObject(type, serializer);

			return null;
		}

		public override bool CanConvert(Type objectType)
		{
			return !objectType.IsAbstract && objectType.IsAssignableFrom(typeof(MetaPayment));
		}
	}
}