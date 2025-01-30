using System;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
	public class PlayfieldResizeComponent : MonoBehaviour
	{
		private const int WIDTH = 11;
		private const int HEIGHT = 15;

		protected const float BORDER_WIDTH = 0.1f;
		protected const float HUD_TOP_HEIGHT = 3f;
		
		protected Vector3 _camZero;
		protected Vector3 _camOne;
		protected float _bannerHeight;
		protected float _hudBottomHeight;

		protected float _worldScreenHeight;
		protected float _worldScreenWidth;
		
		private void Awake()
		{
			Game.Instance.OnScreenResize += OnScreenResize;
			OnScreenResize();
		}

		private void OnDestroy()
		{
			if (Game.Instance)
				Game.Instance.OnScreenResize -= OnScreenResize;
		}

		private float GetBannerHeight()
		{
			return 0;
		}

		public virtual void OnScreenResize()
		{
			if (!this || !transform || !Game.MainCamera)
				return;

			_camZero = Game.MainCamera.ViewportToWorldPoint(new Vector3(0, 0, -Game.MainCamera.transform.position.z));
			_camOne = Game.MainCamera.ViewportToWorldPoint(new Vector3(1, 1, -Game.MainCamera.transform.position.z));
			_bannerHeight = GetBannerHeight();
			_hudBottomHeight = 2.5f + _bannerHeight;

			_worldScreenHeight = (_camOne.y - _camZero.y) - BORDER_WIDTH - _bannerHeight;
			_worldScreenWidth = (_camOne.x - _camZero.x) - BORDER_WIDTH;

			float scaleByWidth = _worldScreenWidth / WIDTH;
			float scaleByHeight = _worldScreenHeight / (HEIGHT + HUD_TOP_HEIGHT + _hudBottomHeight);
			transform.localScale = Vector3.one * Math.Min(scaleByWidth, scaleByHeight);

			// transform.localPosition = new Vector3
			// 				(
			// 				 (_worldScreenWidth - (WIDTH - 1) * transform.localScale.x - _worldScreenWidth) * .5f,
			// 				 (_worldScreenHeight - (HEIGHT - 1) * transform.localScale.x
			// 									 - _worldScreenHeight - (HUD_TOP_HEIGHT - _hudBottomHeight)) * .5f,
			// 				 0f
			// 				);
		}
	}
}