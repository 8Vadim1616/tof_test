using UnityEngine;

namespace Assets.Scripts.UI.General
{
	public class TextSmartAnchors : MonoBehaviour
	{
		public bool anchorToSquare = true;
		public RectTransform parent;
		private RectTransform self;

		public Vector2 anchorMin;
		public Vector2 anchorMax;

		void Awake()
		{
			self = transform.GetComponent<RectTransform>();
		}

		public void Update()
		{
			if (!parent) return;
			SetScales();
		}

		private void SetScales()
		{
			if (self.parent != parent.transform)
				self.SetParent(parent);

			self.localScale = Vector3.one;
			self.anchorMin = anchorMin;
			self.anchorMax = anchorMax;

			self.anchoredPosition = Vector2.zero;
			self.sizeDelta = Vector2.zero;

			if (anchorToSquare)
			{
				var width = parent.rect.width;
				var height = parent.rect.height;

				var w = (width - height) / 2;
				var h = (height - width) / 2;

				if (w > 0)
				{
					self.offsetMin = new Vector2(-w, 0);
					self.offsetMax = new Vector2(-w, 0);
				}
				else
				{
					self.offsetMin = new Vector2(0, -h);
					self.offsetMax = new Vector2(0, -h);
				}
			}

			//self.localPosition = Vector3.zero;
		}
	}
}