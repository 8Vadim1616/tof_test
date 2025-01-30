using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Utils
{
	public class LensEffect: ImageEffect
	{
		[SerializeField] private float _power = 0.5f;
		[SerializeField] private float _radius = 0.037f; // Дефолтный радиус для лупы со скейлом 1

		private CanvasScaler _canvasScaler;
		private RectTransform _canvasRectTransform;

		// Called by camera to apply image effect
		void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			var p = Camera.main.WorldToViewportPoint(transform.position);

			if (_canvasScaler == null)
				_canvasScaler = Game.Instance.GetComponent<CanvasScaler>();

			if (_canvasRectTransform == null)
				_canvasRectTransform = (Game.Instance.transform as RectTransform);

			var deltaScale = _canvasRectTransform.rect.width / _canvasScaler.referenceResolution.x; // Приходится учитывать canvas scaler
			var totalScale = transform.parent.localScale.x / deltaScale;

			Material.SetFloat("_LensX", p.x);
			Material.SetFloat("_LensY", p.y);
			Material.SetFloat("_Power", _power);
			Material.SetFloat("_Radius", _radius * totalScale); //
			Material.SetFloat("_Aspect", 1f * Screen.width / Screen.height);

			Graphics.Blit(source, destination, Material);
		}
	}
}