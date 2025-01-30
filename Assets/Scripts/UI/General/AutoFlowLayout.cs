using System.Collections.Generic;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.General
{
	[AddComponentMenu("Layout/Extensions/Auto Flow Layout Group")]
	public class AutoFlowLayout : LayoutGroup
	{
		public enum Axis 
		{
			Horizontal = 0,
			Vertical = 1 
		}

		[SerializeField] Axis _startAxis = Axis.Horizontal;
		[SerializeField] public int _constrainChildrenCount;
		public Vector2 _spacing;
		[SerializeField] bool _expandHorizontalSpacing;
		[SerializeField] bool _childForceExpandWidth;
		[SerializeField] bool _childForceExpandHeight;
		[SerializeField] bool _invertOrder;

		[SerializeField] public bool fixedRows;
		[SerializeField] public int fixedRowsCount = 1;

		[SerializeField] public bool fixedColumns;
		[SerializeField] public int fixedColumnsCount = 1;

		private Vector2 _cellSize;
		private float _layoutHeight;
		private float _layoutWidth;

		public Axis StartAxis 
		{
			get => _startAxis; 
			set => SetProperty(ref _startAxis, value);
		}

		private void CalculateCellSize()
		{
			CalculateCellSizeNew();
		}

		private void CalculateCellSizeOld()
		{
			var childCount = _constrainChildrenCount == 0 || rectChildren.Count >= _constrainChildrenCount
							? rectChildren.Count
							: _constrainChildrenCount;

			var iColumn = rectTransform.rect.width >= rectTransform.rect.height
							? Mathf.CeilToInt(Mathf.Sqrt(childCount))
							: Mathf.FloorToInt(Mathf.Sqrt(childCount));

			var iRow = iColumn != 0 ? Mathf.CeilToInt(childCount / iColumn) : 0;

			var fHeight = (rectTransform.rect.height - ((iRow - 1) * (_spacing.y))) - (padding.top + padding.bottom);
			var fWidth = (rectTransform.rect.width - ((iColumn - 1) * (_spacing.x))) - (padding.right + padding.left);

			_cellSize = new Vector2(iColumn == 0 ? 0 : fWidth / iColumn, iRow == 0 ? 0 : (fHeight) / iRow);
		}


		private void CalculateCellSizeNew()
		{
			var childCount = _constrainChildrenCount == 0 || rectChildren.Count >= _constrainChildrenCount
							? rectChildren.Count
							: _constrainChildrenCount;

			var horizontal = rectTransform.rect.width >= rectTransform.rect.height;
			var (iColl, iRow) = CalcRowsColumns(childCount, horizontal);

			var availAbleHeight = rectTransform.rect.height - (iRow - 1).Clamp(0, int.MaxValue) * (_spacing.y) - (padding.top + padding.bottom);
			var availAbleWidth = rectTransform.rect.width - (iColl - 1).Clamp(0, int.MaxValue) * (_spacing.x) - (padding.right + padding.left);

			_cellSize = new Vector2(iColl == 0 ? 0 : availAbleWidth / iColl, iRow == 0 ? 0 : availAbleHeight / iRow);
		}

		private (int columns, int rows) CalcRowsColumns(int childCount, bool horizontal)
		{
			if (childCount <= 0) return (0, 0);

			if (!fixedRows && !fixedColumns)
			{
				var biggest = Mathf.CeilToInt(Mathf.Sqrt(childCount));
				var smallest = Mathf.CeilToInt((float) childCount / biggest);

				return horizontal ? (biggest, smallest) : (smallest, biggest);
			}
			
			if (fixedRows && fixedColumns)
			{
				return (fixedColumnsCount, fixedRowsCount);
			}

			var fixedDimension = fixedRows ? fixedRowsCount : fixedColumnsCount;
			var otherDimension = fixedDimension > 0 ? Mathf.CeilToInt((float) childCount / fixedDimension) : 0;
			return fixedRows ? (otherDimension, fixedDimension) : (fixedDimension, otherDimension);
		}


		public override void CalculateLayoutInputHorizontal()
		{
			if (StartAxis == Axis.Horizontal)
			{
				base.CalculateLayoutInputHorizontal();
				CalculateCellSize();
				var minWidth = GetGreatestMinimumChildWidth() + padding.left + padding.right;
				SetLayoutInputForAxis(minWidth, -1, -1, 0);
			}
			else
			{
				_layoutWidth = SetLayout(0, true);
			}
		}

		public override void SetLayoutHorizontal()
		{
			SetLayout(0, false);
		}

		public override void SetLayoutVertical()
		{
			SetLayout(1, false);
		}

		public override void CalculateLayoutInputVertical()
		{
			if (StartAxis == Axis.Horizontal)
			{
				_layoutHeight = SetLayout(1, true);
			}
			else
			{
				base.CalculateLayoutInputHorizontal();
				CalculateCellSize();
				var minHeight = GetGreatestMinimumChildHeigth() + padding.bottom + padding.top;
				SetLayoutInputForAxis(minHeight, -1, -1, 1);
			}
		}

		protected bool IsCenterAlign => childAlignment == TextAnchor.LowerCenter 
			|| childAlignment == TextAnchor.MiddleCenter
			|| childAlignment == TextAnchor.UpperCenter;

		protected bool IsRightAlign => childAlignment == TextAnchor.LowerRight 
			|| childAlignment == TextAnchor.MiddleRight
			|| childAlignment == TextAnchor.UpperRight;

		protected bool IsMiddleAlign => childAlignment == TextAnchor.MiddleLeft 
			|| childAlignment == TextAnchor.MiddleRight
			|| childAlignment == TextAnchor.MiddleCenter;

		protected bool IsLowerAlign => childAlignment == TextAnchor.LowerLeft 
			|| childAlignment == TextAnchor.LowerRight 
			|| childAlignment == TextAnchor.LowerCenter;

		private readonly IList<RectTransform> _itemList = new List<RectTransform>();

		public float SetLayout(int axis, bool layoutInput)
		{
			var groupHeight = rectTransform.rect.height;
			var groupWidth = rectTransform.rect.width;

			float spacingBetweenBars = 0;
			float spacingBetweenElements = 0;
			float offset = 0;
			float counterOffset = 0;
			float groupSize = 0;
			float workingSize = 0;
			if (StartAxis == Axis.Horizontal)
			{
				groupSize = groupHeight;
				workingSize = groupWidth - padding.left - padding.right;
				if (IsLowerAlign)
				{
					offset = padding.bottom;
					counterOffset = padding.top;
				}
				else
				{
					offset = padding.top;
					counterOffset = padding.bottom;
				}
				spacingBetweenBars = _spacing.y;
				spacingBetweenElements = _spacing.x;
			}
			else if (StartAxis == Axis.Vertical)
			{
				groupSize = groupWidth;
				workingSize = groupHeight - padding.top - padding.bottom;
				if (IsRightAlign)
				{
					offset = padding.right;
					counterOffset = padding.left;
				}
				else
				{
					offset = padding.left;
					counterOffset = padding.right;
				}
				spacingBetweenBars = _spacing.x;
				spacingBetweenElements = _spacing.y;
			}

			var currentBarSize = 0f;
			var currentBarSpace = 0f;

			for (var i = 0; i < rectChildren.Count; i++)
			{

				int index = i;
				var child = rectChildren[index];
				float childSize = 0;
				float childOtherSize = 0;

				if (StartAxis == Axis.Horizontal)
				{
					if (_invertOrder)
					{
						index = IsLowerAlign ? rectChildren.Count - 1 - i : i;
					}
					child = rectChildren[index];
					childSize = _cellSize.x;
					childOtherSize = _cellSize.y;
					child.sizeDelta = _cellSize;
				}
				else if (StartAxis == Axis.Vertical)
				{
					if (_invertOrder)
					{
						index = IsRightAlign ? rectChildren.Count - 1 - i : i;
					}
					child = rectChildren[index];
					childSize = _cellSize.y;
					childOtherSize = _cellSize.x;
					child.sizeDelta = _cellSize;
				}
				if (currentBarSize + childSize > workingSize && !Mathf.Approximately(currentBarSize + childSize, workingSize))
				{

					currentBarSize -= spacingBetweenElements;

					if (!layoutInput)
					{
						if (StartAxis == Axis.Horizontal)
						{
							float newOffset = CalculateRowVerticalOffset(groupSize, offset, currentBarSpace);
							LayoutRow(_itemList, currentBarSize, currentBarSpace, workingSize, padding.left, newOffset, axis);
						}
						else if (StartAxis == Axis.Vertical)
						{
							float newOffset = CalculateColHorizontalOffset(groupSize, offset, currentBarSpace);
							LayoutCol(_itemList, currentBarSpace, currentBarSize, workingSize, newOffset, padding.top, axis);
						}
					}

					_itemList.Clear();

					offset += currentBarSpace;
					offset += spacingBetweenBars;

					currentBarSpace = 0;
					currentBarSize = 0;
				}

				currentBarSize += childSize;
				_itemList.Add(child);

				if (childOtherSize > currentBarSpace)
					currentBarSpace = childOtherSize;

				if (i < rectChildren.Count - 1)
					currentBarSize += spacingBetweenElements;
			}

			if (!layoutInput)
			{
				if (StartAxis == Axis.Horizontal)
				{
					float newOffset = CalculateRowVerticalOffset(groupHeight, offset, currentBarSpace);
					currentBarSize -= spacingBetweenElements;
					LayoutRow(_itemList, currentBarSize, currentBarSpace, workingSize - (_childForceExpandWidth ? 0 : spacingBetweenElements), padding.left, newOffset, axis);
				}
				else if (StartAxis == Axis.Vertical)
				{
					float newOffset = CalculateColHorizontalOffset(groupWidth, offset, currentBarSpace);
					currentBarSize -= spacingBetweenElements;
					LayoutCol(_itemList, currentBarSpace, currentBarSize, workingSize - (_childForceExpandHeight ? 0 : spacingBetweenElements), newOffset, padding.top, axis);
				}
			}

			_itemList.Clear();

			offset += currentBarSpace;
			offset += counterOffset;

			if (layoutInput)
				SetLayoutInputForAxis(offset, offset, -1, axis);
			return offset;
		}

		private float CalculateRowVerticalOffset(float groupHeight, float yOffset, float currentRowHeight)
		{
			if (IsLowerAlign)
				return groupHeight - yOffset - currentRowHeight;
			
			if (IsMiddleAlign)
				return groupHeight * 0.5f - _layoutHeight * 0.5f + yOffset;

			return yOffset;
		}

		private float CalculateColHorizontalOffset(float groupWidth, float xOffset, float currentColWidth)
		{
			if (IsRightAlign)
				return groupWidth - xOffset - currentColWidth;

			if (IsCenterAlign)
				return groupWidth * 0.5f - _layoutWidth * 0.5f + xOffset;

			return xOffset;
		}

		protected void LayoutRow(IList<RectTransform> contents, float rowWidth, float rowHeight, float maxWidth, float xOffset, float yOffset, int axis)
		{
			var xPos = xOffset;

			if (!_childForceExpandWidth && IsCenterAlign)
				xPos += (maxWidth - rowWidth) * 0.5f;
			else if (!_childForceExpandWidth && IsRightAlign)
				xPos += (maxWidth - rowWidth);

			var extraWidth = 0f;
			var extraSpacing = 0f;

			if (_childForceExpandWidth)
			{
				extraWidth = (maxWidth - rowWidth) / _itemList.Count;
			}
			else if (_expandHorizontalSpacing)
			{
				extraSpacing = (maxWidth - rowWidth) / (_itemList.Count - 1);
				if (_itemList.Count > 1)
				{
					if (IsCenterAlign)
						xPos -= extraSpacing * 0.5f * (_itemList.Count - 1);
					else if (IsRightAlign)
						xPos -= extraSpacing * (_itemList.Count - 1);
				}
			}

			for (var j = 0; j < _itemList.Count; j++)
			{

				var index = IsLowerAlign ? _itemList.Count - 1 - j : j;

				var rowChild = _itemList[index];

				var rowChildWidth = _cellSize.x + extraWidth; //LayoutUtility.GetPreferredSize(rowChild, 0) + extraWidth;
				var rowChildHeight = _cellSize.y; //LayoutUtility.GetPreferredSize(rowChild, 1);

				if (_childForceExpandHeight)
					rowChildHeight = rowHeight;

				rowChildWidth = Mathf.Min(rowChildWidth, maxWidth);

				var yPos = yOffset;

				if (IsMiddleAlign)
					yPos += (rowHeight - rowChildHeight) * 0.5f;
				else if (IsLowerAlign)
					yPos += rowHeight - rowChildHeight;

				if (_expandHorizontalSpacing && j > 0)
					xPos += extraSpacing;

				if (axis == 0)
					SetChildAlongAxis(rowChild, 0, xPos, rowChildWidth);
				else
					SetChildAlongAxis(rowChild, 1, yPos, rowChildHeight);

				if (j < _itemList.Count - 1)
					xPos += rowChildWidth + _spacing.x;
			}
		}

		protected void LayoutCol(IList<RectTransform> contents, float colWidth, float colHeight, float maxHeight, float xOffset, float yOffset, int axis)
		{
			var yPos = yOffset;

			if (!_childForceExpandHeight && IsMiddleAlign)
				yPos += (maxHeight - colHeight) * 0.5f;
			else if (!_childForceExpandHeight && IsLowerAlign)
				yPos += maxHeight - colHeight;

			var extraHeight = 0f;
			var extraSpacing = 0f;

			if (_childForceExpandHeight)
			{
				extraHeight = (maxHeight - colHeight) / _itemList.Count;
			}
			else if (_expandHorizontalSpacing)
			{
				extraSpacing = (maxHeight - colHeight) / (_itemList.Count - 1);
				if (_itemList.Count > 1)
				{
					if (IsMiddleAlign)
						yPos -= extraSpacing * 0.5f * (_itemList.Count - 1);
					else if (IsLowerAlign)
						yPos -= extraSpacing * (_itemList.Count - 1);
				}
			}

			for (var j = 0; j < _itemList.Count; j++)
			{

				var index = IsRightAlign ? _itemList.Count - 1 - j : j;

				var rowChild = _itemList[index];

				var rowChildWidth = _cellSize.x; //LayoutUtility.GetPreferredSize(rowChild, 0) ;
				var rowChildHeight = _cellSize.y + extraHeight; //LayoutUtility.GetPreferredSize(rowChild, 1) + extraHeight;

				if (_childForceExpandWidth)
					rowChildWidth = colWidth;

				rowChildHeight = Mathf.Min(rowChildHeight, maxHeight);

				var xPos = xOffset;

				if (IsCenterAlign)
					xPos += (colWidth - rowChildWidth) * 0.5f;
				else if (IsRightAlign)
					xPos += colWidth - rowChildWidth;

				if (_expandHorizontalSpacing && j > 0)
					yPos += extraSpacing;

				if (axis == 0)
					SetChildAlongAxis(rowChild, 0, xPos, rowChildWidth);
				else
					SetChildAlongAxis(rowChild, 1, yPos, rowChildHeight);

				if (j < _itemList.Count - 1)
					yPos += rowChildHeight + _spacing.y;
			}
		}

		public float GetGreatestMinimumChildWidth()
		{
			var max = 0f;
			for (var i = 0; i < rectChildren.Count; i++)
				max = Mathf.Max(LayoutUtility.GetMinWidth(rectChildren[i]), max);
			return max;
		}

		public float GetGreatestMinimumChildHeigth()
		{
			var max = 0f;
			for (var i = 0; i < rectChildren.Count; i++)
				max = Mathf.Max(LayoutUtility.GetMinHeight(rectChildren[i]), max);

			return max;
		}
	}
}