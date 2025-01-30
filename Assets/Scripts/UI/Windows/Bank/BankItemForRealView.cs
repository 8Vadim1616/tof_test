using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.User.BankPacks;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Windows.Bank
{
	public class BankItemForRealView : BankItemView
	{
		[SerializeField] protected TMP_Text _label;

		public override void Init(BankWindow bankWindow, UserBankPackItem bankPackItem)
		{
			base.Init(bankWindow, bankPackItem);

			_label.text = bankPackItem.BankItem.Label;
			_count.text = bankPackItem.BankItem.BuyItemCount.Count.ToKiloFormat();
			GetComponent<BasicButton>().SetOnClick(OnBuyClick);
		}
	}
}