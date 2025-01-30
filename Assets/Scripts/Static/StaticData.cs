using System;
using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Network.Queries.Operations.Api.StaticData;
using Assets.Scripts.Static.Bank;
using Assets.Scripts.Static.Monsters;
using Assets.Scripts.Static.Shop;
using Assets.Scripts.Static.Skills;
using Assets.Scripts.Static.Units;
using Assets.Scripts.Static.UnitUpgrades;

namespace Assets.Scripts.Static
{
	public class StaticData
	{
		public const int DEFAULT_GRP = 0;

		private bool isLoaded;
		
        public Dictionary<string, long> Versions { get; private set; }
		public ModelVersionData ModelVersionData { get; private set; }

        public Items.Items Items { get; private set; }
        public Core.Settings Settings { get; private set; }
        public Drops.Drops Drops { get; private set; }
        public ShopItems Shop { get; private set; }
        public StaticBankItems Bank { get; private set; }
        public StaticBankPacks BankPacks { get; private set; }
		public Levels.PlayerLevels PlayerLevels { get; private set; }
		public Units.Units Units { get; private set; }
		public UnitTypes UnitTypes { get; private set; }
		public Monsters.Monsters Monsters { get; private set; }
		public Waves Waves { get; private set; }
		public SummonsProbabilities SummonsProbabilities { get; private set; }
		public SkillParameters SkillParameters { get; private set; }
		public Skills.Skills Skills { get; private set; }
		public UnitLevelCosts UnitLevelCosts { get; private set; }
		public UnitUpgrades.UnitUpgrades UnitUpgrades { get; private set; }
		public UnitUpgradeTypes UnitUpgradeTypes { get; private set; }
		public Artifacts.Artifacts Artifacts { get; private set; }

		public Tower.TowerItems TowerItems { get; private set; }

		public StaticData()
		{
		}

		public IPromise GetStaticData(Action<float> progress)
		{
#if !UNITY_SERVER			
			ServerLogs.LoadGameProgress("static load start");
#endif
			return new StaticDataParser(StaticDataFileName.ALL).Parse(progress)
															   .Then(result =>
																{
#if !UNITY_SERVER
																	ServerLogs.LoadGameProgress("static parse start");
#endif
																	Load(result);
																	return Promise.Resolved();
																});
		}

		public void Load(StaticDataResponse model)
        {
			Versions = model.Versions;
			ModelVersionData = model.ModelVersionData;
			
            Game.Localization.Load(model.Files.Localization);

            Items = new Items.Items(model.Files.ItemsJson);
            PlayerLevels = new Levels.PlayerLevels(model.Files.Levels);
            Settings = model.Files.Settings;
            TowerItems = new Tower.TowerItems(model.Files.TowerItems);
            
            /**
            Drops = new Drops.Drops(model.Files.Drops);
			Shop = new ShopItems(model.Files.Shop);
			Bank = new StaticBankItems(model.Files.Bank);
			BankPacks = new StaticBankPacks(model.Files.BankPacks);
			UnitTypes = new UnitTypes(model.Files.UnitTypes);
			Units = new Units.Units(model.Files.Units);
			Monsters = new Monsters.Monsters(model.Files.Monsters);
			Waves = new Waves(model.Files.Waves);
			SummonsProbabilities = new SummonsProbabilities(model.Files.SummonsProbabilities);
			SkillParameters = new SkillParameters(model.Files.SkillParameters);
			Skills = new Skills.Skills(model.Files.Skills);
			UnitUpgradeTypes = new UnitUpgradeTypes(model.Files.UnitUpgradeTypes); 
			UnitUpgrades = new UnitUpgrades.UnitUpgrades(model.Files.UnitUpgrades);
			UnitLevelCosts = new UnitLevelCosts(model.Files.UnitLevelCosts);
			Artifacts = new Artifacts.Artifacts(model.Files.Artifacts);
			*/
			
			isLoaded = true;
        }
    }
}
