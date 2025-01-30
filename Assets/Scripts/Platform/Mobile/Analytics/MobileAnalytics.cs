using System;
using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Platform.Mobile.Analytics.Partners;
using Assets.Scripts.User;
using Balaso;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Scripts.Platform.Mobile.Analytics
{
    public class MobileAnalytics
    {
        public const string TAG = "[MobileAnalytics] ";

        private const string IDFA_VIEW_KEY = "idfa_view";

        private static Promise<string> _iosIDFAPromise = new Promise<string>();
		
		public const string APPS_FLAYER_ANALYTICS_PARTNER = "af_event";
		public const string FIREBASE_ANALYTICS_PARTNER = "fb_event";
		public const string FACEBOOK_ANALYTICS_PARTNER = "facebook_event";

        public static readonly Dictionary<string, AnalyticsPartnerType> ServerPartnerCodes =
            new Dictionary<string, AnalyticsPartnerType>
            {
                { APPS_FLAYER_ANALYTICS_PARTNER , AnalyticsPartnerType.AppsFlayer},
                { FIREBASE_ANALYTICS_PARTNER , AnalyticsPartnerType.Firebase},
                { FACEBOOK_ANALYTICS_PARTNER , AnalyticsPartnerType.Facebook}
            };

        public Dictionary<AnalyticsPartnerType, IAnalyticsPartner> Partners { get; set; }

        public MobileAnalytics()
        {
            Partners = new Dictionary<AnalyticsPartnerType, IAnalyticsPartner>();

#if !UNITY_WSA && !UNITY_STANDALONE && !UNITY_WEBGL && !BUILD_AMAZON && !BUILD_HUAWEI && !BUILD_CHINA
			if (UserSettings.IsAppsFlyerAnalyticsAvailable)
				Partners.Add(AnalyticsPartnerType.AppsFlayer, new AppsFlyerPartner());
			if (UserSettings.IsFirebaseAnalyticsAvailable)
				Partners.Add(AnalyticsPartnerType.Firebase, new FirebaseAnalyticsPartner());
			// if (UserSettings.IsFacebookAnalyticsAvailable)
			// 	Partners.Add(AnalyticsPartnerType.Facebook, new FacebookAnalyticsPartner());
#elif BUILD_AMAZON || BUILD_HUAWEI
            Partners.Add(AnalyticsPartnerType.AppsFlayer, new AppsFlyerPartner());
            // Partners.Add(AnalyticsPartnerType.Facebook, new FacebookAnalyticsPartner());
#endif

#if (UNITY_IOS && !UNITY_EDITOR)
			AppTrackingTransparency.RegisterAppForAdNetworkAttribution();
			//AppTrackingTransparency.UpdateConversionValue(3);
#endif
		}
        
        public static IPromise<string> GetAdvertisingId()
        {
            var result = new Promise<string>();

            Debug.Log("Try get advertising id");

            if (Application.platform == RuntimePlatform.Android)
            {
                string advertisingID = null;

                try
                {
                    AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
                    AndroidJavaClass client =
                        new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
                    AndroidJavaObject adInfo =
                        client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", currentActivity);

                    advertisingID = adInfo.Call<string>("getId").ToString();

                    Debug.Log("AdvertId = " + advertisingID);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Can't get advert id on Android");
                }

                result.Resolve(advertisingID);
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                _iosIDFAPromise = new Promise<string>();

                AppTrackingTransparency.AuthorizationStatus currentStatus = AppTrackingTransparency.TrackingAuthorizationStatus;

                Debug.Log(string.Format("Current authorization status: {0}", currentStatus.ToString()));

                var alreadyViewed = PlayerPrefs.HasKey(IDFA_VIEW_KEY);

                var curIdfa = AppTrackingTransparency.IdentifierForAdvertising();

                if (curIdfa == null && !alreadyViewed)
                {
                    AppTrackingTransparency.OnAuthorizationRequestDone = OnAuthorizationRequestDone;
                    AppTrackingTransparency.RequestTrackingAuthorization();
                }
                else
                {
                    _iosIDFAPromise.Resolve(curIdfa);
                }

                _iosIDFAPromise.Then((idfa) => result.Resolve(idfa));
            }
            else
            {
                result.Resolve(null);
            }
            /*
             this is deprecated
            if (!Application.RequestAdvertisingIdentifierAsync(
                (string advertisingId, bool trackingEnabled, string error) =>
                {
                    Debug.Log("advertisingId = " + advertisingId);
                    if (string.IsNullOrEmpty(error))
                        result.Resolve(advertisingId);
                    else
                        result.Reject(new Exception("Can't get advertisiong id"));
                }
            ))
            {
                result.Reject(new Exception("Platform not supported advertising Id"));
            }
            */

            return result;
        }

        public AppsFlyerPartner AppsFlyer
        {
            get { return Partners.ContainsKey(AnalyticsPartnerType.AppsFlayer) ? Partners[AnalyticsPartnerType.AppsFlayer] as AppsFlyerPartner : null;  }
        }

#if !UNITY_WSA && !UNITY_WEBGL && !BUILD_AMAZON && !BUILD_HUAWEI && !BUILD_CHINA
        public FirebaseAnalyticsPartner Firebase
        {
            get { return Partners.ContainsKey(AnalyticsPartnerType.Firebase) ? Partners[AnalyticsPartnerType.Firebase] as FirebaseAnalyticsPartner : null; }
        }
#endif

        private void AddDisabledPartner(AnalyticsPartnerType type)
        {
            Debug.Log(TAG + "Disable Partner " + type);
            Partners.Add(type, new DisabledPartner());
        }

        public void HandleServerEvents(Dictionary<string, JToken> events)
        {
            foreach (var eventPair in events)
            {
                try
                {
                    AnalyticsPartnerType type;

                    if (!ServerPartnerCodes.TryGetValue(eventPair.Key, out type))
                    {
                        Debug.LogWarning(TAG + "Unknown ServerCode " + eventPair.Key);
                        continue;
                    }

                    IAnalyticsPartner partner;

                    if (!Partners.TryGetValue(type, out partner))
                    {
                        Debug.LogError(TAG + "Not found Partner " + type);
                        continue;
                    }

                    partner.HandleServerEvent(eventPair.Value);
                }
                catch (Exception e)
                {
                    Debug.LogError(TAG + "Error HandleServerEvents\n" + e);
                }
            }
        }

        public void Init()
        {
            foreach (var analyticsPartner in Partners.Values)
                analyticsPartner.Init();

            foreach (var analyticsPartner in Partners.Values)
                analyticsPartner.StarApp();
        }

        public void SetUserId(string uid)
        {
            foreach (var partnersValue in Partners.Values)
            {
                try
				{
					(partnersValue as ISetUserId)?.SetUserId(uid);
				}
                catch (Exception e)
                {
                    Debug.LogError(TAG + "SetUserId Error: " + e);
                }

            }
        }

        public void SafePartnerAction<T>(Action<T> partnerAction) where T : class, IAnalyticsPartner
        {
            foreach (var partnersValue in Partners.Values)
            {
                var partner = partnersValue as T;
                if (partner != null)
                    partnerAction(partner);
            }
        }


        /// <summary>
        /// Callback invoked with the user's decision
        /// </summary>
        /// <param name="status"></param>
        private static void OnAuthorizationRequestDone(AppTrackingTransparency.AuthorizationStatus status)
        {
            PlayerPrefs.SetInt(IDFA_VIEW_KEY, 1);

            switch(status)
            {
                case AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED:
                    Debug.Log("AuthorizationStatus: NOT_DETERMINED");
                    break;
                case AppTrackingTransparency.AuthorizationStatus.RESTRICTED:
                    Debug.Log("AuthorizationStatus: RESTRICTED");
                    break;
                case AppTrackingTransparency.AuthorizationStatus.DENIED:
                    Debug.Log("AuthorizationStatus: DENIED");
                    break;
                case AppTrackingTransparency.AuthorizationStatus.AUTHORIZED:
                    Debug.Log("AuthorizationStatus: AUTHORIZED");
                    break;
            }

            var idfa = AppTrackingTransparency.IdentifierForAdvertising();

            // Obtain IDFA
            Debug.Log($"IDFA: {idfa}");

            _iosIDFAPromise.Resolve(idfa);
        }
    }
}
