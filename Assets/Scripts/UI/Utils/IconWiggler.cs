using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.UI.ControlElements
{
	public class IconWiggler : MonoBehaviour
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
				vertTween = transform.DOLocalMoveY(down ? -verticalAmplitude : verticalAmplitude, verticalTime).SetLink(gameObject);
			}

			if (horTween == null || horTween.active == false)
			{
				horTween?.Kill();
				var endX = transform.localPosition.x >= 0 ? -horizontalAmplitude : horizontalAmplitude;
				var randomizedX = Random.value * endX;
				horTween = transform.DOLocalMoveX(randomizedX, horizontalTime).SetEase(needEasing).SetLink(gameObject);
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