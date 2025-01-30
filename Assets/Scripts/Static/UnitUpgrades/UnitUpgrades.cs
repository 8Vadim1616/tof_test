using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Static.Units;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static.UnitUpgrades
{
	public class UnitUpgrades : StaticCollectionCode<UnitUpgrade>
	{
		public UnitUpgrade GetByUnit(Unit unit) => All.Values.FirstOrDefault(u => u.Unit == unit);
		
		public UnitUpgrades(JToken token) : base(token)
		{
		}

		public UnitUpgrades(Dictionary<int, UnitUpgrade> token) : base(token)
		{
		}
	}
}