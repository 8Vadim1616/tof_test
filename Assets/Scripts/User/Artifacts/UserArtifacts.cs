using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Static.Artifacts;

namespace Assets.Scripts.User.Artifacts
{
	public class UserArtifacts
	{
		public Dictionary<int, UserArtifact> All { get; } = new Dictionary<int, UserArtifact>();

		public UserArtifacts()
		{
			Game.Static.Artifacts.All.Values
				.Select(a => new UserArtifact(a))
				.Each(a => All[a.Data.Id] = a);
		}

		public UserArtifact Get(Artifact artifact) => All[artifact.Id];

		public void Update(Dictionary<int, int> data)
		{
			if (data == null)
				return;

			foreach (var d in data)
			{
				All[d.Key].IsOwned = true;
				All[d.Key].Update(d.Value);
			}
		}

		public Dictionary<int, int> Clone()
		{
			var servData = new Dictionary<int, int>();
			
			All.Each(u => servData[u.Key] = u.Value.Level.Value);

			return servData;
		}
	}
}