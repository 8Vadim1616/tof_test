using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Platform.Adapter.Actions
{
	public class AdvertisingPreroll : ISocialAction
	{
		public JObject OtherParams { get; private set; }

		public AdvertisingPreroll(JObject otherParams = null)
		{
			OtherParams = otherParams;
		}
	}
}