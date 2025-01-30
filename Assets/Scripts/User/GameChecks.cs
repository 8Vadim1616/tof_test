using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.Windows.Bank;
using UnityEngine;

namespace Assets.Scripts.User
{
	public class GameChecks
	{
		public bool EnoughItems(IList<ItemCount> needItems, bool needOpenBank = true, bool real = false) =>
			needItems?.All(x => EnoughItems(x, needOpenBank, real)) == true;

		public bool EnoughItems(Item item, long count = 1, bool needOpenBank = true, bool real = false) =>
			EnoughItems(new ItemCount(item.Id, count), needOpenBank, real);

		public bool EnoughItems(ItemCount needItem, bool needOpenBank = true, bool real = false)
		{
			if (needItem?.Item is null)
			{
				Debug.LogWarning($"Item not found (id={needItem.ItemId})");
				return true;
			}

			if (Game.User.Items.GetCount(needItem.Item.Id, real) < needItem.Count)
			{
				if (needOpenBank)
					BankWindow.Of(needItem.Item);
				return false;
			}

			return true;
		}

		public bool IsSocialAdvAvailable => true;

		public bool NeedFb
		{
			get
			{
#if UNITY_WEBGL
				return false;
#else
				return true;
#endif
			}
		}
	}
}