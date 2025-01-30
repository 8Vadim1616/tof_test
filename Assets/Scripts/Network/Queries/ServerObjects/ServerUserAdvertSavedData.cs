using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.ServerObjects
{
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class ServerUserAdvertSavedData
	{
		[JsonProperty("buy_moves", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? BuyMovesWatched;
	}
}