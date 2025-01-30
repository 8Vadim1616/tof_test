using Assets.Scripts.Utils;
using TMPro;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.HUD.Panels
{
	public class EnergyPanel : MonoBehaviour
	{
		[SerializeField] private TMP_Text _text;
		[SerializeField] private TMP_Text _timeLeft;

		private void Awake()
		{
			GameTime.Subscribe(OnTimer).AddTo(this);
			OnTimer();
		}

		private void OnTimer()
		{
			if (Game.User == null || Game.User.Energy == null)
				return;

			_text.text = $"{Game.User.Energy.Energy}/{Game.User.Energy.MaxEnergy}";
			var timeLeft = Game.User.Energy.TimeLeftForFullEnergy;
			if (timeLeft > 0)
			{
				_timeLeft.SetActive(true);
				_timeLeft.text = timeLeft.GetNumericTime(false);
			}
			else
				_timeLeft.SetActive(false);
		}
	}
}