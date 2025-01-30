using System.Collections.Generic;
using Assets.Scripts.Static.Artifacts;
using Assets.Scripts.Static.Bank;
using Assets.Scripts.Static.Drops;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Static.Levels;
using Assets.Scripts.Static.Monsters;
using Assets.Scripts.Static.Shop;
using Assets.Scripts.Static.Skills;
using Assets.Scripts.Static.Tower;
using Assets.Scripts.Static.Units;
using Assets.Scripts.Static.UnitUpgrades;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations.Api.StaticData
{
	public struct FileWithVersion
	{
		[JsonProperty("name")] public string Name;
		[JsonProperty("ver")] public long Ver;
	}

	public struct FileWithVersionData
	{
		[JsonProperty("data")]
		public object Data;

		[JsonProperty("ver")]
		public long? Ver;
	}

	//public class StaticDataOperation //: BaseApiOperation<StaticDataOperation.StaticDataRequest, StaticDataResponse>
	//{
		//	public override bool NeedLog => false;

		

		//       public StaticDataOperation(List<string> files, int grp)
		//       {
		//           var obj = new List<FileWithVersion>();

		//           foreach (var file in files)
		//		{
		//			var ver = FileResourcesLoader.GetGroup(grp).GetMax(file);
		//			obj.Add(new FileWithVersion { Name = file, Ver = ver });
		//           }

		//		SetRequestObject(new StaticDataRequest() { Files = obj, Group = grp });
		//       }

		//       internal override void OnResponse(StaticDataResponse response)
		//       {
		//           base.OnResponse(response);
		//       }

		//       public class StaticDataRequest : BaseApiRequest
		//       {
		//           [JsonProperty("files")] public List<FileWithVersion> Files { get; set; }

		//           public StaticDataRequest() : base("getdata") { }
		//       }
	//}

	public class StaticDataResponse : BaseApiResponse
    {
        [JsonProperty("files")] public StaticDataFiles Files;
        [JsonProperty("versions")] public Dictionary<string, long> Versions;
		[JsonProperty("model_data")] public ModelVersionData ModelVersionData;
    }
    
	public class ModelVersionData
	{
		[JsonProperty("ver")] public long Version;
		[JsonProperty("time")] public long Time;
	}

    public class StaticDataFiles
    {
        [JsonProperty(StaticDataFileName.ITEMS)] public Dictionary<int, Item> ItemsJson;
        [JsonProperty(StaticDataFileName.LEVELS)] public Dictionary<int, PlayerLevel> Levels;
        [JsonProperty(StaticDataFileName.SETTINGS)] public Core.Settings Settings;
        [JsonProperty(StaticDataFileName.TOWER_ITEMS)] public Dictionary<int, TowerItem> TowerItems;
        /**
		
        [JsonProperty(StaticDataFileName.DROPS)] public Dictionary<int, Drop> Drops;
        [JsonProperty(StaticDataFileName.SHOP)] public Dictionary<int, ShopItem> Shop;
        [JsonProperty(StaticDataFileName.BANK)] public Dictionary<int, StaticBankItem> Bank;
		[JsonProperty(StaticDataFileName.BANK_PACKS)] public Dictionary<int, StaticBankPack> BankPacks;
		
		[JsonProperty(StaticDataFileName.UNIT_TYPES)] public Dictionary<int, UnitType> UnitTypes;
		[JsonProperty(StaticDataFileName.UNITS)] public Dictionary<int, Unit> Units;
		[JsonProperty(StaticDataFileName.MONSTERS)] public Dictionary<int, Monster> Monsters;
		[JsonProperty(StaticDataFileName.WAVES)] public Dictionary<int, Wave> Waves;
		[JsonProperty(StaticDataFileName.SUMMONS)] public Dictionary<int, SummonProbabilities> SummonsProbabilities;
		[JsonProperty(StaticDataFileName.SKILL_PARAMETERS)] public Dictionary<int, SkillParameter> SkillParameters;
		[JsonProperty(StaticDataFileName.SKILLS)] public Dictionary<int, Skill> Skills;
		[JsonProperty(StaticDataFileName.UNIT_UPGRADES)] public Dictionary<int, UnitUpgrade> UnitUpgrades;
		[JsonProperty(StaticDataFileName.UNIT_UPGRADE_TYPES)] public Dictionary<int, UnitUpgradeType> UnitUpgradeTypes;
		[JsonProperty(StaticDataFileName.UNIT_LEVEL_COSTS)] public Dictionary<int, UnitLevelCost> UnitLevelCosts;
		[JsonProperty(StaticDataFileName.ARTIFACTS)] public Dictionary<int, Artifact> Artifacts;
		*/
		
		

		public Dictionary<string, string> Localization;
    }
}