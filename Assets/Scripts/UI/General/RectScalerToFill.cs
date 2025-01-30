using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.UI.General
{
	public class RectScalerToFill : MonoBehaviour
	{
		public RectTransform target;
		private RectTransform self;

		public void Awake()
		{
			self = GetComponent<RectTransform>();
		}

		public void Update()
		{
			if (!target)
				return;

			ScaleToFill();
		}

		private void ScaleToFill()
		{
			var scales = self.rect.size.GetScalesToFill(target.rect.size);
			self.localScale = new Vector3(scales.x, scales.y, 1f);
		}
	}
}