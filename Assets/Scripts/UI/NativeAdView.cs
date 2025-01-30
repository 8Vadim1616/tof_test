using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.UI.Utils;
using Assets.Scripts.Utils;
using Platform.Mobile.Advertising;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace UI.Screens
{
    public class NativeAdView : MonoBehaviour
    {
        [SerializeField] private RectTransform _targetRect;
        
        [SerializeField] protected RawImage adIcon;
        [SerializeField] protected RawImage adChoices;
        [SerializeField] protected TMP_Text adHeadline;
        [SerializeField] protected TMP_Text adAdvertiser;
        [SerializeField] protected TMP_Text adCallToAction;
        [SerializeField] protected TMP_Text adBodyText;
        [SerializeField] protected TMP_Text sponsoredText;

        private List<BoxCollider> _colliders;
        private bool _inited;
		public bool IsInited => _inited;
        
        public virtual void Init(NativeAdWrapper nativeAd)
        {
            if (_inited)
                return;
            
            _colliders = GetComponentsInChildren<BoxCollider>().ToList();
            
            if (!_targetRect)
                _targetRect = transform as RectTransform;
            
            Texture2D iconTexture = nativeAd.GetIconTexture();
            Texture2D iconAdChoices = nativeAd.GetAdChoicesLogoTexture();
            string headline = nativeAd.GetHeadlineText();
            string cta = nativeAd.GetCallToActionText();
            string advertiser = nativeAd.GetAdvertiserText();
            string bodyText = nativeAd.GetBodyText();
            
            adIcon.texture = iconTexture;
            adChoices.texture = iconAdChoices;
            adHeadline.text = headline;
            if (adAdvertiser)
                adAdvertiser.text = advertiser;
            adCallToAction.text = cta;
            if (adBodyText)
                adBodyText.text = bodyText;

            //register gameobjects
            nativeAd.RegisterIconImageGameObject (adIcon.gameObject);
            nativeAd.RegisterAdChoicesLogoGameObject (adChoices.gameObject);
            nativeAd.RegisterHeadlineTextGameObject (adHeadline.gameObject);
            nativeAd.RegisterCallToActionGameObject (adCallToAction.gameObject);
            
            if (adBodyText)
                nativeAd.RegisterBodyTextGameObject(adBodyText.gameObject);
            
            if (adAdvertiser)
                nativeAd.RegisterAdvertiserTextGameObject (adAdvertiser.gameObject);

            if (sponsoredText)
                sponsoredText.text = "sponsored".Localize();

            _inited = true;
        }
        
        public bool IsAnyCornerOverlappingUI(RectTransform rectTransform)
        {
            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);

            var allPoints = worldCorners.ToList();
            allPoints.Add(rectTransform.position);

            foreach (Vector3 corner in allPoints)
            {
                if (IsPointerOverUIObject(corner.toVector2()))
                    return true;
            }

            return false;
        }
        
        public bool IsPointerOverUIObject(Vector2 screenPosition)
        {
            var screenPoint = Game.MainCamera.WorldToScreenPoint(screenPosition);
			//screenPoint = screenPoint.SetUI(z: 0f);
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = screenPoint;
            
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            if (raycastResults.Count == 0)
                return false;

            for (var i = 0; i < raycastResults.Count; i++)
            {
                var raycastNativeAd = raycastResults[i].gameObject.GetComponentInParents<NativeAdView>();
                if (raycastNativeAd && raycastNativeAd == this)
                    return false;
                
                const int UINonBlockingLayer = 8;
                    if (raycastResults[i].gameObject.layer != UINonBlockingLayer)
                        return true;
            }

            return false;
        }

        private bool _lastOverlaps;

        void Update()
		{
            if (!_inited)
                return;
            
            if (IsAnyCornerOverlappingUI(_targetRect))
            {
                if (!_lastOverlaps)
                {
                    _lastOverlaps = true;
                    _colliders.Each(c => c.enabled = false);
                    Debug.Log("!!!перекрыт");
                }
            }
            else
            {
                if (_lastOverlaps)
                {
                    _lastOverlaps = false;
                    _colliders.Each(c => c.enabled = true);
                    Debug.Log("!!!не перекрыт");
                }
            }
        }
    }
}