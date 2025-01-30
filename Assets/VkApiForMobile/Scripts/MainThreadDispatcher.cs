using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using com.playGenesis.VkUnityPlugin;
#if UNITY_STANDALONE_WIN
//using Microsoft.Win32;
#endif
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour {

    public WebView wv;
	public static string data;
#if (UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_IOS) && !UNITY_STANDALONE_LINUX && !UNITY_EDITOR
	void Start () {
      
		wv = FindObjectOfType<WebView>();
#if UNITY_STANDALONE_WIN
        //Sets the registry key for curent running .exe to prevent IExprorer from emulating IE7
        //\HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION
        /*var process = System.Diagnostics.Process.GetCurrentProcess(); // Or whatever method you are using
        var exeName = Path.GetFileName(process.MainModule.FileName);
        object InstallPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", exeName, null);
        
        if (InstallPath == null)
        {
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", exeName, 11000, RegistryValueKind.DWord);
        }*/
#endif
    }
	
	// Update is called once per frame
	void Update () {
		if (data != null){
			wv.WebViewDone(data);
			data = null;
		}
	}
#endif
}
