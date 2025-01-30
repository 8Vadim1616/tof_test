using System.Linq;
using Assets.Scripts.Static.Items;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Units
{
	public class UnitLevelCost : StaticCollectionItem
	{
		[JsonProperty("unit")]
		private int _unitId;
		private Unit _unit;
		public Unit Unit => _unit ??= Game.Static.Units.Get(_unitId);

		[JsonProperty("cost")]
		private ItemCount[][] _cost;

		public ItemCount[] GetPriceByLevel(int level)
		{
			if (level - 2 <= _cost.Length)
				return _cost[level - 2];

			return null;
		}
	}
}