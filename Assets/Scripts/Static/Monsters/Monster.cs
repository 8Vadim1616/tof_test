using System.Collections.Generic;
using Assets.Scripts.Static.Items;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Monsters
{
	public class Monster : StaticCollectionItemCode
	{
		[JsonProperty("hp")]
		public float HP { get; private set; }
		
		[JsonProperty("php")]
		public float PlusHP { get; private set; }
		
		[JsonProperty("ms")]
		public float MoveSpeed { get; private set; }
	}
}