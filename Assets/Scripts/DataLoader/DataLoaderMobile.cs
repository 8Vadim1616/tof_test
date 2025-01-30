using Assets.Scripts.GameServiceProvider;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Platform.Mobile;
using Assets.Scripts.Platform.Mobile.Advertising;
using Assets.Scripts.Platform.Mobile.Analytics;
using UnityEngine;

namespace Assets.Scripts
{
	public class DataLoaderMobile : DataLoader
	{
		public DataLoaderMobile(bool isReload, bool needChangeSsk) : base(isReload, needChangeSsk) { }

		public override IPromise Load(PreloaderScreen preloaderScreen)
		{
			var promise = new Promise();
			string advertId = null;

			base.Load(preloaderScreen)
				.Then(() => ServerLogs.LoadGameProgress("init analytics start"))
				.Then(MobileAnalytics.GetAdvertisingId)
				.Then(adId =>
				{
					advertId = adId;
					Progress = 2;
					return Promise.Resolved();
				})
				.Then(() => RegisterUser(advertId))
				.Then(GetProfile)
				.Then(InitAnalytics)
				.Then(CheckSSK)
				.Then(GetBackendServer)
				.Then(() => ServerLogs.LoadGameProgress("init analytics end"))
				.Then(() => Progress = 10)
				.Then(() => ServerLogs.LoadGameProgress("init texts start"))
				.Then(InitTexts)
				.Then(CheckSSK)
				.Then(() => Progress = 20)
				.Then(() => Utils.Utils.NextFrame())
				.Then(() => ServerLogs.LoadGameProgress("static start"))
				.Then(() => new StaticDataUpdater().Update())
				.Then(() => GetStaticData(x => SetProgress(x, 20, 40)))
				.Then(() => Utils.Utils.NextFrame())
				.Then(CheckSSK)
				.Then(() => Progress = 40)
				.Then(() => ServerLogs.LoadGameProgress("user info start"))
				.Then(GetUserInfo)
				.Then(CheckSSK)
				.Then(() => Utils.Utils.NextFrame())
				.Then(() => ServerLogs.LoadGameProgress("check stats start"))
				.Then(() => Utils.Utils.NextFrame())
				.Then(CheckSSK)
				.Then(() => ServerLogs.LoadGameProgress("user frnds start"))
				.Then(() => Utils.Utils.NextFrame())
				.Then(() => Progress = 60)
				//.Then(() => Game.Instance.AdMob.TryShowAppStartAd())
				.Then(CheckSSK)
				.Then(() => ServerLogs.LoadGameProgress("init mobile start"))
				.Then(InitMobilePlatform)
				.Then(CheckSSK)
				.Then(() => ServerLogs.LoadGameProgress("init mobile end"))
				.Then(() => promise.Resolve())
				.Catch((e) =>
				{
					Debug.LogError(e);
					promise.Reject(e);
				});

			return promise;

			void SetProgress(float value, int start, int end) => 
				Progress = (int) (start + (end - start) * value);
		}

		protected override void CreateServiceProvider(bool relogin)
		{
#if !UNITY_WEBGL
			if (!Game.Instance.PlatformMobile)
				Game.Instance.PlatformMobile = Game.Instance.gameObject.AddComponent<PlatformMobile>();
#endif

			// Game.ServiceProvider = new MobileServiceProvider();
			// Game.ServiceProvider.Init(relogin);
		}

		protected void Slog(string message)
		{
			//if (Game.User?.Config?.IsNeedSendDLMLogs == true)
			//	ServerLogs.SendLog(message);
		}
	}
}