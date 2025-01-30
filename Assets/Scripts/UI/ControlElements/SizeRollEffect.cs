using System;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.ControlElements
{
	public class SizeRollEffect : MonoBehaviour
	{
		[SerializeField, Tooltip("Задержка срабатывания")] float _delay = 0;
		[SerializeField] float _duration = .5f;
		[SerializeField] Ease _ease = Ease.InOutBack;
		[SerializeField] bool _vertical;
		[SerializeField] bool _horizontal;

		private void Start()
		{
			var rect = (RectTransform) transform;

			var afterSize = rect.sizeDelta;
			var beforeSize = new Vector2(_horizontal ? 0 : afterSize.x, _vertical ? 0 : afterSize.y);

			rect.sizeDelta = beforeSize;
			Scripts.Utils.Utils.Wait(_delay)
				.Then(() =>
				{
					if (this && rect)
						rect.DOSizeDelta(afterSize, _duration)
							.SetLink(gameObject)
							.SetEase(_ease);
				});
		}
	}
}