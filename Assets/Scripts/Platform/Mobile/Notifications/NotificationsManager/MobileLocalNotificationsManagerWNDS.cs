using System.Collections.Generic;
using Assets.Scripts.Platform.Adapter.Implements;

namespace Assets.Scripts.Platform.Mobile.Notifications.NotificationsManager
{
	public class MobileLocalNotificationsManagerWNDS : MobileLocalNotificationsManager
	{
		public override void CancelNotification(int id)
		{

		}

		public override void CancelNotifications(List<MobileNotification> notifications)
		{
			if (Game.Social.Adapter is WNDSAdapter wndsAdapter)
				wndsAdapter.CancelAllLocalNotifications();
		}

		public override void AddNotificationToDeviceSchedule(MobileNotificationSchedule schedule)
		{
			if (Game.Social.Adapter is WNDSAdapter wndsAdapter)
				wndsAdapter.SendLocalNotification(schedule.Message, schedule.Timestamp, schedule.NotificationId);
		}
	}
}