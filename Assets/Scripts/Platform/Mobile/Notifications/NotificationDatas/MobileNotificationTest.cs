namespace Assets.Scripts.Platform.Mobile.Notifications.NotificationDatas
{
	public class MobileNotificationTest : MobileNotificationDataBase
	{
		private const string TYPE = "notification_test";

		public override string Name => TYPE;
		public override string BigIcon => "test_big";

		public MobileNotificationTest(int id) : base(id)
		{
			if (Game.User == null)
				return;

			Time = MobileNotification.TimeNow + 10;
		}
	}
}