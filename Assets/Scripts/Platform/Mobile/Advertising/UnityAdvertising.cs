#if false

using System.Collections.Generic;
using Assets.Scripts.UI.Utils;
using UnityEngine;
using UnityEngine.Advertisements;

namespace Assets.Scripts.Platform.Mobile.Advertising
{
    public class UnityAdvertising : AbstractMobileAdvertising, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        public const string NAME = "unity";
        
        private const string UNITY_GAME_ID = "game_id";
        private const string UNITY_RWRD = "unity_rwrd";
        private const string UNITY_INT = "unity_int";
        
        protected override string Name => NAME;

        private string _gameId;
        private string _rewardUnitId;
        private string _interstitialUnitId;

        private bool _initFirst;
        
        public override void UpdateData(Dictionary<string, string> data)
        {
            if(Inited)
                return;
            
            Debug.Log(TAG + "Init");

            data.TryGetValue(UNITY_GAME_ID, out _gameId);
            data.TryGetValue(UNITY_RWRD, out _rewardUnitId);
            data.TryGetValue(UNITY_INT, out _interstitialUnitId);

            if (_gameId.IsNullOrEmpty())
            {
                Debug.LogWarning(TAG + "Init failed. No game_id!");
                return;
            }

            if (_rewardUnitId.IsNullOrEmpty() && _interstitialUnitId.IsNullOrEmpty())
            {
                Debug.LogError(TAG + "Init failed. No reward or interstitial unit keys");
                return;
            }
            
            Debug.Log(TAG + UNITY_GAME_ID + " = " + _gameId);
            Debug.Log(TAG + UNITY_RWRD + " = " + _rewardUnitId);
            Debug.Log(TAG + UNITY_INT + " = " + _interstitialUnitId);

            _initFirst = true;
            
            Advertisement.Initialize(_gameId, false, true, this);
        }
        
        

        public void OnInitializationComplete()
        {
            Debug.Log("Unity Ads initialization complete.");
            Inited = true;

            if (!_rewardUnitId.IsNullOrEmpty())
                LoadRewardAd();

            if (!_interstitialUnitId.IsNullOrEmpty())
                LoadInterstitialAd();
        }
        
        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            _initFirst = false;
            Inited = false;
            Debug.LogWarning(TAG + "Init failed" + error + " - " + message);
        }
        
        
        

        protected override void LoadInterstitialAd()
        {
            base.LoadInterstitialAd();
            Advertisement.Load(_interstitialUnitId, this);
        }

        protected override void LoadRewardAd()
        {
            base.LoadRewardAd();
            Advertisement.Load(_rewardUnitId, this);
        }

        
        
        

        protected override void StartShowInterstitial()
        {
            Advertisement.Show(_interstitialUnitId, this);
        }

        protected override void StartShowReward()
        {
            Advertisement.Show(_rewardUnitId, this);
        }
        
        
        

        public void OnUnityAdsAdLoaded(string placementId)
        {
            if(placementId == _rewardUnitId)
                OnRewardLoaded();
            else if(placementId == _interstitialUnitId)
                OnInterstitialLoaded();
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            if(placementId == _rewardUnitId)
                OnRewardLoadFailed(error + " - " + message);
            else if(placementId == _interstitialUnitId)
                OnInterstitialLoadFailed(error + " - " + message);
        }

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            if (placementId == _rewardUnitId)
            {
                SendLog(REWARD + " failed: " + error + " - " + message);
                Debug.Log(TAG + "Reward Load failed: " + error + " - " + message);
                OnRewarded();
                OnRewardClosed();
            }
            else if (placementId == _interstitialUnitId)
            {
                OnInterstitialLoadFailed(error + " - " + message);
            }
        }

        public void OnUnityAdsShowStart(string placementId)
        {
            if(placementId == _rewardUnitId)
                OnRewardOpened();
            else if(placementId == _interstitialUnitId)
                OnInterstitialOpened();
        }

        public void OnUnityAdsShowClick(string placementId)
        {
            if (placementId == _rewardUnitId)
            {
                SendLog(REWARD + " clicked");
                Debug.Log(TAG + REWARD + " clicked");
            }
            else if (placementId == _interstitialUnitId)
            {
                SendLog(INTERSTITIAL + " clicked");
                Debug.Log(TAG + INTERSTITIAL + " clicked");
            }
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            if (placementId == _rewardUnitId)
            {
                if(showCompletionState == UnityAdsShowCompletionState.COMPLETED)
                    OnRewarded();
                OnRewardClosed();
            }
            else if(placementId == _interstitialUnitId)
                OnInterstitialClosed();
        }
    }
}
#endif