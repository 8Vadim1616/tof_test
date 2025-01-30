using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playGenesis.VkUnityPlugin{
	public class VKWebViewAndroid : IVKWebView {
		AndroidJavaObject jo;
		public void  OpenWebView(string openurl,string closeurl)
		{
			jo=new AndroidJavaObject("com.playgenesis.vkunityplugin.Initializer"); 
			jo.Call ("OpenWebView",openurl,closeurl);
		}
	}
	
}
