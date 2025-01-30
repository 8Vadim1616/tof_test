using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core.AssetsManager;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Utils;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Assets.Scripts.UI
{
    public static class IconPlacer
    {
        public static Image SpawnIcon(Image i, Transform t = null)
        {
            if (i == null)
            {
                Debug.LogError("No image");
            }

            var gameObject = new GameObject();
            var image = gameObject.AddComponent<Image>();
            image.sprite = i?.sprite;
            gameObject.transform.SetParent(t);
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(128, 128);
            return image;
        }
        
        public static IPromise<Image> SpawnIcon(Item i, Transform t = null)
        {
            var result = new Promise<Image>();

            if (i == null)
            {
                Debug.LogError("No item");
            }

            var gameObject = new GameObject();
            var image = gameObject.AddComponent<Image>();
            gameObject.transform.SetParent(t);
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(128, 128);

            image.LoadItemImage(i).Then(
                sprite =>
                {
                    if (!image) return;
                    image.preserveAspect = true;
                    result.Resolve(image);
                }
            );

            return result;
        }

        public static IPromise<Sprite> LoadFromAssets(this RectTransform rect, string path)
        {
            var img = rect.gameObject.GetComponent<Image>();
            if (!img)
                img = rect.gameObject.AddComponent<Image>();

            return img.LoadFromAssets(path);
        }
		
		public static IPromise<Sprite> LoadFromAssets(this SpriteRenderer img, string path, bool preserveAspect = true,
													  bool hideIfNull = true, bool needCache = false)
		{
			if (!img)
				return Promise<Sprite>.Rejected(null);

			img.SetColor(a: 0);
			return AssetsManager.Instance.Create<Sprite>(path, needCache: needCache)
								.Then(s =>
								 {
									 if (!img)
										 return;

									 if (!s)
									 {
										 GameLogger.warning("No image " + path);
										 if (!hideIfNull)
											 img.SetColor(a: 1);
										 return;
									 }

									 img.sprite = s;
									 if (s) 
										 img.SetColor(a: 1);
								 });
		}
        
        public static IPromise<Sprite> LoadFromAssets(this Image img, string path, bool preserveAspect = true,
			bool hideIfNull = true, bool needCache = false)
		{
			if (!img)
				return Promise<Sprite>.Rejected(null);

            img.SetColor(a: 0);
            return AssetsManager.Instance.Create<Sprite>(path, needCache: needCache)
				.Then(s =>
				{
					if (!img)
						return;

					if (!s)
					{
						GameLogger.warning("No image " + path);
						if (!hideIfNull)
							img.SetColor(a: 1);
						return;
					}

					img.preserveAspect = preserveAspect;
					img.sprite = s;
					if (s) 
						img.SetColor(a: 1);
				});
        }

        public static IPromise LoadTexture(this Image img, string url, bool useCache = true, bool setBackToAlpha = true)
        {
            if (string.IsNullOrEmpty(url)) return Promise.Resolved();
            
            if (setBackToAlpha) img.SetColor(a: 0);

            var result = new Promise();

			LocalFileLoader.GetSprite(url, useCache)
						   .Then(onGetSprite);

			void onGetSprite(Sprite sprite)
            {
				if (!img)
					return;

				img.preserveAspect = true;
				img.sprite = sprite;
                if (setBackToAlpha && img.sprite != null) img.SetColor(a: 1);
                result.Resolve();
            }

            return result;
        }

        public static void LoadAndInstantiateSpriteInsideTransform(this Transform t, string path)
        {
            AssetsManager.Instance.Loader.LoadAndCache<Sprite>(path)
                .Then(x => InstantiateSpriteInsideTransform(t, x));
        }

        public static RectTransform InstantiateSpriteInsideTransform(this Transform t, Sprite s)
        {
            if (!t || s == null)
                return null;

            var g = new GameObject(s.name);
            g.transform.SetParent(t);
            g.transform.localPosition = Vector3.zero;

            var rt = g.AddComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, s.rect.width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, s.rect.height);

            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = s;
            img.preserveAspect = true;

            return rt;
        }

        public static IPromise LoadTexture(this SpriteRenderer spriteRenderer, string url, bool useCache = true,
                                           bool setBackToAlpha = true)
        {
            if (string.IsNullOrEmpty(url)) return Promise.Resolved();

            if (setBackToAlpha) spriteRenderer.SetColor(a: 0);

            var result = new Promise();

			LocalFileLoader.GetSprite(url, useCache)
						   .Then(onGetSprite);

			return result;

            void onGetSprite(Sprite sprite)
            {
                if (!spriteRenderer) return;

                // spriteRenderer.preserveAspect = true;
                spriteRenderer.sprite = sprite;
                if (setBackToAlpha) spriteRenderer.SetColor(a: 1);
                result.Resolve();
            }
		}

        public static Promise<RectTransform> LoadAndInstantiateTexture(this RectTransform t, string url, bool useCache = true)
        {
            var result = new Promise<RectTransform>();
            if (string.IsNullOrEmpty(url))
            {
                result.Reject(new Exception ("LoadAndInstantiateTexture: Empty or null url"));
                return result;
            };

			LocalFileLoader.GetSprite(url, useCache)
						   .Then(onGetSprite);

            void onGetSprite(Sprite s)
            {
                if (!t)
                {
                    result.Resolve(null);
                    return;
                }

                var g = new GameObject(url);
                g.transform.SetParent(t);
                g.transform.localPosition = Vector3.zero;

                var rt = g.AddComponent<RectTransform>();
                rt.localScale = Vector3.one;
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, s.rect.width);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, s.rect.height);

                var img = rt.gameObject.AddComponent<Image>();
                img.sprite = s;

                result.Resolve(rt);
            }

            return result;
        }

        public static IPromise<Sprite> LoadItemImage(this Image img, Item i, bool needCache = false)
        {
            return LoadFromAssets(img, i.IconPath, needCache: needCache);
		}

		// Выпрямляет иерархию обьекта со спрайтрендерерами и сортирует их по их отображению
		public static void FixPositionsInSpriteRendererers(GameObject parent, bool changeInactive = true)
        {
            var srs = parent.GetComponentsInChildren<SpriteRenderer>();
            var target = changeInactive? srs.Where(x => x.gameObject.activeSelf).ToList() : srs.ToList();

            target.Sort((x, y) =>
            {
                if (x == null || y == null || x == y) return 0;

                var firstLayer = SortingLayer.GetLayerValueFromID(x.sortingLayerID);
                var secondLayer = SortingLayer.GetLayerValueFromID(y.sortingLayerID);

                if (firstLayer != secondLayer)
                {
                    return firstLayer < secondLayer ? -1 : 1;
                }

                if (x.sortingOrder != y.sortingOrder)
                {
                    return x.sortingOrder < y.sortingOrder ? -1 : 1;
                }

                // Сортировка по иерархии
                var pX = new List<Transform>();
                var pY = new List<Transform>();

                var p = x.transform;
                while (p != null)
                {
                    if (p == y.transform)
                        return 1;

                    pX.Add(p);
                    p = p.parent;
                }

                p = y.transform;
                while (p != null)
                {
                    if (p == x.transform)
                        return -1;

                    pY.Add(p);
                    p = p.parent;
                }

                pX.Reverse();
                pY.Reverse();

                // нет общего предка - надо смотреть иерархию на сцене но я хз как TODO
                if (pX[0] != pY[0]) return 0;

                var i = 0;
                var minCount = Mathf.Min(pX.Count, pY.Count);
                while (pX[i] == pY[i] && i < minCount)
                {
                    i++;
                }

                // Одно из изображений содержит в потомках другого (по идее мы должны были это исключить выше,
                // когда заполняли массивы pX, pY. Но пусть будет - можно там выше убрать.
                if (i >= minCount)
                    return pX.Count - pY.Count;

                // не проверяем конца списка и нашло ли разные элементы потому что они обязательно будут тк. 
                // исходные обьекты включены в список и находятся в его конце
                return pX[i].GetSiblingIndex() - pY[i].GetSiblingIndex(); 
            });

            foreach (var s in target)
            {
                s.transform.SetParent(parent.transform);
                s.transform.SetAsLastSibling();
            }
        }

        // Первый рекурсивный способ, не ставит обьекты в центр в данный момент
        [Obsolete]
        public static RectTransform TransformSpriteRenderersToImages(this GameObject target, Transform parent)
        {
            if (!target.activeSelf) return null;

            var sr = target.GetComponent<SpriteRenderer>();

            var clone = new GameObject(target.name);

            var rectOriginal = target.GetOrAddComponent<RectTransform>();

            clone.transform.parent = parent;
            clone.transform.localPosition = target.transform.localPosition;
            clone.transform.localScale = target.transform.localScale;

            var rect = clone.AddComponent<RectTransform>();
            rect.anchorMax = Vector2.one / 2;
            rect.anchorMin = rect.anchorMax;
            rect.sizeDelta = rectOriginal.sizeDelta;

            if (sr && rectOriginal)
            {
                clone.AddComponent<Image>().sprite = sr.sprite;
                //rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, sr.size.x);
                //rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sr.size.y);
            }

            float maxX = 0;
            float maxY = 0;

            float minX = 0;
            float minY = 0;

            var notEmpty = 0;
            target.transform.ForeachChild(x =>
            {
                var rt = TransformSpriteRenderersToImages(x.gameObject, clone.transform);

                if (rt != null)
                {
                    notEmpty += 1;

                    maxX = Mathf.Max(rt.rect.xMax + rt.localPosition.x, maxX);
                    maxY = Mathf.Max(rt.rect.yMax + rt.localPosition.y, maxY);

                    minX = Mathf.Min(rt.rect.xMin + rt.localPosition.x, minX);
                    minY = Mathf.Min(rt.rect.yMin + rt.localPosition.y, minY);
                }
            });

            if (!sr && notEmpty == 0)
            {
                Object.Destroy(clone);
                return null;
            }

            if (!sr && notEmpty > 0)
            {
                var maxHeight = Mathf.Max(maxY, -minY);
                var maxWidth = Mathf.Max(maxX, -maxX);
                rect.sizeDelta = new Vector2(maxX - minX, maxY - minY);
            }

            return rect;
        }

        private static RectTransform CloneAndAddImage(GameObject target, Transform parent)
        {
            var sr = target.GetComponent<SpriteRenderer>();
            var clone = new GameObject(target.name);
            var rectOriginal = target.GetOrAddComponent<RectTransform>();

            clone.transform.parent = parent;
            clone.transform.localPosition = target.transform.localPosition;
            clone.transform.localScale = target.transform.localScale;

            var rect = clone.AddComponent<RectTransform>();
            rect.anchorMax = Vector2.one / 2;
            rect.anchorMin = rect.anchorMax;
            rect.sizeDelta = rectOriginal.sizeDelta;

            if (sr) clone.AddComponent<Image>().sprite = sr.sprite;

            return rect;
        }

        public static RectTransform TransformSpriteRenderersToImagesFlat(this GameObject target, Transform parent)
        {
            if (!target.activeSelf) return null;

            var cloneRect = CloneAndAddImage(target.gameObject, parent);

            float maxX = 0;
            float maxY = 0;

            float minX = 0;
            float minY = 0;

            var notEmpty = 0;
            target.transform.ForeachChild(x =>
            {
                var sr = x.GetComponent<SpriteRenderer>();
                if (!x || !x.gameObject.activeSelf|| sr == null) return;

                var rt = CloneAndAddImage(x.gameObject, cloneRect);

                notEmpty += 1;

                var halfWidth = rt.sizeDelta.x * Mathf.Abs(rt.localScale.x / 2);
                var halfHeight = rt.sizeDelta.y * Mathf.Abs(rt.localScale.y / 2);

                maxX = Mathf.Max(rt.localPosition.x + halfWidth, maxX);
                maxY = Mathf.Max(rt.localPosition.y + halfHeight, maxY);

                minX = Mathf.Min( rt.localPosition.x - halfWidth, minX);
                minY = Mathf.Min( rt.localPosition.y - halfHeight, minY);

            });

            var img = cloneRect.gameObject.GetComponent<Image>();
            if (notEmpty > 0 && !img)
            {
                cloneRect.sizeDelta = new Vector2(maxX - minX, maxY - minY);

                var centerNew = new Vector2((maxX + minX) / 2, (maxY + minY) / 2 );
                
                cloneRect.ForeachChild(x =>
                {
                    if (x) x.localPosition = x.localPosition.Add(-centerNew.x, -centerNew.y);
                });
            }

            return cloneRect;
        }
	}
}
