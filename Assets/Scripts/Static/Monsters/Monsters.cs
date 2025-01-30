using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static.Monsters
{
	public class Monsters : StaticCollectionCode<Monster>
	{
		public Monsters(JToken token) : base(token)
		{
		}

		public Monsters(Dictionary<int, Monster> all) : base(all)
		{
		}
	}
}