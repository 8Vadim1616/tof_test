using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utils;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Items
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ItemCount
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ItemId { get; set; }
		
		private Item _item;
		public virtual Item Item => ItemId != 0 ? _item ??= Game.Static.Items.Get(ItemId) : null;

        [JsonProperty("cnt", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long Count { get; set; }

		[JsonProperty("icon", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string ReplaceIconPath { get; set; }

		/// <summary>Замещающая награда, если текущая в наличии</summary>
		[JsonProperty("alt", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public ItemCount AlternativeItemCount { get; set; }

        public ItemCount()
        {

        }

        public bool EnoughInUser => UserCount >= Count;
        public double UserCount => Item?.UserAmount() ?? 0;

        public ItemCount(int itemId, long count, string replaceIconPath = null)
        {
            ItemId = itemId;
            Count = count;
			ReplaceIconPath = replaceIconPath;
        }

        public ItemCount(Item item, long count, string replaceIconPath = null)
        {
            if (item == null) 
				ItemId = 0;
            else 
				ItemId = item.Id;

            Count = count;
			ReplaceIconPath = replaceIconPath;
        }

        public ItemCount(ItemCount ic)
        {
            ItemId = ic.ItemId;
            Count = ic.Count;
			ReplaceIconPath = ic.ReplaceIconPath;
        }

        public void AddCount(long count) => Count += count;

        public override string ToString()
        {
            return string.Format("{0}({1}) : {2}", Item.ModelId, ItemId, Count);
        }

        public ItemCount Clone()
        {
            return new ItemCount(Item, Count, ReplaceIconPath);
        }

        public bool PlayerHasEnough => Game.Checks.EnoughItems(Item, Count);

        public bool PlayerHasEnoughJustCount => Game.Checks.EnoughItems(Item, Count, needOpenBank: false);

        public bool Equals(ItemCount itemCount)
        {
            return ItemId == itemCount.ItemId && Count == itemCount.Count;
        }
        
		public void RemoveReplaceIcon() => ReplaceIconPath = null;

        public class ItemCountDirect : ItemCount
        {
            public override Item Item => _item;
            private Item _item;
            public ItemCountDirect(Item item, long count)
            {
                _item = item;
                Count = count;
            }
            
            public ItemCountDirect(ItemCount itemCount)
            {
                _item = itemCount.Item;
                Count = itemCount.Count;
            }
        }

		public string GetCountAsText(bool hideIfOne = true)
		{
			return GetCountAsText(this, hideIfOne);
		}

		public static string GetCountAsText(ItemCount i, bool hideIfOne = true)
		{
			if (i?.Item == null)
				return "";

			var isEternal = i.Item.IsEternalBoostAdd;
			var val = i.Item.ValueToTime * i.Count;

			var isMoney = i.Item == Game.Static.Items.Money1;
			var countText = isMoney ? i.Count.ToString() : $"x{i.Count}";

			if (hideIfOne && i.Count == 1)
				countText = "";
			else if (i.Count == 0)
				countText = "";

			return isEternal ? $"∞ {val.GetCharNumericTime(groupCount: 1)}" : countText;
		}

		internal ItemCount CheckAlternatives()
		{
			if (AlternativeItemCount == null)
				return this;
			
			if (Item.IsTimeItem ? UserCount > GameTime.Now : UserCount > 0)
				return AlternativeItemCount;

			return new ItemCount(Item, Count);
		}
	}

    public static class ItemCountExtension
    {
		public static List<ItemCount> RemoveUnknownItems(this List<ItemCount> itemCounts)
		{
			var i = 0;
			while (i < itemCounts.Count)
			{
				if (itemCounts[i] == null || itemCounts[i].Item == Game.Static.Items.UnknownItem)
					itemCounts.RemoveAt(i);
				else
					i++;
			}

			return itemCounts;
		}

		public static List<ItemCount> RemoveReplaceIcons(this List<ItemCount> itemCounts)
		{
			foreach (var itemCount in itemCounts)
				itemCount.RemoveReplaceIcon();

			return itemCounts;
		}

        public static IList<ItemCount> PlayerNeededAmount(this IList<ItemCount> priceCounts)
        {
            var needed = new List<ItemCount>();

            foreach (var itemCount in priceCounts)
            {
                var playerAmount = Game.User.Items[itemCount.Item];
                var need = itemCount.Count - playerAmount;
                if (need > 0) needed.Add(new ItemCount(itemCount.Item, need));
            }

            return needed;
        }

        public static List<ItemCount> MergeItemCounts(this List<ItemCount> origin,
            IEnumerable<ItemCount> target)
        {
            var tmp = target?.ToList();
            if (tmp == null || tmp.Empty()) return origin;
            
            for (int i = 0; i < origin.Count(); i++)
                for(int j = 0; j < tmp.Count; j++)
                    if (origin[i].Item == tmp[j].Item)
                    {
                        origin[i].AddCount(tmp[j].Count);
                        tmp.RemoveAt(j--);
                    }

            while (tmp.Count > 0)
            {
                origin.Add(tmp[0]);
                tmp.RemoveAt(0);
            }

            origin.RemoveAll(ic => ic.Count <= 0);

            return origin;
        }

		public static List<ItemCount> MergeItemCountsClone(this List<ItemCount> origin, IEnumerable<ItemCount> target)
		{
			var clone = origin.Clone();

			clone.MergeItemCounts(target);

			return clone;
		}

		public static List<ItemCount> Clone(this List<ItemCount> origin)
		{
			var clone = new List<ItemCount>();

			foreach (var ic in origin)
				clone.Add(new ItemCount(ic));

			return clone;
		}

        public static List<ItemCount> Extract(this List<ItemCount> origin, IEnumerable<ItemCount> target)
        {
            var targetList = target.ToList();
            if (targetList.Empty()) return targetList?.ToList();

            var result = new List<ItemCount>();
            foreach (var ic in origin)
            {
                var targetItem = targetList.Find(item => item.ItemId == ic.ItemId)?.Clone();
                if (targetItem != null)
                {
                    var diff = ic.Count - targetItem.Count;
                    ic.Count = diff > 0 ? diff : 0;
                    if (diff < 0) targetItem.Count -= diff;
                    result.Add(targetItem);
                }
            }

            origin.RemoveAll(ic => ic.Count <= 0);
            result.RemoveAll(ic => ic.Count <= 0);

            return result;
		}

		public static T Multiply<T>(this T target, int mult) where T : IList<ItemCount>, new()
		{
			var ret = new T();
			foreach (var ic in target)
				ret.Add(ic == null ? null : new ItemCount(ic.ItemId, ic.Count * mult));
			return ret;
		}
	}

    public class Undefinable<T>
    {
        public bool HasValue;
        public T Value;
    }

    public class UndefinableConverter<T, K> : JsonConverter<T> where T : Undefinable<K>
    {
        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
