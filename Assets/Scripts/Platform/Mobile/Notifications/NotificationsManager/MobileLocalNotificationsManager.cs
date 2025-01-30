using System.Collections.Generic;

namespace Assets.Scripts.Platform.Mobile.Notifications.NotificationsManager
{
	public abstract class MobileLocalNotificationsManager
	{
		protected const string TAG = "[LocalNotifications]";

		public MobileLocalNotificationsManager()
		{
			AddListeners();
		}

		public virtual void CancelNotification(int id) {}
		public virtual void CancelNotifications(List<MobileNotification> notifications) {}
		public virtual void AddNotificationToDeviceSchedule(MobileNotificationSchedule schedule) {}

		public virtual void CheckOpenedByNotification() { }

		public virtual void AskPermissions() { }

		protected virtual void AddListeners()
		{

		}

		protected virtual void RemoveListeners()
		{

		}

		public virtual void Free()
		{
			RemoveListeners();
		}
	}
}