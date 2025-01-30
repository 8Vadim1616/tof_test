using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playGenesis.VkUnityPlugin{
	public class VKWebViewEditor : IVKWebView {
		public void OpenWebView(string openurl, string closeurl)
        {
            Application.OpenURL(openurl);
        }
	}
}