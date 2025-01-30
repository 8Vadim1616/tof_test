using System;
using Assets.Scripts.Platform.Mobile.Notifications.NotificationDatas;
using Assets.Scripts.Utils;

namespace Assets.Scripts.Platform.Mobile.Notifications
{
	public class MobileNotificationSchedule
	{
		public int NotificationId { get; private set; }
		public long Timestamp { get; private set; }

		public string Type => _data.Name;
		public string Title => _data.Title;
		public string Key => _data.Key;
		public string Message => _data.Text;
		public string SmallIcon => _data.SmallIcon;
		public string BigIcon => _data.BigIcon;
		public string Channel => _data.Channel;
		// public string ChannelDesc => _data.ChannelDesc;

		private MobileNotificationDataBase _data;

		public MobileNotificationSchedule(int notificationId, long timestamp, MobileNotificationDataBase data)
		{
			NotificationId = notificationId;
			Timestamp = timestamp;
			_data = data;
		}

		public void SetTimestamp(long timeStamp) => Timestamp = timeStamp;

		public int GetStartHour => Game.Settings.NotifyStart;
		public int GetEndHour => Game.Settings.NotifyEnd;

		public void ClampNightTime()
		{
			var data = DateTimeOffset.FromUnixTimeSeconds(Timestamp).ToLocalTime();
			var hours = data.Hour;

			var start = GetStartHour;
			var end = GetEndHour;

			int nightTime = 0;
			int addHoursNightSkip = 0;
			var needAddHours = false;

			if (end < start) // end: 02:00, start: 08:00
			{
				if (hours < start && hours >= end)
					needAddHours = true;

				nightTime = start - end;
			}
			else // end: 23:00, start: 08:00
			{
				if (hours < start || hours >= end)
					needAddHours = true;

				nightTime = start - end + 24;
			}

			if (hours <= start)
				addHoursNightSkip = start - hours;
			else
				addHoursNightSkip = start - hours + 24;

			if (needAddHours)
			{
				//var needHours = start - hours;
				//if (needHours < 0) needHours += 24;
				var dataWithSkip = data.AddHours(addHoursNightSkip);

				if (Game.Settings.NotifyStartSeparate)
				{
					// Таким образом все нотифаи должны приходить с утра в течении первых 10 минут, но не все сразу
					dataWithSkip = dataWithSkip.Subtract(TimeSpan.FromMinutes(dataWithSkip.Minute / 6.0));
				}
				else
				{
					// Нотифы прийдут в течении 1 минуты
					dataWithSkip = dataWithSkip.Subtract(new TimeSpan(0, dataWithSkip.Minute, dataWithSkip.Second));
				}

				Timestamp = dataWithSkip.ToUnixTimeSeconds();
				// Debug.Log($"{TAG} Clamp night time at {NotificationId}, hour: {hours}, not in {start}:{end}, adding {nightTime}, final: {dataWithSkip}");
			}
			else
			{
				// Debug.Log($"{TAG} No need to clamp night time at {NotificationId}, hour: {hours}, it is in {start}:{end}, final: {data}");
			}
		}

		public void AddMinTime()
		{
			var time = Timestamp;
			var atLeastMinTime = GameTime.Now + Game.Settings.NotifyMinSeconds;

			if (time < atLeastMinTime)
			{
				Timestamp = atLeastMinTime;
			}
		}
	}
}