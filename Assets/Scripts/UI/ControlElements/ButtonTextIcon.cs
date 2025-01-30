using System;
using System.Collections.Generic;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.ControlElements
{
    public class ButtonTextIcon : ButtonText
    {
        [SerializeField] protected Image icon;

        public Image Icon
        {
            get => icon;
            set => icon = value;
        }

		private Item _item;
		private List<IDisposable> _sub = new List<IDisposable>();
		
        public void SetItemCount(ItemCount ic, List<ItemCount> otherItems = null)
		{
			_sub.ForEach(s => s?.Dispose());
			_sub.Clear();
			_item = ic.Item;
			_sub.Add(Game.User.Items.GetReactiveProperty(_item)
					.Subscribe(_ =>
						{
							Text = ic.GetColoredCount();
							UpdateLock();
						}).AddTo(this));

			if (!otherItems.IsNullOrEmpty())
			{
				foreach (var otherItem in otherItems)
				{
					_sub.Add(Game.User.Items.GetReactiveProperty(otherItem.Item)
								 .Subscribe(_ =>
								  {
									  UpdateLock();
								  }).AddTo(this));
				}
			}


			void UpdateLock()
			{
				SetLock(!ic.EnoughInUser || (!otherItems.IsNullOrEmpty() && !Game.Checks.EnoughItems(otherItems, false)), true);
			}
			
            Icon.LoadItemImage(ic.Item);
        }
    }
}