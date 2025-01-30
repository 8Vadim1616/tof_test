using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static.Levels
{
	public class PlayerLevels : StaticCollection<PlayerLevel>
	{
		private PlayerLevel _maxLevel;
		
		public PlayerLevels(JToken token) : base(token)
		{
		}

		public PlayerLevels(Dictionary<int, PlayerLevel> all) : base(all)
		{
			_maxLevel = All[All.Keys.Max()];
		}

		public PlayerLevel GetByExp(int exp)
		{
			foreach (var levelData in All)
			{
				if (levelData.Value.ExpToNext.Count > exp)
					return levelData.Value;
			}

			return _maxLevel;
		}
	}
}