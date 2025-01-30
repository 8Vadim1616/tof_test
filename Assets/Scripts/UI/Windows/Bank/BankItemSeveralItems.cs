using Assets.Scripts;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.General;
using Assets.Scripts.UI.Windows.Bank;
using Assets.Scripts.User.BankPacks;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;

public class BankItemSeveralItems : BankItemView
{
	[SerializeField] protected TMP_Text _title;
	[SerializeField] protected TMP_Text _label;
	[SerializeField] private ItemCountView _itemCountPrefab;

	private BankWindow _bankWindow;

	public override void Init(BankWindow bankWindow, UserBankPackItem bankPackItem)
	{
		base.Init(bankWindow, bankPackItem);
		_title.text = bankPackItem.Name;
		_label.text = bankPackItem.BankItem.Label;
		GetComponent<BasicButton>().SetOnClick(OnBuyClick);

		_itemCountPrefab.SetActive(true);
		foreach (var item in bankPackItem.Items)
		{
			var itemView = Instantiate(_itemCountPrefab, _itemCountPrefab.transform.parent);
			itemView.ItemCount = item.GetItemCount(1, 0, 0);
		}
		_itemCountPrefab.SetActive(false);
	}
}