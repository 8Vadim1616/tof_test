
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Utils
{
	public class ScrollBtnsController: MonoBehaviour
	{
		[SerializeField] private ScrollRect _scrollRect;
		[SerializeField] private Button _btnLeft;
		[SerializeField] private Button _btnRight;
		[SerializeField] private float _offsetX = 400f;

		private Tween _tween;

		private void Awake()
		{
			_scrollRect.onValueChanged.AddListener(OnValueChange);
			_btnLeft.onClick.AddListener(OnLeftBtnClick);
			_btnRight.onClick.AddListener(OnRightBtnClick);
		}

		public void OnUpdate(bool isInit = false)
		{
			var scrollWidth = _scrollRect.viewport.rect.width;
			var contentPosition = Mathf.Round(_scrollRect.content.anchoredPosition.x);
			var contentWidth = (_scrollRect.content.transform as RectTransform).rect.width;

			var half = Mathf.Round(contentWidth / 2);

			var needBtnLeft = false;
			var needBtnRight = false;

			if (contentWidth > scrollWidth)
			{
				_scrollRect.horizontal = true;

				var min = half - (contentWidth - scrollWidth);

				needBtnLeft = contentPosition < half;
				needBtnRight = contentPosition > min;

				if (isInit) // При инициализации смещаем контейнер, чтоб левая стрелка скрылась
					_scrollRect.content.anchoredPosition = new Vector2(half, _scrollRect.content.anchoredPosition.y);
			}
			else
			{
				_scrollRect.content.anchoredPosition = new Vector2(half, _scrollRect.content.anchoredPosition.y);
				_scrollRect.horizontal = false;
			}

			_btnLeft.SetActive(needBtnLeft);
			_btnRight.SetActive(needBtnRight);

			// GameLogger.debug(contentPosition + " " + contentWidth);
		}

		private void OnLeftBtnClick()
		{
			_tween?.Kill();

			var scrollWidth = _scrollRect.viewport.rect.width;
			var contentWidth = (_scrollRect.content.transform as RectTransform).rect.width;
			var half = Mathf.Round(contentWidth / 2);

			var value = Mathf.Min(_scrollRect.content.anchoredPosition.x + _offsetX, half);
			_tween = _scrollRect.content.DOAnchorPosX(value, 0.3f);
		}

		private void OnRightBtnClick()
		{
			_tween?.Kill();

			var scrollWidth = _scrollRect.viewport.rect.width;
			var contentWidth = (_scrollRect.content.transform as RectTransform).rect.width;
			var half = Mathf.Round(contentWidth / 2);
			var min = half - (contentWidth - scrollWidth);

			var value = Mathf.Max(_scrollRect.content.anchoredPosition.x - _offsetX, min);
			_tween = _scrollRect.content.DOAnchorPosX(value, 0.3f);
		}

		private void OnValueChange(Vector2 pos)
		{
			OnUpdate();
		}
	}
}