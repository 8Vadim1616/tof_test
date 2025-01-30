using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Events;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Platform.Mobile.Advertising;
using Assets.Scripts.UI.Windows;
using Assets.Scripts.User.Ad;
using Assets.Scripts.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Core.Controllers
{
	public class AdvertisingController
	{
		public readonly Dictionary<string, AbstractMobileAdvertising> Partners = new Dictionary<string, AbstractMobileAdvertising>();

		public long StartAdvTime { get; private set; }
		public void SetViewTime() => StartAdvTime = GameTime.Now;
		
		public AdmobAdvertising GetAdMob()
		{
			if (Partners.ContainsKey(AdmobAdvertising.NAME))
				return Partners[AdmobAdvertising.NAME] as AdmobAdvertising;

			return null;
		}

		public bool IsAdvertAvailable(UserAdType point) => IsAdvertAvailable((int) point);
		public bool IsAdvertAvailable(int point)
		{
			var userAdPoint = Game.User.Ads.GetUserAdPoint(point);

			if (userAdPoint == null)
				return false;

			if (userAdPoint.IsReward)
				return IsRewardAvailable(point);
			else if (userAdPoint.IsRewardedInterstitial)
				return IsRewardedInterstitialLoaded(point);
			else if (userAdPoint.IsInterstitial && userAdPoint.GetAvailableInterstitialPartner()?.IsInterstitialAvailable == true)
				return true;

			return false;
		}

		public bool IsRewardAvailable(UserAdType point) => IsRewardAvailable((int) point);
		/// <summary>
		/// Проверяем доступна ли реклама для конкретной рекламмной точки. Для каждой точки может быть свой партнер.
		/// </summary>
		public bool IsRewardAvailable(int point)
		{
			if (Game.User?.Ads is null)
				return false;

			if (!Game.User.Ads.IsRewardAdAvailable(point, false))
				return false;

			var partner = Game.User.Ads.GetUserAdPoint(point)?.GetAvailableRewardPartner()?.Partner;

			if (!partner)
				return false;

			return partner.IsRewardPossible;
		}

		public bool IsRewardedInterstitialLoaded(UserAdType point) => IsRewardedInterstitialLoaded((int) point);
		/// <summary>
		/// Проверяем доступна ли реклама для конкретной рекламмной точки. Для каждой точки может быть свой партнер.
		/// </summary>
		public bool IsRewardedInterstitialLoaded(int point)
		{
			if (Game.User?.Ads is null)
				return false;

			if (!Game.User.Ads.IsRewardedInterstitialAdAvailable(point, true))
				return false;

			var partner = Game.User.Ads.GetUserAdPoint(point)?.GetAvailableRewardedInterstitialPartner()?.Partner;

			if (!partner)
				return false;

			return partner.IsRewardedInterstitialPossible;
		}

		public Promise<AdOptions> ShowAdPromise(UserAdType point) => ShowAdPromise((int) point);

		public Promise<AdOptions> ShowAdPromise(int advertId)
		{
			Promise<AdOptions> promise = new Promise<AdOptions>();

			ShowAdvertising(advertId, afterShow);

			return promise;

			void afterShow(AdOptions adOptions)
			{
				promise.Resolve(adOptions);
			}
		}

		public Promise<AdOptions> ShowAdPromise(UserPartnersPoint availablePartner)
		{
			Promise<AdOptions> promise = new Promise<AdOptions>();

			ShowAdvertising(availablePartner, afterShow, onFail);

			return promise;

			void afterShow(AdOptions adOptions)
			{
				if (promise.CurState == PromiseState.Pending)
					promise.Resolve(adOptions);
			}

			void onFail()
			{
				Utils.Utils.NextFrame()
					.Then(() =>
					{
						if (promise.CurState == PromiseState.Pending)
							promise.Reject(new Exception());
					});
			}
		}

		public void ShowAdPoint(UserAdType type, Action<AdOptions> onComplete, Action onFail = null)
		{
			var point = Game.User.Ads.GetUserAdPoint(type);
			Debug.Log($"[ShowAdPoint] Point request {type}");
			if (point == null)
			{
				OnFail();
				Debug.Log($"[ShowAdPoint] Fail, no point {type}");
				return;
			}

			UserPartnersPoint partner = null;
			if (point.IsInterstitial) partner = point.GetAvailableInterstitialPartner();
			if (point.IsReward) partner = point.GetAvailableRewardPartner();
			if (point.IsRewardedInterstitial) partner = point.GetAvailableRewardedInterstitialPartner();

			if (partner == null)
			{
				OnFail();
				Debug.Log($"[ShowAdPoint] Fail, no partner at {type}");
				return;
			}

			if (point.IsReward && partner.IsRewardAvailable)
				partner.Partner.ShowRewardAd(partner.UserAdPoint.Id, OnComplete, OnFail);
			else if (point.IsRewardedInterstitial && partner.IsRewardedInterstitialAvailable)
				partner.Partner.ShowRewardedInterstitialAd(partner.UserAdPoint.Id, OnComplete, OnFail);
			else if (point.IsInterstitial && partner.IsInterstitialAvailable)
				partner.Partner.ShowInterstitial(partner.UserAdPoint.Id, OnComplete, OnFail);
			else
			{
				Debug.Log($"[ShowAdPoint] Fail, no available partner of type: \"{(point.IsReward ? "rwrd" : (point.IsRewardedInterstitial ? "rwrd_int" : "intr"))}\" ");
				OnFail();
			}

			void OnComplete(Dictionary<string, object> advertParams)
			{
				point.SetDirty();
				onComplete?.Invoke(partner != null ? AdOptions.Of((int)type, partner.ServerPoint, advertParams) : null);
			}
			void OnFail()
			{
				onFail?.Invoke();
			}
		}

		public void ShowAdvertising(UserAdType pointType, Action<AdOptions> onComplete, Action onClosed = null) => ShowAdvertising((int) pointType, onComplete, onClosed);
		public void ShowAdvertising(int pointType, Action<AdOptions> onComplete, Action onClosed = null)
		{
			var point = Game.User.Ads.GetUserAdPoint(pointType);
			var availablePartner = point.GetAvailableRewardPartner();

			if (availablePartner == null || !IsRewardAvailable(pointType))
				DropText();
			else
				availablePartner.Partner.ShowRewardAd(pointType,
					adParams =>
					{
						point.SetDirty();
						onComplete(AdOptions.Of(pointType, availablePartner.ServerPoint, adParams));
					},
					() =>
					{
						EventController.TriggerEvent(new GameEvents.UserAdsWatchStatusUpdate(GameEvents.UserAdsWatchStatusUpdate.AdUpdateStatus.Failed));
						onClosed?.Invoke();
					});
		}

		public void ShowAdvertising(UserPartnersPoint availablePartner, Action<AdOptions> onComplete, Action onClosed = null)
		{
			if (availablePartner == null || !IsRewardAvailable(availablePartner.UserAdPoint.Id))
				DropText();
			else
				availablePartner.Partner.ShowRewardAd(availablePartner.UserAdPoint.Id,
													  adParams =>
													  {
														  availablePartner.UserAdPoint.SetDirty();
														  onComplete(AdOptions.Of(availablePartner.UserAdPoint.Id, availablePartner.ServerPoint, adParams));
													  },
													  () =>
													  {
														  EventController.TriggerEvent(new GameEvents.UserAdsWatchStatusUpdate(GameEvents.UserAdsWatchStatusUpdate.AdUpdateStatus.Failed));
														  onClosed?.Invoke();
													  });
		}

		private void DropText()
		{
			Game.HUD.DropController.DropText("no_reward_text".Localize());
		}

		public void GetStandardReward(int pointType, Action callback = null, bool isReward = true, Dictionary<string, object> extraData = null)
		{
			// Game.Network.QueryManager.RequestPromise(new ShowAdOperation(pointType, extraData))
			// 	.Then(data =>
			// 	 {
			// 		 Game.ServerDataUpdater.Update(data);
			//
			// 		 if (isReward && data.ad != null && data.ad.TryGetValue("error", out JToken err) && err.ToString() == "1")
			// 		 {
			// 			 OnEmptyVideo();
			// 			 return;
			// 		 }
			//
			// 		 var drops = data.GetDrop();
			// 		 YouGotItemsWindow.Of(drops);
			//
			// 		 callback?.Invoke();
			// 	 });
		}

		public static void OnEmptyVideo()
		{
			InfoWindow.Of("attention".Localize(), "no_reward_text".Localize())
					  .AddButton(ButtonPrefab.GreenSquareButtonWithText, "ok".Localize())
					  .FinishAddingButtons();
		}

		public bool IsBankPackAdvAvailable(int advertId) { return IsRewardAvailable((UserAdType) advertId) && Game.User.Ads.IsRewardAdAvailable(advertId, true); }

		public void UpdatePartner(string name, Dictionary<string, string> data)
		{
			AbstractMobileAdvertising partner = null;

			if (Partners.ContainsKey(name))
				partner = Partners[name];
			else if (name == AdmobAdvertising.NAME)
				partner = AddAdmob();
			// else if (name == CrossPromoAdvertising.NAME)
			// 	partner = AddCrossPromo();
			// else if (name == UnityAdvertising.NAME)
			// 	partner = AddUnityAds();
			else if (name == IronSourceAdvertising.NAME)
				partner = AddIronSource();

			//... etc other partners

			if (partner != null)
			{
				partner.UpdateData(data);
				partner.UpdateServerSettings(data);
				Partners[name] = partner;
			}
		}

		public void DestroyPartners()
		{
			foreach (var partner in Partners.Values)
				if (partner)
					Object.DestroyImmediate(partner);
			
			Partners.Clear();
		}

		private AbstractMobileAdvertising AddAdmob()
		{
#if BUILD_HUAWEI && !UNITY_EDITOR
            if (!Game.Instance.gameObject.GetComponent<HuaweiAdvertising>())
                return Game.Instance.gameObject.AddComponent<HuaweiAdvertising>();
#elif UNITY_WEBGL
			if (Game.Checks.IsSocialAdvAvailable && !Game.Instance.gameObject.GetComponent<SocialAdvertising>())
				return Game.Instance.gameObject.AddComponent<SocialAdvertising>();
#elif !UNITY_WSA
			if (!Game.Instance.gameObject.GetComponent<AdmobAdvertising>())
				return Game.Instance.gameObject.AddComponent<AdmobAdvertising>();
#elif UNITY_EDITOR
			if (!Game.Instance.gameObject.GetComponent<TestAdvertising>())
				return Game.Instance.gameObject.AddComponent<TestAdvertising>();
#endif
			return null;
		}

		private AbstractMobileAdvertising AddIronSource()
		{
#if UNITY_EDITOR
			if (!Game.Instance.gameObject.GetComponent<TestAdvertising>())
				return Game.Instance.gameObject.AddComponent<TestAdvertising>();
#else
            if (!Game.Instance.gameObject.GetComponent<IronSourceAdvertising>())
                return Game.Instance.gameObject.AddComponent<IronSourceAdvertising>();
#endif
			return null;
		}

		public T GetMobileAdvertising<T>() where T : AbstractMobileAdvertising
		{
			return Game.Instance.gameObject.GetComponent<T>();
		}

		public bool IsAdShowing => Partners.Values.Any(p => p.IsAdShowing);
	}
}