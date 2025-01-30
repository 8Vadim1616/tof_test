using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Static.Units;

namespace Assets.Scripts.User.Units
{
	public class UserUnits
	{
		public Dictionary<int, UserUnit> All { get; } = new Dictionary<int, UserUnit>();

		public UserUnits()
		{
			Game.Static.Units.All.Values
				.Select(u => new UserUnit(u))
				.Each(u => All[u.Data.Id] = u);
		}

		public UserUnit Get(Unit unit) => All[unit.Id];

		public void Update(Dictionary<int, int> data)
		{
			if (data == null)
				return;

			foreach (var d in data)
				All[d.Key].Update(d.Value);
		}

		public Dictionary<int, int> Clone()
		{
			var servData = new Dictionary<int, int>();
			
			All.Each(u => servData[u.Key] = u.Value.Level.Value);

			return servData;
		}
	}
}