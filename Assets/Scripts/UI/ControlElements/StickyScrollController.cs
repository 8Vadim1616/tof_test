using System;
using System.Linq;
using Assets.Scripts.Utils;
using Scripts.UI.General;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.ControlElements
{
	[RequireComponent(typeof(ScrollRect))]
	public class StickyScrollController : MonoBehaviour
	{
		private ScrollRect scroll;
		private StickyScrollElement element;
		private StickyScrollElement clone;

		public Sides ignoreSides;

		private StickyScrollElement Element
		{
			get
			{
				if (element)
					return element;

				element = GetComponentInChildren<StickyScrollElement>();
				if (element)
				{
					clone = Instantiate(element, transform);
					var layoutE = clone.gameObject.GetOrAddComponent<LayoutElement>();
					layoutE.ignoreLayout = true;
					clone.AttachForSizeSetter(element.Rect);

					OnScrollValueChanged2(scroll.normalizedPosition);

					//element.OnUpdate.Invoke();
					//element.OnVisibilityChange.Invoke();
				}

				return element;
			}
		}


		public void Start()
		{
			scroll = GetComponent<ScrollRect>();
			scroll.onValueChanged.AddListener(OnScrollValueChanged2);
		}

		/*
		private void OnScrollValueChanged(Vector2 value)
		{
			var e = Element;
			if (!e)
				return;

			var bounds = scroll.GetComponent<RectTransform>().WorldRect();
			var fullyVisible = CountInsideCorners(e.Rect, bounds) >= 4;

			e.Alpha = fullyVisible ? 1f : 0f;
			clone.Alpha = fullyVisible ? 0f : 1f;

			var realPos = e.transform.position;
			var realBounds = e.Rect.WorldRect();

			if (fullyVisible) return;

			var anchX = .5f;
			var insideX = true;
			if (realBounds.xMax > bounds.xMax && !ignoreRight)
			{
				anchX = 1;
				insideX = false;
			}
			else if (realBounds.xMin < bounds.xMin && !ignoreLeft)
			{
				anchX = 0;
				insideX = false;
			}

			var anchY = .5f;
			var insideY = true;
			if (realBounds.yMax > bounds.yMax && !ignoreTop)
			{
				anchY = 1;
				insideY = false;
			}
			else if (realBounds.yMin < bounds.yMin && !ignoreBot)
			{
				anchY = 0;
				insideY = false;
			}

			//Debug.Log($"iside {insideX} {insideY} anch: {anchX} {anchY}");

			clone.Rect.pivot = new Vector2(anchX, anchY);
			clone.Rect.anchorMin = clone.Rect.pivot;
			clone.Rect.anchorMax = clone.Rect.pivot;
			clone.Rect.anchoredPosition = Vector2.zero;

			float? x = realPos.x;
			float? y = realPos.y;

			if (insideX || insideY)
				clone.transform.position = clone.transform.position.SetUI(x: insideX ? x : null, y: insideY ? y : null);

		}
		*/
		private void OnScrollValueChanged2(Vector2 value)
		{
			var e = Element;
			if (!e)
				return;

			var bounds = scroll.GetComponent<RectTransform>().WorldRectReal();
			var extendedSides = CountInsideCornersWithIgnore(e.Rect, bounds, ignoreSides);

			var fullyVisible = extendedSides == Sides.None;

			e.Alpha = fullyVisible ? 1f : 0f;
			clone.gameObject.SetActive(!fullyVisible);
			//clone.Alpha = fullyVisible ? 0f : 1f;

			var realPos = e.transform.position;

			if (fullyVisible) return;

			var anchX = .5f;
			if ((extendedSides & Sides.Right) != 0)
			{
				if ((extendedSides & Sides.Left) == 0)
					anchX = 1;
			}
			else if ((extendedSides & Sides.Left) != 0)
				anchX = 0;

			var anchY = .5f;
			if ((extendedSides & Sides.Top) != 0)
			{
				if ((extendedSides & Sides.Bot) == 0)
					anchY = 1;
			}
			else if ((extendedSides & Sides.Bot) != 0)
				anchY = 0;

			clone.Rect.pivot = new Vector2(anchX, anchY);
			clone.Rect.anchorMin = clone.Rect.pivot;
			clone.Rect.anchorMax = clone.Rect.pivot;
			clone.Rect.anchoredPosition = Vector2.zero;

			float? x = realPos.x;
			float? y = realPos.y;
			var insideX = (extendedSides & Sides.Left) == 0 && (extendedSides & Sides.Right) == 0;
			var insideY = (extendedSides & Sides.Top) == 0 && (extendedSides & Sides.Bot) == 0;

			//Debug.Log($"iside {insideX} {insideY} anch: {anchX} {anchY}");

			if (insideX || insideY)
				clone.transform.position = clone.transform.position.Set(x: insideX ? x : null, y: insideY ? y : null);
		}

		private static int CountInsideCorners(RectTransform target, Rect bounds)
		{
			Vector3[] targetCorners = new Vector3[4];
			target.GetWorldCorners(targetCorners);

			return targetCorners.Count(bounds.Contains);
		}

		private static Sides CountInsideCornersWithIgnore(RectTransform target, Rect bounds, Sides ignoreSides)
		{
			Vector3[] targetCorners = new Vector3[4];
			target.GetWorldCorners(targetCorners);

			var sides = Sides.None;
			if ((ignoreSides & Sides.Right) == 0 && targetCorners.Any(x => x.x > bounds.xMax))
				sides |= Sides.Right;
			if ((ignoreSides & Sides.Left) == 0 && targetCorners.Any(x => x.x < bounds.xMin))
				sides |= Sides.Left;
			if ((ignoreSides & Sides.Top) == 0 && targetCorners.Any(x => x.y > bounds.yMax))
				sides |= Sides.Top;
			if ((ignoreSides & Sides.Bot) == 0 && targetCorners.Any(x => x.y < bounds.yMin))
				sides |= Sides.Bot;
			Debug.Log("sides: " + sides);
			return sides;
		}

		[Flags]
		public enum Sides
		{
			None = 0,
			Top = 1,
			Bot = 2,
			Left = 4,
			Right = 8
		}
	}
}