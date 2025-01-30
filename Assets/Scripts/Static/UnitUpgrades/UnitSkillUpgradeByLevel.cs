using Assets.Scripts.Static.Skills;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.UnitUpgrades
{
	public class UnitSkillUpgradeByLevel : SkillParameterValue
	{
		[JsonProperty("id")]
		private int _skillId;
		private Skill _skill;
		public Skill Skill => _skill ??= Game.Static.Skills.Get(_skillId);
	}
}