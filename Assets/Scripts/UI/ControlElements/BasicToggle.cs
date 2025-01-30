using TMPro;
using UnityEngine;
using static UnityEngine.UI.Toggle;

namespace Assets.Scripts.UI.ControlElements
{
	public class BasicToggle : BasicButton
	{
		[Header("Toggle options")]
		[SerializeField] bool _isOn;
		[SerializeField] private bool _addListenerOnStart = true;
		[SerializeField] GameObject _graphicOn;
		[SerializeField] GameObject _graphicOff;
		[SerializeField] TextMeshProUGUI _text;

		public ToggleEvent OnValueChanged = new ToggleEvent();

		public string Text
		{
			get => _text.text;
			set => _text.text = value;
		}

		public bool IsOn
		{
			get => _isOn;
			set => SetValue(value);
		}

		private void SetValue(bool value)
		{
			_isOn = value;
			if (_graphicOn)
				_graphicOn.SetActive(value);
			if (_graphicOff)
				_graphicOff.SetActive(!value);
			OnValueChanged?.Invoke(value);
		}

		private void Start()
		{
			if (_addListenerOnStart)
				onClick.AddListener(() => SetValue(!_isOn));

			SetValue(_isOn);
		}

		private void OnValidate()
		{
			SetValue(_isOn);
		}
	}
}


