using Assets.Scripts;
using Assets.Scripts.UI.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scripts.UI.General
{
    public class VisibilityController : MonoBehaviour
    {
        public VisibilityControlTarget Target { get; private set; }
        private bool Inited = false;
        
        private ScrollRect myScroll;
        private RectTransform viewPortRect;
        private Rect rectScroll;
        
        public void Init(VisibilityControlTarget target, ScrollRect scrollRect)
        {
            myScroll = scrollRect;

            Inited = true;
            Target = target;
            Target.Transform.hasChanged = false;

            if (myScroll)
                myScroll.onValueChanged.AddListener(Check);
        }

        public void Check(Vector2 vec = default)
        {
            if (!gameObject) return;
            if (!gameObject.activeSelf) return;
            
            bool nowTargetIsVisible = false;

            if (myScroll)
            {
                nowTargetIsVisible = CheckIsVisible();
            }
            else
            {
                nowTargetIsVisible = Target.Transform.IsVisible();
                if (nowTargetIsVisible == Target.IsVisible()) return;
            }

            if (nowTargetIsVisible)
                Target.OnVisible();
            else
                Target.OnInvisible();
        }
        
        private void FixedUpdate()
        {
            if (!Inited) return;
            if (!Target.Transform.hasChanged) return;
            Target.Transform.hasChanged = false;

            Check();
        }
        
        private bool CheckIsVisible()
        {
            return (myScroll.transform as RectTransform).Overlaps(Target.Transform);
        }
    }

    public interface VisibilityControlTarget
    {
        bool IsVisible();
        void OnVisible();
        void OnInvisible();
        RectTransform Transform { get; }
    }
    
    public static class RendererExtensions
    {
        private static Rect screenBounds;
        private static Vector3[] objectCorners = new Vector3[4];
        
        public static bool IsVisible(this RectTransform rectTransform)
        {
            if (!rectTransform.parent) return false;
            
            if (screenBounds == default)
                screenBounds = new Rect(0f, 0f, Screen.width, Screen.height);
            
            rectTransform.GetWorldCorners(objectCorners);
     
            for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
            {
                Vector3 tempScreenSpaceCorner = Game.MainCamera.WorldToScreenPoint(objectCorners[i]); // Transform world space position of corner to screen space
                if (screenBounds.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
                    return true;
            }
            return false;
        }
        
        public static bool IsVisibleIn(this RectTransform rectTransform, RectTransform checkTransform)
        {
            if (!rectTransform.parent) return false;
            
            Rect bounds = checkTransform.ToScreenSpaceRect();
            
            rectTransform.GetWorldCorners(objectCorners);
            
            for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
            {
                Vector3 tempScreenSpaceCorner = Game.MainCamera.WorldToScreenPoint(objectCorners[i]); // Transform world space position of corner to screen space
                if (bounds.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
                    return true;
            }
            return false;
        }

        public static Rect ToScreenSpaceRect(this RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            Vector2 pos = Game.MainCamera.WorldToScreenPoint(corners[0]);
            Vector2 size = (Vector2) Game.MainCamera.WorldToScreenPoint(corners[2]) - pos;
            return new Rect(pos, size);
        }
        
        public static bool Overlaps(this RectTransform a, RectTransform b) {
            return a.WorldRect2().Overlaps(b.WorldRect2());
        }
        public static bool Overlaps(this RectTransform a, RectTransform b, bool allowInverse) {
            return a.WorldRect2().Overlaps(b.WorldRect2(), allowInverse);
        }

        public static Rect WorldRect(this RectTransform rectTransform) {
            Vector2 sizeDelta = rectTransform.sizeDelta;
            float rectTransformWidth = sizeDelta.x * rectTransform.lossyScale.x;
            float rectTransformHeight = sizeDelta.y * rectTransform.lossyScale.y;

            Vector3 position = rectTransform.position;
            return new Rect(position.x - rectTransformWidth / 2f, position.y - rectTransformHeight / 2f, rectTransformWidth, rectTransformHeight);
        }

		public static Rect WorldRectReal(this RectTransform rectTransform)
		{
			var r = rectTransform.rect;
			var width = r.width * rectTransform.lossyScale.x;
			var height = r.height * rectTransform.lossyScale.y;
			return new Rect(rectTransform.position.x - width / 2, rectTransform.position.y - height / 2, width, height);
		}


		public static Rect WorldRect2(this RectTransform rectTransform)
        {
            Vector2 sizeDelta = rectTransform.sizeDelta;
            float rectTransformWidth = rectTransform.rect.width * rectTransform.lossyScale.x;
            float rectTransformHeight = rectTransform.rect.height * rectTransform.lossyScale.y;

            Vector3 position = rectTransform.position;
            return new Rect(position.x - rectTransformWidth / 2f, position.y - rectTransformHeight / 2f, rectTransformWidth, rectTransformHeight);
        }
    }
}