#if !UNITY_WEBGL
using Assets.Scripts.Platform.Mobile.Notifications;
using Assets.Scripts.Platform.Mobile.Analytics;
using Assets.Scripts.Platform.Mobile.Ref;
using Assets.Scripts.Platform.Mobile.Share;
using IngameDebugConsole;
#endif
using Assets.Scripts.Platform.Mobile.Purchases;
using UnityEngine;

namespace Assets.Scripts.Platform.Mobile
{
	public class PlatformMobile : MonoBehaviour
	{
		public SocialPurchase SocialPurchase { get; private set; }
#if !UNITY_WEBGL
		public AbstractStorePurchases Purchases { get; private set; }

		public MobileAnalytics Analytics { get; private set; }
		public MobileNotifications Notifications { get; private set; }
		public MobileShare Share { get; private set; }
		public MobileInstallRef InstallRef { get; private set; }

		public string GetPackageName()
		{
#if UNITY_IOS
			return Game.Consts.AppId;
#elif UNITY_WSA
			return GameConsts.WINDOWS_STORE_PRODUCT_ID;
#endif
			return Application.identifier;
		}

		public void GoToStore(string packageName = null)
		{
			string shopUrl = "market://details?id=";

			packageName ??= GetPackageName();

#if UNITY_IOS
			shopUrl = "itms-apps://itunes.apple.com/app/id";
#elif UNITY_ANDROID
#if BUILD_HUAWEI
			shopUrl = "appmarket://details?id=";
#elif BUILD_AMAZON
			shopUrl = "amzn://apps/android?p=";
#else
			shopUrl = "market://details?id=";
#endif
#elif UNITY_WSA
			shopUrl = "ms-windows-store://pdp/?productid=";
#endif
			Application.OpenURL(shopUrl + packageName);
		}

		public string ShareUrl(string packageName = null)
		{
			packageName ??= GetPackageName();

#if UNITY_ANDROID && !BUILD_HUAWEI && !BUILD_AMAZON
			return $"https://play.google.com/store/apps/details?id={packageName}";
#endif

			return "";
		}

		private void Awake()
		{
			LoadPurchases();
			LoadAnalytics();
			LoadNotifications();
			LoadShare();
			LoadInstallRef();
		}

		private void LoadAnalytics()
		{
			Analytics = new MobileAnalytics();
		}

		private void LoadNotifications()
		{
			Notifications = new MobileNotifications();
		}

		private void LoadPurchases()
		{

#if UNITY_EDITOR
			Purchases = new UnityIAP();
#elif UNITY_WEBGL
			SocialPurchase = new SocialPurchase();
#elif BUILD_HUAWEI
            Purchases = new HuaweiIAP();
#else
			Purchases = new UnityIAP();
#endif
		}

		private void LoadShare()
		{
#if UNITY_ANDROID || UNITY_IOS
			Share = new MobileAndroidShare();
#else
			Share = new MobileShare();
#endif
		}

		private void LoadInstallRef()
		{
#if UNITY_ANDROID
			InstallRef = new MobileAndroidInstallRef();
#else
			InstallRef = new MobileInstallRef();
#endif
		}

		private bool _inited = false;

		public void Init()
		{
			if (_inited)
			{
				Notifications?.ResendFirebaseToken();
				return;
			}

			Debug.Log("PlatformMobile inited");
			
			Notifications?.Init();
			
			Share?.Init();
			InstallRef?.Init();
			_inited = true;
		}

		public void AnalyticsInit()
		{
			Debug.Log("Analytics init");
			Analytics?.Init();
		}
	}
}
#endif
