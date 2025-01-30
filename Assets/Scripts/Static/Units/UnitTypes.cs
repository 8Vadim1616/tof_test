using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static.Units
{
	public class UnitTypes : StaticCollectionCode<UnitType>
	{
		private UnitType _common;
		public UnitType Common => _common ??= this[UnitType.COMMON];
		
		private UnitType _epic;
		public UnitType Epic => _epic ??= this[UnitType.EPIC];
		
		private UnitType _rare;
		public UnitType Rare => _rare ??= this[UnitType.RARE];
		
		private UnitType _legend;
		public UnitType Legend => _legend ??= this[UnitType.LEGEND];
		
		private UnitType _mythical;
		public UnitType Mythical => _mythical ??= this[UnitType.MYTHICAL];
		
		public UnitTypes(JToken token) : base(token)
		{
		}

		public UnitTypes(Dictionary<int, UnitType> all) : base(all)
		{
		}
	}
}