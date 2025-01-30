using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Utils.Gestures
{
	public enum SwipeDirection
	{
		Up,
		Down,
		Left,
		Right
	}

	public class GestureEvent
	{
		public SwipeDirection Direction;
		public float Distance;
		public float AbsDistance => Mathf.Abs(Distance);

		public GestureEvent(SwipeDirection direction, float distance)
		{
			Direction = direction;
			Distance = distance;
		}
	}

	public class GesturesController : IDisposable
	{
		private Vector2 _startPosition;

		private IDisposable _observable;
		private float _threshold;

		public UnityEvent<GestureEvent> OnSwipe { get; private set; }
		public UnityEvent<GestureEvent> OnSwipeEnded { get; private set; }

		private bool _horizontal;
		private bool _vertical;

		/// <param name="threshold">Порог обнаружения свайпа, px</param>
		public GesturesController(float threshold = 20, bool horizontal = true, bool vertical = true)
		{
			_horizontal = horizontal;
			_vertical = vertical;

			OnSwipe = new UnityEvent<GestureEvent>();
			OnSwipeEnded = new UnityEvent<GestureEvent>();
			_threshold = threshold;
			_observable = Observable.EveryUpdate()
				.Subscribe(_ => CheckGesture());
		}

		private void CheckGesture()
		{
			if (Input.touchCount == 1 && Input.touches.FirstOrDefault() is Touch touch)
			{
				if (touch.phase == TouchPhase.Began)
					OnStartMove(touch.position);
				else if (touch.phase == TouchPhase.Moved)
					OnMove(touch.position);
				else if (touch.phase == TouchPhase.Ended)
					OnStopMove(touch.position);
			}
			else
			{
				if (Input.GetMouseButtonDown(0))
				{
					if (Util.IsPointerOverBlockSwipeObject())
						return;

					OnStartMove(Input.mousePosition);
				}
				else if (Input.GetMouseButton(0))
				{
					if (Util.IsPointerOverBlockSwipeObject())
						return;

					OnMove(Input.mousePosition);
				}
				else if (Input.GetMouseButtonUp(0))
				{
					if (Util.IsPointerOverBlockSwipeObject())
						return;

					OnStopMove(Input.mousePosition);
				}
			}


			void OnStartMove(Vector2 position)
			{
				_startPosition = position;
				_isSwipe = false;
			}

			void OnMove(Vector2 position)
			{
				CheckSwipe(position);
			}

			void OnStopMove(Vector2 position)
			{
				CheckSwipe(position, true);
				_startPosition = default;
				_isSwipe = false;
			}
		}

		private bool _isSwipe;

		private void CheckSwipe(Vector2 endPosition, bool isComplete = false)
		{
			var delta = _startPosition - endPosition;
			var vertical = _vertical ? Mathf.Abs(delta.y) : 0;
			var horizontal = _horizontal ? Mathf.Abs(delta.x) : 0;

			if (!_isSwipe && delta.magnitude > _threshold)
				_isSwipe = true;

			if (_isSwipe)
				if (horizontal > vertical)
				{
					var dist = delta.x;
					if (_startPosition.x - endPosition.x > 0)
						DetectSwipe(isComplete, SwipeDirection.Right, dist);
					else if (_startPosition.x - endPosition.x < 0)
						DetectSwipe(isComplete, SwipeDirection.Left, dist);
				}
				else if (vertical > horizontal)
				{
					var dist = delta.y;
					if (_startPosition.y - endPosition.y > 0)
						DetectSwipe(isComplete, SwipeDirection.Up, dist);
					else if (_startPosition.y - endPosition.y < 0)
						DetectSwipe(isComplete, SwipeDirection.Down, dist);
				}
		}

		private void DetectSwipe(bool isComplete, SwipeDirection direction, float distance)
		{
			var gusture = new GestureEvent(direction, distance);
			if (isComplete)
				OnSwipeEnded.Invoke(gusture);
			else
				OnSwipe.Invoke(gusture);
		}

		public void Dispose()
		{
			_observable.Dispose();
		}
	}
}
