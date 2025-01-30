#if !UNITY_WSA && !UNITY_WEBGL && !BUILD_AMAZON && !BUILD_HUAWEI && !BUILD_CHINA
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Utils;
using Firebase.Analytics;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Scripts.Platform.Mobile.Analytics.Partners
{
    public class FirebaseAnalyticsPartner : IAnalyticsPartner, ISetUserId
    {
        public static string TAG = "[FirebaseAnalytics] ";
		
		private Promise _initPromise = new Promise();
        
        public class FirebaseServerEvent
        {
            [JsonProperty("ev_name")]
            public string EventName { get; set; }

            [JsonProperty("ev_data")]
            [CanBeNull]
            public Dictionary<string, string> EventData { get; set; }

            private Parameter ConvertToParameter(string key, string value)
            {
                return new Parameter(key, value);
            }

            public Parameter[] GetParameters()
            {
                if (EventData == null)
                    return new Parameter[0];

                return EventData.Select(pair => ConvertToParameter(pair.Key, pair.Value)).ToArray();
            }

            public FirebaseServerEvent(string ev_name, Dictionary<string, string> ev_data)
            {
                EventName = ev_name;
                EventData = ev_data;
            }
        }

        public void Init()
        {
			ServerLogs.AnalyticsInitEvent(TAG + ServerLogs.ANALYTICS_START_INIT_ACTION);
            Debug.Log(TAG + "Firebase ShowInfo");
            GameFirebase.Instance.Init().Then(OnInitializedFirebase);
        }

        void OnInitializedFirebase()
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

            // SetItemCount default session duration values.
            //FirebaseAnalytics.SetMinimumSessionDuration(new TimeSpan(0, 0, 10));
            FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));

            Debug.Log(TAG + "Firebase Analytics Initialized");
			
			if (_initPromise.IsPending)
			{
				_initPromise.ResolveOnce();
				ServerLogs.AnalyticsInitEvent(TAG + ServerLogs.ANALYTICS_END_INIT_ACTION);
			}
        }

        public void StarApp()
        {
        }

        public void HandleEvent(FirebaseServerEvent ev)
        {
			//if (!GameFirebase.Instance.IsInitialized)
			//{
			//    Debug.Log(TAG + "Error HandleEvent not firebaseInitialized");
			//    return;
			//}

			GameFirebase.Instance.Init().Then(() =>
			{
				try
				{
					Debug.Log(TAG + "HandleEvent " + ev.EventName);
					FirebaseAnalytics.LogEvent(ev.EventName, ev.GetParameters());
				}
				catch (Exception e)
				{
					Debug.LogWarning(TAG + "Error HandleEvent " + e);
				}
			});
        }

        public void HandleServerEvent(JToken token)
        {
            if (!GameFirebase.Instance.IsInitialized)
            {
                Debug.Log(TAG + "Error HandleServerEvent not firebaseInitialized");
            }

			GameFirebase.Instance.Init().Then(() =>
			{
				try
				{
					var serverEvents = token.ToObject<FirebaseServerEvent[]>();
					foreach (var serverEvent in serverEvents)
					{
						Debug.Log(TAG + "HandleServerEvent " + serverEvent.EventName);

						var serverEventData = serverEvent.EventData ?? new Dictionary<string, string>();

						serverEvent.EventData = serverEventData;

						var eventName = serverEvent.EventName;
						var parameters = serverEvent.GetParameters();

						FirebaseAnalytics.LogEvent(eventName, parameters);
					}
				}
				catch (Exception e)
				{
					Debug.LogWarning(TAG + "Error HandleServerEvent " + e);
				}
			});
        }

        public void EndApp()
        {
            
        }

        public void ResetAnalyticsData()
        {
            Debug.Log(TAG + "Reset analytics data.");
            FirebaseAnalytics.ResetAnalyticsData();
        }

        public void SetUserId(string uid)
        {
            GameFirebase.Instance.Init().Then(() => FirebaseAnalytics.SetUserId(uid));
        }

		public void LogIronSourceImpressionData(IronSourceImpressionData impressionData)
		{
			GameFirebase.Instance.Init().Then(() =>
			{
				if (impressionData != null)
				{
					Parameter[] AdParameters = {
						new Parameter("ad_platform", "ironSource"),
						new Parameter("ad_source", impressionData.adNetwork),
						new Parameter("ad_unit_name", impressionData.instanceName),
						new Parameter("ad_format", impressionData.adUnit),
						new Parameter("currency","USD"),
						new Parameter("value", impressionData.revenue ?? 0d)
					};

					ServerLogs.SendLog("fb_ad_impression", new Dictionary<string, object>()
					{
						{ "ad_platform",    "ironSource" },
						{ "ad_source",      impressionData.adNetwork },
						{ "ad_unit_name",   impressionData.instanceName },
						{ "ad_format",      impressionData.adUnit },
						{ "currency",       "USD" },
						{ "value",          impressionData.revenue ?? 0d }
					});

					FirebaseAnalytics.LogEvent("ad_impression", AdParameters);
				}
			});
		}
    }
}
#endif
