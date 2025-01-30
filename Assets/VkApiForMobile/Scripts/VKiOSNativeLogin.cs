using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


#if UNITY_IOS && !UNITY_EDITOR //to prevent android il2cpp complaining about undefined symbols
namespace com.playGenesis.VkUnityPlugin {
	public class VKiOSNativeLogin : IVkNativeLogin {
		private bool loginInProgress;
		[DllImport("__Internal")]
		private static extern void _VkAuthorization(string authUrl);
		
		[DllImport("__Internal")]
		private static extern void _doLogOutUser();

		[DllImport("__Internal")]
		private static extern bool _IsVkAppPresent();
#pragma warning disable 168
		private void BackToAppFix(){
			try {
				if (!VkApi.VkSetts.forceOAuth || _IsVkAppPresent())
				{
					VkApi.VkApiInstance.LoggedIn -= BackToAppFix;
				}

			} catch (Exception ex) {
				
			}
			loginInProgress = false;
		}
#pragma warning restore 168
		
		public IEnumerator OnApplicationFocus(bool focus,GameObject mVkApi)
        {
			yield return new WaitForSeconds(2);
			if (focus && loginInProgress)
			{
				mVkApi.GetComponent<MessageHandler>().AccessDeniedMessage("1#AuthorizationFailed");
			}
		}

		public void Login()
		{
			if (VkApi.VkSetts.forceOAuth || !_IsVkAppPresent())
			{
				LoginLogoutBridge.WebViewAuth();
				return;
			}
			VkApi.VkApiInstance.LoggedIn += BackToAppFix;
			loginInProgress = true;
			_VkAuthorization (LoginLogoutBridge.FormLoginUrl());
		}
		public void Logout()
		{
			VkApi.VkApiInstance.onLoggedOut ();
			_doLogOutUser ();
		}
	}
}
#endif