using System;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.UI.ControlElements;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.General
{
    public enum ScrollType
    {
        Horizontal,
        Vertical
    }

    public class ScrollRectController : MonoBehaviour, IEndDragHandler, IBeginDragHandler
    {
        [SerializeField] public ScrollRect scrollRect;
        [SerializeField] private BasicButton leftBtn;
        [SerializeField] private BasicButton rightBtn;

		[Tooltip("Сколько элементов листается при скролле")]
        [SerializeField] private int itemsCountToScroll = 1;
		[Tooltip("Сколько элементов видно")]
        [SerializeField] private int itemsVisible = 1;
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private bool hideBtn = true;

		public int ItemsVisible => itemsVisible;
		public int ItemsCountToScroll => itemsCountToScroll;
		public float Duration => duration;

		public int curPageIndex = 0;
		public int maxPageIndex = 0;
        public int curItemIndex = 0;
		private Tween _tween;
        public Action<Vector2> onValueChanged;
        public Action onScrollStart;
        public Action onScrollComplete;

        private ScrollType _type = ScrollType.Horizontal;
        private LayoutGroup _layoutGroup;

		private float _spacing
		{
			get
			{
				if (_layoutGroup is GridLayoutGroup gridLayoutGroup)
				{
					if (_type == ScrollType.Vertical)
						return gridLayoutGroup.spacing.y;

					return gridLayoutGroup.spacing.x;
				}

				if ((_layoutGroup is HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup))
					return horizontalOrVerticalLayoutGroup.spacing;

				return 0;
			}
		}

		private int _rowsColumns
		{
			get
			{
				if (_layoutGroup is GridLayoutGroup gridLayoutGroup)
					return gridLayoutGroup.constraintCount;

				return 1;
			}
		}

		private void Awake()
        {
            _layoutGroup = scrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>();

			if (!_layoutGroup)
				_layoutGroup = scrollRect.content.GetComponent<GridLayoutGroup>();

			if (_layoutGroup is VerticalLayoutGroup || _layoutGroup is GridLayoutGroup && (_layoutGroup as GridLayoutGroup).constraint == GridLayoutGroup.Constraint.FixedColumnCount) // Автоматическое определение типа
				_type = ScrollType.Vertical;

			Scripts.Utils.Utils.NextFrame(1)
				   .Then(() =>
					{
						if (leftBtn != null)
						{
							leftBtn.onClick.RemoveAllListeners();
							leftBtn.onClick.AddListener(prevPage);
						}

						if (rightBtn != null)
						{
							rightBtn.onClick.RemoveAllListeners();
							rightBtn.onClick.AddListener(nextPage);
						}
					});

            if (scrollRect != null)
            {
                scrollRect.onValueChanged.RemoveAllListeners();
                scrollRect.onValueChanged.AddListener(ShowPages);

				UpdateScrollBtns(); // чтобы на старте не было стрелок покзано
			}
        }

        private IPromise SnapToPage(int pageIndex, bool immediately = false, bool needEvents = true)
        {
            if (scrollRect == null) return Promise.Resolved();
            if (scrollRect.content == null) return Promise.Resolved();

			float childCount = scrollRect.content.transform.childCount;
			float childCountRounded = Mathf.Round(childCount / _rowsColumns) * _rowsColumns;

			int itemIndex = pageIndex * itemsCountToScroll;

			itemIndex = (int)Math.Min(itemIndex, childCountRounded - itemsVisible);

			if (itemIndex > 0)
			{
				if (itemIndex == curItemIndex)
				{
					if (pageIndex == curPageIndex + 1)
						pageIndex += 1;
					else if (pageIndex == curPageIndex - 1)
						pageIndex -= 1;

					itemIndex = pageIndex * itemsCountToScroll;
				}
			}

			curPageIndex = pageIndex;

			_tween?.Kill();
			scrollRect.StopMovement();

            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content.GetComponent<RectTransform>());

			float endPos = itemIndex / childCountRounded;

			return internal_Scroll(endPos, immediately, needEvents);
        }

        private IPromise SnapToItem(int itemIndex, bool immediately = false, bool needEvents = true)
        {
            if (scrollRect == null) return Promise.Resolved();
            if (scrollRect.content == null) return Promise.Resolved();

			_tween?.Kill();
			scrollRect.StopMovement();

			LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content.GetComponent<RectTransform>());

            float childCount = scrollRect.content.transform.childCount;
			float childCountRounded = Mathf.Round(childCount / _rowsColumns) * _rowsColumns;

			itemIndex = (int)Math.Min(itemIndex, childCountRounded - itemsVisible);

			float endPos = itemIndex / childCountRounded;

			return internal_Scroll(endPos, immediately, needEvents);
		}

		private IPromise internal_Scroll(float endPos, bool instant, bool needEvents)
		{
			var promise = new Promise();

			float clamp = 0;

			float spacing = _spacing;
			float tm = instant ? 0f : duration;

			if (needEvents)
				OnBeginDrag();

			if (_type == ScrollType.Horizontal)
			{
				clamp = Mathf.Clamp(-(scrollRect.content.sizeDelta.x + spacing) * endPos, -scrollRect.content.sizeDelta.x, 0);

				_tween = scrollRect.content.DOAnchorPosX(clamp, tm).OnComplete(onComplete);
			}
			else
			{
				clamp = Mathf.Clamp((scrollRect.content.sizeDelta.y + spacing) * endPos, 0, scrollRect.content.sizeDelta.y);

				_tween = scrollRect.content.DOAnchorPosY(clamp, tm).OnComplete(onComplete);
			}

			return promise;

			void onComplete()
			{
				if (needEvents)
					OnEndDrag();

				promise.Resolve();
			}
		}

        public IPromise SnapToCoord(float coord, bool immediately = false, bool needEvents = true)
        {
            if (scrollRect == null) return Promise.Resolved();
            if (scrollRect.content == null) return Promise.Resolved();

			var promise = new Promise();

			_tween?.Kill();
			scrollRect.StopMovement();

            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content.GetComponent<RectTransform>());

            var tm = duration;
            if (immediately) tm = 0;

            if (needEvents)
                OnBeginDrag();

            if (_type == ScrollType.Horizontal)
            {
                float viewportWidth = scrollRect.viewport.rect.width;
                float contentWidth = scrollRect.content.rect.width;
                float realCoord = coord;
                if (coord > 0f)
                    realCoord = 0f;
                if ((contentWidth - viewportWidth + coord) > 0f)
                    realCoord = contentWidth - viewportWidth;
                _tween = scrollRect.content.DOAnchorPosX(-realCoord, tm).OnComplete(onComplete);
            }
            else
            {
                float viewportHeight = scrollRect.viewport.rect.height;
                float contentHeight = scrollRect.content.rect.height;
                float realCoord = coord;
                if (coord < 0f)
                    realCoord = 0f;
                if ((contentHeight - viewportHeight - coord) < 0f)
                    realCoord = contentHeight - viewportHeight;
                _tween = scrollRect.content.DOAnchorPosY(realCoord, tm).OnComplete(onComplete);
            }

			return promise;

			void onComplete()
			{
				if (needEvents)
					OnEndDrag();

				promise.Resolve();
			}
        }

		public IPromise ScrollToPage(int page, bool immediately = false, bool needEvents = true)
		{
			return SnapToPage(page, immediately, needEvents);
		}

        public IPromise ScrollToItemIndexPage(int itemIndex, bool immediately = false, bool needEvents = true)
        {
			int pageIndex = Mathf.FloorToInt((float)itemIndex / (float)itemsCountToScroll);

            return SnapToPage(pageIndex, immediately, needEvents);
        }

        public IPromise ScrollToItemIndex(int itemIndex, bool immediately = false, bool needEvents = true)
        {
			return SnapToItem(itemIndex, immediately, needEvents);
        }

        public void prevPage()
        {
            if (scrollRect == null) return;
            if (scrollRect.content == null) return;

			var needPage = curPageIndex - 1;

			float curPageFloat = (float)curItemIndex / (float)itemsCountToScroll;

			if (curPageFloat % 1 != 0)
			{
				needPage = Mathf.FloorToInt(curPageFloat);
			}

			if (needPage < 0)
				needPage = 0;

            SnapToPage(needPage);
        }

        public void nextPage()
        {
            if (scrollRect == null) return;
            if (scrollRect.content == null) return;

			var needPage = curPageIndex + 1;

			float curPageFloat = (float)curItemIndex / (float)itemsCountToScroll;

			if (curPageFloat % 1 != 0)
			{
				needPage = Mathf.CeilToInt(curPageFloat);
			}

            if (needPage >= maxPageIndex)
				needPage = maxPageIndex;

            SnapToPage(needPage);
        }

        public void ShowPages(Vector2 pos)
        {
            float childs = scrollRect.content.transform
                .Cast<Transform>()
                .Count(x => x.gameObject?.activeSelf == true);

			var tmp = (itemsVisible + itemsCountToScroll) / 2;
			var roundedChildCount = Mathf.Round(childs / _rowsColumns) * _rowsColumns;
			maxPageIndex = Mathf.CeilToInt((roundedChildCount - tmp) / itemsCountToScroll);

            var spacing = _spacing;

			if (_type == ScrollType.Horizontal)
            {
                curItemIndex = Mathf.RoundToInt(-1f * scrollRect.content.anchoredPosition.x * roundedChildCount / (scrollRect.content.sizeDelta.x + spacing)); // Если ScrollRect.content pivot != 0, то работает хреново
                // curItemIndex = Mathf.RoundToInt((maxPageIndex * itemsPerPage)* ScrollRect.horizontalNormalizedPosition); // Так намного лучше
            }
            else
            {
                curItemIndex = Mathf.RoundToInt(scrollRect.content.anchoredPosition.y  * roundedChildCount / (scrollRect.content.sizeDelta.y + spacing));
                //curEndItemIndex = Mathf.RoundToInt(childs * (ScrollRect.content.anchoredPosition.y + ScrollRect.viewport.rect.height) / (ScrollRect.content.sizeDelta.y + spacing));
            }

			curPageIndex = Mathf.Clamp(Mathf.FloorToInt((float)curItemIndex / (float)itemsCountToScroll), 0, maxPageIndex);

			if (curItemIndex + itemsVisible >= roundedChildCount)
				curPageIndex = maxPageIndex;

			if (!hideBtn) return;

            if (rightBtn != null)
            {
                if (curPageIndex >= maxPageIndex)
                    rightBtn.gameObject.SetActive(false);
                else
                    rightBtn.gameObject.SetActive(true);
            }

            if (leftBtn != null)
            {
                if (curItemIndex <= 0)
                    leftBtn.gameObject.SetActive(false);
                else
                    leftBtn.gameObject.SetActive(true);
            }

            if (onValueChanged != null) onValueChanged(pos);
        }

		public void UpdateScrollBtns()
		{
			ShowPages(scrollRect.normalizedPosition);
		}

        public void SetItemsVisible(int count) => itemsVisible = count;

        public void OnBeginDrag(PointerEventData eventData = null)
        {
            onScrollStart?.Invoke();
        }

        public void OnEndDrag(PointerEventData eventData = null)
        {
            onScrollComplete?.Invoke();
        }

        private void OnDestroy()
        {
            _tween?.Kill();
        }
    }
}
