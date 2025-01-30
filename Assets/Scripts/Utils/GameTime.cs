using System;
using System.Globalization;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class GameTime
	{
		public static string SRV_NOW_KEY = "srv_tm";
		
		public const int ONE_DAY_SECONDS = 60 * 60 * 24;
		public const int ONE_HOUR_SECONDS = 60 * 60;
		public const int ONE_MINUTE_SECONDS = 60;

        private static long _serverDelta;
        private static bool _isActual;
		
		public static long ServerTime => PlayerPrefs.GetInt(SRV_NOW_KEY, 0);

        public static void UpdateFromServer(long serverNow)
        {
            if (serverNow == default(long))
                return;
			
			PlayerPrefs.SetInt(SRV_NOW_KEY, (int)serverNow);
			PlayerPrefs.Save();

            _serverDelta = serverNow - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			_isActual = false;
		}

        private static double _now;
        
        public static double NowMilli
        {
            get
			{
				Actualize();
                return _now;
            }
        }

		private static void Actualize()
		{
			if (!_isActual)
			{
				_now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d + _serverDelta;
				//_isActual = true;
			}
		}
		
		public static long Now
		{
			get
			{
				Actualize();
				return (long) _now;
			}
		}

        public static IDisposable Subscribe(Action callBack, long interval = 1000) =>
            Observable.Interval(TimeSpan.FromMilliseconds(interval))
                .Subscribe(x => callBack());

		public static void Init()
		{
			// Observable.Interval(TimeSpan.FromSeconds(1f))
			// 		  .Subscribe(_ => MarkNotActual());
		}

		public static void MarkNotActual() =>
            _isActual = false;
		
		public static bool IsTimePassed(long time)
		{
			return time - Now <= 0;
		}
    }
}
