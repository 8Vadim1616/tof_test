using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static.Units
{
	public class Units : StaticCollectionCode<Unit>
	{
		public Units(JToken token) : base(token)
		{
		}

		public Units(Dictionary<int, Unit> all) : base(all)
		{
		}
	}
}