using System;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.ControlElements
{
    [RequireComponent(typeof(Slider))]
    public class TweenSlider<T> : MonoBehaviour where T : IComparable
    {
        // значение которое должно быть на слайдере в данный момент
        [HideInInspector] public ReactiveProperty<T> DisplayValue = new ReactiveProperty<T>();
        // значение которое должно быть на слайдере по итогам анимации
        [HideInInspector] public ReactiveProperty<T> RealValue = new ReactiveProperty<T>();

        [SerializeField] protected TextMeshProUGUI text;

        protected IDisposable textUpdateSub;

        protected T maxValue;
        protected T minValue;

        protected Slider slider;
        protected Tween changeTween;

        private void Awake()
        {
            OnAwake();
        }

        public bool HasText => text;
        public void SetupText(TextMeshProUGUI textMesh)
        {
            textUpdateSub?.Dispose();
            text = textMesh;

            if (!text) return;
            DisplayValue.Subscribe(UpdateText).AddTo(text);
        }

        protected void Update()
		{
			IsMovingUpdate();
        }

		protected virtual void IsMovingUpdate()
		{
			if (IsMoving.Value)
			{
				if (DisplayValue.Value.CompareTo(RealValue.Value) == 0)
					IsMoving.Value = false;
			}
			else if (DisplayValue.Value.CompareTo(RealValue.Value) != 0)
				IsMoving.Value = true;
		}

        protected virtual void Init(T min, T max, T current)
        {
            
        }

        protected virtual void OnAwake()
        {
            slider = GetComponent<Slider>();

            if (!slider)
            {
                Debug.LogError($"{name} has no slider for ValueSlider component");
            }

            RealValue.Subscribe(UpdateDisplay).AddTo(this);
            SetupText(text);
        }

        public virtual void Set(T value, bool instant = false)
        {
            if (instant)
            {
                RealValue.Value = value;

                changeTween?.Kill();

                DisplayValue.Value = value;

                return;
            }

			RealValue.Value = value;
			IsMoving.Value = true;
		}

        protected virtual void UpdateDisplay(T value)
        {
            DisplayValue.Value = value;
        }

        protected virtual void UpdateText(T value)
        {
            text.text = value.ToString();
        }

        /// <summary>
        /// Время на изменение значения 
        /// </summary>
        public float TIME_DISPLAY_UPDATE = 1f;

        /// <summary>
        /// Время когда мы считаем что анимация запуска изменения уже прошла
        /// </summary>
        public float TIME_DISPLAY_UPDATE_STARTED => TIME_DISPLAY_UPDATE * .3f;

        public ReactiveProperty<bool> IsMoving { get; } = new ReactiveProperty<bool>(false);
    }
}