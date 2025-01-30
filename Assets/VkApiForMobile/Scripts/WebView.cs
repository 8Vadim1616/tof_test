using System;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;

namespace com.playGenesis.VkUnityPlugin
{
    public class WebView : QueueWorker<WebViewRequest>
    {

#if UNITY_STANDALONE_WIN &&  !UNITY_EDITOR

		IVKWebView webViewOpener = new VKWebViewWindows();

#elif UNITY_STANDALONE_OSX &&  !UNITY_EDITOR

		IVKWebView webViewOpener = new VKWebViewMac();

#elif UNITY_ANDROID && !UNITY_EDITOR

        IVKWebView webViewOpener = new VKWebViewAndroid();

#elif UNITY_IOS && !UNITY_EDITOR

        IVKWebView webViewOpener = new VKWebViewiOS();

#elif UNITY_EDITOR
       
        IVKWebView webViewOpener = new VKWebViewEditor();
#else
         
        IVKWebView webViewOpener = new VKWebViewDummy();
#endif




        public event Action<string> WebViewDoneEvent;
        public static WebView Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            
        }
        private void OpenWebView(string navigateToUrl, string closeWhenNavigatedToUrl)
        {
           webViewOpener.OpenWebView(navigateToUrl,closeWhenNavigatedToUrl);
        }

        protected override void StartProcessing()
        {
            OpenWebView(_current.Element.NavigateToUrl, _current.Element.CloseWhenNavigatedToUrl);
        }

        public string parseErrorFormUrl(string url)
        {
			if (url.Contains("cancel=1") || url.Contains("fail=1")||url.Contains("error=access_denied") )
            {
                return "Canceled by user";
            }
            if (url.Contains("network_error=1"))
            {
                return "Network error";
            }
            return null;
        }

        private void OnWebViewDoneIntrnal(string url)
        {
			Debug.Log("InternalWebView");
            _current.Element.LastUrlWithParams = url;
            var error = parseErrorFormUrl(url);

            if (!string.IsNullOrEmpty(error))
            {
                _current.Element.Error = new WebViewError(url, error);
            }
            _current.Element.CallbackAction(_current.Element);

            ProccessNext();
        }

        public void WebViewDone(string url)
        {
			Debug.Log("webview done with url "+url);
            //example  http://web.com?param1=a&param2=b&errormsg=no_network
            if (WebViewDoneEvent != null)
                WebViewDoneEvent(url);

            OnWebViewDoneIntrnal(url);
        }
        void OnDestroy(){
            Instance = null;
        }
    }
}