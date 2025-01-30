using Assets.Scripts.Static.Items;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Drops
{
    public class DropItem
    {
        public enum DropItemType { Item, Drop }
		
		[JsonIgnore]
		public string Id { get; }

        private int _item = 0;
        private int _drop = 0;
        public int Percent { get; }
        public int Count { get; }
		[JsonIgnore]
		public Drop ParentDrop { get; private set; }

		public DropItem(Drop parentDrop, string item, int count, int percent)
		{
			ParentDrop = parentDrop;
			Id = ParentDrop.Id + ":" + item;
			
            var key = item.Split(':');
			if (key[0] == "items")
				_item = int.Parse(key[1]);
			else if (key[0] == "drops")
				_drop = int.Parse(key[1]);

			Count = count;
            Percent = percent;
        }

        public DropItemType Type => _drop != 0 ? DropItemType.Drop : DropItemType.Item;

        public Item Item => _item > 0 ? Game.Static.Items[_item] : null;

        public Drop Drop => _drop != 0 ? Game.Static.Drops[_drop] : null;
	}
}
