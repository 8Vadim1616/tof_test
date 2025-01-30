using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Utils
{
    public static class ObservableUtils
	{
		public static IDisposable DoWhenClickedOutside(this RectTransform t, Camera cam, Action a) => Observable.EveryUpdate()
			.Where(_ => t != null)
			.Where(_ => Input.GetMouseButtonDown(0))
			.Where(_ => !RectTransformUtility.RectangleContainsScreenPoint(t, Input.mousePosition, cam))
			.Subscribe(_ => a.Invoke());

		public static IDisposable DoWhenMouseDown(this RectTransform t, Action a) => Observable.EveryUpdate()
			.Where(_ => t != null)
			.Where(_ => Input.GetMouseButtonDown(0))
			.Subscribe(_ => a.Invoke());

		public static IDisposable DoWhenMouseScroll(this RectTransform t, Action<float> a) => Observable.EveryUpdate()
			.Where(_ => t != null)
			.Where(_ => Input.mouseScrollDelta.y != 0)
			.Subscribe(_ => a.Invoke(Input.mouseScrollDelta.y));

		public static IDisposable DoWhenClickedOutside(this IList<RectTransform> transforms, Camera cam, Action a) => Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonDown(0))
                .Where(_ => transforms.All(t => !RectTransformUtility.RectangleContainsScreenPoint(t, Input.mousePosition, cam)))
                .Subscribe(_ => a?.Invoke());  

        public static IDisposable DoWhenMouseUpOutside(this RectTransform t, Camera cam, Action a) => MouseUpObservable(t, cam, a)
            .Subscribe(_ => a.Invoke());

        public static IDisposable DoWhenMouseUpOutsideSkipOne(this RectTransform t, Camera cam, Action a) => MouseUpObservable(t, cam, a)
            .Skip(1)
            .Subscribe(_ => a.Invoke());

        private static IObservable<long> MouseUpObservable(this RectTransform t, Camera cam, Action a) => Observable
            .EveryUpdate()
            .Where(_ => t != null)
            .Where(_ => Input.GetMouseButtonUp(0))
            .Where(_ => !RectTransformUtility.RectangleContainsScreenPoint(t, Input.mousePosition, cam));

        public static CompositeDisposable OnTapAsObservable(this Image o, Action action, float threshold = 0.5f)
        {
            var total = new CompositeDisposable();
            total.AddTo(o);
            IDisposable sub = null;

            o.OnPointerDownAsObservable()
				.Subscribe(e0 =>
				{
					var tap = Time.unscaledTime;
					sub?.Dispose();
					sub = o.OnPointerUpAsObservable()
						.Subscribe(e1 =>
						{
							if (Time.unscaledTime - tap > threshold)
								return;

							action.Invoke();
							sub?.Dispose();

						})
						.AddTo(total);
				})
				.AddTo(total);

            return total;
        }

		public static CompositeDisposable OnClickAsObservable(this Image o, Action action)
		{
			var total = new CompositeDisposable();
			total.AddTo(o);
			IDisposable sub = null;

			o.OnPointerDownAsObservable()
				.Subscribe(e0 =>
				{
					var tap = Time.unscaledTime;
					sub?.Dispose();
					sub = o.OnPointerUpAsObservable()
						.Subscribe(e1 =>
						{
							if (e1 == null || !e1.pointerEnter || !o || !o.gameObject)
								return;

							if (e1.pointerEnter != o.gameObject)
							{
								var images = e1.pointerEnter.GetComponentsInParent<Image>();
								if (images == null || !images.Contains(o))
									return;
							}

							action.Invoke();
							sub?.Dispose();
						})
						.AddTo(total);
				})
				.AddTo(total);

			return total;
		}

		public static IDisposable DoWhenMouseDownOutsideOfRectOrNotOverUI(this RectTransform t, Camera cam, Action a)
        {
            return Observable.EveryUpdate().Subscribe(_ =>
            {
                if (!Input.GetMouseButtonDown(0)) return;
                if (!t) return;
                if (Util.IsPointerOverUIObject() &&
                    RectTransformUtility.RectangleContainsScreenPoint(t, Input.mousePosition, cam)) return;

                a?.Invoke();
            }).AddTo(t);
        }

        public static IDisposable DoWhenMouseClickOutsideOfRectOrNotOverUI(this RectTransform t, Camera cam, Action<float> a)
        {
            Vector2 startPos = Vector2.zero;
            return Observable.EveryUpdate().Subscribe(_ =>
            {
                if (Input.GetMouseButtonDown(0))
                    startPos = Input.mousePosition;
                if (Input.GetMouseButtonUp(0) && t)
                {
                    var dist = Vector2.Distance(startPos, Input.mousePosition);
                    if (Util.IsPointerOverUIObject() && RectTransformUtility.RectangleContainsScreenPoint(t, Input.mousePosition, cam))
                        return;
                    if (dist < 10)
                        a?.Invoke(dist);
                }
            }).AddTo(t);
        }

        public static IDisposable TryEachFrame(Func<bool> canDoFunc, Action onCanDoAction)
        {
            IDisposable dis = null;
            dis = Observable.IntervalFrame(1).SkipWhile(_ => !canDoFunc.Invoke()).Subscribe(_ =>
            {
                onCanDoAction.Invoke();
                dis?.Dispose();
            });

            return dis;
        }

        public static IDisposable TryGetEachFrame<T>(Func<(bool, T)> canDoFunc, Action<T> onCanDoAction)
        {
            IDisposable dis = null;
            dis = Observable.IntervalFrame(1).Subscribe(_ =>
            {
                var can = canDoFunc.Invoke();

                if (!can.Item1) return;

                onCanDoAction(can.Item2);
                dis?.Dispose();
            });

            return dis;
        }

        public static IDisposable DoOnceWhenPredicateTrue(Func<bool> predicate, Action doAction)
        {
            IDisposable sub = null;
            sub = Observable.EveryUpdate().Subscribe(_ =>
            {
                if (predicate.Invoke())
                {
                    sub?.Dispose();
                    doAction?.Invoke();
                }
            });

            return sub;
        }

		public static IDisposable SetupGameTimer(this GameObject obj, long dueTime, Action onTimer = null, Action onTimerDone = null)
		{
			IDisposable sub = null;
			sub = Observable.Interval(TimeSpan.FromSeconds(1)).StartWith(0).Subscribe(_ => TimerCheck()).AddTo(obj);

			void TimerCheck()
			{
				onTimer?.Invoke();
				if (GameTime.Now >= dueTime)
					TimerFinish();
			}

			void TimerFinish()
			{
				sub?.Dispose();
				onTimerDone?.Invoke();
			}

			return sub;
		}
	}
}