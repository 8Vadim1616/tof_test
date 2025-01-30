using System.Collections.Generic;
using Assets.Scripts.Static.Items;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.ServerObjects
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class ServerDrop
    {
		[JsonProperty("action", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Action { get; set; }

		[JsonProperty("drop", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public List<ItemCount> Items { get; set; }

		[JsonProperty("hide", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool IsHidden;

		[JsonProperty("add", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool NeedAddToUserItems;
	}
}
