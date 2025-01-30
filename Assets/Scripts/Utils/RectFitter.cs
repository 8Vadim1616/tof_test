using UnityEngine;

namespace Assets.Scripts.Utils
{
    /// <summary>
    /// Меняет размер таргетного рект трансформа при этом используя скейл трансформа
    /// </summary>
    public static class RectFitter 
    {
        public static RectTransform FitRectTransformScale(this RectTransform target, RectTransform constraints, float border = 5)
        {
            if (target == null || constraints == null) return target;

            var rect = target.rect;
            var sizeNow = new Vector2(rect.width, rect.height);
            var sizeTarget = new Vector2(
                constraints.rect.width > border ? constraints.rect.width - border : constraints.rect.width,
                constraints.rect.height > border ? constraints.rect.height - border : constraints.rect.height);

            var scales = sizeNow.GetScalesToFit(sizeTarget);
            target.localScale = new Vector3(scales.x, scales.y);
            target.localPosition = new Vector3(
                (target.pivot.x - 0.5f) * rect.width,
                (target.pivot.y - 0.5f) * rect.height, 0);

            return target;
        }
    }
}
