using Assets.Scripts.UI.General;
using Assets.Scripts.User.BankPacks;
using UnityEngine;

namespace Assets.Scripts.UI.Windows.Bank
{
	public class BankItemEventView : BankItemForRealView
	{
		[SerializeField] private ItemCountView _additionalRewardLabel;
		
		public override void Init(BankWindow bankWindow, UserBankPackItem bankPackItem)
		{
			base.Init(bankWindow, bankPackItem);
			_additionalRewardLabel.ItemCount = bankPackItem.BankItem.BuyItems[1];
		}
	}
}