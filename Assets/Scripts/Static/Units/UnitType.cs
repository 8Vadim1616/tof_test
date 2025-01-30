using Assets.Scripts.Static.Items;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Units
{
	public class UnitType : StaticCollectionItemCode
	{
		public const string COMMON = "Common";
		public const string RARE = "Rare";
		public const string EPIC = "Epic";
		public const string LEGEND = "Legend";
		public const string MYTHICAL = "Mythical";

		public bool CanMerge => MergeType != null;
		
		[JsonProperty("sell")]
		public ItemCount SellReward { get; private set; }

		public UnitType GetLowerType()
		{
			switch (ModelId)
			{
				case RARE : return Game.Static.UnitTypes.Common;
				case EPIC : return Game.Static.UnitTypes.Rare;
				case LEGEND : return Game.Static.UnitTypes.Epic;
				
				default : return null;
			}
		}

		public UnitType MergeType
		{
			get
			{
				switch (ModelId)
				{
					case COMMON : return Game.Static.UnitTypes.Rare;
					case RARE : return Game.Static.UnitTypes.Epic;
					case EPIC : return Game.Static.UnitTypes.Legend;
					default : return null;
				}
			}
		}
	}
}