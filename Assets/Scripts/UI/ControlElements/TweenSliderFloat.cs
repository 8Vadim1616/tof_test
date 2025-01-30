using Assets.Scripts.Utils;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Libraries.RSG;

namespace Assets.Scripts.UI.ControlElements
{
    [RequireComponent(typeof(Slider))]
    public class TweenSliderFloat : TweenSlider<float>
    {
        public bool TextAsPercent = false;

		public Promise _tweenPromise;
		public IPromise TweenPromise => _tweenPromise ?? Promise.Resolved();

		private Ease? _choosedEasing;

		protected override void OnAwake()
        {
            base.OnAwake();

            DisplayValue.Subscribe(x => slider.value = x);
        }

        protected override void UpdateDisplay(float value)
        {
            if (DisplayValue.Value.CloseTo(value))
            {
                DisplayValue.Value = value;

				_tweenPromise?.ResolveOnce();
				_tweenPromise = new Promise();
				_tweenPromise.ResolveOnce();

				return;
            }

            var progress = changeTween?.position ?? 0;
            var wasInProgress = changeTween?.active == true && progress <= TIME_DISPLAY_UPDATE_STARTED;

			_tweenPromise?.ResolveOnce();
			_tweenPromise = new Promise();

			changeTween?.Kill();

			if (gameObject != null)
			{
				changeTween = DOTween.To(() => DisplayValue.Value, x => DisplayValue.Value = x, value, TIME_DISPLAY_UPDATE)
					.SetLink(gameObject)
					.OnComplete(_tweenPromise.ResolveOnce)
					.OnKill(_tweenPromise.ResolveOnce)
					.SetEase(_choosedEasing.HasValue ? _choosedEasing.Value : (wasInProgress ? Ease.OutCubic : Ease.InOutCubic));
			}
			else
				_tweenPromise.ResolveOnce();
		}

		public void SetupValues(float min, float max)
		{
			minValue = min;
			maxValue = max;

			if (!slider)
				slider = GetComponent<Slider>();

			if (!slider)
				return;

			slider.minValue = minValue;
			slider.maxValue = maxValue;
		}

		public void SetEasing(Ease ease)
		{
			_choosedEasing = ease;
		}

		protected override void UpdateText(float value)
        {
            if (!text)
				return;

            text.text = TextAsPercent ? value.ToString("##0%") : value.ToString("F");
        }

		protected override void IsMovingUpdate()
		{
			var closeTo = DisplayValue.Value.CloseTo(RealValue.Value);
			IsMoving.Value = !closeTo;
		}
	}
}