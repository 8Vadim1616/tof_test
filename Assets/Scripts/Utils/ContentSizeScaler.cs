using UnityEngine;

namespace Assets.Scripts.UI.Utils
{
    [ExecuteInEditMode]
    public class ContentSizeScaler : MonoBehaviour 
    {
        private  readonly Vector2 center = new Vector2(.5f, .5f);

        [SerializeField] private RectTransform target;
        private RectTransform rect;

        private void OnRectTransformDimensionsChange()
        {
            Validate();
        }

        public void Validate()
        {
            if (target == null) return;
            if (rect == null) rect = GetComponent<RectTransform>();

            target.anchorMin = center;
            target.anchorMax = center;
            target.pivot = center;

            var size = rect.rect;

            if (target.sizeDelta.y == 0 || target.sizeDelta.x == 0) return;
            var aspect = target.sizeDelta.x / target.sizeDelta.y;

            float x = 0;
            float y = 0;

            y = size.height;
            x = y * aspect;

            if (x > size.width)
            {
                x = size.width;
                y = x / aspect;
            }
        
            target.sizeDelta = new Vector2(x, y);
        }

        public void Validate(RectTransform t)
        {
            target = t;
            Validate();

        }
    }
}
