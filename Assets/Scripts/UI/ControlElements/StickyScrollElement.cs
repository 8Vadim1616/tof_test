using System;
using Assets.Scripts.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI.ControlElements
{
	[RequireComponent(typeof(RectTransform))]
	public class StickyScrollElement : MonoBehaviour
	{
		//public UnityEvent OnUpdate;
		//public UnityEvent OnVisibilityChange;
		private CanvasGroup canvasGroup;

		public RectTransform Rect { get; private set; }

		public float Alpha
		{
			get => canvasGroup?.alpha ?? 0;
			set
			{
				if (canvasGroup)
					canvasGroup.alpha = value;
			}
		}

		public void Awake()
		{
			Rect = GetComponent<RectTransform>();
			canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
		}


		private IDisposable attachSub;
		public void AttachForSizeSetter(RectTransform t)
		{
			var attach = t;
			var self = GetComponent<RectTransform>();
			attachSub?.Dispose();
			attachSub = Observable.EveryUpdate().Subscribe(_ =>
			{
				self.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, attach.rect.width);
				self.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, attach.rect.height);

			}).AddTo(attach.gameObject).AddTo(self.gameObject);
		}
	}
}