using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Utils;
using DG.Tweening;

namespace Assets.Scripts.User.Ad.Points
{
	public class UserAdPoint
	{
		public string AdvertType { get; }

		public int Id { get; }

		public int TimeWhenAvailable { get; private set; }

		private List<UserPartnersPoint> Partners = new List<UserPartnersPoint>();

		public bool IsAppOpen => AdvertType == ServerAd.APP_OPEN;
		public bool IsReward => AdvertType == ServerAd.REWARD;
		public bool IsRewardedInterstitial => AdvertType == ServerAd.REWARDED_INTERSTITIAL;
		public bool IsInterstitial => AdvertType == ServerAd.INTERSTITIAL;
		public bool IsOfferwall => AdvertType == ServerAd.OFFERWALL;
		public bool IsBanner => AdvertType == ServerAd.BANNER;
		public bool IsNativeBanner => AdvertType == ServerAd.NATIVE_BANNER;

		public bool IsDirty { get; private set; }
		public bool IsDislined { get; private set; }

		public UserAdPoint(int id, ServerAd data)
		{
			Id = id;
			AdvertType = data.AdvertType;

			Update(data);
		}

		public virtual void Update(ServerAd data)
		{
			if (data.Time.HasValue)
				TimeWhenAvailable = data.Time.Value;

			if (data.Partners != null)
			{
				Partners.Clear();
				for (var priority = 0; priority < data.Partners.Length; priority++)
				{
					var adPartner = data.Partners[priority];
					var userAdPartner = new UserPartnersPoint(this, adPartner.Name, adPartner.PointId, adPartner.Reward);
					userAdPartner.ServAvailable = adPartner.Available;
					Partners.Add(userAdPartner);
				}
			}

			IsDirty = false;
		}

		/**
		 * Через которое время реклама будет доступна на данной точке
		 * @return
		 */
		public long GetTimeLeftToAvailable() => Math.Max(0, TimeWhenAvailable - GameTime.Now);

		/**
		 * Ограничения для показа рекламы
		 * @return возвращает true если ограничение не выполняются.
		 */
		protected virtual bool IsAvailableByTime => GetTimeLeftToAvailable() <= 0;

		public UserPartnersPoint GetAvailableAppOpenPartner() => IsAppOpen ? Partners.FirstOrDefault(p => p.IsAppOpenPossible) : null;

		public UserPartnersPoint GetAvailableRewardPartner() => IsReward ? Partners.FirstOrDefault(p => p.IsRewardPossible) : null;
		public UserPartnersPoint GetLoadedRewardPartner() => IsReward ? Partners.FirstOrDefault(p => p.IsRewardAvailable) : null;

		public UserPartnersPoint GetAvailableOfferwallPartner() => IsOfferwall ? Partners.FirstOrDefault(p => p.IsOfferwallAvailable) : null;
		public UserPartnersPoint GetAvailableInterstitialPartner() => IsInterstitial ? Partners.FirstOrDefault(p => p.IsInterstitialAvailable) : null;

		public bool IsAvailableReward() => IsReward && IsAvailableByTime && GetAvailableRewardPartner() != null && !IsDirty && !IsDislined;
		public bool IsLoadedReward() => IsReward && IsAvailableByTime && GetLoadedRewardPartner() != null && !IsDirty && !IsDislined;
		
		public bool IsAvailableAppOpen() => IsAppOpen && GetAvailableAppOpenPartner() != null && !IsDirty && !IsDislined;


		public UserPartnersPoint GetAvailableRewardedInterstitialPartner() => IsRewardedInterstitial ? Partners.FirstOrDefault(p => p.IsRewardedInterstitialPossible) : null;
		public UserPartnersPoint GetLoadedRewardedInterstitialPartner() => IsRewardedInterstitial ? Partners.FirstOrDefault(p => p.IsRewardedInterstitialAvailable) : null;

		public bool IsAvailableRewardedInterstitial() => IsRewardedInterstitial && IsAvailableByTime && GetAvailableRewardedInterstitialPartner() != null && !IsDirty && !IsDislined;
		public bool IsLoadedRewardedInterstitial() => IsRewardedInterstitial && IsAvailableByTime && GetLoadedRewardedInterstitialPartner() != null && !IsDirty && !IsDislined;


		public UserPartnersPoint GetAvailableBannerPartner() => IsBanner ? Partners.FirstOrDefault(p => p.IsBannerPossible) : null;
		public bool IsAvailableBanner() => IsBanner && IsAvailableByTime && GetAvailableBannerPartner() != null && !IsDirty;
		
		public UserPartnersPoint GetAvailableNativeBannerPartner() => IsNativeBanner ? Partners.FirstOrDefault(p => p.IsNativeBannerPosible) : null;
		public bool IsAvailableNativeBanner() => IsNativeBanner && GetAvailableNativeBannerPartner() != null;

		public void DislinePoint()
		{
			if (IsReward)
				DislineReward();
			else if (IsInterstitial)
				DislineInterstitial();
			else if (IsRewardedInterstitial)
				DislineRewardedInterstitial();
		}

		public void DislineReward()
		{
			IsDislined = true;

			var availablePartner = GetAvailableRewardPartner()?.Partner;
			int time = 600;
			if (availablePartner)
				time = availablePartner.LoadingDislineTimeout;

			DOVirtual.DelayedCall(time, () => { IsDislined = false; });
		}

		public void DislineInterstitial()
		{
			IsDislined = true;

			var availablePartner = GetAvailableInterstitialPartner()?.Partner;
			int time = 600;
			if (availablePartner)
				time = availablePartner.LoadingDislineTimeout;

			DOVirtual.DelayedCall(time, () => { IsDislined = false; });
		}

		public void DislineRewardedInterstitial()
		{
			IsDislined = true;

			var availablePartner = GetAvailableRewardedInterstitialPartner()?.Partner;
			int time = 600;
			if (availablePartner)
				time = availablePartner.LoadingDislineTimeout;

			DOVirtual.DelayedCall(time, () => { IsDislined = false; });
		}

		public bool IsAvailableInterstitial() => IsInterstitial && IsAvailableByTime && GetAvailableInterstitialPartner() != null;

		public UserPartnersPoint GetPartner(string name) => Partners?.FirstOrDefault(x => x.PartnerName == name);

		public void SetDirty()
		{
			IsDirty = true;
		}
	}
}