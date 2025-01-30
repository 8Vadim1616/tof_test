using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core.AssetsManager;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.UI.Windows;
using Assets.Scripts.Utils;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.WindowsSystem
{
	public class WindowsController : MonoBehaviour
	{
		private const string TAG = "[WindowsController]";

		public readonly ReactiveProperty<bool> AllHidden = new ReactiveProperty<bool>();

		public Transform HOLDER;
		/// <summary>префабы окон для редактора</summary>
		public List<AbstractWindow> Windows = new List<AbstractWindow>();
		public ReactiveProperty<AbstractWindow> CurrentScreen { get; } = new ReactiveProperty<AbstractWindow>(null);
		public bool CanCloseAnything => CurrentScreen.Value;
		public int OpenedWindowsCount => OpenWindowsStack.Count;
		public int QueueCount => WindowsQueue.Count;

		protected Stack<AbstractWindow> OpenWindowsStack { get; set; } = new Stack<AbstractWindow>(10);
		protected Queue<(AbstractWindow, Action<AbstractWindow>)> WindowsQueue { get; } = new Queue<(AbstractWindow, Action<AbstractWindow>)>(10);
		/// <summary>Список окон которые вызовутся при следующей перезагрузке (Нужно после отключения/подключения к соц сети)</summary>
		protected Queue<Func<AbstractWindow>> RestartWindowsQueue { get; } = new Queue<Func<AbstractWindow>>(10);

		private List<string> _newWindowsCreationLocks = new List<string>(4);
		private Canvas windowsCanvas;
		private CanvasGroup windowsCanvasGroup;
		private Tween windowsVisibleTween;
		private Promise lastPromise;

		private const int HOLDER_START_SORT_ORDER = 2;
		private const int HOLDER_TOP_SORT_ORDER = 15;

		#region Events
		public delegate void WindowEvent(AbstractWindow window = null);

		public event WindowEvent WindowOpenEvent;
		public event WindowEvent WindowClosedEvent;
		public event WindowEvent WindowClosedAllEvent;
		public event WindowEvent WindowSomeAction;
		#endregion

		#region UNITY_ACTIONS
		protected void Awake()
		{
			DontDestroyOnLoad(gameObject);
			Debug.Log("[WindowsController] Awake");
			CurrentScreen.Subscribe(x =>
			{
				//if (x) GameLogger.debug(x.name);
				Debug.Log("[WindowsController] windowChange");
				if (CurrentScreen.Value) Debug.Log("[WindowsController] " + CurrentScreen.Value.name);
			}).AddTo(this);

			windowsCanvas = HOLDER.GetComponent<Canvas>();
			windowsCanvasGroup = HOLDER.GetComponent<CanvasGroup>();
		}
		#endregion

		private void Update()
		{
			if (CurrentScreen.Value?.CanClose.Value == false || CurrentScreen.Value?.CanCloseByBackButton == false)
				return;

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE || UNITY_WSA || UNITY_STANDALONE
			if (!Game.Locker.IsLocked.Value && Input.GetKeyDown(KeyCode.Escape))
			{
				if (OpenedWindowsCount > 0)
				{
					if (!CurrentScreen.Value.IsOpening && CurrentScreen.Value.CanClose.Value)
						CurrentScreen.Value.Back();
				}
				else if (!Game.Loader.IsShowing)
				{
					//ExitWindow.Of();
				}
			}
#endif
		}

		public void DispatchSomeWindowAction(AbstractWindow win)
		{
			WindowSomeAction?.Invoke(win);
		}

		public IPromise WhenAllClosed()
		{
			if (CurrentScreen.Value == null && WindowsQueue.Empty()) return Promise.Resolved();

			var result = new Promise().WithName("WhenAllClosed") as Promise;

			WindowClosedAllEvent += onCloseAll;
			void onCloseAll(AbstractWindow win = null)
			{
				WindowClosedAllEvent -= onCloseAll;
				if (result.IsPending)
					result.Resolve();
			}

			return result;
		}

		public void ClearQueue()
		{
			if (WindowsQueue.Count <= 0) 
				return;
			WindowsQueue.Clear();
		}

		protected bool DisplayWindowInQueueIfAny()
		{
			if (WindowsQueue.Count <= 0) return false;
			if (CurrentScreen.Value) return false;

			var (window, action) = WindowsQueue.Dequeue();
			while (window == null && WindowsQueue.Count > 0)
			{
				(window, action) = WindowsQueue.Dequeue();
			}
			if (window == null) return false;

			OpenScreen(action);
			return true;
		}

		public void AddNewWindowsCreationAvailableLock(string lockKey)
		{
			_newWindowsCreationLocks.Add(lockKey);
			Debug.Log($"WindowsCreationLocks added key {lockKey}, count = {_newWindowsCreationLocks.Count}");
		}

		public void AddNewWindowsCreationAvailableLockOnce(string lockKey)
		{
			if (!_newWindowsCreationLocks.Contains(lockKey))
			{
				_newWindowsCreationLocks.Add(lockKey);
				Debug.Log($"WindowsCreationLocks added key {lockKey}, count = {_newWindowsCreationLocks.Count}");
			}
		}

		public void RemoveNewWindowsCreationAvailableLock(string lockKey)
		{
			_newWindowsCreationLocks.Remove(lockKey);
			Debug.Log($"WindowsCreationLocks removed key {lockKey}, count = {_newWindowsCreationLocks.Count}");
		}

		public void RemoveAllNewWindowsCreationAvailableLocks(string lockKey)
		{
			for (int i = _newWindowsCreationLocks.Count - 1; i >= 0; i--)
				if (_newWindowsCreationLocks[i] == lockKey)
					_newWindowsCreationLocks.RemoveAt(i);

			Debug.Log($"WindowsCreationLocks removed all keys {lockKey}, count = {_newWindowsCreationLocks.Count}");
		}

		public void ClearAllNewWindowsCreationAvailableLockers()
		{
			_newWindowsCreationLocks.Clear();

			Debug.Log("WindowsCreationLocks cleared");
		}

		public bool NewWindowsCreationAvailable => _newWindowsCreationLocks.Count == 0;

		/// <summary>Создает окно, выполняет действие и вызывает ивенты</summary>
		/// <typeparam name="T"> Тип окна </typeparam>
		/// <param name="action"> Выполняемое дествие </param>
		/// <returns> Экземпляр окна </returns>
		protected T OpenScreen<T>(Action<T> action, Dictionary<string, object> addLogParams = null) where T : AbstractWindow
		{
			var window = GetWindow<T>();

			if (!window)
			{
				Debug.LogError($"Screen '{typeof(T).Name}' not found");
				return null;
			}

			var wnd = Instantiate(window, HOLDER);
			wnd.name = window.name;

			OpenWindowsStack.Push(wnd);

			wnd.Controller = this;
			CurrentScreen.Value = wnd;
			if (addLogParams != null) wnd.SetAdditionalLogParams(addLogParams);
			action?.Invoke(wnd);
			WindowOpenEvent?.Invoke(wnd);
			return wnd;
		}

		protected Promise<T> OpenScreenAsync<T>(string prefabPath, Action<T> action, bool showLoader, Dictionary<string, object> addLogParams) where T : AbstractWindow
		{
			var promise = new Promise<T>();

			GetWindowAsync<T>(prefabPath)
						   .Then(prefab =>
						   {
							   if (prefab)
							   {
								   var wnd = Instantiate(prefab, HOLDER);

								   OpenWindowsStack.Push(wnd);

								   wnd.Controller = this;
								   CurrentScreen.Value = wnd;
								   if (addLogParams != null) wnd.SetAdditionalLogParams(addLogParams);
								   action?.Invoke(wnd);
								   WindowOpenEvent?.Invoke(wnd);

								   promise.Resolve(wnd);
							   }
							   else
								   Debug.LogError($"Prefab '{prefabPath}' not found");
						   });

			return promise;
		}

		private T GetWindow<T>() where T : AbstractWindow
		{
			Type type = typeof(T);
#if UNITY_EDITOR

			if (WindowsHolder.Windows.ContainsKey(type))
			{
				T window = Resources.Load<T>(WindowsHolder.Windows[type]);
				if (window != null)
					return window;
			}
			return WindowAssetsGenerator.GenerateAndGetPrefab<T>();

#else
            if (WindowsHolder.Windows.ContainsKey(type))
                return Resources.Load<T>(Assets.Scripts.UI.WindowsSystem.WindowsHolder.Windows[type]);
            
            throw new Exception($"Не найдено окно {type}!");
#endif
		}

		protected Promise<T> GetWindowAsync<T>(string prefabPath = null) where T : AbstractWindow
		{
			var promise = new Promise<T>();

			var win = GetWindow<T>();

			if (win) // Если есть префаб в контроллере
			{
				promise.Resolve(win); // то возвращаем его
				return promise;
			}

			if (prefabPath == null) // Если не указан явно путь
				prefabPath = AssetsManager.Instance.Loader.TryGetPathByType(typeof(T)); // То пробуем получить его из ключей addressable

			if (prefabPath == null)
			{
				promise.Resolve(null);
				return promise;
			}

			AssetsManager.Instance.Loader.Load<GameObject>(prefabPath)
				.Then(go =>
				{
					if (go)
					{
						promise.Resolve(go.GetComponent<T>());
					}
					else
						Debug.LogError($"GameObject '{prefabPath}' not found");
				});

			return promise;
		}

		public T GetOpenWindow<T>() where T : AbstractWindow
		{
			return OpenWindowsStack.OfType<T>().FirstOrDefault();
		}

		/// <summary>
		/// Показывает определенный экран и выполняет над ним действие после иницализации. Если открыт другой экран
		/// то закрывает текущий и открывает новый. При возврате открывает предыдущий. 
		/// </summary>
		/// <typeparam name="T"> Класс экрана для отображения </typeparam>
		/// <param name="closeAllOther"> Если истина то делает экран первым в истории экранов, предыдущие забываются. </param>
		/// <param name="action"> Действие выполняемое после инициализации </param>
		/// <param name="minimizeTopScreen"> Если остальные окна не закрываются - нужно ли прятать предыдущее  </param>
		public T ScreenChange<T>(bool closeAllOther = false, Action<T> action = null, bool minimizeTopScreen = true, Dictionary<string, object> addLogParams = null) where T : AbstractWindow
		{
			// хз зачем инфо
			bool isInfo = false;// typeof(T) == typeof(InfoScreen);  // дадим этому окошку неограниченные права
			bool isChooseSaveWindow = false;//typeof(T) == typeof(ChooseSaveWindow);

			if (!NewWindowsCreationAvailable && !isInfo && !isChooseSaveWindow)
			{
				Debug.Log($"New screen creation locked by keys: {string.Join(", ", _newWindowsCreationLocks)}");
				return default;
			}

			if (closeAllOther)
			{
				if (!isInfo && CurrentScreen.Value != null && !CurrentScreen.Value.CanClose.Value)
					return null;
				CloseAllScreens($"by {typeof(T)}");
			}
			else if (minimizeTopScreen)
				MinimizeTopScreen();

			return OpenScreen(action, addLogParams);
		}

		public void CloseCommand()
		{
			CloseTopScreen();
		}

		public void RemoveFromController(AbstractWindow wnd)
		{
			if (!wnd)
				return;

			WindowClosedEvent?.Invoke(wnd);

			ClearEmptyScreensIfAny();

			if (!OpenWindowsStack.Contains(wnd))
				return;

			Scripts.Utils.Utils.NextFrame() // Ждём следующий кадр
				   .Then(() =>
					{
						var topScreen = false;

						if (OpenWindowsStack.Count > 0)
							topScreen = OpenWindowsStack.Peek() == wnd;

						// убираем окно из иерархии
						OpenWindowsStack = new Stack<AbstractWindow>(OpenWindowsStack
							.ToArray()
							.Where(x => x != wnd)
							.Reverse());

						if (OpenWindowsStack.Count == 0)
						{
							CurrentScreen.Value = null;
							WindowClosedAllEvent?.Invoke(wnd);
						}

						if (topScreen)
							OnTopScreenClose();
					});
		}

		private void CloseTopScreen()
		{
			ClearEmptyScreensIfAny();

			if (OpenWindowsStack.Count <= 0) return;

			var win = OpenWindowsStack.Pop();
			if (win.CanClose.Value)
				win.Close();

			OnTopScreenClose();
		}

		protected void OnTopScreenClose()
		{
			ClearEmptyScreensIfAny();
			if (OpenWindowsStack.Count > 0)
			{
				OpenWindowsStack.Peek().Maximize();
				CurrentScreen.Value = OpenWindowsStack.Peek();
			}
			else
			{
				CurrentScreen.Value = null;
				DisplayWindowInQueueIfAny();
			}
		}


		public void CloseScreensOfType<T>()
		{
			ClearEmptyScreensIfAny();

			var wnds = OpenWindowsStack.Where(x => x is T);
			foreach (var w in wnds)
			{
				w.Close();
				if (!w.IsClosing)
					// Чтобы пыталось закрыть окно когда его можно будет закрыть
					w.CanClose.Subscribe(_ => w.Close()).AddTo(w);
			}
		}

		[Obsolete("Не учитывает AbstractWindow.CanClose. Можно использовать CloseAllScreensPromise")]
		public void CloseAllScreens()
		{
			CloseAllScreens(null);
		}

		[Obsolete("Не учитывает AbstractWindow.CanClose. Можно использовать CloseAllScreensPromise")]
		public void CloseAllScreens(string tag)
		{
			Debug.Log("CloseAllScreens " + (tag ?? ""));
			if (!NewWindowsCreationAvailable)
			{
				Debug.Log($"Screen closing locked by keys: {string.Join(", ", _newWindowsCreationLocks)}");
				return;
			}

			while (OpenWindowsStack.Count > 0)
			{
				var wnd = OpenWindowsStack.Pop();
				if (wnd)
				{
					wnd.Close();
				}
			}
			WindowClosedAllEvent?.Invoke();
			CurrentScreen.Value = null;
		}

		/// <summary>
		/// Закрывает окна, дожидаясь закрытия окон с CanClose == false
		/// </summary>
		/// <param name="tag">Тэг для дебага</param>
		/// <param name="force">Выставить окнам CanClose = true перед закрытием</param>
		/// <returns>Возвращает промис, который ресолвится при закрытии всех окон</returns>
		public IPromise CloseAllScreensPromise(string tag = null, bool force = false)
		{
			if (!NewWindowsCreationAvailable)
			{
				Debug.Log($"Screen closing locked by keys: {string.Join(", ", _newWindowsCreationLocks)}");
				return Promise.Resolved();
			}

			Debug.Log("CloseAllScreensPromise " + (tag ?? ""));

			var result = new List<Promise>();

			while (OpenWindowsStack.Count > 0)
			{
				var wnd = OpenWindowsStack.Pop();
				if (wnd)
				{
					if (force)
						wnd.CanClose.Value = true;
					result.Add(wnd.ClosePromise);
					wnd.CloseWhenCan();
				}
			}

			return Promise.All(result).Then(() =>
			{
				WindowClosedAllEvent?.Invoke();
				CurrentScreen.Value = null;
			});
		}

		protected void ClearEmptyScreensIfAny()
		{
			if (OpenWindowsStack.Count <= 0) return;
			AbstractWindow wnd = OpenWindowsStack.Peek();

			while (!wnd)
			{
				OpenWindowsStack.Pop();
				if (OpenWindowsStack.Count <= 0) return;
				wnd = OpenWindowsStack.Peek();
			}
		}

		protected void MinimizeTopScreen()
		{
			ClearEmptyScreensIfAny();

			if (OpenWindowsStack.Count > 0)
			{
				OpenWindowsStack.Peek().Minimize();
			}
		}

		public IPromise SetWindowsVisible(bool visible)
		{
			if (AllHidden.Value == !visible)
			{
				AllHidden.Value = !visible;
				return Promise.Resolved();
			}

			windowsVisibleTween?.Kill();
			windowsVisibleTween = windowsCanvasGroup
				.DOFade(visible ? 1f : 0f, .25f)
				.OnComplete(() =>
				{
					AllHidden.Value = !visible;
					lastPromise?.ResolveOnce();
				})
				.SetLink(windowsCanvasGroup.gameObject);

			if (lastPromise == null || !lastPromise.IsPending)
				lastPromise = new Promise();

			return lastPromise;
		}

		public void TriggerWindowOpenEvent(AbstractWindow win)
		{
			WindowOpenEvent?.Invoke(win);
		}

		/// <summary>Добавляет окно к показу при перезагрузке игры</summary>
		public void OpenWindowOnRestart(Func<AbstractWindow> action)
		{
			RestartWindowsQueue.Enqueue(action);

			GameLogger.debug($"{TAG} Add window on restart. Count = {RestartWindowsQueue.Count}");
		}

		public IPromise CheckRestartWindowQueue()
		{
			if (BuildSettings.BuildSettings.IsEditor)
				return Promise.Resolved();

			var promise = new Promise();

			check();

			return promise;

			void check()
			{
				if (RestartWindowsQueue.Count == 0)
				{
					promise.Resolve();
					return;
				}

				var createWindowFunc = RestartWindowsQueue.Dequeue();

				if (createWindowFunc != null)
				{
					var window = createWindowFunc();

					if (window)
						window.ClosePromise
								.Then(check);
					else
						check();
				}
				else
					promise.Resolve();
			}
		}

		public void HolderSetBaseOrder()
		{
			windowsCanvas.sortingOrder = HOLDER_START_SORT_ORDER;
		}

		public void HolderSetTopOrder()
		{
			windowsCanvas.sortingOrder = HOLDER_TOP_SORT_ORDER;
		}
	}
}
