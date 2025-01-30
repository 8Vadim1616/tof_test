using System;
using Assets.Scripts.Utils;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.UI.ControlElements
{
	public class IconCircleWiggler : MonoBehaviour
	{
		public float Radius = 10;
		public float MoveSpeed = 20;

		public WiggleType Wiggle;
		public Ease Ease = Ease.Linear;

		private Vector2 lastPoint;
		private Tween moveTween;

		private Vector2 _initialLocalPos;

		public enum WiggleType
		{
			OnCircle, InsideCircle
		}

		private void Start()
		{
			ResetLocalPos();
		}

		private void Update()
		{
			var isMoving = moveTween?.active == true;
			var arrived = transform.localPosition.x.CloseTo(lastPoint.x) &&
						  transform.localPosition.y.CloseTo(lastPoint.y);

			if (isMoving && !arrived)
				return;
			StartMoving();
		}

		private void StartMoving()
		{
			if (MoveSpeed.CloseTo(0))
				return;

			moveTween?.Kill();
			var p = GetTargetPoint() + _initialLocalPos;

			var dist = (p - transform.localPosition.toVector2()).magnitude;
			var time = dist / MoveSpeed;

			moveTween = transform.DOLocalMove(p, time).SetEase(Ease).SetLink(gameObject);
		}

		public void ResetLocalPos()
		{
			_initialLocalPos = transform.localPosition;
		}

		private Vector2 GetTargetPoint()
		{
			switch (Wiggle)
			{
				case WiggleType.OnCircle:
					var randAngle = Random.value * 2 * Mathf.PI;
					return new Vector2(Mathf.Cos(randAngle), Mathf.Sin(randAngle)) * Radius;
				case WiggleType.InsideCircle:
					return Random.insideUnitCircle * Radius;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}