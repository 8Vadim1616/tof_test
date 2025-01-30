using System.Collections.Generic;
using System.Runtime.Serialization;
using Assets.Scripts.Static.HR;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Bank
{
	public class StaticBankItem : StaticCollectionItem
	{
		[JsonProperty("pos_type", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string PosType { get; private set; }

		[JsonProperty("icon", DefaultValueHandling = DefaultValueHandling.Ignore)] [CanBeNull]
		public string Icon { get; private set; }

		[JsonProperty("weight_usd", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int WeightUsd { get; private set; }

		[JsonProperty("tier", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Tier { get; private set; }

		[JsonProperty("sort", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int Sort { get; private set; }

		[JsonProperty("uni_pos", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string UniversalPosition { get; private set; }

		[JsonProperty("sn", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Sn { get; private set; }

		[JsonProperty("sn_data", DefaultValueHandling = DefaultValueHandling.Ignore)]
		private Dictionary<string, StaticBankItemSnData> SnDatas { get; set; }
		public StaticBankItemSnData SnData { get; set; }

		[JsonProperty("close_actions")]
		public string CloseActions { get; private set; }

		public bool SnAlowed { get; private set; }

		public List<ItemCountFormula> Items
		{
			get
			{
				if (SnData == null)
				{
					GameLogger.error("SnData is null");
					return null;
				}

				if (SnData.DropId > 0)
				{
					var drop = Game.Static?.Drops?.GetDrop(SnData.DropId);

					if (drop != null)
						return drop.GetItems(true, true);
					else
						GameLogger.warning("No drop " + SnData.DropId + " for bank item id " + Id);
				}

				return SnData.Items;
			}
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			SnAlowed = SnDatas?.ContainsKey(Game.Social.Network) == true
				/*|| !UniversalPosition.IsNullOrEmpty()*/;

			if (SnDatas == null || !SnDatas.ContainsKey(Game.Social.Network))
			{
				SnData = new StaticBankItemSnData();
				return;
			}

			SnData = SnDatas[Game.Social.Network];
		}

		public class StaticBankItemSnData
		{
			[JsonProperty("items", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public List<ItemCountFormula> Items { get; private set; }

			[JsonProperty("drop", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public int DropId { get; private set; }

			[JsonProperty("sale", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public int Sale { get; private set; }

			[JsonProperty("best", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public int Best { get; private set; }

			[JsonProperty("old_value", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public int OldValue { get; private set; }

			[JsonProperty("period", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public int Period { get; private set; }
		}
	}
}