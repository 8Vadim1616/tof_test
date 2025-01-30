using Assets.Scripts.Utils;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.ControlElements
{
	[RequireComponent(typeof(RectMask2D))]
	public class SizeRollEffectMask : MonoBehaviour
	{
		[SerializeField, Tooltip("Задержка срабатывания")] public float delay = 0;
		[SerializeField] public float duration = .5f;
		[SerializeField] public Ease ease = Ease.Unset;
		[SerializeField, Tooltip("(1,1) - Вправо, вверх")] public Vector2 maskMoveDirection;
		[SerializeField, Tooltip("Если истина - элемент пропадает")] public bool maskMoveHide;

		private void Start()
		{
			var rect = GetComponent<RectTransform>();
			var mask = GetComponent<RectMask2D>();

			var sizeElem = rect.rect.size;
			sizeElem = sizeElem.MultiplyElementWise(x: rect.localScale.x, y: rect.localScale.y);

			var dirX = maskMoveDirection.x;
			var dirY = maskMoveDirection.y;

			var moveLeft = dirX < 0;
			var moveRight = dirX > 0;

			var moveTop = dirY > 0;
			var moveBot = dirY < 0;

			// Vector4 padding - left, , top, right bottom
			var hiddenVectorStart = new Vector4(moveLeft ? sizeElem.x : 0, moveTop ? sizeElem.y : 0, moveRight ? sizeElem.x : 0, moveBot ? sizeElem.y : 0);
			var hiddenVectorEnd = new Vector4(moveRight ? sizeElem.x : 0,  moveBot ? sizeElem.y : 0, moveLeft ? sizeElem.x : 0, moveTop ? sizeElem.y : 0);

			var startVector = maskMoveHide ? Vector4.zero : hiddenVectorStart;
			var endVector = maskMoveHide ? hiddenVectorEnd : Vector4.zero;

			mask.padding = startVector;
			DOVirtual.DelayedCall(delay, () =>
			{
				if (!this) return;

				float v = 0;

				void Setter(float x)
				{
					v = x;
					mask.padding = Vector4.Lerp(startVector, endVector, x);
				}

				DOTween.To(() => v, Setter, 1, duration)
					   .SetEase(ease)
					   .SetLink(gameObject);

			}).SetLink(gameObject);
		}
	}
}