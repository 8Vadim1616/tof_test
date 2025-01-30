using UnityEngine;

namespace Assets.Scripts.UI.Utils
{
	public class ScaleToFitScreenAnyCam : MonoBehaviour
	{
		private const float CAM_DIST = 10f;

		private struct ResizeBounds
		{
			public float WidthCollider;
			public float HeightCollider;
			public float WidthWorldNeed;
			public float HeightWorldNeed;

			public ResizeBounds(float widthCollider, float heightCollider, float widthWorldNeed, float heightWorldNeed)
			{
				WidthCollider = widthCollider;
				HeightCollider = heightCollider;
				WidthWorldNeed = widthWorldNeed;
				HeightWorldNeed = heightWorldNeed;
			}
		}

		[SerializeField] public Transform target;
		[SerializeField] public BoxCollider2D sizeCollider;
		[Space]
		[SerializeField] public bool KeepAspectRatio = true;
		[SerializeField] public bool ExpandWithAspect = true;
		[SerializeField] public float addBorder = .2f;

		private void Start()
		{
			Resize();
			Game.Instance.OnScreenResize += Resize;
		}

		private void OnDestroy()
		{
			Game.Instance.OnScreenResize -= Resize;
		}

		private void Resize()
		{
			var calc = GetResizeBounds();

			var width = calc.WidthCollider;
			var height = calc.HeightCollider;

			var worldScreenHeight = calc.HeightWorldNeed;
			var worldScreenWidth = calc.WidthWorldNeed;

			Vector3 scaleFactor;
			if (!KeepAspectRatio)
			{
				var scaleY = height != 0 ? worldScreenHeight / height : 0;
				var scaleX = width != 0 ? worldScreenWidth / width : 0;
				scaleFactor = new Vector3(scaleX, scaleY, 1);
			}
			else
			{
				var aspectCam = worldScreenWidth / worldScreenHeight;
				var aspectTarget = width / height;

				if (ExpandWithAspect && aspectCam >= aspectTarget || !ExpandWithAspect && aspectCam < aspectTarget)
				{
					// скейл по ширине
					var s = worldScreenWidth / width;
					scaleFactor = new Vector3(s, s, 1);
				}
				else
				{
					// скейл по высоте
					var s = worldScreenHeight / height;
					scaleFactor = new Vector3(s, s, 1);
				}
			}

			Debug.Log($"ScaleToFit {gameObject.name} resized, size collider: {calc.WidthCollider} {calc.HeightCollider}\n" +
					  $"sizeTargetWorld: {calc.WidthWorldNeed} {calc.HeightWorldNeed}\n" +
					  $"scaleFactor: {scaleFactor}, camAspect: {worldScreenWidth / worldScreenHeight}");

			target.localScale = scaleFactor;
		}

		private ResizeBounds GetResizeBounds()
		{
			var camera = Game.MainCamera;

			var bounds = sizeCollider.bounds;

			var width = bounds.size.x / target.transform.localScale.x;
			var height = bounds.size.y / target.transform.localScale.y;

			var camZero = camera.ViewportToWorldPoint(new Vector3(0, 0, CAM_DIST));
			var camOne = camera.ViewportToWorldPoint(new Vector3(1, 1, CAM_DIST));

			var worldScreenHeight = (camOne.y - camZero.y) + addBorder;
			var worldScreenWidth = (camOne.x - camZero.x) + addBorder;

			return new ResizeBounds(width, height, worldScreenWidth, worldScreenHeight);
		}

	}
}