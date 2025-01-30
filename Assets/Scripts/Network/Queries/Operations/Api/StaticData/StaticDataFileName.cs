using System.Collections.Generic;
using Assets.Scripts.Localization;
using Assets.Scripts.Utils;

namespace Assets.Scripts.Network.Queries.Operations.Api.StaticData
{
	public static class StaticDataFileName
    {
        public const string ITEMS = "items";
        public const string LEVELS = "user_levels";
        public const string SETTINGS = "sett";
        public const string TOWER_ITEMS = "tower_items";
        /**
		
		public const string DROPS = "drops";
		public const string SHOP = "shop";
		public const string BANK = "bank";
		public const string BANK_PACKS = "bank_packs";
		
		public const string UNIT_TYPES = "unit_types";
		public const string UNITS = "units";
		public const string MONSTERS = "monsters";
		public const string WAVES = "waves";
		public const string SUMMONS = "summon";
		public const string SKILL_PARAMETERS = "skill_parameters";
		public const string SKILLS = "skills";
		public const string UNIT_UPGRADE_TYPES = "upgrade_types";
		public const string UNIT_UPGRADES = "unit_upgrades";
		public const string UNIT_LEVEL_COSTS = "unit_level_cost";
		public const string ARTIFACTS = "artifacts";
		*/
		public static string LANG => "lang_" + GameLocalization.GetValidLocale();

        public static List<string> ALL
        {
            get
            {
                var result = typeof(StaticDataFileName).GetAllPublicStaticFieldsValues<string>();
                result.Add(LANG);
                return result;
            }
        }
        
        public static List<string> GetAllForBuild()
        {
            var result = typeof(StaticDataFileName).GetAllPublicStaticFieldsValues<string>();
            return result;
        }
    }
}