using System;
using Assets.Scripts.Core.AssetsManager;
using Assets.Scripts.Events;
using Assets.Scripts.GameServiceProvider;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Queries.Operations.Api;
using Assets.Scripts.Utils;
using UniRx;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameReloader : MonoBehaviour
    {
	    public event Action ApplicationFocus;
	    public event Action ApplicationUnFocus;

	    public static event Action Reloaded;

		public const string TAG = "[Reloader]";

		private long _pauseTime;
		private Promise _autoExitPromise;
		private bool _active = true;
		private bool _alreadyAddedCallback = false;
		private IDisposable _sub = null;

		/// <summary>
		/// Перезагрузка игры
		/// </summary>
		/// <param name="needChangeSsk">В случае выставления флага сервер сбрасывает ssk и показывает окна в ServerWindowsController.</param>
		/// <param name="forceReload">Необходимая перезагрузка, отключает очередь и не ждёт ответов от сервера</param>
        public static IPromise Reload(bool needChangeSsk = false, bool forceReload = false)
		{
			if (!Game.Instance.IsLoaded.Value)
				return Promise.Resolved();

			Debug.Log(TAG + " Start Reload Game");

#if !UNITY_WEBGL
			Game.Mobile.Notifications.ClearNotifications();
#endif
			Game.Instance.OnStartReload();

			var queryPromise = forceReload ?
							Promise.Resolved()
								   .Then(Game.QueryManager.Invalidate) :
							Promise.Resolved()
								   .Then(Game.QueryManager.LockNewRequestsQueue)
								   .Then(Game.QueryManager.WaitForLastAnswers)
								   .Then(Game.QueryManager.Invalidate);


			return
							Promise.Resolved()
								   .Then(() => queryPromise)
								   .Then(() => Game.User?.Sockets?.Dispose())
								   .Then(() => Game.Windows.CloseAllScreensPromise(TAG, true))
								   .Then(Game.Instance.ShowPreloader)
								   .Then(AssetsManager.Instance.Loader.UnloadAll)
								   .Then(Game.HUD.Free)
								   .Then(() => Utils.Utils.NextFrame(5))
								   .Then(() =>
									{
										EventController.TriggerEvent(new GameEvents.DeleteDataOnGameReload());

										Game.Instance.UserData?.Value?.Dispose();
										Game.Instance.UserData = null;
									})
								   .Then(Game.Social.Free)
								   .Then(Game.QueryManager.ResetManager)
								   .Then(() => StaticDataUpdater.CheckedGroupsInCurrentSession.Clear())
								   .Then(() => Game.AdvertisingController?.DestroyPartners())
								   .Then(() => Game.Instance.StartLoad(true, needChangeSsk, needShowPreloader: false))
								   .Then(() =>
									{
										Game.Locker.enabled = false;
										Game.Locker.ClearAllLocks();
									});

		}

		/// <summary>
		/// Активно ли приложение в конкретный момент времени или находится в свернутом состоянии.
		/// </summary>
		public bool IsActive
		{
			get => _active;

			private set
			{
				if (_active == value)
					return;

				Debug.Log($"{TAG} ApplicationFocus = {value}");
				_active = value;

				if (_active)
#if UNITY_EDITOR
					OnActivate();
#else
					Utils.Utils.NextFrame(3)
						.Then(() => OnActivate());
#endif
				else
					OnDeactivate();
			}
		}

		public IPromise WaitActivate()
		{
			if (_active)
				return Promise.Resolved();

			var promise = new Promise();
			ApplicationFocus += onFocus;
			return promise;

			void onFocus()
			{
				ApplicationFocus -= onFocus;
				promise.Resolve();
			}
		}


#if UNITY_EDITOR || UNITY_WEBGL
		private void OnApplicationFocus(bool focus) => IsActive = focus;	// клавиатура забирает фокус, проблема с паузой сокетов
#endif
		private void OnApplicationPause(bool pause) => IsActive = !pause;

		private void OnDeactivate()
		{
			_pauseTime = GameTime.Now;
			Game.GameLogger.Save();

			if (Game.Settings != null)
			{
				var inGame = false;//Game.Instance.IsLoaded.Value && Game.IsInGame;
				var restartDelta = inGame ? Game.Settings.INACTIVE_RELOAD_GAME : Game.Settings.INACTIVE_RELOAD_MENU;

				var dateTime = DateTimeOffset.FromUnixTimeSeconds(GameTime.Now + restartDelta).LocalDateTime;

				Debug.Log($"{TAG} AutoReload with {restartDelta} seconds in {dateTime}");
			}

// #if !UNITY_EDITOR
// 			StartAutoExit();
// #endif

			ApplicationUnFocus?.Invoke();
		}

		private void OnActivate()
		{
			// KillAutoExit();

			if (Game.IsSskError)
				return;

			if (Game.Settings == null)
				return;

			GameTime.MarkNotActual();

			var timeNow = GameTime.Now;
			var deltaTime = timeNow - _pauseTime;
			var inGame = false;//Game.Instance.IsLoaded.Value && Game.IsInGame;
			var restartDelta = inGame ? Game.Settings.INACTIVE_RELOAD_GAME : Game.Settings.INACTIVE_RELOAD_MENU;

			Debug.Log($"{TAG} Current Time: {timeNow}, Pause Time: {_pauseTime}, Delta Time: {deltaTime}, InGame: {inGame}, RestartDelta: {restartDelta}");

			if (!tryAutoReload())
			{
				ApplicationFocus?.Invoke();
			}

			bool tryAutoReload()
			{
				if (deltaTime > restartDelta && restartDelta > 0)
				{
#if UNITY_EDITOR || UNITY_WEBGL || UNITY_WSA
					Debug.Log($"{TAG} Platform without reloading");
					return false;
#endif

					Debug.Log($"{TAG} RunAutoReload app was inactive for {deltaTime} sec");
					Reload();
					ApplicationFocus?.Invoke();

					return true;
				}

				return false;
			}
		}

		[Obsolete("Корутины не работают в спящем режиме")]
		private void StartAutoExit()
		{
			KillAutoExit();

			if (Game.Settings == null)
				return;

			var inGame = false;

			var restartDelta = inGame ? Game.Settings.INACTIVE_RELOAD_GAME : Game.Settings.INACTIVE_RELOAD_MENU;

			var dateTime = DateTimeOffset.FromUnixTimeSeconds(GameTime.Now + restartDelta).LocalDateTime;

			Debug.Log($"{TAG} StartAutoExit with {restartDelta} seconds in {dateTime}");

			_autoExitPromise = Utils.Utils.Wait(restartDelta);
			_autoExitPromise.Then(Game.Quit);
		}

		private void KillAutoExit()
		{
			if (_autoExitPromise != null)
			{
				Debug.Log($"{TAG} KillAutoExit");
				_autoExitPromise.RejectOnce();
			}
			_autoExitPromise = null;
		}

		// private void TrySendOnline()
		// {
		// 	if (Game.Settings == null)
		// 		return;
		// 	if (Game.User == null)
		// 		return;
		//
		// 	var timeNow = GameTime.Now;
		// 	var deltaTime = timeNow - _pauseTime;
		//
		// 	if (deltaTime > UserData.ONLINE_TIME_INTERVAL)
		// 	{
		// 		Game.User?.SendOnline(false);
		// 		Game.User?.Sockets?.CheckConnection();
		// 	}
		// }
	}
}