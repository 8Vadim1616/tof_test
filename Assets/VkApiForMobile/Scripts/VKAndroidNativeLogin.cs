using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playGenesis.VkUnityPlugin {
	public class VKAndroidNativeLogin : IVkNativeLogin {
		AndroidJavaObject jo;
		public void Login()
		{
			//com.playgenesis.vkunityplugin.
			jo = new AndroidJavaObject ("com.playgenesis.vkunityplugin.Initializer"); 
			var loginUrl = LoginLogoutBridge.FormLoginUrl ();
			var isVkAppPresent =jo.CallStatic<bool>("isVkAppPresent");

			if (VkApi.VkSetts.forceOAuth || !isVkAppPresent)
			{
				LoginLogoutBridge.WebViewAuth();
				return;
			}
			jo.Set<String> ("urlBase64", loginUrl);
			jo.Call ("Init");
		}

		public void Logout()
		{
			using (AndroidJavaObject jo = new AndroidJavaObject ("com.playgenesis.vkunityplugin.Initializer")) 
			{
				VkApi.VkApiInstance.onLoggedOut ();
				jo.Call ("Logout",VkApi.VkSetts.VkAppId.ToString());
			}
			
		}
	}
}