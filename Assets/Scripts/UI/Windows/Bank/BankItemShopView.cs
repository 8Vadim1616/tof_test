using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.General;
using Assets.Scripts.User.BankPacks;
using Assets.Scripts.Utils;
using TMPro;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.Windows.Bank
{
	public class BankItemShopView : BankItemView
	{
		[SerializeField] private ItemCountView _price;
		[SerializeField] private TMP_Text _timer;
		
		public override void Init(BankWindow bankWindow, UserBankPackItem bankPackItem)
		{
			base.Init(bankWindow, bankPackItem);
			
			if (bankPackItem.ShopItem.Price != null)
			{
				_price.SetActive(true);
				_price.SetItemCount(bankPackItem.ShopItem.Price);
				_timer.SetActive(false);
			}
			else
			{
				_price.SetActive(false);
				_timer.SetActive(true);

				GameTime.Subscribe(OnTimer).AddTo(this);
			}

			_count.text = BankPackItem.ShopItem.Count.ToKiloFormat();
			GetComponent<BasicButton>().SetOnClick(()=>
			{
				OnBuyClick();
				OnTimer();
			});
		}

		private void OnTimer()
		{
			if (Game.User == null)
				return;
			
			var timeLeft = Game.User.Bank.GetTimeLeftForFreeShopItem(BankPackItem.ShopItem);

			if (timeLeft == 0)
				_timer.text = "free".Localize();
			else
				_timer.text = timeLeft.GetNumericTime();
		}
	}
}