using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.UI.General
{
	public class RectScalerToFillWithMinZone : MonoBehaviour
	{
		private RectTransform _self;
		public RectTransform _selfMax;
		public RectTransform _target;

		public void Awake()
		{
			_self = GetComponent<RectTransform>();
		}

		public void Update()
		{
			if (!_selfMax || !_target)
				return;

			ScaleToFill();
		}

		private void ScaleToFill()
		{
			var scales = MathUtils.GetScalesToFillWithMinZone(_self.rect.size, _selfMax.rect.size, _target.rect.size);
			_self.localScale = new Vector3(scales.x, scales.y, 1f);
		}
	}
}