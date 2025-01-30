using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace com.playGenesis.VkUnityPlugin{
	public class LoginLogoutBridge  {

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
		public IVkNativeLogin nativeLoginer = new VKWindowsNativeLogin();
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
		public IVkNativeLogin nativeLoginer = new VKMacNativeLogin();
#endif


#if UNITY_IOS && !UNITY_EDITOR
		public IVkNativeLogin nativeLoginer = new VKiOSNativeLogin();
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
		public IVkNativeLogin nativeLoginer = new VKAndroidNativeLogin();
#endif

#if UNITY_EDITOR
		public IVkNativeLogin nativeLoginer = new VkEditorLogin();
#endif

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
		public void Login(){
			nativeLoginer.Login();
		}
		public void Logout(){
			nativeLoginer.Logout();
		}
		public static void WebViewAuth()
		{
			var r = new WebViewRequest
			{
				NavigateToUrl = FormLoginUrl(),
				CloseWhenNavigatedToUrl = "https://oauth.vk.com/blank.html",
				CallbackAction = (w) =>{
					if (w.Error != null)
					{
						VkApi.VkApiInstance.SendMessage("AccessDeniedMessage", "-1#Canceled by user");
					}
					else
					{
						VkApi.VkApiInstance.SendMessage("ReceiveNewTokenMessage",
														VKToken.ParseFromAuthUrl(w.LastUrlWithParams));
					}
				}
			};
			WebView.Instance.Add(r);
		}
		public static string FormLoginUrl()
		{
			var VkSetts = Resources.Load<VkSettings>("VkSettings");
			var scope=string.Join(",", VkSetts.scope.ToArray());
			
			
			var url = "https://oauth.vk.com/authorize?client_id=" + VkSetts.VkAppId +
				"&scope=" + scope +
					"&redirect_uri=https://oauth.vk.com/blank.html&display=popup" +
					"&forceOAuth="+ VkSetts.forceOAuth.ToString()+
					"&revoke="+ (VkSetts.revoke ? 1:0 )+
					"&v="+ VkSetts.apiVersion +
					"&response_type=token";
			return url;
			
		}
#else
        public void Login(){}
        public void Logout(){}
        public static void WebViewAuth(){}
        public static string FormLoginUrl(){return "";}
#endif
	}
}