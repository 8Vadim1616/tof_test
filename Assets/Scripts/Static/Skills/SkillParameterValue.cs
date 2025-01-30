using Newtonsoft.Json;

namespace Assets.Scripts.Static.Skills
{
	public class SkillParameterValue
	{
		[JsonProperty("type")]
		private int _typeId;
		private SkillParameter _type;
		public SkillParameter Type => _type ??= Game.Static.SkillParameters.Get(_typeId);
		
		[JsonProperty("val")]
		public float Value { get; set; }

		public SkillParameterValue()
		{
			
		}
		
		public SkillParameterValue(SkillParameter type, float value)
		{
			_typeId = type.Id;
			Value = value;
		}

		public SkillParameterValue Clone()
		{
			return new SkillParameterValue {_typeId = _typeId, Value = Value};
		}
	}
}