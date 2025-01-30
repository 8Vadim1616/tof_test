using UnityEngine;

namespace Assets.Scripts.Utils
{
    [RequireComponent(typeof(RectTransform))]
    public class ImageAnchorsScaler : MonoBehaviour
    {
        private bool initialized = false;
        private RectTransform rectTransform;
        public bool includeSelf;

        private void Init()
        {
            if(initialized) return;
            initialized = true;

            rectTransform = GetComponent<RectTransform>();
        }

        public void SetupAnchors()
        {
            Init();

            var transforms = GetComponentsInChildren<RectTransform>();

            foreach (var t in transforms)
            {
                if (t == rectTransform && !includeSelf) continue;
                SetupRect(t);
            }
        }

        private void SetupRect(RectTransform t)
        {
            if (t.parent == null) return;
            if (t.parent.transform == null) return;
            if (!(t.parent.transform is RectTransform parent)) return;

            var min = t.rect.min;
            var max = t.rect.max;

            var localVec2 = new Vector2(t.localPosition.x, t.localPosition.y);

            var minLocal = localVec2 + min;
            var maxLocal = localVec2 + max;

            var minCorner = parent.rect.min;
            var maxCorner = parent.rect.max;

            var minDistance = minLocal - minCorner;
            var maxDistance = maxCorner - maxLocal;

            minDistance /= parent.rect.size;
            maxDistance /= parent.rect.size;
            maxDistance = Vector2.one - maxDistance;

            //t.SetParent(null);

            t.anchorMin = minDistance;
            t.anchorMax = maxDistance;
        
            //t.SetParent(parent);

            t.offsetMin = Vector3.zero;
            t.offsetMax = Vector3.zero;
        }
    }
}
