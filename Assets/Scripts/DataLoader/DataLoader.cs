using Assets.Scripts.BuildSettings;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Localization;
using Assets.Scripts.Network;
using Assets.Scripts.Network.Queries.Operations.Api;
using Assets.Scripts.Platform.Adapter.Implements;
using Assets.Scripts.Static;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using Febucci.UI.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using Assets.Scripts.Core.AssetsManager;
using Assets.Scripts.Network.Queries.Operations;
using Assets.Scripts.Platform.Mobile.Advertising;
using Assets.Scripts.User.Sockets;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
	public abstract class DataLoader : IDataLoader
	{
		protected PreloaderScreen Preloader;
		protected bool _isReload;
		protected bool _needChangeSsk;
		protected JObject _iframeVars = null;

		private int _progress = 0;

		public DataLoader(bool isReload, bool needChangeSsk)
		{
			_isReload = isReload;
			_needChangeSsk = needChangeSsk;
		}

		protected IPromise InitTexts()
		{
			return Utils.Utils.NextFrame()
						.Then(TAnimBuilder.InitializeGlobalDatabase)
						.Then(() => Utils.Utils.NextFrame());
		}

		public virtual IPromise Load(PreloaderScreen preloaderScreen)
		{
			SetPreloader(preloaderScreen);
			return CreateBase();
		}

		public virtual void SetPreloader(PreloaderScreen preloaderScreen)
		{
			Preloader = preloaderScreen;
		}

		public virtual int Progress
		{
			get => _progress;
			set
			{
				_progress = value;
				if (Preloader)
					Preloader.Progress = value;
			}
		}

		protected abstract void CreateServiceProvider(bool relogin);

		protected virtual IPromise CreateBase()
		{
			Promise createPromise = new Promise();

			if (Game.Instance.GameNetwork == null)
			{
				Game.Instance.GameNetwork = new GameNetwork();
				Game.Instance.GameNetwork.Init();
				Game.Instance.GameNetwork.QueryManager.Server = GameConsts.ServerEntryPoint;
			}
			GameLogger.debug("GameNetwork");

			Game.Instance.GameConsts = new GameConsts();
			GameLogger.debug("GameConsts");

			Promise socialInitPromise = InitSocialNetwork();

			Game.Instance.StaticData = new StaticData();
			GameLogger.debug("StaticData");

			Game.Instance.UserData = new UniRx.ReactiveProperty<UserData>(new UserData());
			GameLogger.debug("UserData");

			Game.Instance.GameLocalization = new GameLocalization();
			GameLogger.debug("GameLocalization");


			CreateServiceProvider(!_needChangeSsk);

			createPromise.Resolve();

			return Promise.All(new[]{socialInitPromise, createPromise});
		}

		public IPromise GetStaticDataPromise() => GetStaticData();

		protected IPromise GetStaticData(Action<float> progress = null) => Game.Static.GetStaticData(progress);

		protected virtual Promise InitSocialNetwork()
		{
			Promise socialInitPromise = new Promise();
			Game.Social.Init(GameLocalization.Locale, _iframeVars, OnSocialInit);
			GameLogger.debug("Social");
			return socialInitPromise;

			void OnSocialInit(AbstractSocialAdapter adapter)
			{
#if UNITY_WEBGL
				InitLocale(); // Берём локаль из айфрейма
#endif
				socialInitPromise.ResolveOnce();
			}
		}
		
				/// <summary>
		/// Получаем инфу о сервере, к которому коннектится
		/// </summary>
		/// <returns></returns>
		protected IPromise GetBackendServer()
		{
			return process();

			IPromise process()
			{
				var result = new Promise();

				Game.Network.QueryManager.RequestPromise(new GetServerOperation()).Then(
					 response =>
					 {
						 if (!string.IsNullOrEmpty(response.server))
						 {
							 var wasEntryPointChange = Game.Network.QueryManager.Server != response.server;

							 Game.Network.QueryManager.Server = response.server;

							 if (wasEntryPointChange)
							 {
								 GameLogger.debug("New Entry Point " + response.server);

								 Game.Network.QueryManager.ResetManager();

								 process().Then(result.Resolve);

								 return;
							 }
						 }

						 Game.GameLogger.Init(response.max_crash_for_session, response.crash_url);
						 Game.SessionKey = null;

						 // AssetsManager.PARTS_URL = response.partsurl;
						 // AssetsManager.ResourcesLoadServerPath = response.addressables;
						 // AssetsManager.PreloadCatalogs = response.preload_catalogs;

						 if (!string.IsNullOrEmpty(response.server))
							 Game.Network.QueryManager.Server = response.server;

						 if (response.timeout != 0)
							 Game.Network.QueryManager.DefaultRequestTimeout = response.timeout;

						 // if (response.attempts_timeout.HasValue)
							//  Game.Network.QueryManager.BetweenAttemptsTimeout = response.attempts_timeout.Value;

						 if (response.attempts != 0)
							 Game.Network.QueryManager.Attempts = response.attempts;

						 if (response.crashlog_tm != 0)
							 GameLogger.CrashlogDelay = response.crashlog_tm;

						 if (response.fps != 0)
						 {
#if !UNITY_WEBGL
							 Application.targetFrameRate = response.fps;
#endif
						 }

						 if (response.consent.HasValue)
							 Game.ShowConsent = response.consent.Value;

						 if (response.is_test_suite.HasValue)
							 IronSourceAdvertising.IsTestSuite = response.is_test_suite.Value;

						 if (response.ping_int != 0)
							 Game.User.Sockets.PingInterval = response.ping_int;

						 UserSockets.Init(response.wss);
						 GameLogger.debug("GetBackendServer");
						 result.Resolve();
					 },
					 (exception =>
					 {
						 result.Reject(exception);
						 GameLogger.error("GetBackendServer Error");
					 })
					);

				return result;
			}
		}
		
		/// <summary>
		/// Регистрация игрока
		/// </summary>
		/// <param name="hwid"></param>
		/// <returns></returns>
		protected IPromise RegisterUser(string advertId)
		{
			var result = new Promise();

			if (string.IsNullOrEmpty(Game.User.RegisterData.MobileUid))
			{
				Game.Network.QueryManager.RequestPromise(new RegisterOperation(Game.Social.Network, advertId)).Then(
					response =>
					{
						GameLogger.debug("Register User " + response.MobileUid + " : " + response.MobileAuthKey);
						Game.User.RegisterData.Register(response.MobileUid, response.MobileAuthKey, response.NeedShowGDPR);
						result.Resolve();
					},
					(exception =>
					{
						result.Reject(exception);
						GameLogger.debug("Register User Error");
					})
				);
			}
			else
			{
				Preloader.UID = Game.User.Uid;
				result.Resolve();
			}

			return result;
		}

		protected IPromise GetProfile()
		{
			var result = new Promise();
			
			Game.Network.QueryManager.RequestPromise(new GetProfileOperation(Game.Social.Network, Game.User.RegisterData.MobileUid, Game.User.RegisterData.MobileAuthKey)).Then(
				response =>
				{
					Game.User.RegisterData.ChangeUid(response.Uid, response.AuthKey);
					GameLogger.debug("GetProfile User " + response.Uid + " : " + response.AuthKey);
					Preloader.UID = Game.User.Uid;
					result.Resolve();
				},
				(exception =>
				{
					result.Reject(exception);
					GameLogger.debug("Register User Error");
				})
			);

			return result;
		}

		/// <summary>
		/// Получаем данные игрока
		/// </summary>
		/// <returns></returns>
		protected virtual IPromise GetUserInfo()
		{
			var result = new Promise();

			Game.QueryManager.RequestPromise(new UserInfoOperation(Game.Static.Versions, Game.Static.ModelVersionData))
				.Then(response =>
				{
					Game.User.Init();
					Game.ServerDataUpdater.Update(response);
					result.Resolve();
				});

			return result;
		}

		protected IPromise CheckSSK()
		{
			if (Game.IsSskError)
				return Promise.Rejected(new Exception("SSK Error"));

			return Promise.Resolved();
		}

		protected IPromise InitMobilePlatform()
		{
#if !UNITY_WEBGL
			//Game.AdvertisingController?.DestroyPartners();
			Game.Mobile.Init();

			Debug.Log("[DataLoaderMobile] Purchases");
			Game.Mobile.Purchases.StartInitialize();
#endif

			return Promise.Resolved();
		}
		
		protected IPromise InitAnalytics()
		{
#if !UNITY_WEBGL
			Game.Mobile.AnalyticsInit();

			Debug.Log("[DataLoaderMobile] AnalyticsInit");
#endif

			return Promise.Resolved();
		}

	}
}