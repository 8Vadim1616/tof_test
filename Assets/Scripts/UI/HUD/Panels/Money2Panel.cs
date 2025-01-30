using Assets.Scripts.UI.Windows.Bank;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.HUD.Panels
{
	public class Money2Panel : ResourcePanel
	{
		[SerializeField] private GameObject _adNotify;

		private bool _iconsHided = false;

		public override LongReactiveProperty Property => Game.User.Items.GetReactiveProperty(Game.Static.Items.Money2);

		private void Start()
		{
			SetupButtonClicks(OnClick);
		}

		private void OnClick()
		{
			BankWindow.Of();
		}
	}
}
