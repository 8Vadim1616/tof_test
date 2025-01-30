using System;
using System.Collections.Generic;
using Assets.Scripts.Localization;
using Assets.Scripts.Network.Queries.Operations.Api;
using Assets.Scripts.Platform.Mobile.Analytics;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.User
{
	public class UserSettings
	{
		public static string VersionInfo => Application.version;
		
		public static bool IsFirebaseAnalyticsAvailable
		{
			get => PlayerPrefs.GetInt(MobileAnalytics.FIREBASE_ANALYTICS_PARTNER, 1) > 0;
			set => PlayerPrefs.SetInt(MobileAnalytics.FIREBASE_ANALYTICS_PARTNER, value ? 1 : 0);
		}
		
		public static bool IsFacebookAnalyticsAvailable
		{
			get => PlayerPrefs.GetInt(MobileAnalytics.FACEBOOK_ANALYTICS_PARTNER, 1) > 0;
			set => PlayerPrefs.SetInt(MobileAnalytics.FACEBOOK_ANALYTICS_PARTNER, value ? 1 : 0);
		}
		
		public static bool IsAppsFlyerAnalyticsAvailable
		{
			get => PlayerPrefs.GetInt(MobileAnalytics.APPS_FLAYER_ANALYTICS_PARTNER, 1) > 0;
			set => PlayerPrefs.SetInt(MobileAnalytics.APPS_FLAYER_ANALYTICS_PARTNER, value ? 1 : 0);
		}
		
		public int RateUs
		{
			get => Get<int>("rate_us");
			set => Set("rate_us", value);
		}

		public bool IsRatedUs()
		{
			return RateUs > 0;
		}

		public bool IsNotificationsOn
		{
			get => Get<int>("notif") > 0;
			set
			{
				if (Get<int>("notif") > 0 != value)
				{
					Set("notif", value ? 1 : 0);
				}
			}
		}

		public bool IsMusic
		{
			get => Get<int>("music") > 0;
			set
			{
				if (Get<int>("music") > 0 != value)
				{
					Set("music", value ? 1 : 0);
					ChangeMusic(value);
				}
			}
		}

		public bool IsSound
		{
			get => Get<int>("sound") > 0;
			set
			{
				if (Get<int>("sound") > 0 != value)
				{
					Set("sound", value ? 1 : 0);
					ChangeSound(value);
				}
			}
		}
		
		public bool IsVibration
		{
			get => Get<int>("vibro") > 0;
			set
			{
				if (Get<int>("vibro") > 0 != value)
				{
					Set("vibro", value ? 1 : 0);
				}
			}
		}
		
		public bool WasInterstitialShown
		{
			get => Get<int>("interstitial") > 0;
			set
			{
				if (Get<int>("interstitial") > 0 != value)
				{
					Set("interstitial", value ? 1 : 0);
				}
			}
		}

		private void ChangeSound(bool isSound)
		{
			Game.Sound?.ChangeSound(isSound);
		}

		private void ChangeMusic(bool isMusic)
		{
			Game.Sound?.ChangeMusic(isMusic);
		}
		
		public int TutorStep
		{
			get => Get<int>("tutor_step");
			set => Set("tutor_step", value);
		}
		
		public long LastRewardTime
		{
			get => Get<long>("last_reward_time");
			set => Set("last_reward_time", value);
		}
		
		public bool WasShownMainAfterTutor
		{
			get => Get<int>("show_main") > 0;
			set
			{
				if (Get<int>("show_main") > 0 != value)
				{
					Set("show_main", value ? 1 : 0);
				}
			}
		}
		
		public bool WasShownChooseOfferFirstTime
		{
			get => Get<int>("show_choose_offer_first_time") > 0;
			set
			{
				if (Get<int>("show_choose_offer_first_time") > 0 != value)
				{
					Set("show_choose_offer_first_time", value ? 1 : 0);
				}
			}
		}

		public void Clear()
		{
			LastSetData.Clear();

			//Необходимо дублировать в конструкторе словаря
			LastSetData.Add("sound", 1);
			LastSetData.Add("music", 1);
			LastSetData.Add("hint", 1);
			LastSetData.Add("notif", 1);
			LastSetData.Add("chat", 1);
			LastSetData.Add("vibro", 1);
		}

		public int PaymentInitTime
		{
			get => Get<int>("payment_init_time");
			set => Set("payment_init_time", value);
		}

		public string LangPref
		{
			get
			{
				string str = Get<string>("preferred_language");

				if (string.IsNullOrEmpty(str))
					return GameLocalization.Locale;

				return str;
			}
			set => Set("preferred_language", value);
		}

		//Необходимо дублировать в Clear
		private static readonly Dictionary<string, object> LastSetData = new Dictionary<string, object>()
		{
			{"sound", 1},
			{"music", 1},
			{"hint", 1},
			{"notif", 1},
			{"chat", 1},
			{"vibro", 1},
		};

		private Dictionary<string, object> dataHelper = new Dictionary<string, object>();

		public bool Contains(string key)
		{
			return LastSetData.ContainsKey(key);
		}

		public T Get<T>(string key)
		{
			if (!LastSetData.ContainsKey(key))
				return default;

			return Parse<T>(LastSetData[key]);
		}

		private string GetString(string key)
		{
			LastSetData.TryGetValue(key, out var data);
			return data?.ToString();
		}

		private T Parse<T>(object value)
		{
			try
			{
				if (!typeof(T).IsEnum)
					return (T) Convert.ChangeType(value, typeof(T));

				if (Enum.IsDefined(typeof(T), value)) // enum должен быть : long, а не int
					return (T) value;

				return default;
			}
			catch (Exception e)
			{
				return default;
			}
		}

		public void Save(string key, object value)
		{
			dataHelper.Clear();
			dataHelper.Add(key, value);
			Save(dataHelper);
		}

		public void Save(Dictionary<string, object> data)
		{
			Game.QueryManager.MultiRequest(new SettingsSetOperation(data));
		}

		public void Set(string key, object value, bool needSave = true)
		{
			if (LastSetData.ContainsKey(key) && Equals(LastSetData[key], value))
				return;
			
			LastSetData[key] = value;

			if (needSave)
				Save(key, value);
		}

		public void Update(Dictionary<string, object> data)
		{
			if (data == null) return;

			foreach (var key in data.Keys)
				Set(key, data[key], false);
		}
	}
}