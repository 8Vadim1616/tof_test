using System;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Platform.Adapter.Actions;
using Assets.Scripts.Platform.Adapter.Implements;
using Newtonsoft.Json.Linq;
using UnityEngine;

#if (UNITY_IOS || UNITY_TVOS || UNITY_STANDALONE_OSX)
using UnityEngine.iOS;
#endif

namespace Assets.Scripts.Platform.Adapter
{
	public class SocialNetwork : MonoBehaviour
	{
		public const string ODNOKLASSNIKI = "ok";
		public const string VKONTAKTE = "vk";
		public const string YANDEX = "yg";
		public const string FACEBOOK = "fb";
		public const string FACEBOOK_2 = "fb_2";
		public const string MOJMIR = "mm";
		public const string NK = "nk";
		public const string DRAUGIEM = "dg";
		public const string KONGREGATE = "kg";
		public const string DRAUGAS = "dgs";
		public const string YABAGE = "yb";
		public const string SPIELAFFE = "sl";
		public const string BIGFISH = "bf";
		public const string HYVES = "hv";
		public const string FOTOSTRANA = "fs";
		public const string WASD = "wd";
		public const string NICONICO = "nn";
		public const string YAHOO = "yh";
		public const string WYSEPLAY = "wp";
		public const string SITE = "site";
		public const string RAMBLER = "rb";
		public const string PLINGA = "pg";
		public const string GAMES_MAIL_RU = "gmr";
		public const string APPLE = "apple";

		public const string MOBILE_FB = "mfb";
		// public const string MOBILE_FB_DUAL = "mfb_dual";
		// public const string AND = "and";
		public const string IOS = "ios";
		public const string AND_RU = "and_ru";
		public const string AND_HW = "and_hw";
		// public const string AND_FB = "and_fb";
		// public const string IOS_RU = "ios_fb";
		// public const string IOS_FB = "ios_fb";
		public const string MOBILE_SITE = "msite";
		// public const string MOBILE_SERVER = "msrv";
		public const string WINDOWS_STORE = "wnds";
		// public const string STEAM = "steam";
		public const string AND_AMZ = "and_amz";

		public const string OKMM = "mscode";
		public const string GOOGLE_PLAY_GAMES = "gpg";
		public const string GOOGLE_SIGN_IN = "gsi";
		public const string HUAWEI_SIGN_IN = "hsi";

		public const string LOCAL = "local";
		public const string NONE = "none";

		[SerializeField] public GameObject VkApiGameObject;

		private static string _network = NONE;
		public string Network => _network;
		public AbstractSocialAdapter Adapter { get; private set; }

		static SocialNetwork()
		{
#if BUILD_HUAWEI
			_network = AND_HW;
#elif BUILD_AMAZON
			_network = AND_AMZ;
#elif UNITY_ANDROID
			_network = AND_RU;
#elif UNITY_IOS
            _network = IOS;
#elif UNITY_WSA
			_network = WINDOWS_STORE;
#elif UNITY_STANDALONE
			_network = AND_RU;
#elif UNITY_EDITOR
			_network = AND_RU;
#endif
		}

		public void Init(string locale, JObject iframeVars = null, Action<AbstractSocialAdapter> onInitCallback = null)
		{
			SocialAdapterParams socialAdapterParams = new SocialAdapterParams(this)
													 .AddLocale(locale)
													 .AddIframeVars(iframeVars)
													 .AddInitCallBack(onInitCallback);

			if (iframeVars != null)
			{
				_network = (string) iframeVars["sn"];
			}

			Adapter = SocialAdapterFactory.Create(Network, socialAdapterParams);
		}

		public IPromise<object> Request(ISocialAction action)
		{
			if (Adapter != null)
				return Adapter.Request(action);

			return Promise<object>.Rejected(new Exception());
		}

		private void OnDestroy()
		{
			Free();
		}

		public void Free()
		{
			if (Adapter != null)
				Adapter.Free();

			Adapter = null;
		}

		public static bool NeedAppleSignIn
		{
			get
			{
#if (UNITY_IOS || UNITY_TVOS || UNITY_STANDALONE_OSX)
				return new Version(Device.systemVersion) >= new Version("13.0");
#endif
				return false;
			}
		}

		public static bool NeedSoc
		{
			get
			{
#if UNITY_WEBGL || BUILD_CHINA
				return false;
#elif UNITY_ANDROID && BUILD_AMAZON
				return false;
#endif
				return true;
			}
		}

		public static bool NeedRusSoc
		{
			get
			{
// #if UNITY_WSA || BUILD_AMAZON || BUILD_HUAWEI || BUILD_CHINA
// 				return false;
// #endif
				return false;
			}
		}

		public static bool HaveNotify
		{
			get
			{
#if UNITY_WSA || BUILD_HUAWEI || BUILD_AMAZON || BUILD_CHINA
				return false;
#endif
				return true;
			}
		}

		public static bool NeedSupport
		{
			get
			{
#if UNITY_WEBGL || UNITY_WSA || (UNITY_ANDROID && BUILD_HUAWEI) || UNITY_EDITOR || BUILD_CHINA
				return false;
#endif
				return true;
			}
		}

		public static bool NeedGPG
		{
			get
			{
#if (BUILD_AMAZON || BUILD_HUAWEI || BUILD_CHINA)
				return false;
#endif
#if UNITY_ANDROID
				return true;
#endif
				return false;
			}
		}

		public static bool NeedGSI
		{
			get
			{
#if UNITY_WEBGL || BUILD_CHINA
				return false;
#elif UNITY_ANDROID && (BUILD_HUAWEI || BUILD_AMAZON)
				return false;
#endif
				return true;
			}
		}

		public static bool NeedHSI
		{
			get
			{
#if UNITY_ANDROID && BUILD_HUAWEI
				return true;
#endif
				return false;
			}
		}

		public static bool NeedVK
		{
			get
			{
				if (NeedRusSoc)
					return true;

#if UNITY_ANDROID && BUILD_HUAWEI
				return true;
#endif
				return false;
			}
		}

		public static bool NeedCustomWebIap
		{
			get
			{
				return Game.Social.Network == YANDEX;
			}
		}

		public static bool CurrencyIsText
		{
			get
			{
				if (IsFacebook ||
					Game.Social.Network == DRAUGIEM ||
					Game.Social.Network == YAHOO ||
					Game.Social.Network == RAMBLER ||
					Game.Social.Network == GAMES_MAIL_RU ||
					Game.Social.Network == PLINGA ||
					Game.Social.Network == YANDEX)
					return true;

#if UNITY_ANDROID || UNITY_IOS || UNITY_WSA
				return true;
#endif

				return false;
			}
		}

		public static bool CanSaveProgress
		{
			get
			{

#if BUILD_GOOGLE || BUILD_HUAWEI || UNITY_IOS
				return true;
#endif
				return false;
			}
		}

		public static bool IsSocialAdvAvailable => IsLocal || Game.Social.Network == ODNOKLASSNIKI || Game.Social.Network == VKONTAKTE || Game.Social.Network == PLINGA || Game.Social.Network == YANDEX;

		public static bool IsFacebook => Game.Social.Network == FACEBOOK || Game.Social.Network == FACEBOOK_2;
		public static bool IsLocal => Game.Social.Adapter is LocalAdapter;
		/**Игра запущена на телефоне через браузер*/
		public bool IsMobile => Adapter.IsMobile;
		public static bool IsDirectGames => Game.Social.Adapter.IsDirectGames;
	}
}