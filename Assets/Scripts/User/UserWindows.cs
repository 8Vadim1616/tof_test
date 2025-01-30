using System;
using System.Collections.Generic;
using Assets.Scripts.Utils;

namespace Assets.Scripts.User
{
	public class UserWindows
	{
		private Dictionary<string, (int, long)> _data;

		public UserWindows()
		{
		}

		public void Update(Dictionary<string, (int, long)> data)
		{
			if (data == null)
				return;

			_data = data;
		}

		public long GetTimeSinceLastOpen(string type, int id)
		{
			var timeWhenOpened = GetTimeWindowOppened(type, id);
			return Math.Max(0, GameTime.Now - timeWhenOpened);
		}

		public long GetTimeWindowOppened(string type, int id)
		{
			if (_data != null && _data.TryGetValue(type, out var value) && value.Item1 == id)
				return _data[type].Item2;
			
			return 0;
		}
	}
}