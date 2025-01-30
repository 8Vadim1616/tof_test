using System;
using System.Collections.Generic;
using Assets.Scripts.Events;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Network.Queries.Operations.Api;
using Assets.Scripts.User.Ad;
using Assets.Scripts.Utils;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Platform.Mobile.Advertising
{
	/// <summary>
	/// Класс упрвляет загрузкой, показом и логированием рекламы на различных платформах.
	/// </summary>
	public abstract class AbstractMobileAdvertising : MonoBehaviour
	{
		public enum State
		{
			NONE, 
			LOADING, 
			LOADED,
			FAILED
		}

		protected const string INTERSTITIAL = "INTERSTITIAL";
		protected const string REWARDED_INTERSTITIAL = "REWARDED_INTERSTITIAL";
		protected const string REWARD = "REWARD";
		protected const string OFFERWALL = "OFFERWALL";
		protected const string BANNER = "BANNER";
		protected const string APPOPEN = "APPOPEN";

		private const string LOADING_DISLINE_TIMEOUT = "dis_timeout";
		private const string LOADING_ERROR_TIMEOUT = "err_timeout";
		private const string LOADING_SUCCESS_TIMEOUT = "succ_timeout";

		public int LoadingDislineTimeout = 600;
		private int _loadingErrorTimeout = 900;
		private int _loadingSuccessTimeout = 2;

		protected string TAG => $"[ADS][{Name.ToUpper()}] ";

		protected abstract string Name { get; }

		public ReactiveProperty<State> CurrentRewardState { get; protected set; } = new ReactiveProperty<State>(State.NONE);
		
		public ReactiveProperty<State> CurrentAppOpenState { get; protected set; } = new ReactiveProperty<State>(State.NONE);

		public bool IsAppOpenPossible => IsAppOpenLoading || IsAppOpenLoaded;
		public bool IsAppOpenLoading => CurrentAppOpenState.Value == State.LOADING;
		public bool IsAppOpenLoaded => CurrentAppOpenState.Value == State.LOADED;
		
		public bool IsRewardPossible => IsRewardLoading || IsRewardLoaded;

		public bool IsRewardLoading => CurrentRewardState.Value == State.LOADING;
		public bool IsRewardLoaded => CurrentRewardState.Value == State.LOADED;
		public bool IsRewardFailed => CurrentRewardState.Value == State.FAILED;

		public ReactiveProperty<State> CurrentRewardedInterstitialState { get; protected set; } = new ReactiveProperty<State>(State.LOADING);

		public bool IsRewardedInterstitialPossible => IsRewardedInterstitialLoading || IsRewardedInterstitialLoaded;

		public bool IsRewardedInterstitialLoading => CurrentRewardedInterstitialState.Value == State.LOADING;
		public bool IsRewardedInterstitialLoaded => CurrentRewardedInterstitialState.Value == State.LOADED;
		public bool IsRewardedInterstitialFailed => CurrentRewardedInterstitialState.Value == State.FAILED;
		
		public ReactiveProperty<State> CurrentBannerState { get; protected set; } = new ReactiveProperty<State>(State.NONE);

		public bool IsBannerPossible => !BannerUnitId.IsNullOrEmpty() || IsBannerLoading || IsBannerLoaded;

		public bool IsBannerLoading => CurrentBannerState.Value == State.LOADING;
		public bool IsBannerLoaded => CurrentBannerState.Value == State.LOADED;
		public bool IsBannerFailed => CurrentBannerState.Value == State.FAILED;
		
		public virtual bool IsNativeBannerPosible => false;

		public virtual ReactiveProperty<bool> IsRewardLoadedReactive { get; } = new ReactiveProperty<bool>();
		public ReactiveProperty<bool> IsRewardedInterstitialLoadedReactive { get; } = new ReactiveProperty<bool>();
		public ReactiveProperty<bool> IsInterstitialLoadedReactive { get; } = new ReactiveProperty<bool>();
		public ReactiveProperty<bool> IsOfferwallLoadedReactive { get; } = new ReactiveProperty<bool>();
		public ReactiveProperty<bool> IsBannerLoadedReactive { get; } = new ReactiveProperty<bool>();
		public ReactiveProperty<bool> IsBannerShown { get; } = new ReactiveProperty<bool>();
		public ReactiveProperty<bool> IsAppOpenLoadedReactive { get; } = new ReactiveProperty<bool>();

		private long LastInterstitialWatchTime { get; set; }
		private long LastInterstitialTime { get; set; }
		public bool IsAdShowing { get; private set; } = false;
		public bool Inited { get; protected set; }
		
		public string BannerUnitId { get; protected set; }
		public Dictionary<string, object> BannerParams { get; protected set; }

		protected Tween errorRewardTimer;
		protected Tween errorRewardedInterstitialTimer;
		protected Tween errorInterstitialTimer;
		protected Tween errorOfferwallTimer;

		public ReactiveProperty<Dictionary<string, object>> OnBannerPaidData =
						new ReactiveProperty<Dictionary<string, object>>();

		protected void Start()
		{
			IsRewardLoadedReactive.Subscribe(x => EventController.TriggerEvent(new GameEvents.UserAdsUpdated()));
			IsRewardedInterstitialLoadedReactive.Subscribe(x => EventController.TriggerEvent(new GameEvents.UserAdsUpdated()));
			IsInterstitialLoadedReactive.Subscribe(x => EventController.TriggerEvent(new GameEvents.UserAdsUpdated()));
			IsOfferwallLoadedReactive.Subscribe(x => EventController.TriggerEvent(new GameEvents.UserAdsUpdated()));
		}


		private const string AD_ACTION = "ad";
		public void SendLog(string status)
		{
			var data = new Dictionary<string, object>();
			data["name"] = Name;
			data["stat"] = status;

			ServerLogs.SendLog(AD_ACTION, data);
		}

		public void SendLog(string status, Dictionary<string, object> additional = null)
		{
			var data = new Dictionary<string, object>
					   {
									   ["name"] = Name,
									   ["stat"] = status
					   };

			if (additional != null)
				foreach (var add in additional)
					data[add.Key] = add.Value;

			ServerLogs.SendLog(AD_ACTION, data);
			Debug.Log(TAG + status);
		}

		public abstract void UpdateData(Dictionary<string, string> data);

		public void UpdateServerSettings(Dictionary<string, string> data)
		{
			if (data == null)
				return;

			if (data.ContainsKey(LOADING_DISLINE_TIMEOUT))
				LoadingDislineTimeout = int.Parse(data[LOADING_DISLINE_TIMEOUT]);

			if (data.ContainsKey(LOADING_ERROR_TIMEOUT))
				_loadingErrorTimeout = int.Parse(data[LOADING_ERROR_TIMEOUT]);

			if (data.ContainsKey(LOADING_SUCCESS_TIMEOUT))
				_loadingSuccessTimeout = int.Parse(data[LOADING_SUCCESS_TIMEOUT]);
		}

		private IPromise WaitFromAppActivate()
		{
			var promise = new Promise();

			Game.ExecuteOnMainThread(() =>
			{
				Game.GameReloader.WaitActivate()
					.Then(() => Utils.Utils.NextFrame(10))      // тест, ждём пока аппка прочухается после закрытия ad
					.Finally(promise.ResolveOnce);
			});

			return promise;
		}

		#region interstitial

		public static ReactiveProperty<int> InterstitialsCount { get; private set; } = new ReactiveProperty<int>(0);

		protected abstract void StartShowInterstitial();

		protected virtual void LoadInterstitialAd()
		{
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Interstitial start to load"));
			IsInterstitialLoadedReactive.Value = false;
		}

		public void ShowInterstitial(int adPoint, Action<Dictionary<string, object>> onComplete, Action onFail)
		{
			Game.ExecuteOnMainThread(() =>
			{
				if (IsInterstitialLoadedReactive.Value)
				{
					SendLog(INTERSTITIAL + " start show", new Dictionary<string, object>
														  {
																		  { "ad_point", adPoint }
														  });

					IsAdShowing = true;
					LastInterstitialTime = GameTime.Now;
					
					Game.Sound.OnDeactivate();
					StartShowInterstitial();

					_onCompleteReward = onComplete;
					_onCloseReward = onFail;
				}
				else
					onFail?.Invoke();
			});
		}

		protected void OnInterstitialLoaded(Dictionary<string, object> advertParams = null)
		{
			if (advertParams == null)
				SendLog(INTERSTITIAL + " loaded");
			else
				SendLog(INTERSTITIAL + " loaded", new Dictionary<string, object>() { { "params", advertParams } });

			IsInterstitialLoadedReactive.Value = true;
			Game.ExecuteOnMainThread(() =>
			{
				InterstitialsCount.Value++;
				Debug.Log(TAG + "Interstitial Loaded");
			});
		}

		protected virtual void OnInterstitialLoadFailed(string errMsg = "")
		{
			IsInterstitialLoadedReactive.Value = false;
			SendLog(INTERSTITIAL + " failed: " + errMsg);
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Interstitial Load Failed : " + errMsg));
			errorInterstitialTimer = DOVirtual.DelayedCall(30, LoadInterstitialAd);
		}

		protected virtual void OnInterstitialFailedToShow(string errMsg = "")
		{
			IsInterstitialLoadedReactive.Value = false;
			SendLog(INTERSTITIAL + " failed to show: " + errMsg);
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Interstitial Failed to show: " + errMsg));

			IsAdShowing = false;

			_onCloseReward?.Invoke();
			_onCloseReward = null;
			_onCompleteReward = null;

			errorInterstitialTimer = DOVirtual.DelayedCall(30, LoadInterstitialAd);
		}

		protected virtual void OnInterstitialRecordImpression()
		{
			SendLog(INTERSTITIAL + " record impression");
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Interstitial Record Inpression"));
		}

		protected virtual void OnInterstitialPaidEvent(string errMsg = "")
		{
			SendLog(INTERSTITIAL + " paid event: " + errMsg);
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Interstitial Paid Event: " + errMsg));
		}

		protected void OnInterstitialOpened(Dictionary<string, object> advertParams = null)
		{
			if (advertParams == null)
				SendLog(INTERSTITIAL + " opened");
			else
				SendLog(INTERSTITIAL + " opened", new Dictionary<string, object>() { { "params", advertParams } });

			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Interstitial Opened"));
		}

		protected void OnInterstitialClosed(Dictionary<string, object> advertParams = null)
		{
			WaitFromAppActivate()
						   .Then(() =>
							{
								_onCompleteReward?.Invoke(advertParams);
								_onCompleteReward = null;
								_onCloseReward = null;

								IsInterstitialLoadedReactive.Value = false;

								if (advertParams == null)
									SendLog(INTERSTITIAL + " closed");
								else
									SendLog(INTERSTITIAL + " closed", new Dictionary<string, object>() { { "params", advertParams } });

								IsAdShowing = false;
								LastInterstitialWatchTime = Math.Max(GameTime.Now - LastInterstitialTime, 0);

								Debug.Log(TAG + "Interstitial Closed. LastInterstitialWatchTime = " + LastInterstitialWatchTime);

								DOVirtual.DelayedCall(5, LoadInterstitialAd);
								
								Game.Sound.OnActivate();
							});
		}
		#endregion

		#region reward

		private int _lastRewardType = (int) UserAdType.NONE;
		private Action<Dictionary<string, object>> _onCompleteReward;
		private Action _onCloseReward;

		public static event Action RewardRewarded;

		protected abstract void StartShowReward();

		protected virtual void LoadRewardAd()
		{
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Reward start to load"));
			IsRewardLoadedReactive.Value = false;
			CurrentRewardState.Value = State.LOADING;
		}

		protected void OnRewardComplete()
		{
			WaitFromAppActivate()
						   .Then(() =>
							{
								IsAdShowing = false;
								Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Reward Completed"));
								EventController.TriggerEvent(new GameEvents.UserAdsWatchStatusUpdate(GameEvents.UserAdsWatchStatusUpdate.AdUpdateStatus.Successed));
							});
		}

		protected void OnRewardLoaded()
		{
			SendLog(REWARD + " loaded");

			Game.ExecuteOnMainThread(
									 () =>
									 {
										 IsRewardLoadedReactive.Value = true;
										 CurrentRewardState.Value = State.LOADED;
										 Debug.Log(TAG + "Reward Loaded");
										 EventController.TriggerEvent(new GameEvents.UserAdsWatchStatusUpdate());
									 });
		}

		protected virtual void OnRewardLoadFailed(string errMsg = "", bool needFail = false)
		{
			SendLog(REWARD + " failed: " + errMsg);
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Reward Load failed: " + errMsg));
			if (_loadingErrorTimeout >= 0)
				errorRewardTimer = DOVirtual.DelayedCall(_loadingErrorTimeout, LoadRewardAd);

			IsRewardLoadedReactive.Value = false;

			if (needFail)
				CurrentRewardState.Value = State.FAILED;
			else
				CurrentRewardState.Value = State.LOADING;

			_onCloseReward?.Invoke();
			_onCloseReward = null;
		}

		protected virtual void OnRewardFailedToShow(string errMsg = "")
		{
			SendLog(REWARD + " failed to show: " + errMsg);
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Reward Failed to show: " + errMsg));
			if (_loadingErrorTimeout >= 0)
				errorRewardTimer = DOVirtual.DelayedCall(_loadingErrorTimeout, LoadRewardAd);

			IsRewardLoadedReactive.Value = false;
			CurrentRewardState.Value = State.FAILED;

			IsAdShowing = false;

			_onCloseReward?.Invoke();
			_onCloseReward = null;
			_onCompleteReward = null;
		}

		protected virtual void OnRewardRecordImpression()
		{
			SendLog(REWARD + " record impression");
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Reward Record Inpression"));
		}

		protected virtual void OnRewardPaidEvent(string errMsg = "")
		{
			SendLog(REWARD + " paid event: " + errMsg);
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Reward Paid Event: " + errMsg));
		}

		protected void OnRewardOpened(Dictionary<string, object> advertParams = null)
		{
			if (advertParams == null)
				SendLog(REWARD + " opened");
			else
				SendLog(REWARD + " opened", new Dictionary<string, object>() { { "params", advertParams } });

			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Reward opened"));
		}

		protected void OnRewardClosed(Dictionary<string, object> advertParams = null)
		{
			WaitFromAppActivate()
						   .Then(() =>
							{
								Utils.Utils.NextFrame()
									 .Then(() =>
									  {
										  Game.Sound.OnActivate();
										  
										  //Если не загружена, то ничего не делаем
										  if (!IsRewardLoadedReactive.Value)
											  return;
							
										  CurrentRewardState.Value = State.LOADING;

										  if (advertParams == null)
											  SendLog(REWARD + " closed");
										  else
											  SendLog(REWARD + " closed", new Dictionary<string, object>() { { "params", advertParams } });

										  IsAdShowing = false;
										  Debug.Log(TAG + "Reward closed");
										  EventController.TriggerEvent(new GameEvents.UserAdsWatchStatusUpdate(GameEvents.UserAdsWatchStatusUpdate.AdUpdateStatus.Closed));

										  if (_loadingSuccessTimeout >= 0)
											  DOVirtual.DelayedCall(_loadingSuccessTimeout, LoadRewardAd);

										  IsRewardLoadedReactive.Value = false;
										  _onCloseReward?.Invoke();
										  _onCloseReward = null;
									  });
							});
		}

		protected void OnRewarded(string msg = "", Dictionary<string, object> advertParams = null)
		{
			WaitFromAppActivate()
						   .Then(() =>
							{
								if (advertParams == null)
									SendLog("rewarded");
								else
									SendLog("rewarded", new Dictionary<string, object>() { { "params", advertParams } });

								Game.User.Settings.LastRewardTime = GameTime.Now;

								Debug.Log(TAG + "Reward rewarded " + msg);

								if (_lastRewardType != (int) UserAdType.NONE)
									_onCompleteReward?.Invoke(advertParams);
								_onCompleteReward = null;
								_onCloseReward = null;

								_lastRewardType = (int) UserAdType.NONE;

								RewardRewarded?.Invoke();
							});
		}

		public virtual void ShowAppOpenAd()
		{
			
		}

		public bool ShowRewardAd(int adType, Action<Dictionary<string, object>> onComplete, Action onClose)
		{
			if (IsRewardLoaded && !IsAdShowing)
			{
				SendLog(REWARD + " start show", new Dictionary<string, object>
												{
																{ "ad_point", adType }
												});
				//Debug.Log(TAG + "ShowRewardAd");

				IsAdShowing = true;
				_onCompleteReward = onComplete;
				_onCloseReward = onClose;
				_lastRewardType = adType;

				Game.Sound.OnDeactivate();
				StartShowReward();
				// ShowTestReward();

				return true;
			}

			Debug.Log(TAG + "ShowRewardAd Not Loaded");

			return false;
		}

		#endregion

		#region rewarded interstitial

		private int _lastRewardedInterstitialType = (int) UserAdType.NONE;
		private Action<Dictionary<string, object>> _onCompleteRewardedInterstitial;
		private Action _onCloseRewardedInterstitial;

		public static event Action RewardedInterstitialRewarded;

		protected abstract void StartShowRewardedInterstitial();

		protected virtual void LoadRewardedInterstitialAd()
		{
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Rewarded Interstitial start to load"));
			IsRewardedInterstitialLoadedReactive.Value = false;
			CurrentRewardedInterstitialState.Value = State.LOADING;
		}

		protected virtual void OnRewardedInterstitialLoadFailed(string errMsg = "", bool needFail = false)
		{
			SendLog(REWARDED_INTERSTITIAL + " failed: " + errMsg);
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Rewarded Interstitial Load failed: " + errMsg));
			if (_loadingErrorTimeout >= 0)
				errorRewardedInterstitialTimer = DOVirtual.DelayedCall(_loadingErrorTimeout, LoadRewardedInterstitialAd);

			IsRewardedInterstitialLoadedReactive.Value = false;

			if (needFail)
				CurrentRewardedInterstitialState.Value = State.FAILED;
			else
				CurrentRewardedInterstitialState.Value = State.LOADING;

			_onCloseRewardedInterstitial?.Invoke();
			_onCloseRewardedInterstitial = null;

			_onCompleteRewardedInterstitial = null;
		}

		protected void OnRewardedInterstitialFailedFullScreen()
		{
			CurrentRewardedInterstitialState.Value = State.LOADING;

			SendLog($"{REWARDED_INTERSTITIAL} failed full screen");

			IsAdShowing = false;
			Game.ExecuteOnMainThread(
									 () =>
									 {
										 Debug.Log(TAG + $"Rewarded Interstitial failed full screen");
										 EventController.TriggerEvent(new GameEvents.UserAdsWatchStatusUpdate(GameEvents.UserAdsWatchStatusUpdate.AdUpdateStatus.Closed));
									 });

			if (_loadingSuccessTimeout >= 0)
				DOVirtual.DelayedCall(_loadingSuccessTimeout, LoadRewardedInterstitialAd);

			IsRewardedInterstitialLoadedReactive.Value = false;
			_onCloseRewardedInterstitial?.Invoke();
			_onCloseRewardedInterstitial = null;

			_onCompleteRewardedInterstitial = null;
		}

		protected void OnRewardedInterstitialDismiss()
		{
			WaitFromAppActivate()
						   .Then(() =>
							{
								if (_lastRewardedInterstitialType == (int) UserAdType.NONE || _onCloseRewardedInterstitial == null)
									return;

								CurrentRewardedInterstitialState.Value = State.LOADING;

								SendLog($"{REWARDED_INTERSTITIAL} dismiss");

								IsAdShowing = false;
								Game.ExecuteOnMainThread(
														 () =>
														 {
															 Debug.Log(TAG + $"Rewarded Interstitial dismiss");
															 EventController.TriggerEvent(new GameEvents.UserAdsWatchStatusUpdate(GameEvents.UserAdsWatchStatusUpdate.AdUpdateStatus.Closed));
														 });

								if (_loadingSuccessTimeout >= 0)
									DOVirtual.DelayedCall(_loadingSuccessTimeout, LoadRewardedInterstitialAd);

								IsRewardedInterstitialLoadedReactive.Value = false;
								_onCloseRewardedInterstitial?.Invoke();
								_onCloseRewardedInterstitial = null;

								_onCompleteRewardedInterstitial = null;
							});
		}

		protected void OnRewardedInterstitialPresentFullScreenContent()
		{
			SendLog(REWARDED_INTERSTITIAL + " oppened full screen");
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Rewarded Interstitial oppened full screen"));
		}

		protected void OnRewardedInterstitialRecordImpression()
		{
			SendLog(REWARDED_INTERSTITIAL + " record impression");
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Rewarded Interstitial record impression"));
		}

		protected void OnRewardedInterstitialPaidEvent()
		{
			SendLog(REWARDED_INTERSTITIAL + " paid event");
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Rewarded Interstitial paid event"));
		}

		public bool ShowRewardedInterstitialAd(int adType, Action<Dictionary<string, object>> onComplete, Action onClose)
		{
			if (IsRewardedInterstitialLoaded)
			{
				SendLog(REWARDED_INTERSTITIAL + " start show", new Dictionary<string, object>
															   {
																			   { "ad_point", adType }
															   });
				//Debug.Log(TAG + "ShowRewardedInterstitialAd");

				IsAdShowing = true;
				_onCompleteRewardedInterstitial = onComplete;
				_onCloseRewardedInterstitial = onClose;
				_lastRewardedInterstitialType = adType;

				StartShowRewardedInterstitial();

				return true;
			}

			Debug.Log(TAG + "ShowRewardedInterstitialAd Not Loaded");
			onClose?.Invoke();

			return false;
		}

		protected void OnRewardedInterstitialRewarded(string msg = "")
		{
			SendLog(REWARDED_INTERSTITIAL + " rewarded");
			Game.ExecuteOnMainThread(
									 () =>
									 {
										 Debug.Log(TAG + "Rewarded Interstitial rewarded " + msg);

										 if (_lastRewardedInterstitialType != (int) UserAdType.NONE)
											 _onCompleteRewardedInterstitial?.Invoke(null);
										 _onCompleteRewardedInterstitial = null;
										 _onCloseRewardedInterstitial = null;

										 _lastRewardedInterstitialType = (int) UserAdType.NONE;

										 RewardedInterstitialRewarded?.Invoke();
									 });

			if (_loadingSuccessTimeout >= 0)
				DOVirtual.DelayedCall(_loadingSuccessTimeout, LoadRewardedInterstitialAd);

			IsRewardedInterstitialLoadedReactive.Value = false;
		}

		protected void OnRewardedInterstitialLoaded()
		{
			SendLog(REWARDED_INTERSTITIAL + " loaded");

			Game.ExecuteOnMainThread(
									 () =>
									 {
										 IsRewardedInterstitialLoadedReactive.Value = true;
										 CurrentRewardedInterstitialState.Value = State.LOADED;
										 Debug.Log(TAG + "Rewarded Interstitial Loaded");
										 EventController.TriggerEvent(new GameEvents.UserAdsWatchStatusUpdate());
									 });
		}

		protected void OnRewardedInterstitialOpened()
		{
			SendLog(REWARDED_INTERSTITIAL + " opened");
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Rewarded Interstitial opened"));
		}

		protected void OnRewardedInterstitialComplete()
		{
			WaitFromAppActivate()
						   .Then(() =>
							{
								IsAdShowing = false;
								Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Rewarded Interstitial Completed"));
								EventController.TriggerEvent(new GameEvents.UserAdsWatchStatusUpdate(GameEvents.UserAdsWatchStatusUpdate.AdUpdateStatus.Successed));
							});
		}

		#endregion

		#region offerwall


		private Action<Dictionary<string, object>> _onCompleteOfferwall;
		private Action _onCloseOfferwall;


		protected virtual void StartShowOfferwall() { }

		protected virtual void LoadOfferwallAd()
		{
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + OFFERWALL + " start to load"));
			IsOfferwallLoadedReactive.Value = false;
		}

		protected void OnOfferwallLoaded()
		{
			SendLog(OFFERWALL + " loaded");

			Game.ExecuteOnMainThread(
									 () =>
									 {
										 IsOfferwallLoadedReactive.Value = true;
										 Debug.Log(TAG + "Offerwall Loaded");
										 EventController.TriggerEvent(new GameEvents.UserAdsWatchStatusUpdate());
									 });
		}

		protected void OnOfferwallLoadFailed(string errMsg = "")
		{
			IsOfferwallLoadedReactive.Value = false;
			SendLog(OFFERWALL + " failed: " + errMsg);
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Offerwall Load failed: " + errMsg));
			if (_loadingErrorTimeout >= 0)
				errorOfferwallTimer = DOVirtual.DelayedCall(_loadingErrorTimeout, LoadOfferwallAd);

			IsOfferwallLoadedReactive.Value = false;
			_onCloseOfferwall?.Invoke();
			_onCloseOfferwall = null;
		}

		protected void OnOfferwallOpened()
		{
			SendLog(OFFERWALL + " opened");
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Offerwall opened"));
		}

		protected void OnOfferwallClosed()
		{
			IsOfferwallLoadedReactive.Value = false;
			SendLog(OFFERWALL + " closed");

			IsAdShowing = false;
			Game.ExecuteOnMainThread(
									 () =>
									 {
										 Debug.Log(TAG + "Offerwall closed");
										 EventController.TriggerEvent(new GameEvents.UserAdsWatchStatusUpdate(GameEvents.UserAdsWatchStatusUpdate.AdUpdateStatus.Closed));
									 });

			if (_loadingSuccessTimeout >= 0)
				DOVirtual.DelayedCall(_loadingSuccessTimeout, LoadOfferwallAd);

			IsOfferwallLoadedReactive.Value = false;
			_onCloseOfferwall?.Invoke();
			_onCloseOfferwall = null;
			
			Game.Sound.OnActivate();
		}

		protected void OnOfferwallRewarded(Dictionary<string, object> data)
		{
			SendLog(OFFERWALL + " rewarded");

			Debug.Log(TAG + OFFERWALL + " reward data:");
			foreach (var pair in data)
				Debug.Log(TAG + OFFERWALL + " : " + pair.Key + " - " + pair.Value);

			_onCompleteOfferwall?.Invoke(data);
			_onCompleteOfferwall = null;
		}

		public void ShowOfferwallAd(Action<Dictionary<string, object>> onComplete, Action onClose = null)
		{
			if (IsOfferwallLoadedReactive.Value)
			{
				Debug.Log(TAG + "ShowOfferwallAd");
				SendLog(OFFERWALL + " start show");

				IsAdShowing = true;
				_onCompleteOfferwall = onComplete;
				_onCloseOfferwall = onClose;

				Game.Sound.OnDeactivate();
				StartShowOfferwall();
			}
			else
			{
				Debug.Log(TAG + "ShowOfferwallAd Not Loaded");
				OnOfferwallClosed();
			}
		}

		#endregion

		#region banner

		public bool BannerWasDestroyed { get; private set; } = true;

		public virtual void BannerShow() {}
		public virtual void BannerHide() {}
		public virtual void BannerDestroy()
		{
			IsBannerLoadedReactive.Value = false;
			CurrentBannerState.Value = State.NONE;
			BannerWasDestroyed = true;
		}
		
		public virtual void LoadBannerAd(AdSizeType adSize, AdPosition position)
		{
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Banner start to load"));
			IsBannerLoadedReactive.Value = false;
			CurrentBannerState.Value = State.LOADING;
			BannerWasDestroyed = false;
		}
		
		public void OnBannerLoaded(Dictionary<string, object> advertParams = null)
		{
			if (advertParams == null)
				SendLog(BANNER + " loaded");
			else
				SendLog(BANNER + " loaded", new Dictionary<string, object>() { { "params", advertParams } });

			Game.ExecuteOnMainThread(
									 () =>
									 {
										 if (BannerWasDestroyed)
										 {
											 Debug.Log(TAG + "Banner Was Destroyed Before Loaded");
										 }
										 else
										 {
											 IsBannerLoadedReactive.SetValueAndForceNotify(true);
											 CurrentBannerState.Value = State.LOADED;
											 Debug.Log(TAG + "Banner Loaded");
											 // EventController.TriggerEvent(new GameEvents.UserAdsWatchStatusUpdate());
										 }
									 });
		}

		public void OnBannerOpened()
		{
			SendLog(BANNER + " opened");
			
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Banner opened"));
		}
		
		public void OnBannerClosed()
		{
			WaitFromAppActivate()
						   .Then(() =>
							{
								Utils.Utils.NextFrame()
									 .Then(() =>
									  {
										  //Если не загружена, то ничего не делаем
										  if (!IsBannerLoadedReactive.Value)
											  return;
							
										  CurrentBannerState.Value = State.NONE;

										  SendLog(BANNER + " closed");
										  
										  Debug.Log(TAG + "Banner closed");
										  // EventController.TriggerEvent(new GameEvents.UserAdsWatchStatusUpdate(GameEvents.UserAdsWatchStatusUpdate.AdUpdateStatus.Closed));
										  
										  // DOVirtual.DelayedCall(5, LoadBannerAd);

										  IsBannerLoadedReactive.Value = false;
									  });
							});
		}

		public void OnBannerLoadFailed(string message)
		{
			SendLog(BANNER + " failed: " + message);

			CurrentBannerState.Value = State.FAILED;
			IsBannerLoadedReactive.Value = false;
			Game.ExecuteOnMainThread(() => Debug.Log(TAG + "Banner Load Failed : " + message));
			// DOVirtual.DelayedCall(30, LoadBannerAd);
		}

		protected virtual void OnBannerPaidEvent(Dictionary<string, object> param)
		{
			//SendLog(BANNER + " paid event: " + errMsg);
			Game.ExecuteOnMainThread(() =>
			{
				Debug.Log(TAG + "Banner Paid Event");
				OnBannerPaidData.Value = param;
			});
		}

		#endregion
	}
}