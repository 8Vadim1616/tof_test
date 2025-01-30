using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.UI.ControlElements
{
	public class MoveRollEffect : MonoBehaviour
	{
		[SerializeField, Tooltip("Задержка срабатывания")] float _delay = 0;
		[SerializeField] float _duration = .5f;
		[SerializeField] Ease _ease = Ease.InOutBack;
		[SerializeField] Vector2 _startPosition;

		public bool IsEnded { get; private set; } = true;

		private Vector2? initPosition;

		private Sequence _seq;

		private void OnEnable()
		{
			IsEnded = false;

			var rect = (RectTransform) transform;
			initPosition ??= rect.anchoredPosition;

			_seq?.Kill();

			rect.anchoredPosition = initPosition.Value + _startPosition;

			_seq = DOTween.Sequence()
				.SetLink(gameObject)
				.SetLink(rect.gameObject)
				.AppendInterval(_delay)
				.Append(rect.DOAnchorPos(initPosition.Value, _duration).SetEase(_ease))
				.OnKill(OnKill)
				.OnComplete(OnKill);

			void OnKill()
			{
				if (rect)
					rect.anchoredPosition = initPosition.Value;

				IsEnded = true;
			}
		}
	}
}