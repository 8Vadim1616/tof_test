using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Assets.Scripts.Utils;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static.Tower
{
    public class TowerItems : StaticCollectionCode<TowerItem>
    {
		public TowerItems(JToken token) : base(token) { }

		public TowerItems(Dictionary<int, TowerItem> data) : base(data)
		{
		}
		
		private TowerItem money1;
		public TowerItem Money1 => money1 ??= this[TowerItem.MONEY1];
		
		private TowerItem exp;
		public TowerItem Exp => exp ??= this[TowerItem.EXP];
		

	}
}

