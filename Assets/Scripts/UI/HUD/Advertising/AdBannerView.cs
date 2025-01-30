using System;
using Assets.Scripts.Network.Queries.Operations.Api;
using UnityEngine;
using UniRx;
using Assets.Scripts.Platform.Mobile.Advertising;
using Assets.Scripts.UI.WindowsSystem;
using Assets.Scripts.User.Ad;
using Assets.Scripts.User.Ad.Points;
using Assets.Scripts.Utils;
using DG.Tweening;


namespace Assets.Scripts.UI.HUD.Advertising
{
	public class AdBannerView : MonoBehaviour
	{
		private const string TAG = "[ADS]<color=cyan>[BANNER]</color> ";

		[SerializeField] RectTransform _bannerRect;

		public RectTransform BannerRect => _bannerRect;

		public bool IsBannerLoaded { get; private set; }
		private UserAdPoint AdPoint => Game.User?.Ads?.GetUserAdPoint(UserAdType.AD_BANNER);
		private bool IsAdAvailable => AdPoint?.IsAvailableBanner() == true;

		//private AdSize BannerSize => Game.Settings?.BannerType switch
		//{
		//	AdSize.Type.Standard => AdSize.Banner,
		//	AdSize.Type.SmartBanner => AdSize.SmartBanner,
		//	AdSize.Type.AnchoredAdaptive => AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth),
		//	_ => AdSize.IABBanner
		//};

		private AdSizeType BannerSize => Game.Settings?.BannerType ?? AdSizeType.Standard;

		private AbstractMobileAdvertising _advertPartner;
		private AbstractMobileAdvertising GetPartner(UserPartnersPoint partnerPoint) => _advertPartner ??= partnerPoint?.Partner;

		private UserPartnersPoint GetPartnerPoint() => AdPoint?.GetAvailableBannerPartner();

		private IDisposable windowObservable;
		private IDisposable _partnerBannerLoadObservable;
		private IDisposable _partnerBannerOnLoadFailed;

		private void Awake()
		{
			_bannerRect.SetActive(false);
		}

		public void OnEnable()
		{
			RequestBanner();
		}

		public bool RequestBanner()
		{
			//_bannerRect.SetActive(false);
			IsBannerLoaded = false;

			// if (_bannerRect)
			// 	_bannerRect.SetActive(false);

			if (!IsAdAvailable || !Game.Instance)
			{
				Debug.Log(TAG + "banner ad is not available");
				return false;
			}

			var partnerPoint = GetPartnerPoint();
			Debug.Log(TAG + $"GetPartnerPoint = {partnerPoint.UserAdPoint.Id}");
			var partner = GetPartner(partnerPoint);

			if (!partner)
			{
				Debug.Log(TAG + "partner not found");
				return false;
			}
			
			Debug.Log(TAG + $"partner = {partner}");

			if (!partner.Inited)
			{
				Debug.Log(TAG + "partner is not inited");
				return false;
			}

			if (partner.IsBannerLoading)
			{
				Debug.Log(TAG + "banner already loading");
				return true;
			}

			if (partner.IsBannerLoadedReactive.Value)
			//{
				Debug.Log(TAG + "existed banner openned");
				//HandleOnAdLoaded(partner, partnerPoint);
				//return true;
			//}

			_partnerBannerLoadObservable?.Dispose();
			_partnerBannerLoadObservable = partner.IsBannerLoadedReactive
												  .Subscribe((val) =>
												   {
													   if (val)
														   HandleOnAdLoaded(partner, partnerPoint);
												   })
												  .AddTo(this);

			_partnerBannerOnLoadFailed?.Dispose();
			_partnerBannerOnLoadFailed = partner.CurrentBannerState
				.Subscribe(HandleOnAdLoadFailed)
				.AddTo(this);

			//Debug.Log(TAG + $"banner pos x: {posX}, y: {posY} (banner canvas H: {GetBannerCanvasHeight()}, banner H: {GetBannerHeight()})");
			//bannerView = new BannerView(adUnitId, BannerSize, posX, posY);
			if (partner.BannerWasDestroyed)
			{
				partner.LoadBannerAd(BannerSize, AdPosition.Top);
			}

			windowObservable?.Dispose();
			windowObservable = Game.Windows.CurrentScreen
								   .Subscribe(OnWindowChanged)
								   .AddTo(this);

			Game.Instance.OnScreenResize -= OnScreenResized;
			Game.Instance.OnScreenResize += OnScreenResized;

			Debug.Log(TAG + "banner requested");
			return true;
		}

		void OnScreenResized()
		{
			if (IsBannerLoaded && _advertPartner != null)   // костыль на обновление позиции баннера после переворота экрана
			{
				bannerShowDelay?.Kill();
				_advertPartner.BannerHide();
				//LogBanner(false, "OnScreenResized");
				ShowOrHideBanner();
			}
		}

		// public void LogBanner(bool isShow, string fromFunc)
		// {
		// 	Debug.Log(TAG + (isShow ? $"SHOW ({fromFunc}) " : $"HIDE ({fromFunc}) ") + $"IsAdAvailable = {IsAdAvailable}, Game.Instance = {Game.Instance != null}, CurrentLevelReactive = {Game.Instance.CurrentLevelReactive.Value}, CurrentScreen = {Game.Windows.CurrentScreen.Value}, QueueCount = {Game.Windows.QueueCount}");
		// }

		private void OnWindowChanged(AbstractWindow win)
		{
			ShowOrHideBanner();
		}

		private UserPartnersPoint _lastPartnerPoint;
		public void HandleOnAdLoaded(AbstractMobileAdvertising partner, UserPartnersPoint partnerPoint)
		{
			_lastPartnerPoint = partnerPoint;
			Game.ExecuteOnMainThread(() =>
			{
				// if (!isActiveAndEnabled)
				// {
				// 	VisualDestroyBanner();
				// 	return;
				// }
				
				if (!IsBannerLoaded)
				{
					var bannerAdHeight = GetBannerHeight();
#if BUILD_HUAWEI
					_bannerRect.SetActive(true);
					_bannerRect.sizeDelta = _bannerRect.sizeDelta.Set(y: bannerAdHeight);
					Debug.Log(TAG + $"Huawei banner H: {bannerAdHeight}");
#else
					var bannerRectHeight = GetBannerHeight(bannerAdHeight);

					//Debug.Log(TAG + $"Ad Height: {bannerView.GetHeightInPixels()} ({bannerAdHeight} => {bannerRectHeight}), width: {bannerView.GetWidthInPixels()}");

					//_bannerRect.SetActive(true);
					_bannerRect.sizeDelta = _bannerRect.sizeDelta.Set(y: bannerRectHeight);
#endif
				}

				IsBannerLoaded = true;
				
				/*
				Game.ServiceProvider.RequestPromise(new AdShowOperation(partnerPoint.UserAdPoint.Id, partnerPoint.ServerPoint, adParams: partner.BannerParams))
					.Then(Game.ServerDataUpdater.Update);

				if (Game.Instance)
					Game.Instance.ForceInvokeScreenResize();
					*/

				ShowOrHideBanner();

			});
		}

		private Tween bannerShowDelay;

		private bool IsBannerAvailableByGameState
		{
			get
			{
				return !(!IsAdAvailable || !Game.Instance
					|| Game.Windows.CurrentScreen.Value != null || Game.Windows.QueueCount > 0
					/*
					|| !IsBannerLoaded
					|| !_advertPartner.IsBannerLoaded*/);
			}
		}

		private void ShowOrHideBanner()
		{
			Debug.Log(TAG + $"ShowOrHideBanner {_advertPartner}");
			if (!_advertPartner)
				return;
			
			if (!IsBannerAvailableByGameState)
			{
				Debug.Log(TAG + "Banner is not available by game state");
				bannerShowDelay?.Kill();
				if (!IsAdAvailable)
					_bannerRect.SetActive(false);
				_advertPartner.BannerHide();
			}
			else
			{
				Debug.Log(TAG + "Banner is available by game state");
				bannerShowDelay?.Kill();
				bannerShowDelay = DOVirtual.DelayedCall(Game.Settings.BannerShowDelay, () =>
					{
						if (IsBannerAvailableByGameState)
						{
							_bannerRect.SetActive(true);
							_advertPartner.BannerShow();
							// Game.ServiceProvider.RequestPromise(new AdShowOperation(_lastPartnerPoint.UserAdPoint.Id,
							// 										_lastPartnerPoint.ServerPoint,
							// 										_lastPartnerPoint.PartnerName,
							// 										adParams: _advertPartner.BannerParams));
						}
					})
					.SetLink(gameObject);
			}
		}

		private void HandleOnAdLoadFailed(AbstractMobileAdvertising.State state)
		{
			switch (state)
			{
				case AbstractMobileAdvertising.State.FAILED:
					VisualDestroyBanner(true);

					if (!isActiveAndEnabled)
						break;

					DOVirtual.DelayedCall(Game.Settings.BannerReloadDelay, () => RequestBanner());
					break;

				//case AbstractMobileAdvertising.State.NONE:
				//	if (_bannerRect)
				//		_bannerRect.SetActive(false);
				//	break;
			}
		}

		private float GetBannerHeight(float bannerHeight)
		{
			if (bannerHeight <= 0)
				return 0;

			var screenHeight = ((RectTransform) Game.MainCanvas.transform).rect.height;
			var bannerCanvasHeight = GetBannerCanvasHeight();

			if (screenHeight <= 0 || bannerCanvasHeight <= 0)
				return 0;

			Debug.Log(TAG + $"screen H: {screenHeight}, banner canvas H: {bannerCanvasHeight}, banner H: {bannerHeight} /// result: {(screenHeight / bannerCanvasHeight * bannerHeight)}");
			return screenHeight / bannerCanvasHeight * bannerHeight;
		}

		private int GetBannerCanvasHeight() => BannerSize switch // выяснено методом тыка
		{
			AdSizeType.Standard => 800,
			AdSizeType.AnchoredAdaptive => 1440,
			_ => 1980
		}; 

		private float GetBannerHeight() => BannerSize switch
		{
#if BUILD_HUAWEI
			AdSizeType.Standard => 50,
			_ => BannerAdSize.GetCurrentDirectionBannerSize((int) _bannerRect.rect.width).HeightPx
#else
			AdSizeType.Standard => 50,
			AdSizeType.AnchoredAdaptive => 100,
			// _ => bannerView?.GetHeightInPixels() ?? 0
			_ => 0
#endif
		};

		private void OnDisable() => VisualDestroyBanner();
		private void OnDestroy() => VisualDestroyBanner();

		public void DestroyBanner()
		{
			VisualDestroyBanner(true);
		}

		private void VisualDestroyBanner(bool destroyBanner = false)
		{
			Debug.Log(TAG + "VisualDestroyBanner");
			
			Game.Instance.OnScreenResize -= OnScreenResized;

			if (_advertPartner != null)
			{
				//LogBanner(false, "VisualDestroyBanner");
				bannerShowDelay?.Kill();
				_advertPartner.BannerHide();
			}

			if (destroyBanner && _advertPartner != null)
			{
				Debug.Log(TAG + "VisualDestroyBanner real destroy");
				//LogBanner(false, "VisualDestroyBanner (DESTROY)");
				_advertPartner.BannerDestroy();
			}

			_partnerBannerLoadObservable?.Dispose();

			windowObservable?.Dispose();

			//if (_bannerRect)
			//	_bannerRect.SetActive(false);

			//if (Game.Instance)
			//	Game.Instance.ForceInvokeScreenResize();
		}
	}
}