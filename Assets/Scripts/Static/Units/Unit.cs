using System.Linq;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Utils;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Units
{
	public class Unit : StaticCollectionItemCode
	{
		[JsonProperty("type")]
		private int _unitTypeId;
		
		private UnitType _unityType;
		public UnitType UnitType => _unityType ??= Game.Static.UnitTypes.Get(_unitTypeId);
		
		[JsonProperty("dmg")]
		public float Damage { get; private set; }
		
		[JsonProperty("pdmg")]
		public float PlusDamage { get; private set; }
		
		[JsonProperty("udmg")]
		public float UpgradeDamage { get; private set; }
		
		[JsonProperty("aspeed")]
		public float AttackSpeed { get; private set; }
		
		[JsonProperty("paspeed")]
		public float PlusAttackSpeed { get; private set; }
		
		[JsonProperty("uaspeed")]
		public float UpgradeAttackSpeed { get; private set; }
		
		[JsonProperty("r")]
		public float AttackRange { get; private set; }
		
		[JsonProperty("ms")]
		public float MoveSpeed { get; private set; }
		
		[JsonProperty("sp")]
		public float Sp { get; private set; }

		[JsonProperty("attack_type")]
		public UnitAttackType AttackType { get; private set; }

		private UnitLevelCost _levelCosts;
		public UnitLevelCost LevelCosts =>
			_levelCosts ??= Game.Static.UnitLevelCosts.All.Values.FirstOrDefault(l => l.Unit == this);

		public ItemCount[] GetLevelCost(int nextLevel) => _levelCosts.GetPriceByLevel(nextLevel);

		private Item _card;
		public Item Card => _card ??= LevelCosts?.GetPriceByLevel(2)?.FirstOrDefault(c => !c.Item.IsMoney1)?.Item;

		public string PrefabPath => $"Units/{ModelId}";
		public string Name => $"unit_{Id}".Localize();
		public string Desc => $"unit_desc_{Id}".Localize();
		public string IconPath => $"img/icons/{ModelId}";
	}
}