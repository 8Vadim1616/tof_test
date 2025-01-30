using System.Collections.Generic;
using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Platform.Mobile.Advertising;
using Assets.Scripts.Static.Items;
using Assets.Scripts.User.Ad.Points;

namespace Assets.Scripts.User.Ad
{
	public class UserPartnersPoint
	{
		/** Серверная рекламная точка (5, 105, 205 ...) */
		public int ServerPoint { get; }

		/** Рекламный партнер (admob, admob2, pm8, ...) */
		public string PartnerName { get; }

		/** Доступна ли реклама с сервера */
		public bool ServAvailable { get; set; }

		public UserAdPoint UserAdPoint { get; }

		/** Награда при просмотре данной точки */
		public List<ItemCount> RewardItems { get; private set; } = new List<ItemCount>();

		/**Ключ для текста снизу YouGotItemsNewWindow*/
		public string YouGotItemsText => string.IsNullOrEmpty(YouGotItemsTextKey)
						? string.Empty
						: Game.Localize(YouGotItemsTextKey, YouGotItemsTextArgs);

		public string YouGotItemsTextKey { get; private set; }
		public string[] YouGotItemsTextArgs { get; private set; }

		public UserPartnersPoint(UserAdPoint userAdPoint, string name, int serverPoint, ServerAdReward reward)
		{
			UserAdPoint = userAdPoint;
			PartnerName = name;
			ServerPoint = serverPoint;

			if (reward != null)
			{
				RewardItems = reward.Items;
				YouGotItemsTextKey = reward.TranslKey;
				YouGotItemsTextArgs = reward.TranslArgs;
			}
		}

		public bool HasPartner => Game.AdvertisingController.Partners.ContainsKey(PartnerName);
		public AbstractMobileAdvertising Partner => Game.AdvertisingController.Partners[PartnerName];

		public bool IsAppOpenPossible => ServAvailable && HasPartner && Partner.Inited && Partner.IsAppOpenPossible;
		
		public bool IsRewardPossible => ServAvailable && HasPartner && Partner.Inited && Partner.IsRewardPossible;
		public bool IsRewardAvailable => ServAvailable && HasPartner && Partner.Inited && Partner.IsRewardLoadedReactive.Value;
		public bool IsRewardedInterstitialPossible => ServAvailable && HasPartner && Partner.Inited && Partner.IsRewardedInterstitialPossible;
		public bool IsRewardedInterstitialAvailable => ServAvailable && HasPartner && Partner.Inited && Partner.IsRewardedInterstitialLoadedReactive.Value;
		public bool IsInterstitialAvailable => ServAvailable && HasPartner && Partner.Inited && Partner.IsInterstitialLoadedReactive.Value;
		public bool IsOfferwallAvailable => ServAvailable && HasPartner && Partner.Inited && Partner.IsOfferwallLoadedReactive.Value;
		public bool IsBannerPossible => ServAvailable && HasPartner && Partner.Inited && Partner.IsBannerPossible;
		public bool IsNativeBannerPosible => ServAvailable && HasPartner && Partner.Inited && Partner.IsNativeBannerPosible;
	}
}