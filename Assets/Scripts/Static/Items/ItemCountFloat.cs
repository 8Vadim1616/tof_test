using Newtonsoft.Json;

namespace Assets.Scripts.Static.Items
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ItemCountFloat
	{
		[JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int ItemId { get; set; }

		[JsonProperty("cnt", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public float Count { get; set; }
		
		[JsonIgnore]
		public virtual Item Item => Game.Static.Items[ItemId];
	}
}