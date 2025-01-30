using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace com.playGenesis.VkUnityPlugin{
	public class VKWebViewWindows: IVKWebView {

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void WvNativeCallback(string msg);
		public static WvNativeCallback onWebViewDoneStandAlone;

        [STAThread]
		[DllImport("PGWebViewWin",SetLastError=true)]
		extern static int OpenWebViewNative(string exepath, WvNativeCallback _callback, string openurl, string closeurl);
		
		[DllImport("PGWebViewWin",SetLastError=true)]
		extern static int QuitWebViewNative();

		public void  OpenWebView(string openurl,string closeurl)
		{
			var process = System.Diagnostics.Process.GetCurrentProcess(); // Or whatever method you are using
			string fullPath = process.MainModule.FileName;
			onWebViewDoneStandAlone = (string data)=>{
				MainThreadDispatcher.data = data;
				QuitWebViewNative();
			};
			try
			{
				Thread t = new Thread(delegate () {
					OpenWebViewNative(fullPath, onWebViewDoneStandAlone, openurl, closeurl);
				});
				t.Start();           
			}
			catch (System.Exception e)
			{
				
				Debug.Log(e.Message);
			}
			
		}

	}
}