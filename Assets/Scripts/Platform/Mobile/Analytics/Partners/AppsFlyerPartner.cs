using System;
using System.Collections.Generic;
using AppsFlyerSDK;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Platform.Mobile.Analytics.Partners
{
    public class AppsFlyerPartner : IAnalyticsPartner, ISetUserId
    {
        public const string TAG = "[AppsFlyerPartner] ";

		public static Dictionary<string, object> ConversionData = null;

		private static bool _wasInited = false;
		private Promise _initPromise = new Promise();
        
        public class AppsFlyerServerEvent
        {
            [JsonProperty("ev_name")]
            public string EventName { get; set; }

            [JsonProperty("ev_data")]
            [CanBeNull]
            public Dictionary<string, string> EventData { get; set; }

            public AppsFlyerServerEvent(string ev_name, Dictionary<string, string> ev_data)
            {
                EventName = ev_name;
                EventData = ev_data;
            }
        }

        public void Init()
        {
			if (_wasInited)
				return;
			
			ServerLogs.AnalyticsInitEvent(TAG + ServerLogs.ANALYTICS_START_INIT_ACTION);
			Debug.Log(TAG + "Start initializing");
            var callbackObject = new GameObject("AppsFlyerCustomTrackerCallbacks");
            var callbackHandler = callbackObject.AddComponent<AppsFlyerCustomTrackerCallbacks>();
            Object.DontDestroyOnLoad(callbackObject);

            var devKey = Game.Consts.AppsFlyerSettings.DevKey;
            var appId = Game.Consts.AppsFlyerSettings.AppId;
            
            Debug.Log(TAG + $"DevKey = {devKey}; appId = {appId}");
            
            AppsFlyer.setIsDebug(true);
            AppsFlyer.initSDK(devKey, appId, callbackHandler);
#if UNITY_IOS && !UNITY_EDITOR
			AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);
#endif
            AppsFlyer.startSDK();

			AppsFlyerAdRevenue.start();

			_wasInited = true;
			_initPromise.ResolveOnce();
			ServerLogs.AnalyticsInitEvent(TAG + ServerLogs.ANALYTICS_END_INIT_ACTION);
		}

        public void StarApp()
        {
            string appsFlyerUid = AppsFlyer.getAppsFlyerId();
            
            if (string.IsNullOrEmpty(appsFlyerUid))
				return;
            
            Debug.Log(TAG + "appsFlyerUid = " + appsFlyerUid);
            
            if (Game.User != null)
                Game.User.RegisterData.AppsFlyerId = appsFlyerUid;
        }

        public string AppsFlyerId => AppsFlyer.getAppsFlyerId();

        public void SetUserId(string uid)
        {
            AppsFlyer.setCustomerUserId(uid);
        }

        public void HandleEvent(AppsFlyerServerEvent ev)
        {
            try
            {
                Debug.Log(TAG + "sendEvent " + ev.EventName);
                AppsFlyer.sendEvent(ev.EventName, ev.EventData);
            }
            catch (Exception e)
            {
                Debug.LogWarning(TAG + "Error HandleEvent " + e);
            }
        }

        public void HandleServerEvent(JToken token)
		{
			_initPromise.Then(() =>
			{
				try
				{
					var serverEvents = token.ToObject<AppsFlyerServerEvent[]>();

					foreach (var serverEvent in serverEvents)
					{
						var sendEventData = new Dictionary<string, string>();

						if (serverEvent.EventData != null)
							foreach (var dataPair in serverEvent.EventData)
							{
								sendEventData[dataPair.Key] = dataPair.Value;
							}

						Debug.Log(TAG + "HandleServerEvent " + serverEvent.EventName);
						AppsFlyer.sendEvent(serverEvent.EventName, sendEventData);
					}
				}
				catch (Exception e)
				{
					Debug.LogWarning(TAG + "Error HandleServerEvent " + e);
				}
			});
		}

		public void LogAdRevenue(string monetizationNetwork, AppsFlyerAdRevenueMediationNetworkType mediationNetwork,
			double eventRevenue, string revenueCurrency, Dictionary<string, string> additionalParameters = null)
		{
			AppsFlyerAdRevenue.logAdRevenue(monetizationNetwork, mediationNetwork, eventRevenue, revenueCurrency, additionalParameters);
		}


		public void EndApp()
        {
            //AppsFlyer.stopSDK(true);
        }
    }
}
