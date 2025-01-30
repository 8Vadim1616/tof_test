using System;
using System.Collections.Generic;
using Assets.Scripts.Static.Shop;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Network.Queries.ServerObjects
{
	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class ServerUserBank
	{
		[JsonProperty("b_weight", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? BuyWeight;

		[JsonProperty("b_weight_sum", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? BuyWeightSum;
		
		[JsonProperty("bought", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public List<int> BoughtBankPos;

		[JsonIgnore]
		public int FixedBuyWeightSum => Mathf.Max(BuyWeightSum ?? 0, BuyWeight ?? 0);

		[JsonProperty("pay_count", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? PayCount;

		[JsonProperty("level_last_pay", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long? LevelLastPay;

		[JsonIgnore]
		public bool IsPayer => BuyWeight.HasValue && BuyWeight > 0;
		
		[JsonProperty("shop_timers", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<int, long> ShopTimers { get; set; }

		[JsonIgnore]
		public int FixedPayCount
		{
			get
			{
				if (PayCount.HasValue && PayCount.Value > 0)
					return PayCount.Value;

				if (BuyWeight.HasValue && BuyWeight.Value > 0)
					return 1;

				return 0;
			}
		}
	}
}