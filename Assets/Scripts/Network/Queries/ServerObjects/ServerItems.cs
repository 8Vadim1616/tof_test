using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Assets.Scripts.GameServiceProvider;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Utils;

namespace Assets.Scripts.Network.Queries.ServerObjects
{
    public class ServerItems : Dictionary<int, long>
    {
        public float GetCount(Item item) => GetCount(item.Id);
        public float GetCount(int itemId) => ContainsKey(itemId) ? this[itemId] : 0;

		public void Fix()
		{
			foreach (var it in Keys.ToList())
			{
				if (float.IsNaN(this[it]))
				{
					this[it] = 0;
				}
			}
		}

		public void Clear(Item item)
		{
			var itemId = item.Id;
			
			if (!ContainsKey(itemId))
				return;

			this[itemId] = 0;
		}
	}
}