using System.Collections.Generic;
using Assets.Scripts.Static.Items;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Monsters
{
	public class Wave : StaticCollectionItem
	{
		[JsonProperty("lvl")]
		public int Level { get; private set; }
		
		[JsonProperty("mnstr")]
		private int _monsterId;
		private Monster _monster;
		public Monster Monster => _monster ??= Game.Static.Monsters.Get(_monsterId);
		
		[JsonProperty("boss")]
		private int _bossId;
		private Monster _boss;
		public Monster Boss => _boss ??= Game.Static.Monsters.Get(_bossId);
		
		[JsonProperty("def")]
		public float Defence { get; private set; }
		
		[JsonProperty("cnt")]
		public int SpawnCount { get; private set; }
		
		[JsonProperty("int")]
		public float SpawnInterval { get; private set; }
		
		[JsonProperty("delay")]
		public float NextDelay { get; private set; }
		
		[JsonProperty("stpnts")]
		public int StartGivePoint { get; private set; }
		
		[JsonProperty("monster_drop")]
		public List<ItemCount> DropOnDeath { get; private set; }
	}
}