using System;
using Assets.Scripts.Utils;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.General
{
	public class SettingsToggler : MonoBehaviour, IPointerDownHandler
	{
		private const float FADE_DURATION = .2f;

		[SerializeField] TextMeshProUGUI _captionText;
		[SerializeField] TextMeshProUGUI _onOffText;
		[SerializeField] GameObject handlerOn;
		[SerializeField] GameObject handlerOff;
		[SerializeField] Slider _slider;

		public bool Value { get; private set; }
		private event Action<bool> ValueChanged;

		public void Init(string caption, bool value, Action<bool> onValueChanged)
		{
			_captionText.text = caption;
			Init(value, onValueChanged);
		}

		public void Init(bool value, Action<bool> onValueChanged)
		{
			ValueChanged = onValueChanged;

			Set(value);
		}

		public void Set(bool value)
		{
			Value = value;
			ValueChanged?.Invoke(Value);

			var startValue = value ? 0f : 1f;
			var endValue = value ? 1f : 0f;

			DOTween.To(() => startValue, Progress, endValue, FADE_DURATION)
				.SetLink(gameObject);

			_onOffText.text = (value ? "tg_on" : "tg_off").Localize();

			void Progress(float progress)
			{
				_slider.value = progress;
				handlerOn.SetAlpha(progress);
				handlerOff.SetAlpha(1 - progress);
			}
		}

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			Set(!Value);
		}
	}
}