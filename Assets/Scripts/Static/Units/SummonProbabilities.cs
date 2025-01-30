using Newtonsoft.Json;

namespace Assets.Scripts.Static.Units
{
	public class SummonProbabilities : StaticCollectionItem
	{
		[JsonProperty("type")]
		private int _unitTypeId;
		private UnitType _unitType;
		public UnitType Type => _unitType ??= Game.Static.UnitTypes.Get(_unitTypeId);

		[JsonProperty("init")]
		public float Value { get; set; }
		
		[JsonProperty("upgrade")]
		public float UpgradeValue { get; set; }
	}
}