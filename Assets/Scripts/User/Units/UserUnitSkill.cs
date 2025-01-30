using System.Linq;
using Assets.Scripts.Gameplay;
using Assets.Scripts.Static.Skills;
using UnityEngine;

namespace Assets.Scripts.User.Units
{
	public class UserUnitSkill : Skill
	{
		private UserUnit _unit;
		private Skill _skill;
		
		public UserUnitSkill(UserUnit unit, Skill skill)
		{
			_unit = unit;
			_skill = skill;
			
			if (_skill.Params != null)
				Params = _skill.Params.Select(p => p.Clone()).ToList();
			DamageType = _skill.DamageType;
			Chance = _skill.Chance;
			Id = _skill.Id;
			Type = _skill.Type;
		}
		
		public float Range => GetParam(SkillParameter.Range)?.Value ?? 0f;
		public float DamageMult => (GetParam(SkillParameter.Damage)?.Value ?? 0f) / 100f * _unit.SkillDamageMultiplayer;
		public float StunTime => GetParam(SkillParameter.Stun).Value * _unit.StunMultiplayer;
		public int Count => (int) (GetParam(SkillParameter.Count)?.Value ?? 0);
		public float Interval => GetParam(SkillParameter.Interval)?.Value ?? 0;
		public float Duration => GetParam(SkillParameter.Duration)?.Value ?? 0;
		public float AttackSpeed => GetParam(SkillParameter.AttackSpeed)?.Value ?? 0;
		public float SlowMult => (GetParam(SkillParameter.Slow)?.Value ?? 100f) / 100f;

		/**
		public bool CanRun(PlayfieldView playfieldView)
			=> Random.Range(0f, 1f) <= Chance / 100f + _unit.SkillChanceAdd + playfieldView.AbilitiesController.SkillActivationChanceAdd.Value;
			*/

		public void AddValueToParameter(string paramId, float addValue)
		{
			var param = GetParam(paramId);
			
			//При апгрейде Range используются %, а когда добавляем такой новый параметр - там абсолютное значение
			
			if (param == null)
			{
				param = new SkillParameterValue(Game.Static.SkillParameters.Get(paramId), addValue);
				Params.Add(param);
			}
			else
			{
				if (paramId == SkillParameter.Range)
				{
					param.Value += addValue / 100f * _skill.GetParam(paramId).Value;
				}
				else
					param.Value += addValue;
			}
		}
	}
}