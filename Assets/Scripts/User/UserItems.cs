using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Queries.Operations.Api;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Utils;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.User
{
    public class UserItems
    {
        private readonly UserData _user;
        
        private Dictionary<int, LongReactiveProperty> _items = new Dictionary<int, LongReactiveProperty>();
        private Dictionary<int, long> dropedItems = new Dictionary<int, long>();

        public event Action<ItemCount> OnDropStartEvent;
        public event Action<ItemCount> OnDropFinishEvent;
        public event Action OnAllDropFinishEvent;

        public event Action Updated;

        public UserItems(UserData user)
		{
            _user = user;
        }

        public void Clear()
        {
            foreach (var itemReactive in _items.Values)
                itemReactive.Value = 0;
        }

        public void OnDropStart(ItemCount itemCount)
        {
            if (!dropedItems.ContainsKey(itemCount.ItemId))
                dropedItems[itemCount.ItemId] = 0;
            dropedItems[itemCount.ItemId] += itemCount.Count;

            RemoveItems(itemCount);

            OnDropStartEvent?.Invoke(itemCount);
        }

        public void OnDropFinish(ItemCount itemCount)
        {
            if (!dropedItems.ContainsKey(itemCount.ItemId))
                dropedItems[itemCount.ItemId] = 0;
            dropedItems[itemCount.ItemId] -= itemCount.Count;

            if (dropedItems[itemCount.ItemId] <= 0)
                dropedItems.Remove(itemCount.ItemId);

            AddItems(itemCount);
            
            OnDropFinishEvent?.Invoke(itemCount);
			if (dropedItems.Count == 0)
				OnAllDropFinishEvent?.Invoke();
        }

        public long GetDropedCount(int itemId)
        {
            return dropedItems.ContainsKey(itemId) ? dropedItems[itemId] : 0;
        }

        public float GetDropedCount(Item item)
        {
            return dropedItems.ContainsKey(item.Id) ? dropedItems[item.Id] : 0;
        }
        
        public long GetCount(Item item, bool real = false)
        {
            return GetCount(item.Id, real);
        }

        public long GetCount(int itemId, bool real = false)
        {
            if (real)
                return GetCountWithDroped(itemId);
            return this[itemId];
        }

        public long GetCountWithDroped(int itemId)
        {
            var dropedCount = GetDropedCount(itemId);
            return this[itemId] + dropedCount;
        }

        public void Update(Dictionary<int, long> items, Dictionary<int, long> delta)
        {
            if (items != null)
	            foreach (var serverUserItem in items)
	                this[serverUserItem.Key] = serverUserItem.Value;

			if (delta != null)
				foreach (var serverUserItem in delta)
					this[serverUserItem.Key] += serverUserItem.Value;
        }

        public LongReactiveProperty GetReactiveProperty(Item item)
        {
			if (item is null)
				return null;
            if (!_items.ContainsKey(item.Id))
                _items[item.Id] = new LongReactiveProperty(0);
            return _items[item.Id];
        }


        public bool RemoveItems(List<ItemCount> itemsCount) => RemoveItems(itemsCount.ToArray());
        public bool RemoveItems(ItemCount itemCount) => RemoveItems(itemCount.Item, itemCount.Count);
        public bool RemoveItems(int itemId, long count) => RemoveItems(Game.Static.Items[itemId], count);
		public bool RemoveItems(ItemCount[] itemsCount)
		{
			if (itemsCount == null) return true;
			foreach (var itemCount in itemsCount)
				if (!RemoveItems(itemCount.Item, itemCount.Count)) return false;
			return true;
		}

		public bool RemoveItems(Item item, long count, bool needLog = true)
		{
			if (item.HasTimeItem)
			{
				count = (long) (item.Value * count);
				item = item.TimeItem;
			}

			this[item] -= count;

            return true;
        }

		public void AddItems(List<ItemCount> itemsCount)
        {
            foreach (var itemCount in itemsCount)
                AddItems(itemCount.Item, itemCount.Count);
        }

        public void AddItems(ItemCount itemCount) => AddItems(itemCount.Item, itemCount.Count);
        
        public void AddItems(Item item, long count, bool needLog = true)
		{
			if (AddWithTimeItem(item, count)) 
				return;

			this[item] += count;
        }

		public bool AddWithTimeItem(Item i, long count)
		{
			if (i.HasTimeItem)
			{
				count = (long) i.Value * count;
				i = i.TimeItem;
			}

			if (i.IsTimeItem)
			{
				ChangeTimeItem(true, i, count);
				return true;
			}

			return false;
		}

		public void ChangeTimeItem(bool add, Item i, long count)
		{
			if (!i.IsTimeItem)
				return;

			if (add == false)
			{
				this[i] -= count;
				return;
			}

			if (IsTimerDone(this[i]))
				this[i] = GetTimeFromNow(count);
			else
				this[i] += count;


			bool IsTimerDone(float t) => GameTime.Now >= t;
			long GetTimeFromNow(long count) => GameTime.Now + count;
		}

		public float Exp => this[Game.Static.Items.Exp];
		public LongReactiveProperty ReactiveExp => GetReactiveProperty(Game.Static.Items.Exp);

        public float Money1 => this[Game.Static.Items.Money1];
        public LongReactiveProperty ReactiveMoney1 => GetReactiveProperty(Game.Static.Items.Money1);

        public float Money2 => this[Game.Static.Items.Money2];
        public LongReactiveProperty ReactiveMoney2 => GetReactiveProperty(Game.Static.Items.Money2);
/*
        public IPromise ItemUse(Item item, float count)
        {
            var result = new Promise();
            Game.QueryManager.RequestPromise(new ItemUseOperation(item, count))
                .Then(response =>
                {
                    Game.ServerDataUpdater.Update(response);
                    result.Resolve();
                });
            return result;
        }
*/
        public long this[string modelId] => this[Game.Static.Items[modelId]];

        public long this[Item item]
        {
            get => item == null ? 0 : this[item.Id];
			set
			{
				if (item != null) this[item.Id] = value;
			}
		}

        public long this[int id]
        {
            get => !_items.ContainsKey(id) ? 0 : _items[id].Value;
            set
            {
                if (!_items.ContainsKey(id))
                    _items[id] = new LongReactiveProperty(value);
                else
                    _items[id].Value = value;

				if (id == Game.Static.Items.Exp.Id)
				 	Game.User.CheckLevelUp();
				
                Updated?.Invoke();
            }
        }

		public void LogAllForDebug()
		{
			if (dropedItems.Count > 0)
				Debug.Log("CurrentDrop = " + string.Join(", ", dropedItems.Select(x => $"item_{x.Key} = {x.Value}")));
			else
				Debug.Log("CurrentDrop is null");
		}
	}

}
