using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.UI
{
	public class SimpleItemDropAnimated : MonoBehaviour, IItemDropAnimated
	{
		private Vector3 _baseScale;
		private Sequence _selectTween;

		public void Awake()
		{
			_baseScale = transform.localScale;
		}

		public void OnItemDropArrival()
		{
			if (!this || !gameObject)
				return;

			var curTransform = transform;

			_selectTween?.Kill();
			_selectTween = DOTween.Sequence()
				.SetLink(gameObject)
				.Append(curTransform.DOScale(new Vector3(_baseScale.x * 0.95f, _baseScale.y * 1.01f, _baseScale.z), 0.05f))
				.Append(curTransform.DOScale(new Vector3(_baseScale.x * 1.05f, _baseScale.y * 0.9f, _baseScale.z), 0.05f))
				.Append(curTransform.DOScale(new Vector3(_baseScale.x * 0.95f, _baseScale.y * 1.01f, _baseScale.z), 0.05f))
				.Append(curTransform.DOScale(_baseScale, 0.05f))
				.OnComplete(() => _selectTween = null);

			_selectTween.Play();
		}

		public Vector3 GetPositionGlobal() => transform.position;
	}
}