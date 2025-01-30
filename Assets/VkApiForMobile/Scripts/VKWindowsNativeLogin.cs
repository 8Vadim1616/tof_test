using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using com.playGenesis.VkUnityPlugin;
using UnityEngine;

namespace com.playGenesis.VkUnityPlugin {
	public class VKWindowsNativeLogin : IVkNativeLogin {

		[STAThread]
		[DllImport("vkunityplugin",SetLastError=true)]
		extern static int LogoutFrom(string path, string url);   
			
		public void Login()
		{
			LoginLogoutBridge.WebViewAuth ();
			return;
		}
		public void Logout()
		{
			try
			{
				var process = System.Diagnostics.Process.GetCurrentProcess(); // Or whatever method you are using
				string fullPath = process.MainModule.FileName;   
				Thread t = new Thread(delegate () {
					LogoutFrom(fullPath,"https://vk.com");
				});
				t.Start();
			}
			catch (System.Exception e)
			{
				
				Debug.Log(e.Message);
			}
			VkApi.VkApiInstance.onLoggedOut ();
		}

	}
}
