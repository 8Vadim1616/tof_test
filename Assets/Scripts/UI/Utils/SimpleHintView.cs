using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.Utils
{
	public class SimpleHintView : MonoBehaviour
	{
		[SerializeField] protected RectTransform topPipka;
		[SerializeField] protected RectTransform botPipka;

		protected virtual int DELTA_OFFSET_Y => 220;

		private Tween _showTween;
		private bool isShowed;

		protected virtual bool CloseOnAnyMouseDown => false;
		public Promise ClosePromise { get; private set; }

		private void Awake()
		{
			var rect = (RectTransform) transform;

			if (CloseOnAnyMouseDown)
				rect.DoWhenMouseDown(Hide)
					.AddTo(this);
			rect.DoWhenClickedOutside(Game.MainCamera, Hide)
					.AddTo(this);
			rect.DoWhenMouseScroll(_ => Hide())
				.AddTo(this);

			OnAwake();

			if (!isShowed)
				Hide();
		}

		protected virtual void OnAwake()
		{

		}

		public virtual void SetPosition(RectTransform toTransform)
		{
			transform.position = toTransform.position;

			CheckFlip();
		}

		public virtual void CheckFlip()
		{
			var needFlipY = transform.position.y < 0;

			topPipka.gameObject.SetActive(!needFlipY);
			botPipka.gameObject.SetActive(needFlipY);

			if (needFlipY)
			{
				transform.localPosition += new Vector3(0, DELTA_OFFSET_Y, 0);
			}
			else
			{
				transform.localPosition += new Vector3(0, -DELTA_OFFSET_Y, 0);
			}
		}

		public virtual void Show()
		{
			isShowed = true;

			gameObject.SetActive(true);
			transform.localScale = Vector3.zero;

			_showTween?.Kill();
			_showTween = transform.DOScale(Vector3.one, 0.1f)
				.SetEase(Ease.OutBack)
				.SetLink(gameObject);

			ClosePromise?.ResolveOnce();
			ClosePromise = new Promise();
		}

		public virtual void Hide()
		{
			isShowed = false;
			_showTween?.Kill();

			gameObject.SetActive(false);

			ClosePromise?.ResolveOnce();
		}
	}
}