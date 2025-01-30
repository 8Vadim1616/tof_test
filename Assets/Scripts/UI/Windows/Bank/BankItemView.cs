using System.Collections.Generic;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.User.BankPacks;
using Assets.Scripts.User.MetaPayments;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Windows.Bank
{
	public abstract class BankItemView : MonoBehaviour
	{
		[SerializeField] protected Image _icon;
		[SerializeField] protected TMP_Text _count;
		[SerializeField] protected BasicButton _btn;

		public UserBankPackItem BankPackItem { get; private set; }
		protected BankWindow _bankWindow;

		public virtual void Init(BankWindow bankWindow, UserBankPackItem bankPackItem)
		{
			_bankWindow = bankWindow;
			BankPackItem = bankPackItem;

			if (_icon)
			{
				if (!bankPackItem.Icon.IsNullOrEmpty())
					_icon.LoadFromAssets("bank/" + bankPackItem.Icon);
				else
					_icon.LoadItemImage(BankPackItem.MainItemCount()?.Item);
			}
		}

		protected virtual void OnBuyClick()
		{
			if (BankPackItem.ShopPos > 0)
			{
				var shopItem = Game.Static.Shop.Get(BankPackItem.ShopPos);
				if (shopItem.Price == null || Game.Checks.EnoughItems(shopItem.Price))
				{
					// Game.User.Shop.BuyItem(shopItem, true)
					// 	.Then(() =>
					// 	 {
					// 		 Game.HUD.DropController.DropItem(shopItem.BuyItemCount, _icon.transform.position);
					// 	 });
				}
			}
			else
			{
				Game.User.Bank.Buy(BankPackItem.BankItem, new ShopMetaPayment(BankPackItem.BankItem));
			}
		}
	}
}