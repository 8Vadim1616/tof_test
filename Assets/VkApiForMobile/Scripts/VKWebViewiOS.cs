using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

#if UNITY_IOS && !UNITY_EDITOR //to prevent android il2cpp complaining abount undefined symbols
namespace com.playGenesis.VkUnityPlugin{
	public class VKWebViewiOS: IVKWebView  {
        
        [DllImport("__Internal")]
		private static extern void _OpenWebView(string openUrl,string closeurl, WvNativeCallback callback);
        
		[DllImport("__Internal")]
        extern static void quitWebView();

		public delegate void WvNativeCallback(string msg);

        [MonoPInvokeCallback(typeof(WvNativeCallback))]
        static void onWebViewDoneStandAlone(string data){
             MainThreadDispatcher.data = data;
                quitWebView();
        }
		public void  OpenWebView(string openurl,string closeurl)
		{

			_OpenWebView( openurl, closeurl, onWebViewDoneStandAlone);
		}

	}
}
#endif