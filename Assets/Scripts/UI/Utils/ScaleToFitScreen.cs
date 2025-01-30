using System;
using Assets.Scripts.Utils;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.Utils
{
    public class ScaleToFitScreen : MonoBehaviour
    {
        private IDisposable initSub;
        [SerializeField] public Transform target;
        [SerializeField] public BoxCollider2D sizeCollider;
        [SerializeField] public Camera cam;

        [SerializeField] public bool KeepAspectRatio = true;
        [SerializeField] public bool ExpandWithAspect = true;

        private bool lastKeepAspectRatio;
        private bool lastExpandWithAspect;

        private bool ChangedVal => lastExpandWithAspect != ExpandWithAspect || lastKeepAspectRatio != KeepAspectRatio;

		void Awake()
        {
            //initSub = Observable.EveryUpdate().Subscribe(_ => InitOnce()).AddTo(this);

            initSub = ObservableUtils.DoOnceWhenPredicateTrue(() => Game.Instance != null, InitOnce).AddTo(this);

            lastExpandWithAspect = ExpandWithAspect;
            lastKeepAspectRatio = KeepAspectRatio;
        }

        private void InitOnce()
                 {
                     Game.Instance.OnScreenResize += OnScreenResize;
                     OnScreenResize();
                 }

        void OnScreenResize()
        {
            if (!cam)
            {
                cam = Camera.main;
                if (!cam) return;
            }

            var midPoint = cam.ViewportToWorldPoint(new Vector3(0.5f, .5f, 0));
            target.position = midPoint.Set(z: 0);

            var bounds = sizeCollider.bounds;

            var width = bounds.size.x / target.transform.localScale.x;
            var height = bounds.size.y / target.transform.localScale.y;

            var worldScreenHeight = cam.orthographicSize * 2.0f;
            var worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

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

            target.localScale = scaleFactor;
        }

        void OnDestroy()
        {
            if (Game.Instance) Game.Instance.OnScreenResize -= OnScreenResize;
        }

        void Update()
        {
            if (!ChangedVal || initSub != null) return;

            lastExpandWithAspect = ExpandWithAspect;
            lastKeepAspectRatio = KeepAspectRatio;

            OnScreenResize();
        }

    }
}
