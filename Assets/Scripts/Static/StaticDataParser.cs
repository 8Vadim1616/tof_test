using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Localization;
using Assets.Scripts.Network;
using Assets.Scripts.Network.Queries.Operations.Api.StaticData;
using Assets.Scripts.Static.Bank;
using Assets.Scripts.Static.Drops;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Static.Shop;
using System;
using System.Collections.Generic;
using Assets.Scripts.Static.Artifacts;
using Assets.Scripts.Static.Levels;
using Assets.Scripts.Static.Monsters;
using Assets.Scripts.Static.Skills;
using Assets.Scripts.Static.Tower;
using Assets.Scripts.Static.Units;
using Assets.Scripts.Static.UnitUpgrades;
using UnityEngine;

namespace Assets.Scripts.Static
{
	public class StaticDataParser
	{
		private const float NEX_TICK_DELTA_TIME = .2f;

		private readonly List<FileWithVersion> _files;

		public StaticDataParser(List<string> fileNames)
		{
			_files = new List<FileWithVersion>();

			foreach (var file in fileNames)
				_files.Add(new FileWithVersion
				{
					Name = file,
					Ver = FileResourcesLoader.GetGroup(StaticData.DEFAULT_GRP).GetMax(file)
				});
		}

		public IPromise<StaticDataResponse> Parse(Action<float> progress)
		{
			var startTime = Time.realtimeSinceStartup;
			var result = new Promise<StaticDataResponse>();
			var data = new StaticDataResponse { Files = new StaticDataFiles() };

			var promise = Promise.Resolved();

			var totalFilesCount = _files.Count;

			for (int i = 0; i < _files.Count; i++)
			{
				var index = i;
				var nextFile = _files[index];
				promise = promise
						 .Then(() => LoadNextFile(nextFile))
						 .Then(() => CheckTime(index, totalFilesCount));
			}

			promise.Then(() =>
			{
				data.Versions = FileResourcesLoader.NoGroup().GetMaxFilesVersion();
				data.ModelVersionData = FileResourcesLoader.NoGroup().LoadJson<ModelVersionData>(FileResourcesLoader.MODEL_DATA_FILE);

				result.Resolve(data);
			});

			IPromise LoadNextFile(FileWithVersion file)
			{
				var groupData = FileResourcesLoader.NoGroup();

				if (file.Name == StaticDataFileName.ITEMS)
					data.Files.ItemsJson = LoadJson<Dictionary<int, Item>>();
				else if (file.Name == StaticDataFileName.LEVELS)
					data.Files.Levels = LoadJson<Dictionary<int, PlayerLevel>>();
				else if (file.Name == StaticDataFileName.SETTINGS)
					data.Files.Settings = LoadJson<Core.Settings>();
				if (file.Name == StaticDataFileName.TOWER_ITEMS)
					data.Files.TowerItems = LoadJson<Dictionary<int, TowerItem>>();
				
				/**
				else if (file.Name == StaticDataFileName.DROPS)
					data.Files.Drops = LoadJson<Dictionary<int, Drop>>();
				else if (file.Name == StaticDataFileName.SHOP)
					data.Files.Shop = LoadJson<Dictionary<int, ShopItem>>();
				else if (file.Name == StaticDataFileName.BANK)
					data.Files.Bank = LoadJson<Dictionary<int, StaticBankItem>>();
				else if (file.Name == StaticDataFileName.BANK_PACKS)
					data.Files.BankPacks = LoadJson<Dictionary<int, StaticBankPack>>();
				else if (file.Name == StaticDataFileName.UNITS)
					data.Files.Units = LoadJson<Dictionary<int, Unit>>();
				else if (file.Name == StaticDataFileName.UNIT_TYPES)
					data.Files.UnitTypes = LoadJson<Dictionary<int, UnitType>>();
				else if (file.Name == StaticDataFileName.MONSTERS)
					data.Files.Monsters = LoadJson<Dictionary<int, Monster>>();
				else if (file.Name == StaticDataFileName.WAVES)
					data.Files.Waves = LoadJson<Dictionary<int, Wave>>();
				else if (file.Name == StaticDataFileName.SUMMONS)
					data.Files.SummonsProbabilities = LoadJson<Dictionary<int, SummonProbabilities>>();
				else if (file.Name == StaticDataFileName.SKILL_PARAMETERS)
					data.Files.SkillParameters = LoadJson<Dictionary<int, SkillParameter>>();
				else if (file.Name == StaticDataFileName.SKILLS)
					data.Files.Skills = LoadJson<Dictionary<int, Skill>>();
				else if (file.Name == StaticDataFileName.UNIT_UPGRADE_TYPES)
					data.Files.UnitUpgradeTypes = LoadJson<Dictionary<int, UnitUpgradeType>>();
				else if (file.Name == StaticDataFileName.UNIT_UPGRADES)
					data.Files.UnitUpgrades = LoadJson<Dictionary<int, UnitUpgrade>>();
				else if (file.Name == StaticDataFileName.UNIT_LEVEL_COSTS)
					data.Files.UnitLevelCosts = LoadJson<Dictionary<int, UnitLevelCost>>();
				else if (file.Name == StaticDataFileName.ARTIFACTS)
					data.Files.Artifacts = LoadJson<Dictionary<int, Artifact>>();
				*/
				else if (file.Name.StartsWith("lang"))
				{
					var langFile = LoadJson<Dictionary<string, Dictionary<string, string>>>();
					if (langFile != null && langFile.TryGetValue("lang", out var locales))
						data.Files.Localization = locales;
					else
					{
						file.Name = $"lang_{GameLocalization.DEFAULT}";
						data.Files.Localization = LoadJson<Dictionary<string, Dictionary<string, string>>>()["lang"];
					}
				}

				return Promise.Resolved();

				T LoadJson<T>() => groupData.LoadJson<T>(file.Name);

				string LoadText() => groupData.LoadText(file.Name);

				IEnumerable<string> LoadLines() => groupData.LoadLines(file.Name);
			}

			IPromise CheckTime(int current = 0, int total = 0)
			{
				if (Time.realtimeSinceStartup - startTime > NEX_TICK_DELTA_TIME)
				{
					if (total > 0)
						progress?.Invoke(current / (float) total);
					return Utils.Utils.NextFrame()
						.Then(() => startTime = Time.realtimeSinceStartup);
				}

				return Promise.Resolved();
			}


			return result;
		}
	}
}
