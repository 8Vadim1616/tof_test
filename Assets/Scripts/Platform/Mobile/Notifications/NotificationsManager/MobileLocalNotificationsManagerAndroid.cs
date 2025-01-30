#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Utils;
using Newtonsoft.Json.Linq;
using Unity.Notifications.Android;
using UnityEngine;

namespace Assets.Scripts.Platform.Mobile.Notifications.NotificationsManager
{
	public class MobileLocalNotificationsManagerAndroid : MobileLocalNotificationsManager
	{
		public override void CancelNotification(int id)
		{
			AndroidNotificationCenter.CancelNotification(id);
		}

		public override void CancelNotifications(List<MobileNotification> notifications)
		{
			// foreach (var notification in notifications)
			// {
			// 	var ids = notification.GetIds();
			//
			// 	foreach (var id in ids)
			// 	{
			// 		CancelNotification(id);
			// 	}
			// }
			AndroidNotificationCenter.CancelAllNotifications();
		}

		public override void AddNotificationToDeviceSchedule(MobileNotificationSchedule schedule)
		{
			if (schedule.Timestamp <= GameTime.Now)
				return;

			var notification = new AndroidNotification();
			notification.Title = schedule.Title;
			notification.Text = schedule.Message;
			notification.FireTime = DateTimeOffset.FromUnixTimeSeconds(schedule.Timestamp).LocalDateTime;
			notification.SmallIcon = schedule.SmallIcon;
			notification.LargeIcon = schedule.BigIcon;
			notification.ShouldAutoCancel = true;

			Dictionary<string, string> intentDataDic = new Dictionary<string, string>()
			{
				{ "lang_key",		schedule.Key },
				{ "type",			schedule.Type }
			};

			notification.IntentData = JObject.FromObject(intentDataDic).ToString();

			var channel = CreateChannel(schedule.Channel);

			AndroidNotificationCenter.SendNotificationWithExplicitID(notification, channel.Id, schedule.NotificationId);
		}

		public override void AskPermissions()
		{
			CreateChannel();
		}

		private AndroidNotificationChannel CreateChannel(string channelName = "General")
		{
			var channel = new AndroidNotificationChannel
			{
				Id = channelName,
				Name = channelName,
				Description = channelName,
				Importance = Importance.High
			}; //Creating an existing notification channel with its original values performs no operation, so it's safe to call this code when starting an app.

			AndroidNotificationCenter.RegisterNotificationChannel(channel);

			return channel;
		}

		private void ProcessNotification(AndroidNotificationIntentData notificationIntentData, bool opened)
		{
			Debug.Log($"{TAG} Received a new message");

			var data = new Dictionary<string, object>();

			var notification = notificationIntentData.Notification;

			Debug.Log($"{TAG} channel_id: {notificationIntentData.Channel}, title: {notification.Title}, body: {notification.Text}, data: {notification.IntentData}, opened: {opened}");

			data["notif_type"] = notificationIntentData.Id;
			//data["title"] = notification.Title;
			//data["body"] = notification.Text;
			data["channel_id"] = notificationIntentData.Channel;
			data["opened"] = opened;

			if (!string.IsNullOrEmpty(notification.IntentData))
			{
				try
				{
					var parse = JObject.Parse(notification.IntentData);

					foreach (var kv in parse)
					{
						data[kv.Key] = kv.Value;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
			}

			if (data.Count > 0)
				ServerLogs.SendLog("notif", data);
		}

		private void ReceivedNotificationHandler(AndroidNotificationIntentData notificationIntentData)
		{
			bool opened = false;

			var openedNotif = AndroidNotificationCenter.GetLastNotificationIntent();

			if (openedNotif != null && openedNotif.Id == notificationIntentData.Id)
				opened = true;

			ProcessNotification(notificationIntentData, opened);
		}

		public override void CheckOpenedByNotification()
		{
			var openedNotif = AndroidNotificationCenter.GetLastNotificationIntent();

			if (openedNotif != null)
				ProcessNotification(openedNotif, true);
		}

		protected override void AddListeners()
		{
			AndroidNotificationCenter.OnNotificationReceived += ReceivedNotificationHandler;
		}

		protected override void RemoveListeners()
		{
			AndroidNotificationCenter.OnNotificationReceived -= ReceivedNotificationHandler;
		}
	}
}
#endif