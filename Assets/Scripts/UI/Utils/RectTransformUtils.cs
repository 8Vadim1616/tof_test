using UnityEngine;

namespace Assets.Scripts.UI.ControlElements
{
	public static class RectTransformUtils
	{
		public static RectTransform SetupAnchors(this RectTransform target, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
		{
			target.SetParent(parent);
			target.anchorMin = anchorMin;
			target.anchorMax = anchorMax;

			target.sizeDelta = sizeDelta;

			return target;
		}
	}
}