using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Platform.Adapter.Actions
{
	public class Advertising : ISocialAction
	{
		public static string PREPARE = "prepareMidroll";
		public static string SHOW = "showMidroll";

		public JObject OtherParams { get; private set; }

		public string State { get; private set; }

		public Advertising(string state, JObject otherParams = null)
		{
			State = state;
			OtherParams = otherParams;
		}
	}
}