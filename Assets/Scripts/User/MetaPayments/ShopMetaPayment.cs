using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Static.Bank;
using Assets.Scripts.Static.Items;
using Newtonsoft.Json;
using System.Collections.Generic;
using Assets.Scripts.UI.Windows.Bank;
using UnityEngine;

namespace Assets.Scripts.User.MetaPayments
{
	public class ShopMetaPayment : MetaPayment
	{
		public const string TYPE = "shop";

		[JsonProperty("type")]
		protected override string Type => TYPE;

		[JsonProperty("screen")]
		private bool _needScreenReward;

		public ShopMetaPayment() { }
		public ShopMetaPayment(UserBankItem bankItem, bool needScreenReward = true) : base(bankItem)
		{
			_needScreenReward = needScreenReward;
		}

		public override IPromise OnConfirm(List<ItemCount> drop)
		{
			Debug.Log("[Bank] BuySuccess");
			
			var bankWindow = Game.Windows.GetOpenWindow<BankWindow>();

			if (bankWindow)
				bankWindow.Close();
			
			Game.User.Items.AddItems(drop);
			Game.HUD.DropController.DropItems(drop);
			
			return Promise.Resolved();
		}

		public override IPromise OnCancel()
		{
			Debug.Log("[Bank] FailedToBuy (Error)");

			Finally();

			return Promise.Resolved();
		}

		private void Finally()
		{
			Game.Locker.Unlock("bankBuy");
		}
	}
}