using Assets.Scripts.Static.Items;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Levels
{
	public class PlayerLevel : StaticCollectionItem
	{
		[JsonProperty("need")]
		public ItemCount ExpToNext { get; private set; }
		
		[JsonProperty("reward")]
		public ItemCount[] Reward { get; private set; }
		
		private PlayerLevel _nextLevel;
		public PlayerLevel NextLevel => _nextLevel ??= Game.Static.PlayerLevels.Get(Id + 1, false);
		
		private PlayerLevel _prevLevel;
		public PlayerLevel PrevLevel => _prevLevel ??= Game.Static.PlayerLevels.Get(Id - 1, false);
	}
}