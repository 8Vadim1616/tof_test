using System;
using System.Linq;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Static.UnitUpgrades;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Static.Artifacts
{
	public class Artifact : StaticCollectionItemCode
	{
		/// <summary>
		/// Зелье. Добавляет +% к урону
		/// </summary>
		public const string ATTACK_INCREASE = "artifact_1";
		
		/// <summary>
		/// Лук. Добавляет +% к скорости атаки
		/// </summary>
		public const string ATTACK_SPEED_INCREASE = "artifact_2";
		
		/// <summary>
		/// Перчатки. Stun duration increased by {0}%	
		/// </summary>
		public const string STUN_DURATION_INCREASE = "artifact_3";
		
		/// <summary>
		/// Секира. Damage to bosses increased by {0}%	
		/// </summary>
		public const string BOSS_DAMAGE_INCREASE = "artifact_4";
		
		/// <summary>
		/// Свиток. Skill damage increased by {0}%
		/// </summary>
		public const string SKILL_DAMAGE_INCREASE = "artifact_5";
		
		/// <summary>
		/// Черепаха. Applies {0} movement speed slowing effect to all enemies
		/// </summary>
		public const string SLOW_EFFECT_INCREASE = "artifact_6";
		
		/// <summary>
		/// Свинья на вертеле. Applies {0} defense reduction effect to all enemies	
		/// </summary>
		public const string DEFENSE_REDUCTION = "artifact_7";
		
		/// <summary>
		/// Бита. Physical damage increased by {0}%
		/// </summary>
		public const string PHYSICAL_DAMAGE_INCREASE = "artifact_8";
		
		/// <summary>
		/// Лампа Алладина. Magic damage increased by {0}%
		/// </summary>
		public const string MAGIC_DAMAGE_INCREASE = "artifact_9";
		
		/// <summary>
		/// Мешок с деньгами. Добавляет монет при получении {0}
		/// </summary>
		public const string ADD_COINS = "artifact_10";
		
		/// <summary>
		/// Пещера. Monster count free +{0}
		/// </summary>
		public const string MONSTERS_COUNT_MAX = "artifact_11";
		
		/// <summary>
		/// Гусь. Wave paid coins +{0}
		/// </summary>
		public const string ADD_COINS_FOR_WAVE = "artifact_12";
		
		/// <summary>
		/// Бочка с порохом. When enemies are stunned, damage increased by {0}%	
		/// </summary>
		public const string DAMAGE_INCREASE_WHEN_STUN = "artifact_13";
		
		/// <summary>
		/// Дудка. Gamble with {0}% chance to summon heroes
		/// </summary>
		public const string SUMMON_CHANCE_EPIC = "artifact_14";
		
		/// <summary>
		/// When selling heroes, you get an additional +1 lucky stone with {0}% chance	
		/// </summary>
		public const string ADD_WAVE_SPECIAL_COIN_WHEN_SELL = "artifact_15";
		
		/// <summary>
		/// Сундук. When killing, you get +{0} coins	
		/// </summary>
		public const string WAVE_COINS_WHEN_KILL = "artifact_16";
		
		/// <summary>
		/// Skill activation chance {0}%
		/// </summary>
		public const string SKILL_ACTIVATION_CHANCE = "artifact_17";
		
		/// <summary>
		/// Rarity draw chance increased by {0}% from normal summons
		/// </summary>
		public const string RARE_CHANCE_INCREASE_FROM_NORMAL_SUMMON = "artifact_18";
		
		/// <summary>
		/// Movement speed increased by {0}%	
		/// </summary>
		public const string MOVE_SPEED = "artifact_19";
		
		/// <summary>
		/// Critical hit chance increased by {0}%	artifact_20
		/// </summary>
		public const string CRIT_CHANCE_INCREASE = "artifact_20";
		
		/// <summary>
		/// Give +{0} lucky stones every 10 waves	artifact_21
		/// </summary>
		public const string WAVE_SPECIAL_EVERY_10_WAVES = "artifact_21";
		
		/// <summary>
		/// Gamble with a legend Summoning chance increased by {0}%	artifact_23
		/// </summary>
		public const string SUMMON_CHANCE_LEGENDARY = "artifact_23";
		
		/// <summary>
		/// Magic damage also applies critical hit chance. Additional critical hit damage {0}%	artifact_25
		/// </summary>
		public const string MAGIC_CRIT_DAMAGE = "artifact_25";
		
		/// <summary>
		/// When critical hit occurs, gain +1 coin with {0}% chance
		/// </summary>
		public const string WAVE_COIN_WHEN_CRIT = "artifact_26";
		
		/// <summary>
		/// Pay {0}% of coins held per wave	
		/// </summary>
		public const string COINS_HELD_PER_WAVE = "artifact_27";
		
		/// <summary>
		/// Total attack power increase increases by {0}% of coins held	
		/// </summary>
		public const string ATTACK_DAMAGE_BY_COINS_HELD = "artifact_28";
		
		/// <summary>
		/// If gamble fails, grant a lower-tier hero of the corresponding gamble grade with {0}% chance	
		/// </summary>
		public const string SUMMON_LOWER_UNIT_ON_GAMBLE_CHANCE_ = "artifact_29";

		/// <summary>
		/// Maximum number of heroes held increases by +{0}	
		/// </summary>
		public const string MAX_UNITS = "artifact_30";
		
		/// <summary>
		/// If gamble fails, refund the gamble cost with {0}% chance
		/// </summary>
		public const string REFUND_SUMMON_COST_IF_FAIL_CHANCE = "artifact_31";
		
		
// Give +{0} lucky stones when clearing missions	artifact_22
// Returns {0}% of mana when using ultimate skill	artifact_24



		private const string PARAM_COLOR = "00ff00";
		
		[JsonProperty("bonus")]
		public float Bonus { get; private set; }
		
		[JsonProperty("upgrade_bonus")]
		public float UpgradeBonus { get; private set; }
		
		[JsonProperty("cost")]
		private ItemCount[][] _cost;
		
		private Item _card;
		public Item Card => _card ??= GetPriceByLevel(2)?.FirstOrDefault(c => !c.Item.IsMoney1)?.Item;
		
		public ItemCount[] GetPriceByLevel(int level)
		{
			if (level - 2 <= _cost.Length)
				return _cost[level - 2];

			return null;
		}
		
		public ItemCount[] GetLevelCost(int nextLevel) => GetPriceByLevel(Math.Max(2, nextLevel));
		
		public string IconPath => $"img/icons/{ModelId}";

		public float GetBonus(int level) => Bonus + GetUpgradeBonus(level);
		public float GetUpgradeBonus(int level) => UpgradeBonus * (level - 1);

		public string Name => $"artifact_{Id}".Localize();
		
		public string Desc(int level) => $"artifact_desc_{Id}".Localize(
			Bonus.ToKiloFormat() + $"(+{GetUpgradeBonus(level+1)})".GetColoredText(PARAM_COLOR));
		public string Desc2 => $"artifact_desc2_{Id}".Localize();
	}
}