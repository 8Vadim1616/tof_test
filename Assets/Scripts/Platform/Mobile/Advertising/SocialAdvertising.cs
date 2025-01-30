using System;
using System.Collections.Generic;
using Assets.Scripts.Core.Controllers;
using Assets.Scripts.Libraries.RSG;
using DG.Tweening;
using ExternalScripts;
#if UNITY_WEBGL
using MarksAssets.FullscreenWebGL;
#endif
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Scripts.Platform.Mobile.Advertising
{
    public class SocialAdvertising : AbstractMobileAdvertising
    {
		private const string SOCIAL = "social";
        protected override string Name => SOCIAL;

		protected enum RewardType { Interstitial, Reward, RewardedIntestitial };

		public override void UpdateData(Dictionary<string, string> data)
        {
	        if(Inited)
		        return;
	        
            Debug.Log(TAG + "Init");

			Inited = true;

			LoadRewardAd();
			LoadRewardedInterstitialAd();
		}

		public Promise VerifyAdvExistence()
		{
			var promise = new Promise();

			errorRewardTimer?.Kill();
			errorRewardedInterstitialTimer?.Kill();
			errorInterstitialTimer?.Kill();
			errorOfferwallTimer?.Kill();

			var obj = new JObject();
			obj["ref"] = "AdvPrepare";
			obj["loc"] = Game.User.Group;

			Game.Social.Request(new Adapter.Actions.Advertising(Adapter.Actions.Advertising.PREPARE, obj))
				.Then(onComplete, onError);

			return promise;

			void onComplete(object data)
			{
				var isExist = (bool) data;
				if(isExist)
				{
					OnRewardLoaded();
					OnInterstitialLoaded();
					OnRewardedInterstitialLoaded();
				}
				else
				{
					OnRewardLoadFailed("No Ad");
					GameLogger.info(TAG + "Ad failed to load. No Ad.");
				}

				promise.Resolve();
			}

			void onError(Exception e)
			{
				OnRewardLoadFailed();
				GameLogger.info(TAG + "Error to load ad.");

				promise.Resolve();
			}
		}

		private void ShowAdv(RewardType rewardType)
		{
			if (!Game.Checks.IsSocialAdvAvailable)
				return;

			switch (rewardType)
			{
				case RewardType.Interstitial:
					OnInterstitialOpened();
					break;
				case RewardType.RewardedIntestitial:
					OnRewardedInterstitialOpened();
					break;
				default:
				case RewardType.Reward:
					OnRewardOpened();
					break;
			}

			Game.AdvertisingController.SetViewTime();
			OnDeactivate();

			if (ExternalInterface.IsAvailable)
				ExternalInterface.CallFromIframe("hideGame");

			GameLogger.info(TAG + "try show video");

			var obj = new JObject();
			obj["ref"] = "AdvShow";
			obj["loc"] = Game.User.Group;

			Game.Social.Request(new Adapter.Actions.Advertising(Adapter.Actions.Advertising.SHOW, obj))
							.Then(onCompleteHandler, onErrorHandler);

			void onCompleteHandler(object data)
			{
				GameLogger.info(TAG + "video was showed. Get reward..");

				if (ExternalInterface.IsAvailable)
					ExternalInterface.CallFromIframe("showGame");

				OnActivate();

				switch (rewardType)
				{
					case RewardType.Interstitial:
						break;
					case RewardType.RewardedIntestitial:
						OnRewardedInterstitialComplete();
						break;
					default:
					case RewardType.Reward:
						OnRewardComplete();
						break;
				}

				DOVirtual.DelayedCall(0.2f, () =>
				{
					switch (rewardType)
					{
						case RewardType.Interstitial:
							OnInterstitialClosed();
							break;
						case RewardType.RewardedIntestitial:
							OnRewardedInterstitialRewarded();
							break;
						default:
						case RewardType.Reward:
							OnRewarded();
							OnRewardClosed();
							break;
					}
				});
			}

			void onErrorHandler(Exception e)
			{
				if (ExternalInterface.IsAvailable)
					ExternalInterface.CallFromIframe("showGame");

				GameLogger.warning(TAG + "video error");
				OnActivate();
				AdvertisingController.OnEmptyVideo();
				
				OnRewardLoadFailed("video error");
			}
		}

		protected override void LoadRewardAd()
        {
			base.LoadRewardAd();
			VerifyAdvExistence();
		}

		protected override void LoadInterstitialAd()
		{
			base.LoadInterstitialAd();
			VerifyAdvExistence();
		}

		protected override void LoadRewardedInterstitialAd()
		{
			base.LoadRewardedInterstitialAd();
			VerifyAdvExistence();
		}

		protected override void StartShowInterstitial() => ShowAdv(RewardType.Interstitial);
        protected override void StartShowReward() => ShowAdv(RewardType.Reward);
		protected override void StartShowRewardedInterstitial() => ShowAdv(RewardType.RewardedIntestitial);

		private void OnDeactivate()
		{
#if !UNITY_EDITOR && UNITY_WEBGL
			if (FullscreenWebGL.isFullscreen())
				FullscreenWebGL.ExitFullscreen();
#endif

			Game.Locker.Lock(TAG);

			Game.Sound.OnDeactivate();
		}

		private void OnActivate()
		{
			Game.Locker.Unlock(TAG);

			Game.Sound.OnActivate();
		}
	}
}