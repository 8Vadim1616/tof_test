using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static.UnitUpgrades
{
	public class UnitUpgradeTypes : StaticCollectionCode<UnitUpgradeType>
	{
		public UnitUpgradeTypes(JToken token) : base(token)
		{
		}

		public UnitUpgradeTypes(Dictionary<int, UnitUpgradeType> token) : base(token)
		{
		}
	}
}