using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Platform.Mobile.Advertising;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.HUD.Panels;
using Assets.Scripts.UI.WindowsSystem;
using Assets.Scripts.User.Ad;
using Assets.Scripts.User.BankPacks;
using Assets.Scripts.Utils;
using DG.Tweening;
using Platform.Mobile.Advertising;
using UI.Screens;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Windows.Bank
{
	public class BankWindow : AbstractWindow
	{
		[SerializeField] private ScrollRect _commonContent;
		[SerializeField] private ScrollRect _dnaContent;
		[SerializeField] private ButtonText _btnCommon;
		[SerializeField] private ButtonText _btnDNA;
		[SerializeField] private GameObject _dnaAlarm;
		[SerializeField] private GameObject _bottom;

		[SerializeField] private TabPages _tabs;

		[SerializeField] private BankGroupView _groupPrefab;
		[SerializeField] private SomeResourcesPanel _someResourcesPanel;
		[SerializeField] private BankPayToRemoveInterstitialPanel _payToRemoveInterstitialPanel;

		private List<BankGroupView> _groups = new List<BankGroupView>();
		private Item _needItem;
		private IEnumerable<IGrouping<int, UserBankPackItem>> _bookmarks;
		[SerializeField] private NativeAdView _nativeAd;

		private static BankWindow _instance = null;
		
		public static BankWindow Of()
		{
			if (_instance)
				return _instance;
			
			return Game.Windows.ScreenChange<BankWindow>(true, w => w.Init());
		}
		
		public static BankWindow Of(Item needItem)
		{
			if (_instance)
			{
				_instance._needItem = needItem;
				_instance.ScrollToItem(needItem);
				return _instance;
			}

			return Game.Windows.ScreenChange<BankWindow>(true, w => w.Init(needItem));
		}

		protected override void OnClose()
		{
			_instance = null;
			
			if (Game.AdvertisingController != null && _nativeAd.IsInited)
			{
				Game.AdvertisingController.GetAdMob()?.LoadNativeBanner();
			}
			
			base.OnClose();
		}

		private void Init(Item needItem = null)
		{
			_instance = this;
			_needItem = needItem;

			_payToRemoveInterstitialPanel.Init();

			_tabs.Init( (btn, isSelected, index) =>
			{
				btn.SetAlphaTween(isSelected ? 1 : 0.75f);
			});

			_btnCommon.Text = "bank_bookmark_1".Localize();
			_btnDNA.Text = "bank_bookmark_2".Localize();
			
			_someResourcesPanel.Init(new List<Item> { Game.Static.Items.Money1, Game.Static.Items.Money2 });

			_bookmarks = Game.User.BankPacks.All
							  .Where(p => p.Group > 0/* && Game.User.Level.Value.Id >= p.StartLevel*/)
							  .GroupBy(p => p.Bookmark)
							  .Where(g => g.Any());

			if (_bookmarks.Count() == 1)
			{
				_bottom.SetActive(false);
				_commonContent.viewport.offsetMin = new Vector2();
			}

			_groupPrefab.SetActive(true);
			foreach (var bookmark in _bookmarks)
			{
				var scroll = bookmark.Key == 1 ? _commonContent : _dnaContent;
				
				var groups = bookmark
					.GroupBy(p => p.Group)
					.OrderBy(p => p.First().Sort);
				
				foreach (var group in groups)
				{
					var groupView = Instantiate(_groupPrefab, scroll.content);
					groupView.Init(scroll, this, group.ToList(), $"bank_group_title_{group.Key}".Localize());
					_groups.Add(groupView);
				}
			}
			_groupPrefab.SetActive(false);

			UpdateNativeAd();
		}
		
		private void UpdateNativeAd()
		{
			if (Game.User.Ads.GetUserAdPoint(UserAdType.AD_NATIVE_BANNER_BANK)?.IsAvailableNativeBanner() == true)
			{
				if (Game.AdvertisingController != null)
					Game.AdvertisingController.GetAdMob()?.OnNativeAdShow(UserAdType.AD_NATIVE_BANNER_BANK);

				_nativeAd.SetActive(true);
				_nativeAd.Init(new NativeAdWrapper(AdmobAdvertising.NativeAd));
			}
			else
				_nativeAd.SetActive(false);
		}

		protected override void OnShow()
		{
			base.OnShow();

			if (_needItem != null)
				ScrollToItem(_needItem);
		}

		private void ScrollToItem(Item needItem)
		{
			if (needItem != null)
			{
				var bookmark = _bookmarks.FirstOrDefault(b => b.Any(p => p.MainItemCount()?.Item == needItem));
				if (bookmark != null)
					_tabs.SelectTab(bookmark.Key - 1);
				
				var needGroupView = _groups.FirstOrDefault(g => g.Items.Any(i => i.BankPackItem.MainItemCount()?.Item == needItem));
				ScrollTo(needGroupView);
			}
		}
		
		private IPromise ScrollTo(BankGroupView view)
		{
			if (!view)
				return Promise.Resolved();

			var scroll = view.Scroll;

			if (!scroll)
				return Promise.Resolved();

			var viewport = scroll.viewport;
			var scrollDuration = .5f;

			var targetPosition = viewport.anchoredPosition - view.RectTransform.anchoredPosition;
			
			scroll.content.DOAnchorPosY(targetPosition.y, scrollDuration)
				  .SetEase(Ease.InOutQuart);
			
			return Scripts.Utils.Utils.Wait(scrollDuration);

			// float viewportHeight = scroll.viewport.rect.height;
			//
			// float targetPositionY = scroll.transform.InverseTransformPoint(scroll.content.position).y
			// 						- scroll.transform.InverseTransformPoint(view.transform.position).y
			// 						- viewportHeight * .5f;
			//
			// float maxPosition = scroll.content.rect.height - viewportHeight;
			//
			// targetPositionY = Mathf.Clamp(targetPositionY, 0f, maxPosition);

			// if (IsOnScreen(targetPositionY, scroll.content.anchoredPosition.y, viewportHeight * .5f))
			// 	return Promise.Resolved();

			// scroll.content.DOAnchorPosY(targetPositionY, 0.5f)
			// 	   .SetLink(gameObject)
			// 	   .SetEase(Ease.InOutQuart);
			//
			// return Scripts.Utils.Utils.Wait(0.5f);
			//
			// bool IsOnScreen(float targetPosY, float anchorPos, float viewportHalfHeight) =>
			// 				targetPosY <= anchorPos + viewportHalfHeight && targetPosY >= anchorPos - viewportHalfHeight;
		}
	}
}