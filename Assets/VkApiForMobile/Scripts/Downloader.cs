using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace com.playGenesis.VkUnityPlugin
{
    #pragma warning disable 612
    public class Downloader : MonoBehaviour
    {
        public void download(DownloadRequest d)
        {
            StartCoroutine(_download(d));
        }

        private IEnumerator _download(DownloadRequest d)
        {
            var request = d.url;
            var www = UnityWebRequestTexture.GetTexture(Uri.EscapeUriString(request));
            yield return www.SendWebRequest();
            d.DownloadResult = www;
            if (d.onFinished != null)
                d.onFinished(d);
        }
    }

    public class DownloadRequest
    {
        public string url { get; set; }
        public Action<DownloadRequest> onFinished { get; set; }
 
        public UnityWebRequest DownloadResult { get; set; }
        public object[] CustomData { get; set; }
    }
}