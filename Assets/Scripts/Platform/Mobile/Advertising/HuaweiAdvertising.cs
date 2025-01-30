#if !UNITY_WEBGL && !UNITY_WSA && !UNITY_STANDALONE && BUILD_HUAWEI
using System;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using HmsPlugin;
using HuaweiConstants;
using UniRx;
using UnityEngine;
using Reward = HuaweiMobileServices.Ads.Reward;

namespace Assets.Scripts.Platform.Mobile.Advertising
{
    public class HuaweiAdvertising : AbstractMobileAdvertising
    {
        protected override string Name => "huawei";

		private const string GA_RWRD = "ga_rwrd";
		//private const string GA_RWRD_INT = "ga_rwrd_int";
		private const string GA_INT = "ga_int";
		private const string GA_BANNER = "ga_bn";

		protected bool IsRewardLoaded => HMSAdsKitManager.Instance.IsRewardedAdLoaded;
        protected bool IsInterstitialLoaded => HMSAdsKitManager.Instance.IsInterstitialAdLoaded;
        protected bool IsBannerAdLoaded => HMSAdsKitManager.Instance.IsBannerAdLoaded;

		private string _adMobRewardId;
		//private string _adMobRewardedInterstitialId;
		private string _adMobInterstitialId;

		public override void UpdateData(Dictionary<string, string> data)
        {
            var serverAdMobRewardId = data?.GetValue(GA_RWRD, null);
			var serveradMobInterstitialId = data?.GetValue(GA_INT, null);
			//var serveradMobRewardedInterstitialId = data?.GetValue(GA_RWRD_INT, null);
			var serverBannerId = data?.GetValue(GA_BANNER, null);

			bool needUpdateReward = !serverAdMobRewardId.IsNullOrEmpty() && _adMobRewardId != serverAdMobRewardId;
			bool needUpdateInterstitial = !serveradMobInterstitialId.IsNullOrEmpty() && _adMobInterstitialId != serveradMobInterstitialId;
			//bool needUpdateRewardedInterstitial = !serveradMobRewardedInterstitialId.IsNullOrEmpty() && _adMobRewardedInterstitialId != serveradMobRewardedInterstitialId;
			bool needUpdateBanner = !serverBannerId.IsNullOrEmpty() && BannerUnitId != serverBannerId;

			bool needUpdate = needUpdateReward
				|| needUpdateInterstitial
				//|| needUpdateRewardedInterstitial
				|| needUpdateBanner;

			var isReinit = Inited && needUpdate;

			if (!needUpdate)
				return;

			if (!gameObject.GetComponent<HMSAdsKitManager>())
				gameObject.AddComponent<HMSAdsKitManager>();

			if (!Inited)
				Init();

			if (data == null)
			{
				Debug.Log(TAG + "Advert params not set");
				return;
			}

			if (!serverAdMobRewardId.IsNullOrEmpty() && needUpdateReward)
			{
				_adMobRewardId = serverAdMobRewardId;
				Debug.Log($"{TAG} {GA_RWRD} = {_adMobRewardId}");
				LoadRewardAd();
			}

			if (!serveradMobInterstitialId.IsNullOrEmpty() && needUpdateInterstitial)
			{
				_adMobInterstitialId = serveradMobInterstitialId;
				Debug.Log($"{TAG} {GA_INT} = {_adMobInterstitialId}");
				LoadInterstitialAd();
			}

			/*if (!serveradMobRewardedInterstitialId.IsNullOrEmpty() && needUpdateRewardedInterstitial)
			{
				_adMobRewardedInterstitialId = serveradMobRewardedInterstitialId;
				Debug.Log($"{TAG} {GA_RWRD_INT} = {_adMobRewardedInterstitialId}");
			}*/

			if (needUpdateBanner)
			{
				BannerUnitId = serverBannerId;
				Debug.Log($"{TAG} {GA_BANNER} = {serverBannerId}");
			}
		}

		private void Init()
		{
			Inited = true;
			Debug.Log(TAG + "Init");

			SubscribeInterstitial();
			SubscribeRewardAd();

			//BannerUnitId = HMSAdsKitSettings.BannerAdID;

			SubscribeBanner();
			// LoadBannerAd();
		}

		private void SubscribeInterstitial()
        {
			UnsubscribeInterstitial();

			HMSAdsKitManager.Instance.OnInterstitialAdClosed = HandleInterstitialAdClosed;
            HMSAdsKitManager.Instance.OnInterstitialAdClicked = HandleInterstitialAdClicked;
            HMSAdsKitManager.Instance.OnInterstitialAdFailed = HandleInterstitialAdFailed;
            HMSAdsKitManager.Instance.OnInterstitialAdImpression = HandleInterstitialAdImpression;
            HMSAdsKitManager.Instance.OnInterstitialAdLeave = HandleInterstitialAdLeave;
            HMSAdsKitManager.Instance.OnInterstitialAdLoaded = HandleInterstitialAdLoaded;
            HMSAdsKitManager.Instance.OnInterstitialAdOpened = HandleInterstitialAdOpened;
        }

		private void UnsubscribeInterstitial()
		{
			HMSAdsKitManager.Instance.OnInterstitialAdClosed = null;
			HMSAdsKitManager.Instance.OnInterstitialAdClicked = null;
			HMSAdsKitManager.Instance.OnInterstitialAdFailed = null;
			HMSAdsKitManager.Instance.OnInterstitialAdImpression = null;
			HMSAdsKitManager.Instance.OnInterstitialAdLeave = null;
			HMSAdsKitManager.Instance.OnInterstitialAdLoaded = null;
			HMSAdsKitManager.Instance.OnInterstitialAdOpened = null;
		}

		IDisposable _loadRewardSub;
        private void SubscribeRewardAd()
        {
			UnsubscribeRewardAd();

			HMSAdsKitManager.Instance.OnRewardAdClosed = HandleRewardAdClosed;
            HMSAdsKitManager.Instance.OnRewardAdOpened = HandleRewardAdOpened;
            HMSAdsKitManager.Instance.OnRewardAdFailedToShow = HandleRewardAdFailedToShow;
            HMSAdsKitManager.Instance.OnRewarded = HandleOnRewarded;

			_loadRewardSub = Observable.Interval(TimeSpan.FromMilliseconds(1000))
                      .Subscribe(x => CheckLoadReward())
                      .AddTo(this);
        }

		private void UnsubscribeRewardAd()
		{
			HMSAdsKitManager.Instance.OnRewardAdClosed = null;
			HMSAdsKitManager.Instance.OnRewardAdOpened = null;
			HMSAdsKitManager.Instance.OnRewardAdFailedToShow = null;
			HMSAdsKitManager.Instance.OnRewarded = null;

			_loadRewardSub?.Dispose();
		}

		private void SubscribeBanner()
		{
			UnsubscribeBanner();

			HMSAdsKitManager.Instance.OnBannerFailedToLoadEvent += HandleBannerAdFailed;
			HMSAdsKitManager.Instance.OnBannerLoadEvent += HandleBannerAdLoaded;
			HMSAdsKitManager.Instance.OnBannerOpening += HandleBannerAdOpened;
			HMSAdsKitManager.Instance.OnBannerClosed += HandleBannerAdClosed;
		}

		private void UnsubscribeBanner()
		{
			HMSAdsKitManager.Instance.OnBannerFailedToLoadEvent -= HandleBannerAdFailed;
			HMSAdsKitManager.Instance.OnBannerLoadEvent -= HandleBannerAdLoaded;
			HMSAdsKitManager.Instance.OnBannerOpening -= HandleBannerAdOpened;
			HMSAdsKitManager.Instance.OnBannerClosed -= HandleBannerAdClosed;
		}

		protected override void LoadInterstitialAd()
        {
			if (string.IsNullOrEmpty(_adMobInterstitialId))
				return;
			
			base.LoadInterstitialAd();
			if (HMSAdsKitManager.Instance.IsInterstitialAdLoaded)
			{
				Debug.Log("[HMS] HMSAdsKitManager Interstitial Ad Already Loaded.");
				IsInterstitialLoadedReactive.Value = true;
			}
			HMSAdsKitManager.Instance.LoadInterstitialAd();
        }

        protected override void LoadRewardAd()
        {
			if (string.IsNullOrEmpty(_adMobRewardId))
				return;

			base.LoadRewardAd();
			if (HMSAdsKitManager.Instance.IsRewardedAdLoaded)
			{
				Debug.Log("[HMS] HMSAdsKitManager Reward Ad Already Loaded.");
				IsInterstitialLoadedReactive.Value = true;
			}
			HMSAdsKitManager.Instance.LoadRewardedAd();
        }
		
		public override void LoadBannerAd(AdSizeType adSize, AdPosition position)
		{
			base.LoadBannerAd(adSize, position);
			HMSAdsKitManager.Instance.LoadBannerAd(ConvertAdmobPositionToHuawei(position), ConvertAdmobSizeToHuawei(adSize));
		}

		protected override void StartShowRewardedInterstitial() { throw new NotImplementedException(); }

		protected override void StartShowInterstitial() => HMSAdsKitManager.Instance.ShowInterstitialAd();
        protected override void StartShowReward() => HMSAdsKitManager.Instance.ShowRewardedAd();

        private void HandleInterstitialAdOpened() => OnInterstitialOpened();
        private void HandleInterstitialAdLoaded() => OnInterstitialLoaded();
        private void HandleInterstitialAdFailed(int errCode) => OnInterstitialLoadFailed("errCode: " + errCode);
        private void HandleInterstitialAdClosed() => OnInterstitialClosed();
        
        private void HandleInterstitialAdLeave() => Debug.Log(TAG + "Interstitial Ad Leave");
        private void HandleInterstitialAdImpression() => Debug.Log(TAG + "Interstitial Ad Impression");
        private void HandleInterstitialAdClicked() => Debug.Log(TAG + "Interstitial Ad Clicked");

        private void HandleRewardAdFailedToShow(int obj) => OnRewardLoadFailed("errCode: " + obj);
        private void HandleRewardAdClosed() => OnRewardClosed();
        private void HandleRewardAdOpened() => OnRewardOpened();
        private void HandleOnRewarded(Reward reward) => OnRewarded(reward.Name + " " + reward.Amount);
		
		private void HandleBannerAdLoaded() => OnBannerLoaded();
		private void HandleBannerAdFailed() => OnBannerLoadFailed("Error");
		private void HandleBannerAdOpened() => OnBannerOpened();
		private void HandleBannerAdClosed() => OnBannerClosed();

		public override void BannerShow() => HMSAdsKitManager.Instance.ShowBannerAd();
		public override void BannerHide() => HMSAdsKitManager.Instance.HideBannerAd();
		public override void BannerDestroy()
		{
			base.BannerDestroy();
			HMSAdsKitManager.Instance.DestroyBannerAd();
		}


		private bool _wasRewardLoad;
        private void CheckLoadReward()
        {
            if (IsRewardLoaded != _wasRewardLoad)
            {
                _wasRewardLoad = IsRewardLoaded;
                if(_wasRewardLoad)
                    OnRewardLoaded();
            }
        }

		private UnityBannerAdPositionCode.UnityBannerAdPositionCodeType ConvertAdmobPositionToHuawei(AdPosition admobPosition)
		{
			switch (admobPosition)
			{
				case AdPosition.Top:
					return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_TOP;
				case AdPosition.Bottom:
					return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_BOTTOM;
				case AdPosition.TopLeft:
					return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_TOP_LEFT;
				case AdPosition.TopRight:
					return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_TOP_RIGHT;
				case AdPosition.BottomLeft:
					return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_BOTTOM_LEFT;
				case AdPosition.BottomRight:
					return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_BOTTOM_RIGHT;
				case AdPosition.Center:
					return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_CENTER;
				default:
					return UnityBannerAdPositionCode.UnityBannerAdPositionCodeType.POSITION_CUSTOM;
			}
		}
		
		private string ConvertAdmobSizeToHuawei(AdSizeType admobSize)
		{
			switch (admobSize)
			{
				case AdSizeType.Standard:
					return UnityBannerAdSize.BANNER_SIZE_320_50;
				case AdSizeType.SmartBanner:
					return UnityBannerAdSize.BANNER_SIZE_SMART;
				case AdSizeType.AnchoredAdaptive:
					return UnityBannerAdSize.BANNER_SIZE_DYNAMIC;
				default:
					return UnityBannerAdSize.BANNER_SIZE_468_60;
			}
		}
    }
}
#endif