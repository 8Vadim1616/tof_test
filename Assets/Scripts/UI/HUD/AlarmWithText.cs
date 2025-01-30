using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.HUD
{
	public class AlarmWithText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _alarmText;
		[SerializeField] private GameObject _alarmImage;

		public void SetText(string text)
		{
			_alarmImage.SetActive(false);
			_alarmText.text = text;
		}

		public void SetAlarm()
		{
			_alarmImage.SetActive(true);
			_alarmText.text = "";
		}
	}
}