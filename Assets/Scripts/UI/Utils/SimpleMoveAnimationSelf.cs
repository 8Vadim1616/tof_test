using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.UI.ControlElements
{
	public class SimpleMoveAnimationSelf : MonoBehaviour
	{
		public float horizontalAmplitude = 5f;
		public float verticalAmplitude = 10f;
		public float horizontalTime = 1f;
		public float verticalTime = 2f;

		public float delay = 0f;
		public Ease needEasing = Ease.OutSine;

		private float _currentDelay = 0f;

		private Tween horTween;
		private Tween vertTween;

		private bool _isStopped = false;

		public void Update()
		{
			if (_currentDelay < delay)
			{
				_currentDelay += Time.deltaTime;
				return;
			}

			if (_isStopped)
				return;
			
			if (vertTween == null || vertTween.active == false)
			{
				vertTween?.Kill();
				var down = transform.localPosition.y >= 0;
				vertTween = transform.DOLocalMoveY(down ? -verticalAmplitude : verticalAmplitude, verticalTime)
					.SetEase(needEasing)
					.SetLink(gameObject);
			}

			if (horTween == null || horTween.active == false)
			{
				horTween?.Kill();
				var left = transform.localPosition.x >= 0;
				horTween = transform.DOLocalMoveX(left ? -horizontalAmplitude : horizontalAmplitude, horizontalTime)
					.SetEase(needEasing)
					.SetLink(gameObject);
			}
		}

		public void Stop()
		{
			_isStopped = true;
			vertTween?.Kill();
			horTween?.Kill();
		}

		public void Play()
		{
			_isStopped = false;
		}
	}
}