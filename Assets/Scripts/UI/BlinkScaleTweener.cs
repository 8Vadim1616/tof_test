using Assets.Scripts.Utils;
using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.UI
{
	public class BlinkScaleTweener : MonoBehaviour
	{
		private Tween scaleTween;

		public Vector3 scaleStart = 1f.toVector3();
		public Vector3 scaleEnd = 1.5f.toVector3();
		public float time = .5f;
		public Ease Ease = DG.Tweening.Ease.OutQuad;

		void OnEnable()
		{
			transform.localScale = scaleStart;
			SetupTween();
		}

		void OnDisable()
		{
			ClearTween();
		}

		private void SetupTween()
		{
			ClearTween();

			var seq = DOTween.Sequence()
							 .Append(transform.DOScale(scaleEnd, time).SetEase(Ease))
							 .Append(transform.DOScale(scaleStart, time).SetEase(Ease))
							 .SetLink(gameObject)
							 .OnComplete(SetupTween);
			seq.Play();
			scaleTween = seq;
		}

		private void ClearTween()
		{
			scaleTween?.Kill();
			scaleTween = null;
		}
	}
}