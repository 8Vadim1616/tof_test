using Assets.Scripts.Utils;

namespace Assets.Scripts.Static.Skills
{
	public class SkillParameter : StaticCollectionItemCode
	{
		public const string Damage = "Damage";
		public const string Critical = "Critical";
		public const string Slow = "Slow";
		public const string Count = "Count";
		public const string Defense = "Defense";
		public const string AttackSpeed = "AttackSpeed";
		public const string AttackDamage = "AttackDamage";
		public const string Percent = "Percent";
		public const string SpRegen = "SpRegen";
		public const string Special = "Special";
		public const string Cost = "Cost";
		public const string PlusDamage = "PlusDamage";
		public const string Chance = "Chance";
		public const string Range = "Range";
		public const string Duration = "Duration";
		public const string Stun = "Stun";
		public const string Interval = "Interval";
		public const string Distance = "Distance";
		public const string SubDamage = "SubDamage";
		public const string AoE = "AoE";

		public string GetParamFormatted(float param, Skill skill)
		{
			switch (ModelId)
			{
				case AttackDamage or AttackSpeed or Chance or Range: return $"{param}%";
				case Damage : return $"{param}% {skill.DamageType.ToString().Localize()}";
				case Duration or Interval or Stun: return param.GetCharNumericTime();
				
				default : return param.ToString();
			}
		}
	}
}