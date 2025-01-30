using System.Collections.Generic;
using Assets.Scripts.Static.HR;
using Assets.Scripts.Static.Items;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Bank
{
	public class StaticBankPack : StaticCollectionItem
	{
		[JsonProperty("slevel", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int SLevel { get; private set; }

		[JsonProperty("elevel", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int ELevel { get; private set; }

		[JsonProperty("sweight", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? SWeight { get; private set; }

		[JsonProperty("eweight", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? EWeight { get; private set; }

		[JsonProperty("bookmark", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Bookmark { get; private set; }

		[JsonProperty("group", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Group { get; private set; }

		[JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Type { get; private set; }

		[JsonProperty("icon", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Icon { get; private set; }

		[JsonProperty("bank_pos", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? BankPos { get; private set; }
		
		[JsonProperty("upgrades", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? Upgrades { get; private set; }

		[JsonProperty("advert_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? AdvertId { get; private set; }
		
		[JsonProperty("sort", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? Sort { get; private set; }
		
		[JsonProperty("sort_all", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? SortAll { get; private set; }

		[JsonProperty("shop_pos", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? ShopPos { get; private set; }
		
		[JsonProperty("sale", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? Sale { get; private set; }

		[CanBeNull]
		[JsonProperty("items", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public List<ItemCountFormula> BuyItems { get; set; }

		[JsonProperty("buy_pos", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int[] PosMustBeBought;
		
		[JsonProperty("no_buy_pos", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int[] PosMustNotBeBought;
		
		[JsonProperty("event", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int eventId;
	}
}