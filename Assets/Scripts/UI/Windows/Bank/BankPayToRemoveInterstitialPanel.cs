using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Windows.Bank
{
	public class BankPayToRemoveInterstitialPanel : MonoBehaviour
	{
		[SerializeField] private TMP_Text _text;

		public void Init()
		{
#if UNITY_IOS

			gameObject.SetActive(false);
			return;

#endif
			
			_text.text = "pay_to_remove_interstitial".Localize();
			gameObject.SetActive(Game.User.Bank.UserBuyWeight == 0);
		}
	}
}