using Assets.Scripts.Localization;
using Assets.Scripts.Platform.Mobile.Analytics.Partners;

namespace Assets.Scripts.BuildSettings
{
    public class GameConsts
    {
	    
	    
	    //public static string UpdateStaticEntryPoint => "https://tomcat-develop.playgenes.com/";
	    //public static string UpdateStaticEntryPoint => "https://arkanoid-develop.playgenes.com/";
	    public static string UpdateStaticEntryPoint => "https://tof-develop.playgenes.com/";
	    //public static string UpdateStaticEntryPoint => "http://localhost:8080/";
	    
	    
		public static string ServerEntryPoint
	    {
		    get
		    {
			    if (BuildSettings.IsTest)
					//return "https://tomcat-develop.playgenes.com/";
				    //return "https://arkanoid-develop.playgenes.com/";
					return "https://tof-develop.playgenes.com/";
					//return "http://localhost:8080/";
				if (BuildSettings.IsRelease)
					//return "https://tomcat-develop.playgenes.com/";
					//return "https://arkanoid-develop.playgenes.com/";
					return "https://tof-develop.playgenes.com/";
					//return "http://localhost:8080/";
			    return null;
		    }
	    }

		public static string BuildNameForModel {
			get
			{
#if BUILD_CHINA
				return "china";
#endif
				return "main";
			}
		}

		public static string FacebookAppId
	    {
		    get
		    {
			    if (BuildSettings.IsTest)
				    return "1092927965289812";
			    else if (BuildSettings.IsRelease)
				    return "1092927965289812";
			    return null;
		    }
	    }

		public static int VkAppId
		{
			get
			{
				
// #if BUILD_HUAWEI
// 				return 51430970;
// #endif
				// if (BuildSettings.IsTest /*|| BuildSettings.IsPredproduction*/)
				// 	return 8115230;
				// else if (BuildSettings.IsRelease)
					return 8096771;

				// return 0;
			}
		}

		public static string GoogleSignInClientId
		{
			get
			{
				string result = "";

#if UNITY_ANDROID
				result = GOOGLE_SIGN_IN_CLIENT_ID_ANDROID;
#elif UNITY_IOS
				result = GOOGLE_SIGN_IN_CLIENT_ID_IOS;
#endif
				return result;

			}
		}
	    
        public string UpdateLink { get; private set; }

        public string GCMProjectNumber { get; private set; }
        public string AppId { get; private set; }
        public string GDPRUrl1 { get; private set; }
        public string GDPRUrl2 { get; private set; }

		public const string IAPPrefixDefault = "aa_";

        /*
        public const string AndroidAdMobInterstitialId = "ca-app-pub-5310047333956334/3127091839";
        public const string AndroidAdMobBannerId = "ca-app-pub-5310047333956334/2718495040";

        public const string AndroidAdMobTestBannerId = "ca-app-pub-3940256099942544/6300978111";
        public const string AndroidAdMobTestInterstitialId = "ca-app-pub-3940256099942544/1033173712";
        public const string AndroidAdMobTestRewardId = "ca-app-pub-3940256099942544/5224354917";
        */

		public const string WINDOWS_STORE_PACKAGE_IDENTITY_NAME = "4ACEF246.GoldenFarm";
		public const string WINDOWS_STORE_PRODUCT_ID = "9NKP7KQ9NN9K";
		public const string WINDOWS_STORE_PACKAGE_PUBLISHER_ID = "05g3z837ka020";
		// public const string WINDOWS_STORE_PACKAGE_SID = "s-1-15-2-4078571963-3295434925-1817741141-515726465-292077152-968683999-824880538";
		public const string WINDOWS_STORE_NOTIFICATION_TITLE = "Golden Farm";
		public const string WINDOWS_STORE_NOTIFICATION_ICON = "Assets/Square71x71Logo.scale-100.png";

		
		public const string DEF_PACKAGE_NAME = "com.playgenes.tof";
		public const string HMS_PACKAGE_NAME = "com.playgenes.tof.huawei";
		public const string AMAZON_PACKAGE_NAME = "com.playgenes.tof.amazon";
		public const string IOS_PACKAGE_NAME = "com.playgenes.tof";
		

		public const string GOOGLE_SIGN_IN_CLIENT_ID_ANDROID = "657256113452-pu0b7l32qil4f3cr5dgjnpl9c14kfkuj.apps.googleusercontent.com"; // Из Firebase google-services.json oauth_client.client_type = 3
		public const string GOOGLE_SIGN_IN_CLIENT_ID_IOS = "657256113452-ok762dscnso5kiugb07hia5fhmh61onj.apps.googleusercontent.com";
		public const string GOOGLE_SIGN_IN_CLIENT_ID_IOS_REVERSE = "com.googleusercontent.apps.657256113452-ok762dscnso5kiugb07hia5fhmh61onj";

		public AppsFlyerSettings AppsFlyerSettings { get; private set; }

		public static string NeedSocialNetwork
		{
			get
			{
#if UNITY_ANDROID || UNITY_IOS
				if (GameLocalization.IsLocaleRU)
					return Platform.Adapter.SocialNetwork.VKONTAKTE;
				else
					return Platform.Adapter.SocialNetwork.FACEBOOK_2;
#elif UNITY_WSA
				return Platform.Adapter.SocialNetwork.FACEBOOK_2;
#endif
				return Platform.Adapter.SocialNetwork.FACEBOOK;
			}
		}
        
        public GameConsts()
        {
#if UNITY_IOS
			AppId = "id6502288455";
#else
			AppId = "com.playgenes.tof";
#endif
            UpdateLink = "https://play.google.com/store/apps/details?id=com.playgenes.alliancedefence";
            GCMProjectNumber = "";
            
            AppsFlyerSettings = new AppsFlyerSettings
            {
                AppId = AppId,
                DevKey = "4mDcbVkKMuZUQKeSeP883V"
            };
        }
    }
}
