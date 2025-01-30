using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Assets.Scripts.Static.HR;
using Assets.Scripts.Static.Items;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Drops
{
    public class Drop
    {
        public enum DropCondition { AND, OR }
        
		[JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Id { get; private set; }
		
		[JsonProperty("cmltv", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool IsCumulativeProbability { get; private set; }

		[JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
		private string _seasonType;

        [JsonProperty("it", DefaultValueHandling = DefaultValueHandling.Ignore)]
		private Dictionary<int, string> Items;
        [JsonProperty("cnt", DefaultValueHandling = DefaultValueHandling.Ignore)]
		private Dictionary<int, int?> Counts;
        [JsonProperty("prc", DefaultValueHandling = DefaultValueHandling.Ignore)]
		private Dictionary<int, int> Percents;
        [JsonProperty("condition", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public DropCondition Condition;
      
        private readonly List<DropItem> _items = new List<DropItem>();

        public List<DropItem> DropItems => _items;
        
        /// <summary>
        /// Генерация реальныйх айтемов в зависимости от настроек дропа
        /// </summary>
        /// <param name="userLevel"></param>
        /// <returns></returns>
        public List<ItemCount> GenerateForUser()
        {
            if (Condition == DropCondition.AND)
            {
                var andProb = UnityEngine.Random.value * 100;
                var items = _items.Where(i =>
                    i.Percent >= andProb && (i.Item == null)).ToList();

                var result = new List<ItemCount>();
                foreach (var d in items)
                {
                    if (d.Item != null)
                        result.Add(new ItemCount(d.Item, d.Count));
                    else if (d.Drop != null)
                        result.AddRange(d.Drop.GenerateForUser());
                }
                return result;
            }
            
            if (Condition == DropCondition.OR)
            {
                int sum = 0;
                var its = new List<(DropItem item, int prob)>();
                for (var i = 0; i < _items.Count; i++)
                {
                    if (_items[i].Item == null)
                        its.Add((item: _items[i], prob: sum += _items[i].Percent));
                }

                var orProb = UnityEngine.Random.value * sum;
                DropItem elem = null;
                foreach (var it in its)
                {
                    if (it.prob >= orProb)
                    {
                        elem = it.item;
                        break;
                    }
                }

                if (elem != null)
                {
                    if (elem.Drop != null)
                        return elem.Drop.GenerateForUser();
                    var result = new List<ItemCount> {new ItemCount(elem.Item, elem.Count)};
                    return result;
                }
            }

            return new List<ItemCount>();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (Items != null && Items.Any())
            {
                foreach (var key in Items.Keys)
                {
                    _items.Add(new DropItem(this, Items[key],
                        Counts[key].HasValue ? Counts[key].Value : 0,
                        Percents[key]));
                }
            }

            Items = null;
            Counts = null;
        }

        /** Все предметы */
        public List<DropItem> GetAllDropItems()
        {
            if (_items.Count == 0)
                return _items;

            return _items.SelectMany(
                item =>
                    item.Type == DropItem.DropItemType.Drop ? 
                    item.Drop.GetAllDropItems() : 
                    new List<DropItem> {item}
                    )
                .ToList();
        }

        public int GetMoney1()
        {
            return _items.FirstOrDefault(item => item.Item == Game.Static.Items.Money1)?.Count ?? 0;
        }
        
        //public int GetMoney2()
        //{
        //    return _items.FirstOrDefault(item => item.Item == Game.Static.Items.Money2)?.Count ?? 0;
        //}

        //public ItemCount GetMoney2ItemCount()
        //{
        //    return new ItemCount(Game.Static.Items.Money2, GetMoney2());
        //}

        public List<ItemCount> GetItems()
        {
            if (_items.Count == 0)
                return new List<ItemCount>();

            List<ItemCount> list = new List<ItemCount>();
            
            foreach (DropItem item in _items)
                if (item.Type == DropItem.DropItemType.Item)
                    list.Add(new ItemCount(item.Item, item.Count));

            return list;
        }
        
        public List<ItemCountFormula> GetItems(bool needBase, bool needOther)
        {
            if (_items.Count == 0)
                return new List<ItemCountFormula>();

            List<ItemCountFormula> list = new List<ItemCountFormula>();

            foreach (DropItem item in _items)
            {
                if (item.Item != null)
                {
                    if (_SuitableItem(item))
                        list.Add(new ItemCountFormula(item.Item, item.Count));
                }
                else
                {
                    list.AddRange(item.Drop.GetItems(needBase, needOther));
                }
            }

            return list;

            bool _SuitableItem(DropItem item) =>
                item.Type == DropItem.DropItemType.Item &&
                item.Item != null &&
                (needBase && item.Item.IsBase || needOther && !item.Item.IsBase);
        }

        /**То что может выпасть*/
        public List<Item> GetAllItems()
        {
            return GetAllDropItems()
                .Select(item => item.Item)
                .Distinct()
                .ToList();
        }

    }
}
