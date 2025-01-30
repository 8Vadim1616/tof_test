using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Utils
{
	public class FixedScreenScaler : MonoBehaviour
	{
		private RectTransform canvasRect;
		private CanvasScaler canvasScaler;

		private Vector2 normalResolution;

		private float needScale = 1;
		private Vector2 previousSizeDelta = Vector2.zero;

		private void Awake()
		{
			var go = GetComponentInParent<Canvas>().rootCanvas.gameObject;
			canvasRect = go.GetComponent<RectTransform>();
			canvasScaler = go.GetComponent<CanvasScaler>();

			normalResolution = canvasScaler.referenceResolution;

			CalculateScale();
		}

		private void Update()
		{
			CalculateScale();
		}

		public void CalculateScale(bool force = false)
		{
			if (previousSizeDelta == canvasRect.sizeDelta && !force)
				return;

			previousSizeDelta = canvasRect.sizeDelta;

			needScale = canvasRect.sizeDelta.y / normalResolution.y;
			transform.localScale = new Vector3(needScale, needScale, transform.localScale.z);
		}
	}
}