using UnityEngine;

namespace Assets.Gameplay.MapObjects
{
	public class TimeDelayChecker
	{
		private float _lastCheckTime;
		public float Delay;
		
		public TimeDelayChecker(float delay = 0.1f)
		{
			Delay = delay;
		}

		public bool Check()
		{
			if (Time.fixedTime - _lastCheckTime < Delay)
				return false;
			
			_lastCheckTime = Time.fixedTime;
			return true;
		}
	}
}