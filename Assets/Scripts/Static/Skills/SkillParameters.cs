using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static.Skills
{
	public class SkillParameters : StaticCollectionCode<SkillParameter>
	{
		public SkillParameters(JToken token) : base(token)
		{
		}

		public SkillParameters(Dictionary<int, SkillParameter> token) : base(token)
		{
		}
	}
}