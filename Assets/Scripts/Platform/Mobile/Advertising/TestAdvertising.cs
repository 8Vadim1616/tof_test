using System.Collections.Generic;
using Assets.Scripts.UI.Windows;

namespace Assets.Scripts.Platform.Mobile.Advertising
{
	public class TestAdvertising : AbstractMobileAdvertising
	{
		protected override string Name => "test_ad";

		public override void UpdateData(Dictionary<string, string> data)
		{
			Inited = true;
			LoadRewardAd();
			LoadInterstitialAd();
			if (data.ContainsKey("is_bn"))
			{
				BannerUnitId = data["is_bn"];
				LoadBannerAd(AdSizeType.Standard, AdPosition.Bottom);
			}
		}

		public virtual void LoadBannerAd(AdSizeType adSize, AdPosition position)
		{
			base.LoadBannerAd(adSize, position);
			OnBannerLoaded();
		}

		protected override void LoadRewardAd()
		{
			base.LoadRewardAd();

			Utils.Utils.Wait(3)
				 .Then(OnRewardLoaded);
		}

		protected override void LoadInterstitialAd()
		{
			base.LoadInterstitialAd();

			Utils.Utils.Wait(3)
				 .Then(() => OnInterstitialLoaded());
		}

		protected override void StartShowInterstitial()
		{
			OnInterstitialOpened();
			InfoWindow.Of("Interstitial", "this is test interstitial")
				.ClosePromise
				.Then(() => OnInterstitialClosed());
		}

		protected override void StartShowReward()
		{
			OnRewardOpened();
			var window = InfoWindow.Of("Reward", "this is test reward. You will rewarded after 5 sec.");
			var closePromise = window.ClosePromise;

			Utils.Utils.Wait(5)
				.Then(() =>
				{
					if (window)
						window.mainText.text = "Rewarded! Close the window";

					//if (closePromise.IsPending)
					//	OnRewarded();
				});

			closePromise.Then(() => { OnRewarded(); OnRewardClosed(); } );
		}

		protected override void StartShowRewardedInterstitial()
		{
			//throw new System.NotImplementedException();
		}
	}
}