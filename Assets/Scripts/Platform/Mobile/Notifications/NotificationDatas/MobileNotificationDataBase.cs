using Assets.Scripts.Utils;

namespace Assets.Scripts.Platform.Mobile.Notifications.NotificationDatas
{
	public class MobileNotificationDataBase
	{
		private const string TYPE = "notification_default";

		public int Id { get; protected set; }

		public virtual string Name { get; protected set; } = TYPE;

		public virtual string Key => $"{Name}_title_1";

		public virtual string Channel => "General";
		// public virtual string ChannelDesc => $"{Channel}_desc";

		public virtual string Title
		{
			get
			{
				var haveKey = Game.Localization.ContainsKey(Key);
				var text = Game.Localize(Key);

				if (!haveKey || text.IsNullOrEmpty())
					return "game_name".Localize();

				return text;
			}
		}

		public virtual string Text => Game.Localize($"{Name}_desc_1");
		public virtual long Time { get; protected set; }

		public virtual string SmallIcon { get; protected set; } = "general_small";
		public virtual string BigIcon { get; protected set; } = "general";

		public MobileNotificationDataBase(int id)
		{
			Id = id;
		}
	}
}