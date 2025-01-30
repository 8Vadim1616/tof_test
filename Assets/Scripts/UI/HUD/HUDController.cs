using System;
using System.Collections.Generic;
using Assets.Scripts.Core;
using Assets.Scripts.UI.General;
using Assets.Scripts.Utils;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.HUD
{
	public class HUDController : MonoBehaviour
    {
		public const float HUD_HIDE_TIME = .5f;
		private const string HIDE_DEF_KEY = "def";
		
        [SerializeField] private Canvas canvas;
        [SerializeField] private HUDContent contentPrefab;
		[SerializeField] private Transform hudHolder;
        [SerializeField] private CanvasGroup canvasGroup;
		
        [Tooltip("Не делает отступ на мобилках")]
        [SerializeField] private RectTransform hudTopLayerNotSafe;
        [SerializeField] private RectTransform hudTopLayer;
        [SerializeField] private RectTransform hudHintsLayer;

        public RectTransform HudTopLayerNotSafe => hudTopLayerNotSafe;
        public RectTransform HudTopLayer => hudTopLayer;
        public RectTransform HudHintsLayer => hudHintsLayer;

        public Locker Locker;
		public Loader Loader;
        public DropController DropController { get; private set; }

		public HUDContent Content { get; private set; }
		protected bool initOnce = false;
		
		private bool _isShowed = true;
		private readonly List<string> _hideKeys = new List<string>();

		public void InitHud()
        {
            if (initOnce)
				return;

            initOnce = true;
            Content = Instantiate(contentPrefab, hudHolder);
			DropController = new DropController(this);
			
			Game.Windows.CurrentScreen.Subscribe(x =>
			{
				if (x == null || !x.HideHudAll)
				{
					Show();
				}
				else if (x.HideHudAll)
				{
					Hide();
				}
			}).AddTo(Content);
		}

        private IDisposable _undoSub;

		public void Hide(string key = HIDE_DEF_KEY)
		{
			_hideKeys.AddOnce(key);
			UpdateVisible();
		}

		public void Show(string key = HIDE_DEF_KEY)
		{
			_hideKeys.Remove(key);
			UpdateVisible();
		}
		
		private Tween _hudTween;

		private void UpdateVisible()
		{
			var needShow = _hideKeys.Count == 0;
			if (_isShowed != needShow)
			{
				_isShowed = needShow;
                
				if(needShow)
					gameObject.SetActive(true);
                
				_hudTween?.Kill();
				_hudTween = TweenHudAlpha(_isShowed ? 1 : 0)
							   .OnComplete(() =>
								{
									if(gameObject && !needShow)
										gameObject.SetActive(false);
								});
			}
		}
		
		private Tween TweenHudAlpha(float endVal)
		{
			Content.CanvasGroup.interactable = endVal >= 1;
			return Content.CanvasGroup.DOFade(endVal, HUD_HIDE_TIME);
		}

		public void Free()
		{
			DropController.Free();

			if (Content)
				Destroy(Content.gameObject);

			HudTopLayerNotSafe.DestroyAllChildren();
			HudTopLayer.DestroyAllChildren();

			initOnce = false;

			Content = null;
		}
	}
}