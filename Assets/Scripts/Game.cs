using Assets.Scripts.BuildSettings;
using Assets.Scripts.Core;
using Assets.Scripts.Core.AssetsManager;
using Assets.Scripts.Core.Controllers;
using Assets.Scripts.Core.Sound;
using Assets.Scripts.Events;
using Assets.Scripts.GameServiceProvider;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Localization;
using Assets.Scripts.Network;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Platform.Adapter;
using Assets.Scripts.Platform.Mobile;
using Assets.Scripts.Static;
using Assets.Scripts.UI;
using Assets.Scripts.UI.General;
using Assets.Scripts.UI.HUD;
using Assets.Scripts.UI.Windows;
using Assets.Scripts.UI.WindowsSystem;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using IngameDebugConsole;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Assets.Scripts.Gameplay;
using Assets.Scripts.Network.Queries;
using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Platform.Mobile.Social;
using Cinemachine;
using GoogleMobileAds.Samples;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using IServiceProvider = Assets.Scripts.GameServiceProvider.IServiceProvider;
using Assets.UIDoors;

namespace Assets.Scripts
{
	public class Game : MonoBehaviour
	{
		public const int BASE_THRESHOLD = 10;
		public const int BASE_DPI = 96;

		public const string TAG = "[Game] ";
		public static Thread MainThread { get; private set; }

		public Promise GameLoadingPromise = new Promise();
		public IPromise LoadNativeLibrariesPromise = Promise.Resolved();

		public static bool ShowConsent = true;

		private static Game instance;
		public static Game Instance => instance;

		public static int? SessionKey { get; set; }
		private bool WasSskError { get; set; }

		private static Queue<Action> MainThreadActions = new Queue<Action>();
		public GoogleMobileAdsConsentController ConsentController { get; private set; }

		private Camera mainCam;
		public static Camera MainCamera => Instance.mainCam == null ? Instance.mainCam = Camera.main : Instance.mainCam;

		[SerializeField] private HUDController _hudController;
		public static HUDController HUD => Instance._hudController;

		[SerializeField] private WindowsController windowsController;
		public static WindowsController Windows => Instance.windowsController;

		[SerializeField] private DebugLogManager _debugConsole;
		public DebugLogManager DebugConsole => _debugConsole;

		[SerializeField] private SocialNetwork _socialNetwork;
		public static SocialNetwork Social => Instance._socialNetwork;

		[SerializeField] private BasePrefabs _basePrefabs;
		public static BasePrefabs BasePrefabs => Instance._basePrefabs;

		[SerializeField] private SoundManager _sound;
		public static SoundManager Sound => Instance ? Instance._sound : null;

		[SerializeField] private AudioListener _audioListener;
		public static AudioListener AudioListener => Instance ? Instance._audioListener : null;

		[SerializeField] private CinemachineBrain _cinemachineBrain;
		public static CinemachineBrain CinemachineBrain => Instance ? Instance._cinemachineBrain : null;

		[SerializeField] private CinemachineVirtualCamera _virtualCamera;
		public static CinemachineVirtualCamera CommonVirtualCamera => Instance ? Instance._virtualCamera : null;
		
		[SerializeField] private CinemachineVirtualCamera _moveVirtualCamera;
		public static CinemachineVirtualCamera MoveVirtualCamera => Instance ? Instance._moveVirtualCamera : null;
		
		[SerializeField] private CinemachineVirtualCamera _targetVirtualCamera;
		public static CinemachineVirtualCamera TargetVirtualCamera => Instance ? Instance._targetVirtualCamera : null;

		public void SetActiveCamera(CinemachineVirtualCamera camera)
		{
			_virtualCamera.SetActive(camera == _virtualCamera);
			_moveVirtualCamera.SetActive(camera == _moveVirtualCamera);
			_targetVirtualCamera.SetActive(camera == _targetVirtualCamera);
		}

#if !UNITY_WEBGL
		public PlatformMobile PlatformMobile { get; internal set; }
		public static PlatformMobile Mobile => Instance.PlatformMobile;
#endif

		public static GameChecks Checks => Instance.GameChecks;
		public GameChecks GameChecks { get; } = new GameChecks();

		public ReactiveProperty<UserData> UserData { get; internal set; }
		public static UserData User => Instance? Instance.UserData?.Value : null;

		public GameLocalization GameLocalization { get; internal set; }
		public static GameLocalization Localization => Instance ? Instance.GameLocalization : null;

		public static GameLogger GameLogger { get; private set; }

		public static GameReloader GameReloader { get; private set; }
		public static bool IsActive => GameReloader.IsActive;

		public GameNetwork GameNetwork { get; internal set; }
		public static GameNetwork Network => Instance.GameNetwork;

		public GameConsts GameConsts { get; internal set; }
		public static GameConsts Consts => Instance.GameConsts;

		public static Settings Settings => Static?.Settings;

		public static ServerDataUpdater ServerDataUpdater => Instance.serverDataUpdater;
		public ServerDataUpdater serverDataUpdater { get; } = new ServerDataUpdater();

		public static PreloaderScreen Preloader => Instance.preloader;
		private PreloaderScreen preloader;

		public static QueryManager QueryManager => Network?.QueryManager;

		public AdvertisingController advertisingController { get; private set; }
		public static AdvertisingController AdvertisingController => Instance.advertisingController;

		public static Locker Locker => HUD == null ? null : HUD.Locker;
		public static Loader Loader => HUD == null ? null : HUD.Loader;

		[SerializeField] public RateUsController rateUsController;
		public static RateUsController RateUsController => Instance ? Instance.rateUsController : null;

		private DataLoader _dataLoader;
		public static DataLoader DataLoader => Instance._dataLoader;

		[SerializeField]

		private Assets.UIDoors.GameController _gameController;
		public static GameController GameController => Instance._gameController;

		public static ServerLogs ServerLogs { get; private set; }

		public static event Action ApplicationQuit;

		private void SetupQualitySettings()
		{
#if UNITY_EDITOR
			Caching.ClearCache();
			Application.runInBackground = true;
#endif
			QualitySettings.SetQualityLevel(0);

#if UNITY_WEBGL
            Application.targetFrameRate = -1;
#else
			Application.targetFrameRate = 60;
#endif

#if UNITY_WSA || UNITY_WEBGL
			QualitySettings.vSyncCount = 1;
#else
			QualitySettings.vSyncCount = 0;
#endif
			QualitySettings.antiAliasing = 0;
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
			QualitySettings.shadows = ShadowQuality.Disable;
			// QualitySettings.shadowCascades = 0;
			// QualitySettings.shadowDistance = 0;

			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}

		private EventSystem _eventSystem;
		public static EventSystem EventSystem
		{
			get
			{
				if (!Instance._eventSystem)
					Instance._eventSystem = MainCanvas.GetComponentInChildren<EventSystem>();
				return Instance._eventSystem;
			}
		}

		private Canvas mainCanvas;

		public static Canvas MainCanvas
		{
			get
			{
				if (!Instance.mainCanvas)
					Instance.mainCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();

				return Instance.mainCanvas;
			}
		}

		private void Init()
		{
			Alive = true;
			
			DG.Tweening.DOTween.SetTweensCapacity(3000, 2000);
			Utils.Utils.SetMainContainer(this);
			EventController.Create();
			AssetsManager.Create();
			advertisingController = new AdvertisingController();
			MainThread = Thread.CurrentThread;
			GameLogger.StartLogging();
			ServerLogs = new ServerLogs();
			GameReloader = gameObject.AddComponent<GameReloader>();
			GameTime.Init();
			Vibration.Init();

			if (ConsentController == null)
				ConsentController = new GoogleMobileAdsConsentController();
			
			StartLoad();

			EventSystem.pixelDragThreshold = (int) (BASE_THRESHOLD * Screen.dpi / BASE_DPI);

			var debugStrings = new[]
			{
				string.Format("{0}: {1:yyyy-MM-dd hh:mm:ss}", "Time", DateTime.Now),
				string.Format("{0}: {1}", "Version", Application.version),
				string.Format("{0}: {1}", "SystemLanguage", Application.systemLanguage),
				string.Format("{0}: {1}", "Platform", Application.platform),
				string.Format("{0}: {1:F1}", "BatteryLevel", SystemInfo.batteryLevel * 100),
				string.Format("{0}: {1}", "OperatingSystem", SystemInfo.operatingSystem),
				string.Format("{0}: {1}", "OperatingSystemFamily", SystemInfo.operatingSystemFamily),
				string.Format("{0}: {1}", "DeviceModel", SystemInfo.deviceModel),
				string.Format("{0}: {1}", "DeviceName", SystemInfo.deviceName),
				string.Format("{0}: {1}", "ProcessorType", SystemInfo.processorType),
				string.Format("{0}: {1}", "ProcessorCount", SystemInfo.processorCount),
				string.Format("{0}: {1}", "MaxTextureSize", SystemInfo.maxTextureSize),
				string.Format("{0}: {1}", "SystemMemorySize", SystemInfo.systemMemorySize),
				string.Format("{0}: {1}", "GraphicsDeviceID", SystemInfo.graphicsDeviceID),
				string.Format("{0}: {1}", "GraphicsDeviceName", SystemInfo.graphicsDeviceName),
				string.Format("{0}: {1}", "GraphicsMemorySize", SystemInfo.graphicsMemorySize),
				string.Format("{0}: {1}", "InstallerName", Application.installerName),
			};

			GameLogger.debug("GameStarted\r\n" + string.Join("\r\n", debugStrings));
		}
		
		/// <summary>
		/// Начало загрзуки
		/// </summary>
		/// <param name="isReload">В случае выставления флага </param>
		/// <param name="needChangeSsk">В случае выставления флага сервер сбрасывает ssk и показывает окна в ServerWindowsController.</param>
		/// <param name="needShowPreloader">Нужно ли показывать прелоадер и инициализировать его ещё раз.</param>
		public void StartLoad(bool isReload = false, bool needChangeSsk = true, bool needShowPreloader = true)
		{
			var dataLoaderPromise = new Promise<DataLoader>();

			IsLoaded.Value = false;
			GameLoadingPromise?.RejectOnce();
			GameLoadingPromise = new Promise();
			
			if (DebugConsole)
				DebugConsole.gameObject.SetActive(/*BuildSettings.BuildSettings.IsEditor*/false);

			if (needShowPreloader)
			{
				if (!preloader)
				{
					preloader = FindObjectOfType<PreloaderScreen>();
					if (preloader == null)
						preloader = Instantiate(BasePrefabs.PreloaderScreen, Windows.transform);
				}
				else
					preloader.gameObject.SetActive(true);

				preloader.Initialize();
			}
#if UNITY_WEBGL
#if UNITY_EDITOR
				_dataLoader = createSocialLoader(LocalSocialSettings.GetFakeInitData());
				dataLoaderPromise.Resolve(_dataLoader);
#else
							  ExternalInterface.OnGameLoaded(obj =>
							  {
								  dataLoaderPromise.Resolve(createSocialLoader((JObject)obj));
								  return null;
							  });
#endif
#else
			_dataLoader = new DataLoaderMobile(isReload, needChangeSsk);
			dataLoaderPromise.Resolve(_dataLoader);
#endif
			dataLoaderPromise.Then(loader => loader.Load(preloader))
							 .Then(() => Utils.Utils.NextFrame(3))
							 .Then(OnLoadComplete);
		}

		/// <summary>
		/// Показать прелоадер
		/// </summary>
		public void ShowPreloader()
		{
			if (!preloader)
			{
				preloader = FindObjectOfType<PreloaderScreen>();
				if (preloader == null)
					preloader = Instantiate(BasePrefabs.PreloaderScreen, Windows.transform);
			}
			else
				preloader.gameObject.SetActive(true);

			preloader.Initialize();
		}

		private const string FADEOUT_LOCK = "Preloader.FadeOut.Lock";

		private void OnLoadComplete()
		{
			GameLogger.debug("OnLoadComplete");

			Locker.ClearAllLocks();

			ServerLogs.LoadGame();
#if !UNITY_WEBGL
			Mobile.Purchases?.OnGameLoad();
#endif
			Sound.OnActivate();
			User.OnLoad();
			IsLoaded.Value = true;
			resolution = new Vector2(Screen.width, Screen.height);

			HUD.InitHud();
			
			GameLoadingPromise.ResolveOnce();

			Promise.Resolved()
				   .Then(() => Utils.Utils.NextFrame(10))
				   .Then(MobileSocialConnector.CheckAdvertProfile)
				   .Then(preloader.CheckSocialSave)
				 //   .Then(() => _mapLoader.Load<Map>(Game.User.Maps.GetById(1), p =>
					// {
					// 	preloader.Progress = 80 + Mathf.FloorToInt(p * 20);
					// }))
					//.Then(() => new MapPrefabLoader().Load<Map>(Game.User.Maps.GetById(1)))
				   .Then(OnGameStart)
				   .Then(fadeOut)
				   .Then(() => Locker.Unlock(FADEOUT_LOCK))
				   .Finally(() => Locker.Unlock(FADEOUT_LOCK))
				   .Finally(OnFullLoad);

			IPromise fadeOut()
			{
				return Promise.Resolved()
							.Then(() => Debug.LogWarning("FadeOut"))
							  .Then(() => Locker.LockOnce(FADEOUT_LOCK))
							  .Then(preloader.FadeOut)
							  .Then(() => Locker.Unlock(FADEOUT_LOCK));
			}
		}

		public ReactiveProperty<PlayfieldView> Playfiled { get; } = new ReactiveProperty<PlayfieldView>();

		public void EndGame()
		{
			if (Playfiled.Value)
			{
				Destroy(Playfiled.Value.gameObject);
				foreach (Transform child in Game.HUD.HudTopLayer)
				{
					Destroy(child.gameObject);
				}
			}
			Playfiled.Value = null;
		}

		private void OnFullLoad()
		{
		}

		private IPromise OnGameStart()
		{
			ServerLogs.SendLog("game_enter");
			return Windows.CheckRestartWindowQueue();
		}

		private bool _isDebugConsole;
		public bool IsDebugConsole
		{
			set
			{
				if (_isDebugConsole != value)
				{
					GameLogger.debug("DebugConsole updated " + _isDebugConsole + " dc = " + DebugConsole);
					_isDebugConsole = value;
					if (DebugConsole == null) return;
					DebugConsole.gameObject.SetActive(_isDebugConsole);
				}
			}
		}

		public ReactiveProperty<bool> IsLoaded { get; internal set; } = new ReactiveProperty<bool>(false);

		public static bool IsSskError => Instance.WasSskError;

		public StaticData StaticData { get; internal set; }
		public static StaticData Static { get { return Instance.StaticData; } }
		public event Action OnScreenResize;

		private void Start()
		{
			instance = this;
			//Input.multiTouchEnabled = false; // выключаем множественное нажатие

			GameLogger = new GameLogger();
			DontDestroyOnLoad(this);
			SetupQualitySettings();
			Init();
			MainThread = Thread.CurrentThread;
			_sound.Init();
			Debug.Log("game");
		}

		public void OnStartReload()
		{
			Instance.IsLoaded.Value = false;
			GameLoadingPromise?.RejectOnce();
			GameLoadingPromise = new Promise();
		}

		private Vector2 resolution;
		private ScreenOrientation orientation;

		private void Update()
		{
			if (resolution.x != Screen.width || resolution.y != Screen.height || orientation != Screen.orientation)
			{
				resolution.x = Screen.width;
				resolution.y = Screen.height;
				orientation = Screen.orientation;
#if !UNITY_WEBGL
				Utils.Utils.NextFrame()
					.Then(ForceInvokeScreenResize);
#endif
			}
		}

		public void ForceInvokeScreenResize() => OnScreenResize?.Invoke();

		public static string Localize(string key, params string[] parameters)
		{
			return Localization?.Localize(key, parameters) ?? key;
		}

		/// <summary>Переход по "доступно обновление игры"</summary>
		public static void UpdateBuild()
		{
#if !UNITY_WEBGL
			Mobile.GoToStore();
#endif
		}

		public static void OnVersionError()
		{
			if (Instance.WasSskError)
				return;

			Instance.WasSskError = true;
			GameLogger.warning("!!!WRONG MOBILE VERSION!!!");

			//if (!BUILD_MOBILE && !BUILD_WINDOWS_STORE) 
			//	return;

			if (Instance.IsLoaded.Value)
			{
				ForcedUpdateWindow.Of();
			}

			if (Preloader)
				Preloader.OnVersionError();
		}

		public static void OnSskError()
		{
			if (IsSskError)
				return;

			GameLogger.warning("!!!WRONG SESSION KEY!!!");

			if (Instance && Instance.IsLoaded.Value && !Instance.WasSskError)
			{
				var win = InfoWindow.Of("attention".Localize(), "second_game_copy".Localize(), false, true)
					.AddButton(ButtonPrefab.GreenSquareButtonWithText, "ok".Localize())
					.FinishAddingButtons();

				if (win)
				{
#if UNITY_WEBGL
					win.CanCloseByBackButton = false;
					win.CanClose.Value = false;
#else
					win.ClosePromise.Then(onClick);
#endif
				}
				else
					onClick();

				Instance.WasSskError = true;
			}

			void onClick()
			{
				Quit();
			}
		}

		void FixedUpdate()
		{
			lock (MainThreadActions)
			{
				while (MainThreadActions.Any())
				{
					var act = MainThreadActions.Dequeue();
					act.Invoke();
				}
			}
		}

		public static void ExecuteOnMainThread(Action action)
		{
			if (Thread.CurrentThread == MainThread)
				action.Invoke();
			else
			{
				lock (MainThreadActions)
				{
					MainThreadActions.Enqueue(() => action());
				}
			}
		}

		public static bool Alive { get; private set; } = false;

		private void OnApplicationQuit()
		{
			ApplicationQuit?.Invoke();

			Alive = false;

			Dispose();

			GameLogger.debug("Application Quit");
		}

		public void Dispose()
		{
			UserData?.Value?.Dispose();
			UserData = null;

			GameLogger?.Dispose();
			GameNetwork?.Dispose();
		}
		
		public static bool IsTablet =>
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WSA
  						false;
#else
            DeviceTypeChecker.GetDeviceType() == DeviceTypeChecker.DeviceType.Tablet;
#endif

		public static bool IsNativeQuit = false;

		private const string NEED_ADDITIONAL_STARS_PREFS_KEY = "need_additional_stars";
		private static bool? _needAdditionalEndStars;
		public static bool NeedAdditionalEndStars
		{
			get
			{
				if (_needAdditionalEndStars.HasValue)
					return _needAdditionalEndStars.Value;

				if (PlayerPrefs.HasKey(NEED_ADDITIONAL_STARS_PREFS_KEY))
				{
					_needAdditionalEndStars = PlayerPrefs.GetInt(NEED_ADDITIONAL_STARS_PREFS_KEY) > 0;
					Debug.Log($"Get additional stars from {_needAdditionalEndStars.Value}");
					return _needAdditionalEndStars.Value;
				}

				return false;
			}
			set
			{
				_needAdditionalEndStars = value;
				Debug.Log(message: $"SetUI additional stars to {_needAdditionalEndStars.Value}");
				PlayerPrefs.SetInt(NEED_ADDITIONAL_STARS_PREFS_KEY, value ? 1 : 0);
			}
		}

		//public static void AskQuit()
		//{
		//    Game.Windows.OpenQuitConfirm();
		//}

		public static void Quit()
		{
			GameLogger.debug("Quit");

			// 			if (!IsNativeQuit)
			// 			{
			// #if UNITY_EDITOR
			// 				UnityEditor.EditorApplication.ExitPlaymode();
			// #else
			// 				Application.Quit();
			// #endif
			// 				return;
			// 			}

#if UNITY_EDITOR
			UnityEditor.EditorApplication.ExitPlaymode();
#elif UNITY_ANDROID
            AndroidJavaClass ajc = new AndroidJavaClass("com.lancekun.quit_helper.AN_QuitHelper");
            AndroidJavaObject UnityInstance = ajc.CallStatic<AndroidJavaObject>("Instance");
            UnityInstance.Call("AN_Exit");
#else
			Application.Quit();
#endif
		}

		public IPromise CheckConsent()
		{
			return Promise.Resolved()
				.Then(ConsentController.GatherConsent);
		}
	}
}