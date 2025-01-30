using Assets.Scripts.Utils;

namespace Assets.Scripts.Platform.Mobile.Notifications.NotificationDatas
{
	public class MobileNotificationReturn : MobileNotificationDataBase
	{
		private const string TYPE = "notification_return";

		public override string Name => TYPE;

		public override long Time
		{
			get
			{
				if (_days == 1)
					return MobileNotification.TimeNow + GameTime.ONE_HOUR_SECONDS;
				else
				{
					return MobileNotification.TimeNow + (_days - 1) * GameTime.ONE_DAY_SECONDS;
				}
			}
		}

		public override string Text => Game.Localize($"{Name}_desc_{_days}");

		//public override string SmallIcon => null;
		//public override string BigIcon => null;

		private int _days = 0;

		public MobileNotificationReturn(int id, int days) : base(id)
		{
			_days = days;
		}
	}
}