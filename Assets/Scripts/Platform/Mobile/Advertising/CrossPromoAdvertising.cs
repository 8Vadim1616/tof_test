#if false
using System.Collections.Generic;
using Assets.Scripts.Core.AssetsManager;
using Assets.Scripts.UI.Screens;
using UnityEngine;
using UnityEngine.Video;

namespace Assets.Scripts.Platform.Mobile.Advertising
{
    public class CrossPromoAdvertising : AbstractMobileAdvertising
    {
        public const string NAME = "pm8";
        protected override string Name => NAME;

        private string _videoName = "intro_2";
        private string _packageName = "ru.playme8.dachniki";

		public override void UpdateData(Dictionary<string, string> data)
        {
            Inited = true;
            UpdateRewardData(data);
        }

        private void UpdateRewardData(Dictionary<string, string> data)
        {
            if (data.ContainsKey("video"))
                _videoName = data["video"];
            if (data.ContainsKey("app"))
                _packageName = data["app"];
            
            Debug.Log(TAG + "video = " + _videoName);
            Debug.Log(TAG + "app = " + _packageName);
            
            LoadRewardAd();
        }

        protected override void StartShowInterstitial() { }
        protected override void StartShowReward()
        {
            OnRewardOpened();
            CrossPromoScreen.Of(_videoName, _packageName)
                .Then(() => OnRewarded())
                .Finally(OnRewardClosed)
                .Finally(LoadRewardAd);
        }

        protected override void LoadRewardAd()
        {
            base.LoadRewardAd();
            AssetsManager.Instance.Loader.LoadAndCache<VideoClip>(_videoName);
            OnRewardLoaded();
        }
	}
}
#endif