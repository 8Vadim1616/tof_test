using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static.Skills
{
	public class Skills : StaticCollection<Skill>
	{
		public Skills(JToken token) : base(token)
		{
		}

		public Skills(Dictionary<int, Skill> all) : base(all)
		{
		}
	}
}