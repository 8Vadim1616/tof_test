using System;
using System.Collections.Generic;
using Assets.Scripts.Platform.Mobile.Notifications.NotificationDatas;
using Assets.Scripts.Utils;

namespace Assets.Scripts.Platform.Mobile.Notifications
{
	public class MobileNotification
	{
		private const int ID_MULTIPLIER = 1000;

		/** Опорный ID нотификации (в итоге умножается на ID_MULTIPLER и добавляются повторы) */
		public int Id { get; private set; }

		/**	Количество повторов после основной нотификации */
		public int RepeatCount { get; private set; }

		/**	Интервал повтора нотификации (в секундах) */
		public int RepeatInterval { get; private set; }

		private Func<int, MobileNotificationDataBase> DataConstructor;

		public MobileNotification(int id, Func<int, MobileNotificationDataBase> dataConstructor, int repeatCount = 0, int repeatInterval = GameTime.ONE_DAY_SECONDS)
		{
			Id = id;
			DataConstructor = dataConstructor;
			RepeatCount = repeatCount;
			RepeatInterval = repeatInterval;
		}

		/**
		 * IDs нотификаций и их повторений
		 * @return Массив ID
		 */
		public List<int> GetIds()
		{
			var result = new List<int>();

			for (var i = -1; i < RepeatCount; i++)
				result.Add(Id * ID_MULTIPLIER + (i + 1));

			return result;
		}

		/**
		 * Создать расписания для уведомлений исходя из необходимово количества повторений
		 * @return Массив расписаний (Пустой если, нотификация не требуется)
		 */
		public List<MobileNotificationSchedule> GetSchedules()
		{
			var result = new List<MobileNotificationSchedule>();

			MobileNotificationDataBase data = DataConstructor.Invoke(Id);

			if (data.Time == 0)
				return result;

			for (var i = -1; i < RepeatCount; i++)
			{
				var id = Id * ID_MULTIPLIER + (i + 1);
				var time = data.Time + (i + 1) * RepeatInterval;

				result.Add(new MobileNotificationSchedule(id, time, data));
			}

			return result;
		}

		/**
		 * Текущее системное время
		 * @return Unix Timestamp в секундах
		 */
		public static long TimeNow => GameTime.Now;
	}
}