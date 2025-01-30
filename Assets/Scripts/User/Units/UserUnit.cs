using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Network.Queries.Operations.Api.GamePlay;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Static.Units;
using Assets.Scripts.Static.UnitUpgrades;
using UniRx;
using UnityEngine;
using Unit = Assets.Scripts.Static.Units.Unit;

namespace Assets.Scripts.User.Units
{
	public class UserUnit
	{
		public Unit Data { get; private set; }
		public IntReactiveProperty Level { get; } = new();
		public bool IsOwned => Data.UnitType.ModelId != UnitType.MYTHICAL;

		public FloatReactiveProperty Attack { get; } = new ();
		public FloatReactiveProperty AttackSpeed { get; } = new ();

		private float _attackMultiplayer;
		private float _attackSpeedMultiplayer;
		
		public float AttackBossMultiplayer { get; private set; }
		public float CritAttackMultiplayer { get; private set; }
		public float SkillDamageMultiplayer { get; private set; }
		
		public float StunMultiplayer { get; private set; }
		public float SkillChanceAdd { get; private set; }

		public List<UserUnitSkill> Skills { get; } = new List<UserUnitSkill>();
		
		public UserUnit(Unit data)
		{
			Data = data;
			Update(1);
		}

		public UserUnitSkill GetSkill(int skillId) => Skills.FirstOrDefault(s => s.Id == skillId);

		public void Upgrade()
		{
			Game.QueryManager.RequestPromise(new UnitUpgradeOperation(Data.Id))
				.Then(r =>
				 {
					 Game.ServerDataUpdater.Update(r);
				 });
		}

		public void Update(int level)
		{
			Level.Value = level;
			Recalc();
		}
		
		private void Recalc()
		{
			RecalcMultiplayers();
			
			Attack.Value = Data.Damage + (Level.Value - 1) * Data.UpgradeDamage;
			AttackSpeed.Value = Data.AttackSpeed + (Level.Value - 1) * Data.UpgradeAttackSpeed;

			Attack.Value *= _attackMultiplayer;
			AttackSpeed.Value *= _attackSpeedMultiplayer;
			
			UpdateSkills();
		}

		private void UpdateSkills()
		{
			Skills.Clear();

			var upgrades = Game.Static.UnitUpgrades.GetByUnit(Data);
			if (upgrades == null)
				return;

			foreach (var upgrade in upgrades.All)
			{
				if (upgrade.Level > Level.Value)
					return;
				 
				if (upgrade.SkillUpgrade == null)
					continue;

				if (upgrade.Type.ModelId == UnitUpgradeType.SkillUnlock)
				{
					Skills.Add(new UserUnitSkill(this, upgrade.SkillUpgrade.Skill));
				}
				else if (upgrade.Type.ModelId == UnitUpgradeType.SkillUpgrade)
				{
					var skill = Skills.FirstOrDefault(s => s.Id == upgrade.SkillUpgrade.Skill.Id);
					if (skill != null)
						skill.AddValueToParameter(upgrade.SkillUpgrade.Type.ModelId, upgrade.SkillUpgrade.Value);
					else
						Debug.LogError($"Try to upgrade skill that not exists in unit {upgrade.SkillUpgrade.Skill.Id}");
				}
			}
		}

		private void RecalcMultiplayers()
		{
			var upgrades = Game.Static.UnitUpgrades.GetByUnit(Data);
			
			_attackMultiplayer = 1;
			_attackSpeedMultiplayer = 1;
			AttackBossMultiplayer = 1;
			CritAttackMultiplayer = Game.Settings.CritDamage / 100f;
			SkillDamageMultiplayer = 1f;
			SkillChanceAdd = 0f;
			StunMultiplayer = 1f;

			if (upgrades == null)
				return;
			
			foreach (var upgrade in upgrades.All)
			{
				if (upgrade.Level > Level.Value)
					return;
				
				if (upgrade.Type.ModelId == UnitUpgradeType.AttackDamage)
					_attackMultiplayer += upgrade.ParamMult;
				else if (upgrade.Type.ModelId == UnitUpgradeType.AttackAll)
				{
					_attackMultiplayer += upgrade.ParamMult;
					_attackSpeedMultiplayer += upgrade.ParamMult;
				}
				else if (upgrade.Type.ModelId == UnitUpgradeType.AttackSpeed)
					_attackSpeedMultiplayer += upgrade.ParamMult;
				else if (upgrade.Type.ModelId == UnitUpgradeType.BossDamage)
					AttackBossMultiplayer += upgrade.ParamMult;
				else if (upgrade.Type.ModelId == UnitUpgradeType.CriticalDamage)
					CritAttackMultiplayer += upgrade.ParamMult;
				else if (upgrade.Type.ModelId == UnitUpgradeType.Stun)
					StunMultiplayer += upgrade.ParamMult;
				
				else if (upgrade.Type.ModelId == UnitUpgradeType.SkillDamage)
					SkillDamageMultiplayer += upgrade.ParamMult;
				else if (upgrade.Type.ModelId == UnitUpgradeType.SkillChance)
					SkillChanceAdd += upgrade.ParamMult;
			}
		}

		public ItemCount[] GetUpgradeCost() => Data.GetLevelCost(Level.Value + 1);
		public ItemCount GetUpgradeMoney1() => Data.GetLevelCost(Level.Value + 1)?.FirstOrDefault(c => c.Item.IsMoney1);
		public ItemCount GetUpgradeCards() => Data.GetLevelCost(Level.Value + 1)?.FirstOrDefault(c => !c.Item.IsMoney1);
	}
}