using Assets.Scripts.Utils;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.ControlElements
{
	[RequireComponent(typeof(TweenSliderFloat))]
	public class ParticlesForTweenSlider : MonoBehaviour
	{
		[SerializeField] private ParticleSystem particles;

		public TweenSliderFloat Slider { get; private set; }
		public float MIN_DISTANCE = 0.01f;

		public const string ME = "[ParticlesForTweenSlider]";

		private void Awake()
		{
			particles.gameObject.SetActive(true);
			particles.Stop();
			particles.Clear();
			Slider = GetComponent<TweenSliderFloat>();
			// Код ниже почему то местами работает с ошибками - если слайдер на нуле партикли не прекращаются
			// иногда может не запускаться при передвежении
			// particles.isPlaying == true particles.isEmitting == false
			// сейчас все это перенесено ради теста в апдейт
			return;
			Slider.IsMoving.Subscribe(x =>
			{
				Debug.Log($"{ME} IsMoving: {x}");
				if (x && !particles.isPlaying)
				{
					var initDif = Mathf.Abs(Slider.DisplayValue.Value - Slider.RealValue.Value);
					if (initDif > MIN_DISTANCE)
					{
						particles.Play();
						Debug.Log($"{ME} ParticlesStart");
					}
				}
				else if (!x)
				{
					Debug.Log($"{ME} ParticlesStop");
					particles.Stop();
				}

				Debug.Log($"{ME} particles playing: {particles.isPlaying} {particles.isEmitting} {particles.emission.enabled}");

			}).AddTo(this);
		}

		private void Update()
		{
			if (Slider.DisplayValue.Value.CloseTo(Slider.RealValue.Value, MIN_DISTANCE))
			{
				if (particles.isPlaying)
					particles.Stop();
			}
			else
				particles.Play();
		}
	}
}