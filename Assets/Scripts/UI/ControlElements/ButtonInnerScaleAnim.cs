using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.ControlElements
{
	[RequireComponent(typeof(BasicButton))]
	public class ButtonInnerScaleAnim: MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
	{
		[SerializeField] private RectTransform _inner;
		[SerializeField] private float _scaleX = 0.95f;
		[SerializeField] private float _scaleY = 0.95f;

		private BasicButton _button;
		private Sequence _pressTween;
		private Vector3 _baseScale;

		public GameObject Inner => _inner.gameObject;

		private bool _wasPointerDown = false;
		private bool _wasPointerExit = false;

		public void Awake()
		{
			_button = GetComponent<BasicButton>();
			_button.NeedAnimateOnClick = false;

			_baseScale = _inner.localScale;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			_wasPointerDown = true;
			OnPressAnimationStart();
		}

		public virtual void OnPressAnimationStart()
		{
			if (!gameObject)
				return;

			_wasPointerExit = false;

			if (_pressTween != null)
			{
				_pressTween.Rewind();
				_pressTween.Play();
				return;
			}

			_pressTween = DOTween.Sequence()
								.Append(_inner.DOScale(new Vector3(_baseScale.x * _scaleX, _baseScale.y * _scaleY, _baseScale.z), 0.05f));

			_pressTween.SetLink(gameObject);
			_pressTween.SetAutoKill(false);

			_pressTween.Play();
		}

		public virtual void OnPressAnimationFinished()
		{
			if (!gameObject)
				return;

			if (_pressTween != null)
			{
				_pressTween.Rewind();
				//pressTween.Play();
				return;
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			_wasPointerDown = false;

			if (_wasPointerExit)
				_wasPointerExit = false;

			//float dragDistance = Vector2.Distance(eventData.pressPosition, eventData.position);
			//var dragThreshold = Screen.dpi / 12;
			//var isDrag = dragDistance > dragThreshold ;

			//Брать приходится каждый раз, т.к. парент у кнопок может поменятся
			var parentCanvas = GetComponentInParent<CanvasGroup>();
			if (parentCanvas != null && !parentCanvas.interactable)
				return;

			OnPressAnimationFinished();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (!_wasPointerDown)
				return;

			_wasPointerExit = true;
			OnPressAnimationFinished();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!_wasPointerDown)
				return;

			_wasPointerExit = false;
			OnPressAnimationStart();
		}
	}
}