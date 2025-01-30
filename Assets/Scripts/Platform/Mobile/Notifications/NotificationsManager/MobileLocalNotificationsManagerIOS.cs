#if UNITY_IOS

using System;
using System.Collections.Generic;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Utils;
using Newtonsoft.Json.Linq;
using Unity.Notifications.iOS;
using UnityEngine;

namespace Assets.Scripts.Platform.Mobile.Notifications.NotificationsManager
{
	public class MobileLocalNotificationsManagerIOS : MobileLocalNotificationsManager
	{
		public override void CancelNotification(int id)
		{
			iOSNotificationCenter.RemoveScheduledNotification(id.ToString());
		}

		public override void CancelNotifications(List<MobileNotification> notifications)
		{
			try
			{
				iOSNotificationCenter.RemoveAllScheduledNotifications();
			}
			catch (Exception e)
			{
				Debug.LogWarning("CancelNotifications error: " + e);
				throw;
			}
		}

		public override void AddNotificationToDeviceSchedule(MobileNotificationSchedule schedule)
		{
			if (schedule is null || schedule.Timestamp <= GameTime.Now)
				return;

			try
			{
				var dt = DateTimeOffset.FromUnixTimeSeconds(schedule.Timestamp).LocalDateTime;
				var timeTrigger = new iOSNotificationCalendarTrigger();
				timeTrigger.Year = dt.Year;
				timeTrigger.Month = dt.Month;
				timeTrigger.Day = dt.Day;
				timeTrigger.Hour = dt.Hour;
				timeTrigger.Minute = dt.Minute;
				timeTrigger.Second = dt.Second;
				timeTrigger.Repeats = false;

				var notification = new iOSNotification();
				notification.Identifier = schedule.NotificationId.ToString();
				notification.Title = schedule.Title;
				notification.Body = schedule.Message;
				//notification.Subtitle = schedule.Message;
				notification.ShowInForeground = false;
				notification.ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound);
				notification.CategoryIdentifier = "category_a";
				notification.ThreadIdentifier = "thread1";
				notification.Trigger = timeTrigger;

				Dictionary<string, string> intentDataDic = new Dictionary<string, string>()
														   {
																		   { "lang_key",       schedule.Key },
																		   { "type",           schedule.Type }
														   };

				notification.Data = JObject.FromObject(intentDataDic).ToString();

				iOSNotificationCenter.ScheduleNotification(notification);
			}
			catch (Exception e)
			{
				Debug.LogWarning("AddNotification error: " + e);
				throw;
			}
		}

		private void ProcessNotification(iOSNotification notificationData, bool opened)
		{
			Debug.Log($"{TAG} Received a new message");

			var data = new Dictionary<string, object>();

			var notification = notificationData;

			Debug.Log($"{TAG} title: {notification.Title}, body: {notification.Body}, data: {notification.Data}, opened: {opened}");

			data["notif_type"] = notification.Identifier;
			//data["title"] = notification.Title;
			//data["body"] = notification.Body;
			data["opened"] = opened;

			if (!string.IsNullOrEmpty(notification.Data))
			{
				try
				{
					var parse = JObject.Parse(notification.Data);

					foreach (var kv in parse)
					{
						data[kv.Key] = kv.Value;
					}
				}
				catch (Exception e)
				{
					Debug.LogError(e);
					throw;
				}
			}

			if (data.Count > 0)
				ServerLogs.SendLog("notif", data);
		}

		private void OnNotificationReceived(iOSNotification notificationData)
		{
			bool opened = false;

			var openedNotif = iOSNotificationCenter.GetLastRespondedNotification();

			if (openedNotif != null && openedNotif.Identifier == notificationData.Identifier)
				opened = true;

			ProcessNotification(notificationData, opened);
		}

		public override void CheckOpenedByNotification()
		{

		}

		protected override void AddListeners()
		{
			iOSNotificationCenter.OnNotificationReceived += OnNotificationReceived;
		}

		protected override void RemoveListeners()
		{
			iOSNotificationCenter.OnNotificationReceived -= OnNotificationReceived;
		}
	}
}

#endif