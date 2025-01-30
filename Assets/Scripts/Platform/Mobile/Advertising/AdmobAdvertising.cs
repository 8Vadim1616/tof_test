#if !UNITY_IOS

using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;
using Assets.Scripts.Utils;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Network.Queries.Operations.Api;
using Assets.Scripts.User.Ad;
using Assets.Scripts.User.Ad.Points;
using GoogleMobileAds.Common;
using Newtonsoft.Json;

namespace Assets.Scripts.Platform.Mobile.Advertising
{
    public class AdmobAdvertising : AbstractMobileAdvertising
    {
        public static NativeAd NativeAd;
        public static AdValue NativeAdValue;
		public const string NAME = "admob";
        
        private const string GA_RWRD = "ga_rwrd";
		private const string GA_RWRD_INT = "ga_rwrd_int"; 
		private const string GA_INT = "ga_int";
        private const string GA_NATIVE_BANNER = "ga_nbanner";
		private const string GA_BANNER = "ga_bn";
		private const string GA_APP_OPEN = "app_open";

		protected override string Name => NAME;
#if UNITY_WEBGL || UNITY_WSA
		public override void UpdateData(Dictionary<string, string> data) { throw new NotImplementedException(); }

		protected override void StartShowInterstitial() { throw new NotImplementedException(); }

		protected override void StartShowReward() { throw new NotImplementedException(); }

		protected override void StartShowRewardedInterstitial() { throw new NotImplementedException(); }
#else
		private string _adMobRewardId;
		private string _adMobRewardedInterstitialId;
		private string _adMobInterstitialId;
		private string _adMobAppOpenId;
		private string _adMobNativeBannerId;

		private AppOpenAd _appOpenAd;
		private RewardedAd _rewardAd;
		private RewardedInterstitialAd _rewardedInterstitialAd;
		private InterstitialAd _interstitial;

		private bool _wasAsyncInited;
		private BannerView _bannerView;
		
		private DateTime _expireTime;
		
		public override bool IsNativeBannerPosible => NativeAd != null || Application.isEditor;

		public override void UpdateData(Dictionary<string, string> data)
		{
			var serverAdMobRewardId = data?.GetValue(GA_RWRD, null);
			var serveradMobInterstitialId = data?.GetValue(GA_INT, null);
			var serveradMobRewardedInterstitialId = data?.GetValue(GA_RWRD_INT, null);
			var serverBannerId = data?.GetValue(GA_BANNER, null);
			var serverAppOpenId = data?.GetValue(GA_APP_OPEN, null);
			var serverNativeBannerId = data?.GetValue(GA_NATIVE_BANNER, null);
			
			//serverAppOpenId = "ca-app-pub-3940256099942544/9257395921";

			bool needUpdateReward = !serverAdMobRewardId.IsNullOrEmpty() && _adMobRewardId != serverAdMobRewardId;
			bool needUpdateInterstitial = !serveradMobInterstitialId.IsNullOrEmpty() && _adMobInterstitialId != serveradMobInterstitialId;
			bool needUpdateRewardedInterstitial = !serveradMobRewardedInterstitialId.IsNullOrEmpty() && _adMobRewardedInterstitialId != serveradMobRewardedInterstitialId;
			bool needUpdateBanner = !serverBannerId.IsNullOrEmpty() && BannerUnitId != serverBannerId;
			bool needUpdateAppOpen = !serverAppOpenId.IsNullOrEmpty() && _adMobAppOpenId != serverAppOpenId;
			bool needUpdateNativeBanner = !serverNativeBannerId.IsNullOrEmpty() && _adMobNativeBannerId != serverNativeBannerId;

			bool needUpdate = needUpdateReward 
				|| needUpdateInterstitial 
				|| needUpdateRewardedInterstitial
				|| needUpdateBanner
				|| needUpdateAppOpen
				|| needUpdateNativeBanner;

			var isReinit = Inited && needUpdate;

			if (!needUpdate)
				return;

			if (data == null)
			{
				Debug.Log(TAG + "Advert params not set");
				return;
			}

			if (!serverAdMobRewardId.IsNullOrEmpty() && needUpdateReward)
			{
				_adMobRewardId = serverAdMobRewardId;
				Debug.Log($"{TAG} {GA_RWRD} = {_adMobRewardId}");
			}

			if (!serveradMobInterstitialId.IsNullOrEmpty() && needUpdateInterstitial)
			{
				_adMobInterstitialId = serveradMobInterstitialId;
				Debug.Log($"{TAG} {GA_INT} = {_adMobInterstitialId}");
			}

			if (!serveradMobRewardedInterstitialId.IsNullOrEmpty() && needUpdateRewardedInterstitial)
			{
				_adMobRewardedInterstitialId = serveradMobRewardedInterstitialId;
				Debug.Log($"{TAG} {GA_RWRD_INT} = {_adMobRewardedInterstitialId}");
			}

			if (needUpdateBanner)
			{
				BannerUnitId = serverBannerId;
				Debug.Log($"{TAG} {GA_BANNER} = {serverBannerId}");
			}
			
			if (!serverAppOpenId.IsNullOrEmpty() && needUpdateAppOpen)
			{
				_adMobAppOpenId = serverAppOpenId;
				Debug.Log($"{TAG} {GA_APP_OPEN} = {_adMobAppOpenId}");
			}
			
			if (!serverNativeBannerId.IsNullOrEmpty() && needUpdateNativeBanner)
			{
				_adMobNativeBannerId = serverNativeBannerId;
				Debug.Log(TAG + $"{GA_NATIVE_BANNER} = {_adMobNativeBannerId}");
				if (Inited)
					LoadNativeBanner();
			}

			if (!Inited)
			{
				Init();
				return;
			}

			if (isReinit && _wasAsyncInited)
			{
				if (!string.IsNullOrEmpty(_adMobRewardId) && needUpdateReward)
					OnMobileAdsRewardInitialized();

				if (!string.IsNullOrEmpty(_adMobInterstitialId) && needUpdateInterstitial)
					OnMobileAdsInterstitialInitialized();

				if (!string.IsNullOrEmpty(_adMobRewardedInterstitialId) && needUpdateRewardedInterstitial)
					OnMobileAdsRewardedInterstitialInitialized();
				
				if (!string.IsNullOrEmpty(_adMobAppOpenId) && needUpdateAppOpen)
					OnMobileAdsAppOpenInitialized();
			}
		}
		
		private void OnMobileAdsInitialized()
		{
			Game.ExecuteOnMainThread(() =>
			{
				Utils.Utils.NextFrame()
					 .Then(() =>
					  {
						  if (!string.IsNullOrEmpty(_adMobNativeBannerId))
						  {
							  LoadNativeBanner();
						  }
					  });
			});
		}
		
		private void Log(string action, EventArgs args)
		{
			Game.ExecuteOnMainThread(() =>
			{
				Debug.Log(TAG + action + "; args = " + (args != null ? JsonConvert.SerializeObject(args) : ""));
			});
		}
		
		public void LoadNativeBanner()
		{
			if (!Inited || _adMobNativeBannerId.IsNullOrEmpty())
			{
				Debug.Log(TAG + "LoadNativeBanner failed. Banner is not inited");
				return;
			}
            
			Debug.Log(TAG + "LoadNativeBanner");
			AdLoader adLoader = new AdLoader.Builder(_adMobNativeBannerId)
							   .ForNativeAd()
							   .Build();
            
			adLoader.OnNativeAdClicked += (sender, args) => Log("OnNativeAdClicked", args);
			adLoader.OnNativeAdClosed += (sender, args) => Log("OnNativeAdClosed", args);
			adLoader.OnNativeAdImpression += (sender, args) => Log("OnNativeAdImpression", args);
			adLoader.OnNativeAdOpening += (sender, args) => Log("OnNativeAdOpening", args);
			adLoader.OnAdFailedToLoad += (sender, args) => Log("OnAdFailedToLoad", args);
			adLoader.OnNativeAdLoaded += (sender, args) =>
			{
				NativeAd = args.nativeAd;
				args.nativeAd.OnPaidEvent += (o, eventArgs) =>
				{
					NativeAdValue = eventArgs.AdValue;
					ServerLogs.SendLog("nbanner", GetAdOptions());
					Log("OnPaidEvent", eventArgs);
				};
				Log("OnNativeAdLoaded", args);
			};
            
			NativeAd = null;
			NativeAdValue = null;
			adLoader.LoadAd(new AdRequest());
		}
		
		public void OnNativeAdShow(UserAdType point)
		{
			Game.QueryManager.MultiRequest(new AdShowOperation((int) point, (int) point,
																	null, needInfo: false, adParams: GetAdOptions()));
		}
		
		public Dictionary<string, object> GetAdOptions()
		{
			if (NativeAdValue != null)
			{
				return new Dictionary<string, object>
				{
					{ "currencyCode", NativeAdValue.CurrencyCode },
					{ "precision", NativeAdValue.Precision },
					{ "value", NativeAdValue.Value }
				};
			}

			return null;
		}

		protected override void StartShowInterstitial()
		{
			throw new NotImplementedException();
		}

		protected override void StartShowReward()
		{
			throw new NotImplementedException();
		}

		protected override void StartShowRewardedInterstitial()
		{
			throw new NotImplementedException();
		}

		// public static void SetTestDevice(string deviceId)
		// {
		// 	var requestConfiguration = new RequestConfiguration
		// 											   .Builder()
		// 							  .SetTestDeviceIds(new List<string> { deviceId })
		// 							  .build();
		// 	MobileAds.SetRequestConfiguration(requestConfiguration);
		// 	Debug.Log($"Device id '{deviceId}' sended");
		// }

		private void Init()
		{
			Debug.Log(TAG + "Init");
			Inited = true;
			
			AppStateEventNotifier.AppStateChanged += OnAppStateChanged;

#if UNITY_IOS && !UNITY_EDITOR
            //включаем отслеживание рекламы через SkAdNetwork для AudienceNetwork
            // if (Game.User.AudienceTrackingEnabled)
                AudienceSettings.SetAdvertiserTrackingEnabled(true);
#endif

//			if (BuildSettings.BuildSettings.IsTest)
//				SetTestDevice("FD4FA58406BED5723539C5BA32458B24");
			
			var initPromise = new Promise();

			Game.Instance.LoadNativeLibrariesPromise = Game.Instance.LoadNativeLibrariesPromise
				.Then(() =>
				{
					MobileAds.Initialize(initStatus =>
					{
						Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();
						foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
						{
							string className = keyValuePair.Key;
							AdapterStatus status = keyValuePair.Value;
							switch (status.InitializationState)
							{
								case AdapterState.NotReady:
									Debug.Log(TAG + "Adapter: " + className + " not ready.");
									SendLog("Adapter: " + className + " not ready.");
									break;
								case AdapterState.Ready:
									Debug.Log(TAG + "Adapter: " + className + " is initialized.");
									SendLog("Adapter: " + className + " is initialized.");
									break;
							}
						}

						Debug.Log(TAG + "Initialized async");
						_wasAsyncInited = true;

						OnMobileAdsRewardInitialized();
						OnMobileAdsInterstitialInitialized();
						OnMobileAdsRewardedInterstitialInitialized();
						OnMobileAdsAppOpenInitialized();
						OnMobileAdsInitialized();
						
						initPromise.ResolveOnce();
					});
				})
				.Then(() => initPromise);
		}

		public IPromise TryShowAppStartAd()
		{
			Debug.Log(TAG + "TryShowAppStartAd");
			LoadAppOpenAd();

			if (!IsAppOpenAdAvailable())
				return Promise.Resolved();

			return Utils.Utils.Wait(3)
						.Then(ShowAppOpenAd);
		}

		private void OnMobileAdsAppOpenInitialized()
		{
		}
		
		/// <summary>
		/// Loads the app open ad.
		/// </summary>
		private void LoadAppOpenAd()
		{
			Debug.Log(TAG + "LoadAppOpenAd");
			// Clean up the old ad before loading a new one.
			if (_appOpenAd != null)
			{
				_appOpenAd.Destroy();
				_appOpenAd = null;
			}

			CurrentAppOpenState.Value = State.LOADING;
			IsAppOpenLoadedReactive.Value = false;

			Debug.Log(TAG + "Loading the app open ad.");
			Game.ExecuteOnMainThread(ServerLogs.AppOpenAdLoadingStarted);

			// Create our request used to load the ad.
			var adRequest = new AdRequest();
		
			// send the request to load the ad.
			AppOpenAd.Load(_adMobAppOpenId, adRequest, (ad, error) =>
			{
				if (error != null || ad == null)
				{
					CurrentAppOpenState.Value = State.FAILED;
					Debug.Log(TAG + "app open ad failed to load an ad with error : " + error);
					return; 
				}
		
				_expireTime = DateTime.Now + TimeSpan.FromHours(4);
				_appOpenAd = ad;
				IsAppOpenLoadedReactive.Value = true;
				CurrentAppOpenState.Value = State.LOADED;
			
				RegisterEventHandlers(ad);
		
				Debug.Log(TAG + "App open ad loaded with response : " + ad.GetResponseInfo());
				Game.ExecuteOnMainThread(ServerLogs.AppOpenAdLoadingFinished);
			});
		}
		
		public virtual void ShowAppOpenAd()
		{
			if (!IsAppOpenAdAvailable())
				return;
		
			Debug.Log(TAG + "Showing app open ad");
			Game.ExecuteOnMainThread(ServerLogs.AppOpenAdShowingStarted);
			
			_appOpenAd.Show();
		}
		
		private void RegisterEventHandlers(AppOpenAd ad)
		{
			// Raised when the ad is estimated to have earned money.
			ad.OnAdPaid += adValue =>
			{
				Debug.Log(TAG + $"App open ad paid {adValue.Value} {adValue.CurrencyCode}.");
				Game.ExecuteOnMainThread(() => ServerLogs.AppOpenAdShowingFinished(adValue, (int) UserAdType.AD_APP_OPEN));
			};
			// Raised when an impression is recorded for an ad.
			ad.OnAdImpressionRecorded += () => Debug.Log("App open ad recorded an impression.");
			// Raised when a click is recorded for an ad.
			ad.OnAdClicked += () => Debug.Log("App open ad was clicked.");
			// Raised when an ad opened full screen content.
			ad.OnAdFullScreenContentOpened += () => Debug.Log("App open ad full screen content opened.");
			// Raised when the ad closed full screen content.
			ad.OnAdFullScreenContentClosed += () =>
			{
				Debug.Log(TAG + "App open ad full screen content closed.");
				LoadAppOpenAd();
			};
			// Raised when the ad failed to open full screen content.
			ad.OnAdFullScreenContentFailed += error =>
			{
				Debug.Log(TAG + "App open ad failed to open full screen content with error : " + error);
				LoadAppOpenAd();
			};
		}
		
		private bool IsAppOpenAdAvailable()
		{
			var result = _appOpenAd != null
				   && _appOpenAd.CanShowAd()
				   && Game.User.Ads.IsAppOpenAdAvailable((int) UserAdType.AD_APP_OPEN);
			
			Debug.Log(TAG + "IsAppOpenAdAvailableTotal = " + result);
			Debug.Log(TAG + "_appOpenAd = " + (_appOpenAd != null));
			Debug.Log(TAG + "CanShowAd = " +  ((_appOpenAd != null) && _appOpenAd.CanShowAd()));
			Debug.Log(TAG + "IsAppOpenAdAvailable = " + Game.User.Ads.IsAppOpenAdAvailable((int) UserAdType.AD_APP_OPEN));

			return result;
		}

		private void OnDestroy()
		{
			// Always unlisten to events when complete.
			AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
		}
		
		private void OnAppStateChanged(AppState state)
		{
			Debug.Log(TAG + "App State changed to : " + state);

			// if the app is Foregrounded and the ad is available, show it.
			if (state != AppState.Foreground)
				return;
		
			Game.ExecuteOnMainThread(()=>ShowAppOpenAd());
		}

        private void OnMobileAdsRewardInitialized()
        {
            // if (!string.IsNullOrEmpty(_adMobRewardId))
            // {
            //     _rewardAd = new RewardedAd(_adMobRewardId);
            //     SubscribeRewardAd();
            //     LoadRewardAd();
            // }
		}
		private void OnMobileAdsInterstitialInitialized()
		{
			// if (!string.IsNullOrEmpty(_adMobInterstitialId))
			// {
			// 	_interstitial = new InterstitialAd(_adMobInterstitialId);
			// 	SubscribeInterstitial();
			// 	LoadInterstitialAd();
			// }
		}
		private void OnMobileAdsRewardedInterstitialInitialized()
		{
			if (!string.IsNullOrEmpty(_adMobRewardedInterstitialId))
			{
				LoadRewardedInterstitialAd();
			}
		}

		// protected override void LoadInterstitialAd()
		// {
		// 	base.LoadInterstitialAd();
		// 	if (_interstitial is null)
		// 		Debug.LogWarning("InterstitialAd is not initialized");
		//
		// 	_interstitial?.LoadAd(AdRequest);
		// }

		// protected override void LoadRewardedInterstitialAd()
		// {
		// 	base.LoadRewardedInterstitialAd();
		// 	Debug.Log("Rewarded Interstitial start to load");
		// 	RewardedInterstitialAd.LoadAd(_adMobRewardedInterstitialId, AdRequest, adLoadCallback);
		//
		// 	void adLoadCallback(RewardedInterstitialAd ad, AdFailedToLoadEventArgs error)
		// 	{
		// 		if (error == null)
		// 		{
		// 			if (_rewardedInterstitialAd != null)
		// 			{
		// 				_rewardedInterstitialAd.OnAdDidDismissFullScreenContent -= HandleOnAdDidDismissFullScreenContent;
		// 				_rewardedInterstitialAd.OnAdDidPresentFullScreenContent -= HandleOnAdDidPresentFullScreenContent;
		// 				_rewardedInterstitialAd.OnAdDidRecordImpression -= HandleOnAdDidRecordImpression;
		// 				_rewardedInterstitialAd.OnAdFailedToPresentFullScreenContent -= HandleOnAdFailedToPresentFullScreenContent;
		// 				_rewardedInterstitialAd.OnPaidEvent -= HandleOnPaidEvent;
		// 			}
		//
		// 			_rewardedInterstitialAd = ad;
		// 			SubscribeRewardedInterstitial();
		// 			OnRewardedInterstitialLoaded();
		// 		}
		// 		else
		// 			HandleRewardedInterstitialFailedToLoad(error);
		// 	}
		// }

		// protected override void LoadRewardAd()
  //       {
  //           base.LoadRewardAd();
		// 	if (_rewardAd is null)
		// 		Debug.LogWarning("RewardAd is not initialized");
  //
		// 	_rewardAd?.LoadAd(AdRequest);
  //       }

  //       protected override void StartShowInterstitial()
  //       {
  //           _interstitial?.Show();
		// }
  //
		// protected override void StartShowRewardedInterstitial()
		// {
		// 	_rewardedInterstitialAd.Show(HandleRewardedInterstitialRewarded);
		// }

		// protected override void StartShowReward()
  //       {
  //           _rewardAd?.Show();
  //       }

  //       private void SubscribeInterstitial()
  //       {
  //           _interstitial.OnAdLoaded += HandleOnInterstitialAdLoaded;
  //           _interstitial.OnAdFailedToLoad += HandleOnInterstitialAdFailedToLoad;
  //           _interstitial.OnAdOpening += HandleOnInterstitialAdOpened;
  //           _interstitial.OnAdClosed += HandleOnInterstitialAdClosed;
		// 	_interstitial.OnAdFailedToShow += HandleOnInterstitialFailedToShow;
		// 	_interstitial.OnAdDidRecordImpression += HandleOnInterstitialRecordImpression;
		// 	_interstitial.OnPaidEvent += HandleOnInterstitialPaidEvent;
  //       }
  //       
  //       private void SubscribeRewardAd()
  //       {
  //           _rewardAd.OnAdLoaded += HandleRewardBasedVideoLoaded;
  //           _rewardAd.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
  //           _rewardAd.OnAdOpening += HandleRewardBasedVideoOpened;
  //           _rewardAd.OnUserEarnedReward += HandleRewardBasedVideoRewarded;
  //           _rewardAd.OnAdClosed += HandleRewardBasedVideoClosed;
		// 	_rewardAd.OnAdFailedToShow += HandleRewardFailedToShow;
		// 	_rewardAd.OnAdDidRecordImpression += HandleRewardDidRecordImpression;
		// 	_rewardAd.OnPaidEvent += HandleRewardPaidEvent;
		// }
  //
		// private void SubscribeRewardedInterstitial()
		// {
		// 	_rewardedInterstitialAd.OnAdDidDismissFullScreenContent += HandleOnAdDidDismissFullScreenContent;
		// 	_rewardedInterstitialAd.OnAdDidPresentFullScreenContent += HandleOnAdDidPresentFullScreenContent;
		// 	_rewardedInterstitialAd.OnAdDidRecordImpression += HandleOnAdDidRecordImpression;
		// 	_rewardedInterstitialAd.OnAdFailedToPresentFullScreenContent += HandleOnAdFailedToPresentFullScreenContent;
		// 	_rewardedInterstitialAd.OnPaidEvent += HandleOnPaidEvent;
		// }
		
		private void SubscribeBanner()
		{
			if (_bannerView == null)
				return;
			
			_bannerView.OnBannerAdLoaded += HandleBannerAdLoaded;
			_bannerView.OnBannerAdLoadFailed += HandleBannerAdFailed;
			_bannerView.OnAdFullScreenContentOpened += HandleBannerAdOpened;
			_bannerView.OnAdFullScreenContentClosed += HandleBannerAdClosed;
			_bannerView.OnAdPaid += HandleBannerPaidEvent;
		}
		
		public override void LoadBannerAd(AdSizeType adSize, AdPosition position)
		{
			base.LoadBannerAd(adSize, position);

			//Debug.Log(TAG + $"banner pos x: {posX}, y: {posY} (banner canvas H: {GetBannerCanvasHeight()}, banner H: {GetBannerHeight()})");
			//bannerView = new BannerView(adUnitId, BannerSize, posX, posY);

			AdSize admobAdSize;
			if (adSize == AdSizeType.AnchoredAdaptive)
				admobAdSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
			else
				admobAdSize = AdSize.Banner;
			
			_bannerView = new BannerView(BannerUnitId, admobAdSize, (GoogleMobileAds.Api.AdPosition) position);
			
			SubscribeBanner();
			
			_bannerView.LoadAd(new AdRequest());
		}

		// private void HandleOnInterstitialAdLoaded(object sender, EventArgs args) => Game.ExecuteOnMainThread(() => OnInterstitialLoaded());
  //       private void HandleOnInterstitialAdOpened(object sender, EventArgs args) => Game.ExecuteOnMainThread(() => OnInterstitialOpened());
  //       private void HandleOnInterstitialAdClosed(object sender, EventArgs args) => Game.ExecuteOnMainThread(() => OnInterstitialClosed());
		// private void HandleOnInterstitialRecordImpression(object sender, EventArgs args) => Game.ExecuteOnMainThread(() => OnInterstitialRecordImpression());
		// private void HandleOnInterstitialFailedToShow(object sender, AdErrorEventArgs args) => Game.ExecuteOnMainThread(() => OnInterstitialFailedToShow($"[{args.AdError.GetCode()}] {args.AdError.GetMessage()}"));
		// private void HandleOnInterstitialPaidEvent(object sender, AdValueEventArgs args) => Game.ExecuteOnMainThread(() => OnInterstitialPaidEvent($"{args.AdValue.Value} [{args.AdValue.CurrencyCode}]"));
  //
		// private void HandleRewardBasedVideoLoaded(object sender, EventArgs args) => Game.ExecuteOnMainThread(() => OnRewardLoaded());
  //       private void HandleRewardBasedVideoOpened(object sender, EventArgs args) => Game.ExecuteOnMainThread(() => OnRewardOpened());
  //       private void HandleRewardBasedVideoClosed(object sender, EventArgs args) => Game.ExecuteOnMainThread(() => OnRewardClosed());
  //       private void HandleRewardBasedVideoRewarded(object sender, Reward args) => Game.ExecuteOnMainThread(() => OnRewarded(args.Amount + " " + args.Type));
		// private void HandleRewardFailedToShow(object sender, AdErrorEventArgs args) => Game.ExecuteOnMainThread(() => OnRewardFailedToShow($"[{args.AdError.GetCode()}] {args.AdError.GetMessage()}"));
		// private void HandleRewardDidRecordImpression(object sender, EventArgs args) => Game.ExecuteOnMainThread(() => OnRewardRecordImpression());
		// private void HandleRewardPaidEvent(object sender, AdValueEventArgs args) => Game.ExecuteOnMainThread(() => OnRewardPaidEvent($"{args.AdValue.Value} [{args.AdValue.CurrencyCode}]"));
  //
		// private void HandleOnAdDidDismissFullScreenContent(object sender, EventArgs args) => Game.ExecuteOnMainThread(() => OnRewardedInterstitialDismiss());
		// private void HandleOnAdDidPresentFullScreenContent(object sender, EventArgs args) => Game.ExecuteOnMainThread(() => OnRewardedInterstitialPresentFullScreenContent());
		// private void HandleOnAdDidRecordImpression(object sender, EventArgs args) => Game.ExecuteOnMainThread(() => OnRewardedInterstitialRecordImpression());
		// private void HandleOnAdFailedToPresentFullScreenContent(object sender, AdErrorEventArgs args) => Game.ExecuteOnMainThread(() => OnRewardedInterstitialFailedFullScreen());
		// private void HandleOnPaidEvent(object sender, AdValueEventArgs args) => Game.ExecuteOnMainThread(() => OnRewardedInterstitialPaidEvent());
		// private void HandleRewardedInterstitialRewarded(Reward args) => Game.ExecuteOnMainThread(() => OnRewardedInterstitialRewarded(args.Amount + " " + args.Type));

		private void HandleBannerAdLoaded() => Game.ExecuteOnMainThread(() => OnBannerLoaded());
		private void HandleBannerAdFailed(LoadAdError e) => Game.ExecuteOnMainThread(() => OnBannerLoadFailed(e.GetMessage()));
		private void HandleBannerAdOpened() => Game.ExecuteOnMainThread(() => OnBannerOpened());
		private void HandleBannerAdClosed() => Game.ExecuteOnMainThread(() => OnBannerClosed());
		private void HandleBannerPaidEvent(AdValue args) => Game.ExecuteOnMainThread(() =>
		{
			var data = new Dictionary<string, object>
			{
				{ "currencyCode", args.CurrencyCode },
				{ "precision", args.Precision },
				{ "value", args.Value }
			};
			
			OnBannerPaidEvent(data);
		});

		private UserAdPoint AdPoint => Game.User?.Ads?.GetUserAdPoint(UserAdType.AD_BANNER);

		public override void BannerShow()
		{
			_bannerView?.Hide();
			_bannerView?.Show();
			IsBannerShown.Value = true;
		}

		public override void BannerHide()
		{
			_bannerView?.Hide();
			IsBannerShown.Value = false;
		}

		public override void BannerDestroy()
		{
			base.BannerDestroy();

			IsBannerShown.Value = false;
			
			if (_bannerView == null)
				return;
			
			_bannerView.OnBannerAdLoaded -= HandleBannerAdLoaded;
			_bannerView.OnBannerAdLoadFailed -= HandleBannerAdFailed;
			_bannerView.OnAdFullScreenContentOpened -= HandleBannerAdOpened;
			_bannerView.OnAdFullScreenContentClosed -= HandleBannerAdClosed;
			_bannerView.OnAdPaid -= HandleBannerPaidEvent;
			_bannerView?.Destroy();
			_bannerView = null;
		}

		private void HandleOnInterstitialAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
			OnInterstitialLoadFailed($"[{args.LoadAdError.GetCode()}] {args.LoadAdError.GetMessage()}");

            Debug.Log("Load error string: " + args.LoadAdError);
            Debug.Log("Response info: " + args.LoadAdError.GetResponseInfo());
        }

        private void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
			OnRewardLoadFailed($"[{args.LoadAdError.GetCode()}] {args.LoadAdError.GetMessage()}");

            Debug.Log("Load error string: " + args.LoadAdError);
            Debug.Log("Response info: " + args.LoadAdError.GetResponseInfo());
		}

		private void HandleRewardedInterstitialFailedToLoad(AdFailedToLoadEventArgs args)
		{
			OnRewardedInterstitialLoadFailed($"[{args.LoadAdError.GetCode()}] {args.LoadAdError.GetMessage()}");

			Debug.Log("Load error string: " + args.LoadAdError);
			Debug.Log("Response info: " + args.LoadAdError.GetResponseInfo());
		}
#endif
	}
}

#endif