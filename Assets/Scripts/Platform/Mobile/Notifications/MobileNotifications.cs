#if UNITY_WSA
using Assets.Scripts.Platform.Adapter.Implements;
#elif UNITY_WEBGL
#else
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Network.Logs;
#endif
#if !UNITY_WSA && !BUILD_AMAZON && !UNITY_WEBGL && !BUILD_HUAWEI
using Firebase.Extensions;
using Firebase;
using Firebase.Messaging;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Platform.Mobile.Notifications.NotificationDatas;
using Assets.Scripts.Platform.Mobile.Notifications.NotificationsManager;
using Assets.Scripts.Utils;
using UniRx;

namespace Assets.Scripts.Platform.Mobile.Notifications
{
	public class MobileNotifications
	{
		public const string TAG = "[MobileNotifications] ";

		public const bool NOTIFICATIONS_TEST_MODE = false; // Тестирование нотификаций.
		private const int TIME_BETWEEN_NOTIFICATIONS = NOTIFICATIONS_TEST_MODE ? 10 : 30;

		private static readonly List<MobileNotification> allNotifications = new List<MobileNotification>()
		{
			new MobileNotification(1, (id) => new MobileNotificationReturn(id, 1)), // 1 час
			new MobileNotification(2, (id) => new MobileNotificationReturn(id, 2)), // 2 дня
			new MobileNotification(3, (id) => new MobileNotificationReturn(id, 3)), // 3 дня
		};

		private MobileLocalNotificationsManager _localNotificationsManager;

		public void Init()
		{
			Game.GameReloader.ApplicationFocus += OnApplicationFocus;
			Game.GameReloader.ApplicationUnFocus += OnApplicationUnFocus;
			Game.ApplicationQuit += OnQuit;

#if UNITY_ANDROID
			_localNotificationsManager = new MobileLocalNotificationsManagerAndroid();
#elif UNITY_IOS
			_localNotificationsManager = new MobileLocalNotificationsManagerIOS();
#else
			_localNotificationsManager = new MobileLocalNotificationsManagerWNDS();
#endif
			_localNotificationsManager.AskPermissions();

#if !UNITY_WSA && !UNITY_WEBGL && !BUILD_AMAZON && !BUILD_HUAWEI
			GameFirebase.Instance.Init().Then(OnInitializedFirebase);
#endif

			//Game.Instance.GameLoadingPromise.Then(OnApplicationFocus);

			IDisposable loadSubscribe = null;
			loadSubscribe = Game.Instance.IsLoaded
				.Subscribe(x =>
				{
					if (!x)
						return;

					loadSubscribe?.Dispose();
					loadSubscribe = null;

					OnApplicationFocus();
					_localNotificationsManager?.CheckOpenedByNotification();
				})
				.AddTo(Game.Instance);
		}

		public void ResendFirebaseToken()
		{
#if !UNITY_WSA && !UNITY_WEBGL && !BUILD_AMAZON && !BUILD_HUAWEI
			GameFirebase.Instance.Init().Then(OnInitializedFirebaseOnlyToken);
#endif
		}

		public void Free()
		{
			_localNotificationsManager?.Free(); _localNotificationsManager = null;
			Game.GameReloader.ApplicationFocus -= OnApplicationFocus;
			Game.GameReloader.ApplicationUnFocus -= OnApplicationUnFocus;
			Game.ApplicationQuit -= OnQuit;
		}

		private void OnApplicationFocus()
		{
			if (Game.Instance?.IsLoaded.Value ?? false)
				ClearNotifications();
		}

		private void OnApplicationUnFocus()
		{
			if (Game.Instance?.IsLoaded.Value ?? false)
				RegisterNotifications();
		}

		private void OnQuit()
		{
			RegisterNotifications();
		}

		/**
		 * Удаление всех нотификаций
		 */
		public void ClearNotifications()
		{
			_localNotificationsManager?.CancelNotifications(allNotifications);

			Log("Notifications Cleared");
		}

		/**
		 * Регистрация всех нотификаций
		 */
		public void RegisterNotifications()
		{
			if (_localNotificationsManager is null)
				return;

			ClearNotifications();

			if (Game.User?.Settings?.IsNotificationsOn == false)
				return;

			// Пробегаем по всем нотификациям
			var allSchedules = new List<MobileNotificationSchedule>();
			foreach (var notification in allNotifications)
			{
				// Формируем расписания
				var schedules = notification.GetSchedules();

				// Регистрируем расписания
				foreach (var schedule in schedules)
				{
					allSchedules.Add(schedule);
				}
			}

			allSchedules = CreateConcreteSchedules(allSchedules);

			for (var i = 0; i < allSchedules.Count; i++)
			{
				var schedule = allSchedules[i];

				_localNotificationsManager.AddNotificationToDeviceSchedule(schedule);

				// if (Game.User?.IsTester == true)
				Log($"Notification registered: id {schedule.NotificationId} {schedule.Type} {DateTimeOffset.FromUnixTimeSeconds(schedule.Timestamp).LocalDateTime.ToString()}");
			}
		}

		private List<MobileNotificationSchedule> CreateConcreteSchedules(List<MobileNotificationSchedule> v)
		{
			var result = v.ToList();
			result.Sort((s1, s2) => Convert.ToInt32(s1.Timestamp - s2.Timestamp));

			foreach (var schedule in result)
			{
				schedule.AddMinTime();
				schedule.ClampNightTime();
			}

			return result;
		}
#if !UNITY_WSA && !UNITY_WEBGL && !BUILD_AMAZON && !BUILD_HUAWEI
		protected bool LogTaskCompletion(Task task, string operation)
		{
			bool complete = false;
			if (task.IsCanceled)
			{
				Debug.Log(TAG + operation + " canceled.");
			}
			else if (task.IsFaulted)
			{
				Debug.Log(TAG + operation + " encounted an error.");
				foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
				{
					string errorCode = "";
					FirebaseException firebaseEx = exception as FirebaseException;
					if (firebaseEx != null)
					{
						errorCode = String.Format("Error.{0}: ",
                            ((Error)firebaseEx.ErrorCode).ToString());
					}
					Debug.Log(TAG + errorCode + exception.ToString());
				}
			}
			else if (task.IsCompleted)
			{
				Debug.Log(TAG + operation + " completed");
				complete = true;
			}
			return complete;
		}

		private void OnInitializedFirebase()
		{
			MessagingOptions firebaseOptions = new MessagingOptions();
			firebaseOptions.SuppressNotificationPermissionPrompt = false;

			//FirebaseMessaging.MessageReceived += OnMessageReceived;
			FirebaseMessaging.TokenReceived += OnTokenReceived;

			Debug.Log(TAG + "Firebase Messaging Initialized");

			FirebaseMessaging.RequestPermissionAsync()
				.ContinueWithOnMainThread(task =>
				{
					if (task.IsCompleted)
						LogTaskCompletion(task, "RequestPermissionAsync");
				});

			FirebaseMessaging.GetTokenAsync()
				.ContinueWithOnMainThread(task =>
				{
					if (task.IsCompleted)
						SetFirebaseToken(task.Result);
				});
		}

		private void OnInitializedFirebaseOnlyToken()
		{
			Debug.Log(TAG + "Firebase Messaging Initialized Only Token");

			FirebaseMessaging.GetTokenAsync()
				.ContinueWithOnMainThread(task =>
				{
					if (task.IsCompleted)
						SetFirebaseToken(task.Result);
				});
		}

		public virtual void OnMessageReceived(object sender, MessageReceivedEventArgs e)
		{
			Debug.Log(TAG + "Received a new message");

			var data = new Dictionary<string, object>();

			var notification = e.Message.Notification;
			if (notification != null)
			{
				data["title"] = notification.Title;
				data["body"] = notification.Body;
				Debug.Log("title: " + notification.Title);
				Debug.Log("body: " + notification.Body);
				var android = notification.Android;
				if (android != null)
				{
					data["channel_id"] = android.ChannelId;
					Debug.Log(TAG + "android channel_id: " + android.ChannelId);
				}
			}

			data["opened"] = e.Message.NotificationOpened;
			Debug.Log(TAG + "opened: " + e.Message.NotificationOpened);

			if (e.Message.From.Length > 0)
			{
				data["from"] = e.Message.From;
				Debug.Log(TAG + "from: " + e.Message.From);
			}
			if (e.Message.Link != null)
			{
				data["link"] = e.Message.Link.ToString();
				Debug.Log(TAG + "link: " + e.Message.Link);
			}
			if (e.Message.Data.Count > 0)
			{
				Debug.Log(TAG + "data:");
				foreach (KeyValuePair<string, string> iter in e.Message.Data)
				{
					Debug.Log("  " + iter.Key + ": " + iter.Value);
					data[iter.Key] = iter.Value;
				}
			}

			if (data.Count > 0)
				ServerLogs.SendLog("notif", data);
		}

		public virtual void OnTokenReceived(object sender, TokenReceivedEventArgs token)
		{
			SetFirebaseToken(token.Token);
		}

		private void SetFirebaseToken(string token)
		{
			Debug.Log(TAG + "Received Registration Token: " + token);
			if (!string.IsNullOrEmpty(token) && Game.User != null)
				Game.User.RegisterData.FirebaseCloudMessagingToken = token;
		}
#endif
		private void Log(string str) => GameLogger.debug(TAG + str);

		public void Test()
		{
			RegisterNotifications();
		}
	}
}
