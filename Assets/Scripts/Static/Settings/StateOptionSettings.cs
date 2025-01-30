using Newtonsoft.Json;

namespace Assets.Scripts.Static.Settings
{
	public class StateOptionSettings
	{
		[JsonProperty("int_interval", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long InterstitialInterval;
	}
}
