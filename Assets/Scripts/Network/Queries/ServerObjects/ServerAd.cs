using System.Collections.Generic;
using Assets.Scripts.Static.Items;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.ServerObjects
{
	public class ServerAd
	{
		public const string REWARD = "reward";
		public const string REWARDED_INTERSTITIAL = "rewarded_interstitial";
		public const string INTERSTITIAL = "interstitial";
		public const string OFFERWALL = "offerwall";
		public const string BANNER = "banner";
		public const string APP_OPEN = "app_open";
		public const string NATIVE_BANNER = "nbanner";

		[JsonProperty("tm", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? Time;

		[JsonProperty("cnt", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? Count;

		[JsonProperty("partners", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public ServerAdPartner[] Partners;

		[JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string AdvertType;
	}

	public class ServerAdPartner
	{
		[JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int PointId;

		[JsonProperty("partner", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Name;

		[JsonProperty("available", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool Available;

		[JsonProperty("reward", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public ServerAdReward Reward;
	}

	public class ServerAdReward
	{
		[JsonProperty("items", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public List<ItemCount> Items;

		[JsonProperty("transl_key", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string TranslKey;

		[JsonProperty("transl_args", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string[] TranslArgs;
	}
}