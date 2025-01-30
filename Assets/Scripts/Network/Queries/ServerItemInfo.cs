using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries
{
	public class ServerItemInfo
	{
		[JsonProperty("c", NullValueHandling = NullValueHandling.Ignore)]
		public double? Change;

		[JsonProperty("r", NullValueHandling = NullValueHandling.Ignore)]
		public double? Count;

		public ServerItemInfo(double change, double wasCount)
		{
			Change = change;
			Count = wasCount;
		}
	}
}