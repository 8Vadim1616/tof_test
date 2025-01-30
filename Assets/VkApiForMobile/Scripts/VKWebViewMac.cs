using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace com.playGenesis.VkUnityPlugin{
	public class VKWebViewMac : IVKWebView {
	
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void WvNativeCallback(string msg);
		public static WvNativeCallback onWebViewDoneStandAlone;
		
		[STAThread]
		[DllImport("vkunityplugin_mac",SetLastError=true)]
		extern static int OpenWebViewNative(string exepath, WvNativeCallback _callback, string openurl, string closeurl);
		
		[DllImport("vkunityplugin_mac",SetLastError=true)]
		extern static int quitWebView();

		public void  OpenWebView(string openurl,string closeurl)
		{
			var process = System.Diagnostics.Process.GetCurrentProcess(); // Or whatever method you are using
			string fullPath = process.MainModule.FileName;
			onWebViewDoneStandAlone = (string data)=>{
				MainThreadDispatcher.data = data;
			};
			try{ 
				OpenWebViewNative(fullPath, onWebViewDoneStandAlone, openurl, closeurl);
			}catch (System.Exception e){
				Debug.Log(e.Message);
			}
		}
    }
}

