using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static.Units
{
	public class SummonsProbabilities : StaticCollection<SummonProbabilities>
	{
		public SummonsProbabilities(JToken token) : base(token)
		{
		}

		public SummonsProbabilities(Dictionary<int, SummonProbabilities> all) : base(all)
		{
		}
	}
}