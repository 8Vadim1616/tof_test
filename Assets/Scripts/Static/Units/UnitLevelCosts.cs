using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static.Units
{
	public class UnitLevelCosts : StaticCollection<UnitLevelCost>
	{
		public UnitLevelCosts(JToken token) : base(token)
		{
		}

		public UnitLevelCosts(Dictionary<int, UnitLevelCost> all) : base(all)
		{
		}
	}
}