using System;
using System.Collections.Generic;
using Assets.Scripts.Core.Controllers;
using Assets.Scripts.User.BankPacks;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Windows.Bank
{
	public class BankGroupView : MonoBehaviour
	{
		[SerializeField] private TMP_Text _title;
		[SerializeField] private BankItemShopView _itemShopPrefab;
		[SerializeField] private BankItemForRealView _itemForRealPrefab;
		[SerializeField] private BankItemSeveralItems _itemSeveralForRealPrefab;
		[SerializeField] private BankItemEventView _bankItemEventView;
		[SerializeField] private RectTransform _itemsContainer;
		
		public List<BankItemView> Items { get; private set; } = new List<BankItemView>();
		public ScrollRect Scroll { get; private set; }
		public RectTransform RectTransform { get; private set; }
		
		public void Init(ScrollRect scrollRect, BankWindow bankWindow, List<UserBankPackItem> packs, string title)
		{
			RectTransform = GetComponent<RectTransform>();
			
			Scroll = scrollRect;
			_title.text = title;
			
			_itemShopPrefab.SetActive(true);
			_itemForRealPrefab.SetActive(true);
			_itemSeveralForRealPrefab.SetActive(true);
			_bankItemEventView.SetActive(true);
			
			foreach (var pack in packs)
			{
				BankItemView packView = null;
				
				if (pack.EventId > 0)
					packView = Instantiate(_bankItemEventView, _itemsContainer);
				else if (pack.ShopPos > 0)
					packView = Instantiate(_itemShopPrefab, _itemsContainer);
				else if (pack.BankPos > 0)
				{
					if (pack.BankItem.BuyItems.Count > 1)
						packView = Instantiate(_itemSeveralForRealPrefab, _itemsContainer);
					else
						packView = Instantiate(_itemForRealPrefab, _itemsContainer);
				}

				if (packView)
				{
					packView.Init(bankWindow, pack);
					Items.Add(packView);
				}
			}
			
			_itemShopPrefab.SetActive(false);
			_itemForRealPrefab.SetActive(false);
			_itemSeveralForRealPrefab.SetActive(false);
			_bankItemEventView.SetActive(false);
		}

		private void HandleEventFinishing()
		{
			gameObject.SetActive(false);
		}
	}
}