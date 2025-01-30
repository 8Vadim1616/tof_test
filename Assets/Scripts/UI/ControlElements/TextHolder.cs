using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.ControlElements
{
	public class TextHolder : MonoBehaviour
	{
		[SerializeField] TextMeshProUGUI _text;

		public string Text
		{
			get => _text.text;
			set => _text.text = value;
		}

		public void SetTextActive(bool value) => 
			_text.SetActive(value);
	}
}