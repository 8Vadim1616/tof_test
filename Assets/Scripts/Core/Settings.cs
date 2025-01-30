using Assets.Scripts.BuildSettings;
using Assets.Scripts.Localization;
using Assets.Scripts.Platform.Mobile.Advertising;
using Assets.Scripts.Static.Items;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Assets.Scripts.Static.Units;
using Gameplay;
using Newtonsoft.Json.Serialization;

namespace Assets.Scripts.Core
{
	public class Settings
	{
		[JsonProperty("log_delay")]
		public int LogDelay = 4;

		[JsonProperty("log_save_delay")]
		public int LogSaveDelay = 5;

		[JsonProperty("start_user_items")]
		public ItemCount[] StartItems;

		[JsonProperty("gdpr_term1")]
		public string GDPR_TERM1_HREF;

		[JsonProperty("gdpr_term2")]
		public string GDPR_TERM2_HREF;

		[JsonProperty("max_local_logs")]
		public int MAX_LOCAL_LOGS
		{
			get => GameLogger.MaxLogs;
			set => GameLogger.MaxLogs = value;
		}

		[JsonProperty("delete_confirm_langs")]
		private Dictionary<string, JObject> _deleteConfirmLangs;

		public Dictionary<string, string> DeleteConfirmLangs;
		public string GetDeleteConfirmStringForLang(string lang)
		{
			if (DeleteConfirmLangs.TryGetValue(lang, out string needConfirm))
				return needConfirm;

			if (DeleteConfirmLangs.TryGetValue(LOCALE.EN, out string needConfirmEn))
				return needConfirmEn;

			return "OK";
		}

		[JsonProperty("inactive_time")]
		public int INACTIVE_TIME = 1; // 1 секунда

		[JsonProperty("inactive_reload_menu")]
		public int INACTIVE_RELOAD_MENU = 0;

		[JsonProperty("inactive_reload_game")]
		public int INACTIVE_RELOAD_GAME = 0;

		[JsonProperty("grp")]
		public int Group;

		[JsonProperty("notify_start_time", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int NotifyStart = 8;
		[JsonProperty("notify_end_time", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int NotifyEnd = 22;
		[JsonProperty("notify_min_sec", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int NotifyMinSeconds = 600;
		[JsonProperty("notify_start_time_separate", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool NotifyStartSeparate = false;

		[JsonProperty("show_gdpr_default")]
		public bool GDPRDefaultValue = true;

		[JsonProperty("need_show_loader_when_query_locks_screen")]
		public bool NEED_SHOW_LOADER_WHEN_QUERY_LOCKS_SCREEN = true;

	    [JsonProperty("need_show_save_progress_in_preloader")]
		public bool NEED_SHOW_SAVE_PROGRESS_IN_PRELOADER;

		[JsonProperty("need_show_save_progress_in_preloader_reg")]
		public bool NEED_SHOW_SAVE_PROGRESS_IN_PRELOADER_REG;

		[JsonProperty("iap_prefix")]
		public string IAPPrefix = GameConsts.IAPPrefixDefault;
		
		[JsonProperty("iap_prefix_old")]
		public string IAPPrefixOld = null;
		
		[JsonProperty("ad_banner_type")]
		public AdSizeType BannerType = AdSizeType.AnchoredAdaptive;

		[JsonProperty("can_delete_progress")]
		public bool CAN_DELETE_PROGRESS = true;

		[JsonProperty("banner_reload_delay")]
		public int BannerReloadDelay = 30;

		[JsonProperty("place_hint_dur")]
		public float PlaceHintDuration = 4f;

		[JsonProperty("check_free_space")]
		public bool CheckFreeSpace = true;
		
		[JsonProperty("bank_time")]
		public float BankLoadWaitTime = 5f;

		[JsonProperty("rate_us_min_level")]
		public int RATE_US_MIN_LEVEL = 3;

		[JsonProperty("banner_show_delay")]
		public float BannerShowDelay = 1f;

		[JsonProperty("summon_cost_base")]
		public int SummonCostBase = 20;
						
		[JsonProperty("summon_cost_inc")]
		public int SummonCostInc = 2;
		
		[JsonProperty("units_max_count")]
		public int UnitsMaxCount = 20;
		
		[JsonProperty("wave_coin_start_count")]
		public int WaveCoinStartCount = 100;
		
		[JsonProperty("wave_special_coin_start_count")]
		public int WaveSpecialCoinStartCount = 0;

		[JsonProperty("move_units_time")]
		public float MoveUnitsTime = 0.1f;
		
		[JsonProperty("crit_chance")]
		public float CritChance = 5f;
		
		[JsonProperty("crit_damage")]
		public float CritDamage = 150f;

		[JsonProperty("summon_upgrade_cost")]
		public ItemCount SummonUpgradeCost;
		
		[JsonProperty("common_rare_upgrade_cost")]
		public ItemCount CommonRareUpgradeCost;
		
		[JsonProperty("common_rare_upgrade_cost_inc")]
		public int CommonRareUpgradeCostInc;
		
		[JsonProperty("epic_upgrade_cost")]
		public ItemCount EpicUpgradeCost;
		
		[JsonProperty("epic_upgrade_cost_inc")]
		public int EpicUpgradeCostInc;
		
		[JsonProperty("legendary_mythic_upgrade_cost")]
		public ItemCount LegendaryMythicUpgradeCost;
		
		[JsonProperty("legendary_mythic_upgrade_cost_inc")]
		public int LegendaryMythicUpgradeCostInc;

		[JsonProperty("summon_rare_prob")]
		public float SummonRareProb;
		
		[JsonProperty("summon_epic_prob")]
		public float SummonEpicProb;
		
		[JsonProperty("summon_legend_prob")]
		public float SummonLegendProb;
		
		[JsonProperty("summon_rare_cost")]
		public ItemCount SummonRareCost;
		
		[JsonProperty("summon_epic_cost")]
		public ItemCount SummonEpicCost;
		
		[JsonProperty("summon_legend_cost")]
		public ItemCount SummonLegendCost;

		[JsonProperty("energy_level_start")]
		public ItemCount EnergyForLevel;

		public float GetBaseSummonUnitProbability(string type)
		{
			if (type == UnitType.RARE)
				return SummonRareProb;
			if (type == UnitType.EPIC)
				return SummonEpicProb;
			if (type == UnitType.LEGEND)
				return SummonLegendProb;

			return 0;
		}

		public ItemCount GetBaseSummonUnitCost(string type)
		{
			if (type == UnitType.RARE)
				return SummonRareCost;
			if (type == UnitType.EPIC)
				return SummonEpicCost;
			if (type == UnitType.LEGEND)
				return SummonLegendCost;

			return null;
		}
		
		[JsonProperty("monsters_max_count")]
		public int MonstersMaxCount { get; private set; }
		
		public int MonstersRedCount = 90;
		public int MonstersYellowCount = 70;
		
		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			DeleteConfirmLangs = new Dictionary<string, string>();
			if (_deleteConfirmLangs != null)
				foreach (var kv in _deleteConfirmLangs)
				{
					if (kv.Value.TryGetValue("confirm", out JToken val))
						DeleteConfirmLangs.Add(kv.Key, val.ToObject<string>());
				}
		}
	}
}