#if !UNITY_WEBGL
using System;
using Assets.Scripts.Utils;
using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.User;
using com.unity3d.mediation;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Platform.Mobile.Advertising
{
	public class IronSourceServerSegment
	{
		[JsonProperty("level")]
		public int Level;
        
		[JsonProperty("user_creation_data")]
		public long UserCreationData;
        
		[JsonProperty("is_paying")]
		public int IsPaying;
		
		[JsonProperty("iap_total")]
		public double IapTotal;
	}
	
    public class IronSourceAdvertising : AbstractMobileAdvertising, ISAdQualityInitCallback
    {
        public static bool IsTestSuite;
        public const string NAME = "iron_source";

        private const string APP_KEY = "app_key_ad_quality";

        private const string IS_RWRD = "is_rwrd";
        private const string IS_INT = "is_int";
        private const string IS_BANNER = "is_bn";

        private string _ISRewardId;
        private string _ISInterstitialId;

        public const string TAG_AD_QUALITY = "[AD_QUALITY] ";

        protected override string Name => NAME;

        private string _appKey;

        private bool _isSDKInited;
        
        public bool IsAdQualityStartInit;
        public Promise AdQualityInit = new Promise();

        protected bool IsRewardAdvertLoaded => IronSource.Agent.isRewardedVideoAvailable();
        protected bool IsInterstitialLoaded => IronSource.Agent.isInterstitialReady();
		
		private LevelPlayBannerAd _bannerAd;

        public static void UpdateSegment(IronSourceServerSegment data)
        {
	        if (data == null)
		        return;

	        var segment = new IronSourceSegment();
	        segment.level = data.Level;
	        segment.userCreationDate = data.UserCreationData;
	        segment.isPaying = data.IsPaying;
	        segment.iapt = data.IapTotal;
	        IronSource.Agent.setSegment(segment);

	        var forServer = new Dictionary<string, object>();
	        forServer["level"] = data.Level;
	        forServer["user_creation_data"] = data.UserCreationData;
	        forServer["is_paying"] = data.IsPaying;
	        forServer["iap_total"] = data.IapTotal;
	        
	        ServerLogs.SendLog("iron_source_set_segment", forServer);
        }

        public override void UpdateData(Dictionary<string, string> data)
        {
            if (data == null)
            {
                Debug.Log(TAG + "Advert params not set");
                return;
            }

            if (data.ContainsKey(APP_KEY))
            {
	            _appKey = data[APP_KEY];
	            if (!IsAdQualityStartInit)
	             	InitAdQuality(_appKey);
            }

            if (IsTestSuite)
            {
                IronSource.Agent.setAdaptersDebug(true);
                IronSource.Agent.setMetaData("is_test_suite", "enable");
            }

            var serverISRewardId = data?.GetValue(IS_RWRD, null);
            var serverISInterstitialId = data?.GetValue(IS_INT, null);
            var serverBannerId = data?.GetValue(IS_BANNER, null);

            bool needUpdateReward = !serverISRewardId.IsNullOrEmpty() && _ISRewardId.IsNullOrEmpty();
            bool needUpdateInterstitial = !serverISInterstitialId.IsNullOrEmpty() && _ISInterstitialId.IsNullOrEmpty();
            bool needUpdateBanner = !serverBannerId.IsNullOrEmpty() && BannerUnitId.IsNullOrEmpty();

            if (!Inited)
            {
                AddListeners();
            }

            if (needUpdateReward)
            {
                _ISRewardId = serverISRewardId;
                Debug.Log($"{TAG}{IS_RWRD} = {_ISRewardId}");
                IronSource.Agent.init(_ISRewardId, IronSourceAdUnits.REWARDED_VIDEO);

                if (_isSDKInited)
                    LoadRewardAd();
            }

            if (needUpdateInterstitial)
            {
                _ISInterstitialId = serverISInterstitialId;
                Debug.Log($"{TAG}{IS_INT} = {_ISInterstitialId}");
                IronSource.Agent.init(_ISInterstitialId, IronSourceAdUnits.INTERSTITIAL);

                if (_isSDKInited)
                    LoadInterstitialAd();
            }
            

            if (needUpdateBanner)
            {
                BannerUnitId = serverBannerId;
                Debug.Log($"{TAG}{IS_BANNER} = {BannerUnitId}");
				_bannerAd = new LevelPlayBannerAd(BannerUnitId);
                IronSource.Agent.init(BannerUnitId, IronSourceAdUnits.BANNER);
            }

            Inited = true;
        }
      
        private void InitAdQuality(string appId)
		{
			IsAdQualityStartInit = true;

			Debug.Log(TAG_AD_QUALITY + "Start initializing");

			ISAdQualityConfig adQualityConfig = new ISAdQualityConfig();

			if (Game.User == null)
				Debug.LogWarning(TAG_AD_QUALITY + "NO USER ID FOR AD QUALITY");
			else
				adQualityConfig.UserId = Game.User.Uid;

			//adQualityConfig.TestMode = true;
			adQualityConfig.LogLevel = ISAdQualityLogLevel.VERBOSE;
			//adQualityConfig.LogLevel = ISAdQualityLogLevel.INFO;
			adQualityConfig.AdQualityInitCallback = this;

			IronSourceAdQuality.Initialize(appId, adQualityConfig);

			Game.User.RegisterData.OnChangeUid += OnChangeUserUid;
		}
        
        public void adQualitySdkInitSuccess()
        {
	        Game.ExecuteOnMainThread(() =>
	        {
		        Debug.Log(TAG_AD_QUALITY + "adQualitySdkInitSuccess");
		        AdQualityInit?.ResolveOnce();
	        });
        }
        
        public void adQualitySdkInitFailed(ISAdQualityInitError adQualityInitError, string errorMessage)
        {
	        Game.ExecuteOnMainThread(() =>
	        {
				Debug.Log(TAG_AD_QUALITY + "adQualitySdkInitFailed; error = " + errorMessage);
	        });
        }
        
        private void OnChangeUserUid(string newUid)
        {
	        if (Game.User == null || Game.User.Uid.IsNullOrEmpty())
		        return;
	        
	        var uid = Game.User.Uid;
			
			AdQualityInit
				.Then(() =>
				{
					Debug.Log($"{TAG_AD_QUALITY} UID changed to {uid}");
					IronSourceAdQuality.ChangeUserId(uid);
				});
		}

        void SdkInitializationCompletedEvent()
        {
            Game.ExecuteOnMainThread(() =>
            {
                _isSDKInited = true;
                if (IsTestSuite)
                    IronSource.Agent.launchTestSuite();

                Debug.Log($"{TAG}SdkInitializationCompletedEvent");
                
                if (!_ISRewardId.IsNullOrEmpty())
                    LoadRewardAd();

                if (!_ISInterstitialId.IsNullOrEmpty())
                    LoadInterstitialAd();
            });
        }

        protected override void LoadOfferwallAd()
        {
            base.LoadOfferwallAd();
            CheckOfferwallLoad();
        }

        protected override void LoadInterstitialAd()
        {
            base.LoadInterstitialAd();
            IronSource.Agent.loadInterstitial();
        }

        protected override void LoadRewardAd()
        {
            base.LoadRewardAd();
            CheckRewardLoad();
        }

		protected override void StartShowRewardedInterstitial()
		{
			throw new NotImplementedException();
		}

		protected override void StartShowOfferwall()
        {
            // if (IronSource.Agent.isOfferwallAvailable())
            //     IronSource.Agent.showOfferwall();
            // else
            //     OnOfferwallClosed();
        }

        protected override void StartShowInterstitial()
        {
            if (IronSource.Agent.isInterstitialReady())
                IronSource.Agent.showInterstitial();
            else
                OnInterstitialClosed();
        }

        protected override void StartShowReward()
        {
            if (IronSource.Agent.isRewardedVideoAvailable())
                IronSource.Agent.showRewardedVideo();
            else
                OnRewardClosed();
        }

        private void OnApplicationPause(bool pause)
        {
            IronSource.Agent.onApplicationPause(pause);
        }

        private void AddListeners()
        {
            if (!_isSDKInited)
            {
                IronSource.Agent.setMetaData("is_child_directed", "false");
            }
            
            //Add Init Event
            IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitializationCompletedEvent;

            //Add Rewarded Video Events
            //IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
            //IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;

            //New Rewarded Video Events
            IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoAdOpenedEvent;
            IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoAdClosedEvent;
            IronSourceRewardedVideoEvents.onAdAvailableEvent += ReardedVideoOnAdAvailable;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += ReardedVideoOnAdUnavailable;
            IronSourceRewardedVideoEvents.onAdLoadFailedEvent += RewardedVideoOnLoadFailed;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoAdShowFailedEvent;
            IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoAdRewardedEvent;
            IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoAdClickedEvent;

            //New Interstitial Events
            IronSourceInterstitialEvents.onAdReadyEvent += InterstitialAdReadyEvent;
            IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
            IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialAdOpenedEvent;
            IronSourceInterstitialEvents.onAdClickedEvent += InterstitialAdClickedEvent;
            IronSourceInterstitialEvents.onAdShowSucceededEvent += InterstitialAdShowSucceededEvent;
            IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialAdShowFailedEvent;
            IronSourceInterstitialEvents.onAdClosedEvent += InterstitialAdClosedEvent;
            
            //New Banner Events
            IronSourceBannerEvents.onAdLoadedEvent += BannerAdLoadedEvent;
            IronSourceBannerEvents.onAdLoadFailedEvent += BannerAdLoadFailedEvent;
            IronSourceBannerEvents.onAdClickedEvent += BannerAdClickedEvent;
            IronSourceBannerEvents.onAdScreenPresentedEvent += BannerAdScreenPresentedEvent;
            IronSourceBannerEvents.onAdScreenDismissedEvent += BannerAdScreenDismissedEvent;
            IronSourceBannerEvents.onAdLeftApplicationEvent += BannerAdLeftApplicationEvent;


            // IronSourceEvents.onOfferwallAvailableEvent += OnOfferwallAvailableEvent;
            // IronSourceEvents.onOfferwallClosedEvent += OnOfferwallClosedEvent;
            // IronSourceEvents.onOfferwallOpenedEvent += OnOfferwallOpenedEvent;
            // IronSourceEvents.onOfferwallAdCreditedEvent += OnOfferwallAdCreditedEvent;
            // IronSourceEvents.onOfferwallShowFailedEvent += OnOfferwallShowFailedEvent;
            // IronSourceEvents.onGetOfferwallCreditsFailedEvent += OnGetOfferwallCreditsFailedEvent;

            //Add ImpressionSuccess Event
            IronSourceEvents.onImpressionDataReadyEvent += ImpressionDataReadyEvent;
        }

        private bool _lastLoadRewardState;
        private void ReardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
        {
            Game.ExecuteOnMainThread(() => RewardedVideoAvailabilityChangedEvent(true));
        }

        private void RewardedVideoOnLoadFailed(IronSourceError error)
        {
            //Game.ExecuteOnMainThread(() => OnRewardLoadFailed("IS reward load failed: " + error));
        }

        private void ReardedVideoOnAdUnavailable()
        {
            //Game.ExecuteOnMainThread(() => OnRewardLoadFailed("IS reward unavailable"));
            Game.ExecuteOnMainThread(() => RewardedVideoAvailabilityChangedEvent(false));
        }

        private void RewardedVideoAvailabilityChangedEvent(bool canShowAd)
        {
            _lastLoadRewardState = canShowAd;
            CheckRewardLoad();
        }

        private void CheckRewardLoad()
        {
            if (_lastLoadRewardState)
                OnRewardLoaded();
        }

        private void RewardedVideoAdOpenedEvent(IronSourceAdInfo adInfo) => Game.ExecuteOnMainThread(() => OnRewardOpened(adInfo?.ToDictionary()));
        private void RewardedVideoAdRewardedEvent(IronSourcePlacement ssp, IronSourceAdInfo adInfo)
        {
	        if (adInfo != null)
	        {
		        ISAdQualityCustomMediationRevenue customMediationRevenue = new ISAdQualityCustomMediationRevenue();
		        customMediationRevenue.MediationNetwork = ISAdQualityMediationNetwork.SELF_MEDIATED;
		        customMediationRevenue.AdType = ISAdQualityAdType.REWARDED_VIDEO;
		        customMediationRevenue.Revenue = adInfo.revenue ?? 0d;

		        ServerLogs.SendLog("ad_qual", new Dictionary<string, object>()
			        {
				        { "MediationNetwork",   customMediationRevenue.MediationNetwork },
				        { "AdType",             customMediationRevenue.AdType },
				        { "Revenue",            customMediationRevenue.Revenue }
			        });

		        IronSourceAdQuality.SendCustomMediationRevenue(customMediationRevenue);
	        }

	        Game.ExecuteOnMainThread(() => OnRewarded(ssp.ToString(), adInfo?.ToDictionary()));
        }

        private void RewardedVideoAdClosedEvent(IronSourceAdInfo adInfo) => Game.ExecuteOnMainThread(() => OnRewardClosed(adInfo?.ToDictionary()));

        private void RewardedVideoAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
        {
            Game.ExecuteOnMainThread(() =>
            {
                SendLog($"{REWARD} show failed: {error.getCode()} - {error.getDescription()}",
                       new Dictionary<string, object>() { { "params", adInfo.ToDictionary() } });
                OnRewardClosed();
            });
        }

        private void RewardedVideoAdClickedEvent(IronSourcePlacement ssp, IronSourceAdInfo adInfo) =>
            Game.ExecuteOnMainThread(() => Debug.Log($"{TAG}RewardedVideoAdClickedEvent, name = {ssp.getRewardName()}, info = {adInfo?.ToDictionary()}"));

        private void RewardedVideoAdStartedEvent() => Game.ExecuteOnMainThread(() => Debug.Log(TAG + "RewardedVideoAdStartedEvent"));
        private void RewardedVideoAdEndedEvent() => Game.ExecuteOnMainThread(() => Debug.Log(TAG + "RewardedVideoAdEndedEvent"));


        private bool _lastLoadOfferwallState;
        private void OnOfferwallAvailableEvent(bool canShowAd)
        {
            Game.ExecuteOnMainThread(() =>
            {
                _lastLoadOfferwallState = canShowAd;
                CheckOfferwallLoad();
            });
        }

        private void CheckOfferwallLoad()
        {
            if (_lastLoadOfferwallState)
                OnOfferwallLoaded();
        }

        private void OnGetOfferwallCreditsFailedEvent(IronSourceError error) => Game.ExecuteOnMainThread(() => OnOfferwallLoadFailed(error.getCode() + " - " + error.getDescription()));
        private void OnOfferwallShowFailedEvent(IronSourceError error)
        {
            Game.ExecuteOnMainThread(() =>
            {
                SendLog(OFFERWALL + " show failed: " + error.getCode() + " - " + error.getDescription());
                OnOfferwallClosed();
            });
        }

        private void OnOfferwallAdCreditedEvent(Dictionary<string, object> data)
        {
            Game.ExecuteOnMainThread(() => OnOfferwallRewarded(data));
        }

        private void OnOfferwallOpenedEvent() => Game.ExecuteOnMainThread(() => OnOfferwallOpened());
        private void OnOfferwallClosedEvent()
        {
            // Game.ExecuteOnMainThread(() =>
            // {
            //     OnOfferwallClosed();
            //     IronSource.Agent.getOfferwallCredits();
            // });
        }


        private void InterstitialAdReadyEvent(IronSourceAdInfo adInfo) => Game.ExecuteOnMainThread(() => OnInterstitialLoaded(adInfo?.ToDictionary()));
        private void InterstitialAdLoadFailedEvent(IronSourceError error) => Game.ExecuteOnMainThread(() => OnInterstitialLoadFailed(error.getCode() + " - " + error.getDescription()));
        private void InterstitialAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
        {
            Game.ExecuteOnMainThread(() =>
            {
                if (adInfo == null)
                    SendLog($"{INTERSTITIAL} show failed: {error.getCode()} - {error.getDescription()}");
                else
                    SendLog($"{INTERSTITIAL} show failed: {error.getCode()} - {error.getDescription()}",
                        new Dictionary<string, object>() { { "params", adInfo.ToDictionary() } });

                OnInterstitialClosed();
            });
        }

        private void InterstitialAdOpenedEvent(IronSourceAdInfo adInfo) => Game.ExecuteOnMainThread(() => OnInterstitialOpened(adInfo?.ToDictionary()));
        private void InterstitialAdClosedEvent(IronSourceAdInfo adInfo) => Game.ExecuteOnMainThread(() => OnInterstitialClosed(adInfo?.ToDictionary()));
        private void InterstitialAdShowSucceededEvent(IronSourceAdInfo adInfo)
        {
	        if (adInfo != null)
	        {
		        ISAdQualityCustomMediationRevenue customMediationRevenue = new ISAdQualityCustomMediationRevenue();
		        customMediationRevenue.MediationNetwork = ISAdQualityMediationNetwork.SELF_MEDIATED;
		        customMediationRevenue.AdType = ISAdQualityAdType.INTERSTITIAL;
		        customMediationRevenue.Revenue = adInfo.revenue ?? 0d;

		        ServerLogs.SendLog("ad_qual", new Dictionary<string, object>()
			        {
				        { "MediationNetwork",   customMediationRevenue.MediationNetwork },
				        { "AdType",				customMediationRevenue.AdType },
				        { "Revenue",			customMediationRevenue.Revenue }
			        });

		        IronSourceAdQuality.SendCustomMediationRevenue(customMediationRevenue);
	        }

	        Game.ExecuteOnMainThread(() => Debug.Log($"{TAG}InterstitialAdShowSucceededEvent, info: {adInfo?.ToDictionary()}"));
        }
        
	    private void InterstitialAdClickedEvent(IronSourceAdInfo adInfo) => Game.ExecuteOnMainThread(() => Debug.Log($"{TAG}InterstitialAdClickedEvent, info: {adInfo?.ToDictionary()}"));

		private void ImpressionDataReadyEvent(IronSourceImpressionData impressionData)
		{
			Game.ExecuteOnMainThread(() =>
			{
				Debug.Log(TAG + "ImpressionSuccessEvent allData: " + impressionData?.allData);
				
				if (impressionData != null)
				{
					Dictionary<string, string> additionalParams = new Dictionary<string, string>()
					{
						{ "auctionId",      impressionData.auctionId },
						{ "adUnit",         impressionData.adUnit },
						{ "country",        impressionData.country },
						{ "ab",             impressionData.ab },
						{ "segmentName",    impressionData.segmentName },
						{ "placement",      impressionData.placement },
						{ "instanceName",   impressionData.instanceName },
						{ "instanceId",     impressionData.instanceId },
						{ "precision",      impressionData.precision }
					};

					if (impressionData.adUnit != null && impressionData.adUnit.Equals("banner"))
					{
						Debug.Log(TAG + "ImpressionSuccessEvent adUnit: " + impressionData.adUnit + " - Ignore!");
					}
					else
					{
						ServerLogs.SendLog("af_ad_revenue", new Dictionary<string, object>()
						{
							{ "auctionId",      impressionData.auctionId },
							{ "adUnit",         impressionData.adUnit },
							{ "country",        impressionData.country },
							{ "ab",             impressionData.ab },
							{ "segmentName",    impressionData.segmentName },
							{ "placement",      impressionData.placement },
							{ "instanceName",   impressionData.instanceName },
							{ "instanceId",     impressionData.instanceId },
							{ "precision",      impressionData.precision }
						});
						
						Debug.Log(TAG + "ImpressionSuccessEvent adUnit: " + impressionData.adUnit + " - Send!");
						Game.Mobile.Analytics.AppsFlyer?.LogAdRevenue(impressionData.adNetwork,
							AppsFlyerSDK.AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeIronSource,
							(impressionData.revenue ?? 0d), "USD", additionalParams);
					}
/*
					Game.Mobile.Analytics.AppsFlyer?.LogAdRevenue(impressionData.adNetwork,
						AppsFlyerSDK.AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeIronSource,
						(impressionData.revenue ?? 0d), "USD", additionalParams);
*/
#if !UNITY_WSA && !UNITY_WEBGL && !BUILD_AMAZON && !BUILD_HUAWEI && !BUILD_CHINA
					Game.Mobile.Analytics.Firebase?.LogIronSourceImpressionData(impressionData);
#endif
				}
			});
		}

        public override void LoadBannerAd(AdSizeType adSize, AdPosition position)
		{
			base.LoadBannerAd(adSize, position);
			IronSource.Agent.loadBanner(ConvertAdmobSizeToIronSource(adSize), ConvertAdmobPositionToIronSource(position));
		}

		public override void BannerShow()
		{
			IronSource.Agent.displayBanner();

			if (!_wasBannerShowed)
			{
				_wasBannerShowed = true;

				if (_lastBannerAdInfo == null)
					SendLog(BANNER + " opened");
				else
					SendLog(BANNER + " opened", new Dictionary<string, object>() { { "params", _lastBannerAdInfo.ToString() } });

				Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Banner opened"));
			}
		}

		public override void BannerHide()
		{
			IronSource.Agent.hideBanner();
		}

		public override void BannerDestroy()
		{
			_lastBannerAdInfo = null;
			BannerParams = null;
			_wasBannerShowed = false;
			base.BannerDestroy();
			IronSource.Agent.destroyBanner();
		}

		void BannerAdLoadedEvent(IronSourceAdInfo adInfo)
		{
			Game.ExecuteOnMainThread(() =>
			{
				_lastBannerAdInfo = adInfo;
				BannerParams = adInfo.ToDictionary();
				_wasBannerShowed = false;

				OnBannerLoaded(adInfo?.ToDictionary());
			});
		}

		void BannerAdLoadFailedEvent(IronSourceError error) =>
			Game.ExecuteOnMainThread(() => OnBannerLoadFailed($"code: {error.getCode()}, description: {error.getDescription()}"));

		void BannerAdClickedEvent(IronSourceAdInfo adInfo)
		{
			Game.ExecuteOnMainThread(() => Debug.Log($"{TAG}BannerAdClickedEvent, adInfo: {adInfo?.ToDictionary()}"));
		}

		void BannerAdScreenPresentedEvent(IronSourceAdInfo adInfo)
		{
			Game.ExecuteOnMainThread(() => Debug.Log($"{TAG}BannerAdScreenPresentedEvent, adInfo: {adInfo?.ToDictionary()}"));
		}

		void BannerAdScreenDismissedEvent(IronSourceAdInfo adInfo)
		{
			Game.ExecuteOnMainThread(() => Debug.Log($"{TAG}BannerAdScreenDismissedEvent, adInfo: {adInfo?.ToDictionary()}"));
		}

		void BannerAdLeftApplicationEvent(IronSourceAdInfo adInfo)
		{
			Game.ExecuteOnMainThread(() => Debug.Log($"{TAG}BannerAdLeftApplicationEvent, adInfo: {adInfo?.ToDictionary()}"));
		}

		private IronSourceBannerPosition ConvertAdmobPositionToIronSource(AdPosition admobPosition)
		{
			switch (admobPosition)
			{
				case AdPosition.Top:
				case AdPosition.TopRight:
				case AdPosition.TopLeft:
					return IronSourceBannerPosition.TOP;
				case AdPosition.Bottom:
				case AdPosition.BottomLeft:
				case AdPosition.BottomRight:
				default:
					return IronSourceBannerPosition.BOTTOM;
			}
		}

		private IronSourceBannerSize ConvertAdmobSizeToIronSource(AdSizeType admobSize)
		{
			IronSourceBannerSize result;
			switch (admobSize)
			{
				//Тут admobSize в половине случаев не соотносится с IronSourceBannerSize, распихал соответствия
				case AdSizeType.Standard:
					result = IronSourceBannerSize.BANNER; // 320 x 50
					break;
				case AdSizeType.SmartBanner:
					result = IronSourceBannerSize.SMART;
					break;
				case AdSizeType.AnchoredAdaptive:
					var size = IronSourceBannerSize.BANNER;
					size.SetAdaptive(true);
					result = size;
					break;
				default:
					result = IronSourceBannerSize.LARGE; // 320 x 90
					break;
			}
			
			result.SetRespectAndroidCutouts(true);

			return result;
		}


        private IronSourceAdInfo _lastBannerAdInfo;
        private bool _wasBannerShowed;
    }

    public static class IronSourceAdInfoExtention
    {
        public static Dictionary<string, object> ToDictionary(this IronSourceAdInfo adInfo)
        {
            return new Dictionary<string, object>()
            {
                { "auctionId",          adInfo.auctionId },
                { "adUnit",             adInfo.adUnit },
                { "country",            adInfo.country },
                { "ab",                 adInfo.ab },
                { "segmentName",        adInfo.segmentName },
                { "adNetwork",          adInfo.adNetwork },
                { "instanceName",       adInfo.instanceName },
                { "instanceId",         adInfo.instanceId },
                { "revenue",            adInfo.revenue },
                { "precision",          adInfo.precision },
                { "lifetimeRevenue",    adInfo.lifetimeRevenue },
                { "encryptedCPM",       adInfo.encryptedCPM }
            };
        }

        public static Dictionary<string, object> ToDictionary(this IronSourceImpressionData adInfo)
        {
            return new Dictionary<string, object>()
            {
                { "auctionId",          adInfo.auctionId },
                { "adUnit",             adInfo.adUnit },
                { "country",            adInfo.country },
                { "ab",                 adInfo.ab },
                { "segmentName",        adInfo.segmentName },
                { "placement",          adInfo.placement },
                { "adNetwork",          adInfo.adNetwork },
                { "instanceName",       adInfo.instanceName },
                { "instanceId",         adInfo.instanceId },
                { "revenue",            adInfo.revenue },
                { "precision",          adInfo.precision },
                { "lifetimeRevenue",    adInfo.lifetimeRevenue },
                { "encryptedCPM",       adInfo.encryptedCPM },
                { "conversionValue",    adInfo.conversionValue }
            };
        }
    }
}
#endif