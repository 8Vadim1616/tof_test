using GoogleMobileAds.Api;
using UnityEngine;
namespace Platform.Mobile.Advertising
{
    public class NativeAdWrapper
    {
        private NativeAd _nativeAd;
        
        public NativeAdWrapper(NativeAd nativeAd)
        {
            _nativeAd = nativeAd;
        }

        public Texture2D GetIconTexture() => _nativeAd?.GetIconTexture() ?? new Texture2D(128, 128);
        public Texture2D GetAdChoicesLogoTexture() => _nativeAd?.GetAdChoicesLogoTexture() ?? new Texture2D(128, 128);
        public string GetHeadlineText() => _nativeAd != null ? _nativeAd.GetHeadlineText() : "Test headline";
        public string GetCallToActionText() => _nativeAd != null ? _nativeAd.GetCallToActionText() : "Test action text";
        public string GetAdvertiserText() => _nativeAd != null ? _nativeAd.GetAdvertiserText() : "Test advertiser text";
        public string GetBodyText() => _nativeAd != null ? _nativeAd.GetBodyText() : "Test body text. Test body text. Test body text.";
        
        public void RegisterBodyTextGameObject(GameObject gameObject)
        {
            if (_nativeAd != null)
                _nativeAd.RegisterBodyTextGameObject(gameObject);
        }

        public void RegisterIconImageGameObject(GameObject gameObject)
        {
            if (_nativeAd != null)
                _nativeAd.RegisterIconImageGameObject(gameObject);
        }
        
        public void RegisterAdChoicesLogoGameObject(GameObject gameObject)
        {
            if (_nativeAd != null)
                _nativeAd.RegisterAdChoicesLogoGameObject(gameObject);
        }
        
        public void RegisterHeadlineTextGameObject(GameObject gameObject)
        {
            if (_nativeAd != null)
                _nativeAd.RegisterHeadlineTextGameObject(gameObject);
        }
        
        public void RegisterCallToActionGameObject(GameObject gameObject)
        {
            if (_nativeAd != null)
                _nativeAd.RegisterCallToActionGameObject(gameObject);
        }
        
        public void RegisterAdvertiserTextGameObject(GameObject gameObject)
        {
            if (_nativeAd != null)
                _nativeAd.RegisterAdvertiserTextGameObject(gameObject);
        }
    }
}