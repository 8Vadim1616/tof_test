using Assets.Scripts.Utils;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.UnitUpgrades
{
	public class UnitUpgradeByLevel
	{
		public const string PARAM_COLOR = "FFE400";
		
		[JsonProperty("type")]
		private int _typeId;
		private UnitUpgradeType _type;
		public UnitUpgradeType Type => _type ??= Game.Static.UnitUpgradeTypes.Get(_typeId);
		
		[JsonProperty("level")]
		public int Level { get; private set; }
		
		[JsonProperty("skill")]
		public UnitSkillUpgradeByLevel SkillUpgrade { get; private set; }
		
		/**
		 * Только для апгрейдов, где не указан skill
		 */
		[JsonProperty("param")]
		public float Param { get; private set; }

		public float ParamMult => Param / 100f;

		public string Desc
		{
			get
			{
				if (SkillUpgrade != null)
				{
					if (Type.ModelId == UnitUpgradeType.SkillUnlock)
					{
						return $"{Type.ModelId}_upgrade_desc".Localize(SkillUpgrade.Skill.Name.GetLink(0, PARAM_COLOR));
					}
					else if (Type.ModelId == UnitUpgradeType.SkillUpgrade)
					{
						var result = $"{Type.ModelId}_upgrade_desc_{SkillUpgrade.Skill.Id}".Localize(SkillUpgrade.Skill.Name.GetLink(0, PARAM_COLOR));
						result = result.Replace("{" + SkillUpgrade.Type.ModelId + "}", 
												("+" + SkillUpgrade.Type.GetParamFormatted(SkillUpgrade.Value, SkillUpgrade.Skill)).GetColoredText(PARAM_COLOR));
						return result;
					}
				}
				else
				{
					return $"{Type.ModelId}_upgrade_desc".Localize(Type.GetParamFormatted(Param).GetColoredText(PARAM_COLOR));
				}

				return "Unknown desc";
			}
		}
	}
}