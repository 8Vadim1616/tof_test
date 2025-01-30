using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.General
{
    public class Loader : MonoBehaviour
    {
        [SerializeField]
        private Image image;
        [SerializeField]
        private Image back;

        [SerializeField]
        private float AlphaBack = 0.4f;
        
        [SerializeField]
        private float FadeTime = 0.5f;

        public bool IsShowing { get => gameObject.activeSelf; }

        private Tween fadeTween;

        private Button _button;

        private Button Button
        {
            get
            {
                if (_button == null)
                    _button = gameObject.AddComponent<Button>();

                return _button;
            }
        }

        public void Show(Action OnClickAction = null)
        {
            Button.onClick.RemoveAllListeners();
            if (OnClickAction != null)
                Button.onClick.AddListener(() =>
                {
                    OnClickAction();
                    Hide();
                });

            if (gameObject.activeSelf) 
				return;
            gameObject.SetActive(true);
            fadeTween?.Kill();
            fadeTween = back.DOFade(AlphaBack, FadeTime)
                .OnComplete(() => fadeTween = null)
				.SetLink(back.gameObject);
        }

        private void Update()
        {
            image.transform.Rotate(Vector3.forward, -2);
        }

        public void Hide(bool instantly = false)
        {
            if (!gameObject.activeSelf) 
				return;
            Button.onClick.RemoveAllListeners();

            if (instantly)
            {
                gameObject.SetActive(false);
                return;
            }

			if (back == null)
				return;

            fadeTween?.Kill();
            fadeTween = back.DOFade(0, FadeTime)
                .OnComplete(() =>
                {
                    fadeTween = null;
                    gameObject.SetActive(false);
                }).SetLink(back.gameObject);
            ;
        }
    }
}