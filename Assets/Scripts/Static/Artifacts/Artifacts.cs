using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static.Artifacts
{
	public class Artifacts : StaticCollectionCode<Artifact>
	{
		public Artifacts(JToken token) : base(token)
		{
		}

		public Artifacts(Dictionary<int, Artifact> token) : base(token)
		{
		}
	}
}