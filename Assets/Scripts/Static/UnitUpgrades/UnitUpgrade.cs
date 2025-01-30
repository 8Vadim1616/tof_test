using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Assets.Scripts.Static.Skills;
using Assets.Scripts.Static.Units;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.UnitUpgrades
{
	public class UnitUpgrade : StaticCollectionItemCode
	{
		[JsonProperty("unit")]
		private int _unitId;
		private Unit _unit;
		public Unit Unit => _unit ??= Game.Static.Units.Get(_unitId);

		private List<Skill> _skills;
		public List<Skill> Skills => _skills ??= All.Where(u => u.SkillUpgrade != null && u.Type.ModelId == UnitUpgradeType.SkillUnlock)
			.Select(u => u.SkillUpgrade.Skill)
			.ToList();
		
		[JsonProperty("upgrade")]
		public List<UnitUpgradeByLevel> All { get; private set; }

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			All = All.OrderBy(u => u.Level).ToList();
		}
	}
}