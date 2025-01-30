using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace com.playGenesis.VkUnityPlugin {
	public class VKMacNativeLogin : IVkNativeLogin {
		[STAThread]
		[DllImport("vkunityplugin_mac", SetLastError=true)]
		extern static int LogoutFrom(string url);

		public void Login()
		{
			LoginLogoutBridge.WebViewAuth ();
			return;
		}
		public void Logout()
		{
			try
			{
				LogoutFrom("vk.com");
			}
			catch (System.Exception e)
			{
				Debug.Log(e.Message);
			}
			VkApi.VkApiInstance.onLoggedOut ();
		}
	}
}
