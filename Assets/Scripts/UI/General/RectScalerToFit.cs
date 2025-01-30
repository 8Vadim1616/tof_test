using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.UI.General
{
	public class RectScalerToFit : MonoBehaviour
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

			ScaleToFit();
		}

		private void ScaleToFit()
		{
			var scales = self.rect.size.GetScalesToFit(target.rect.size);
			self.localScale = new Vector3(scales.x, scales.y, 1f);
		}
	}
}