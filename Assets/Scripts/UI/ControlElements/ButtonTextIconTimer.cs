using System;
using Assets.Scripts.Utils;
using TMPro;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.ControlElements
{
	public class ButtonTextIconTimer : ButtonTextIcon
	{
		[SerializeField] TextMeshProUGUI _timerText;

		public long TimeLeft => (_timerUntil - GameTime.Now).Clamp(0, long.MaxValue);

		private IDisposable _timerSub;
		private long _timerUntil;

		public void SetupTimer(long timerTime, Action onTimerDone)
		{
			_timerUntil = GameTime.Now + timerTime;

			_timerSub?.Dispose();
			_timerSub = Observable.Interval(TimeSpan.FromSeconds(1))
				.StartWith(0)
				.Subscribe(_ => TimerCheck())
				.AddTo(this);

			void TimerCheck()
			{
				if (!this) 
					return;

				var left = TimeLeft;
				if (left <= 0)
				{
					_timerSub?.Dispose();
					onTimerDone?.Invoke();
				}

				_timerText.text = left.GetNumericTime();
			}
		}
	}
}