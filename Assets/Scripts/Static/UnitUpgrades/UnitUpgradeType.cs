namespace Assets.Scripts.Static.UnitUpgrades
{
	public class UnitUpgradeType : StaticCollectionItemCode
	{
		public const string AttackDamage = "AttackDamage";
		public const string AttackSpeed = "AttackSpeed";
		public const string AttackAll = "AttackAll";
		public const string AttackDamageBuff = "AttackDamageBuff";
		public const string BossDamage = "BossDamage";
		public const string CriticalDamage = "CriticalDamage";
		public const string Defense = "Defense";
		public const string MaxSp = "MaxSp";
		public const string RegenSp = "RegenSp";
		public const string SkillChance = "SkillChance";
		public const string SkillDamage = "SkillDamage";
		public const string SkillUnlock = "SkillUnlock";
		public const string SkillUpgrade = "SkillUpgrade";
		public const string Slow = "Slow";
		public const string Stun = "Stun";

		public string GetParamFormatted(float param)
		{
			switch (ModelId)
			{
				case AttackDamage or AttackSpeed or AttackAll or BossDamage or SkillDamage or CriticalDamage or Stun: return $"+{param}%";
				
				default : return param.ToString();
			}
		}
	}
}
