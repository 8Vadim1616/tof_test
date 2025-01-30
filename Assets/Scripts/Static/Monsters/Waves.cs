using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static.Monsters
{
	public class Waves : StaticCollection<Wave>
	{
		public Waves(JToken token) : base(token)
		{
		}

		public Waves(Dictionary<int, Wave> all) : base(all)
		{
		}
	}
}