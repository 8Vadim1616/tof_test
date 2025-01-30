using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Static.UnitUpgrades;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Static.Skills
{
	public class Skill : StaticCollectionItem
	{
		[JsonProperty("param")]
		public List<SkillParameterValue> Params { get; protected set; }
		
		[JsonProperty("dt")]
		public DamageType DamageType { get; protected set; }

		[JsonProperty("ch")]
		public float Chance { get; protected set; }
		
		[JsonProperty("skill_type")]
		public SkillType Type { get; protected set; }

		public string Name => $"skill_{Id}";

		public string Desc
		{
			get
			{
				var result = $"skill_desc_{Id}".Localize();
				if (!Params.IsNullOrEmpty())
				{
					for (var i = 0; i < Params.Count; i++)
					{
						result = result.Replace("{" + Params[i].Type.ModelId + "}", Params[i].Type.GetParamFormatted(Params[i].Value, this).GetColoredText(UnitUpgradeByLevel.PARAM_COLOR));
						result = result.Replace("{" + i + "}", Params[i].Type.GetParamFormatted(Params[i].Value, this).GetColoredText(UnitUpgradeByLevel.PARAM_COLOR));
					}
				}
				
				//todo перенести в параметры (непонятно почему у них сделано отдельно)
				result = result.Replace("{Chance}", Game.Static.SkillParameters.Get(SkillParameter.Chance).GetParamFormatted(Chance, this).GetColoredText(UnitUpgradeByLevel.PARAM_COLOR));
				
				return result;
			}
		}
		
		public string IconPath => $"img/skills/Skill_{Id}";

		public SkillParameterValue GetParam(string id)
		{
			return Params?.FirstOrDefault(p => p.Type?.ModelId == id);
		}

		public bool TryGetParam(string id, out SkillParameterValue param)
		{
			param = GetParam(id);
			return param != null;
		}
	}
}