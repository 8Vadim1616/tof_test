using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.General
{
    public class ScrollViewAdvanced : MonoBehaviour
    {
        [SerializeField] public RectTransform scrollContent;
        
        [SerializeField] private ScrollItemViewContent contentPrefab;
        [SerializeField] private ScrollItemViewAdvanced containerPrefab;
        
        private Queue<ScrollItemViewContent> prefabCache = new Queue<ScrollItemViewContent>();
        public List<ScrollItemViewAdvanced> Items { get; private set; }= new List<ScrollItemViewAdvanced>();

        public object Container { get; private set; }

        public ScrollItemViewAdvanced ContainerPrefab => containerPrefab;
        public ScrollItemViewContent ContentPrefab => contentPrefab;

        public ScrollRect ScrollRect { get; private set; }
        public Action<ScrollItemViewContent> OnCreate { get; set; }

        private void Awake()
        {
            ScrollRect = gameObject.GetComponent<ScrollRect>();
        }

        public void Init(IEnumerable<object> data, object container = null)
        {
            Container = container;
            Clear();

            if (data == null) return;

            var scrollSnapSimple = GetComponent<SimpleScrollSnap>();
            foreach (var obj in data)
            {
                ScrollItemViewAdvanced item;
                if (scrollSnapSimple)
                {
                    item = scrollSnapSimple.AddToBack(containerPrefab.gameObject).GetComponent<ScrollItemViewAdvanced>();
                }
                else
                {
                    item = Instantiate(containerPrefab, scrollContent);
                }
                Items.Add(item);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollContent);
            if (scrollSnapSimple)
            {
                scrollSnapSimple.Start();
            }
            
            var i = 0;
            foreach (var item in data)
            {
                Items[i].Init(this, item);
                i++;
            }
        }

        public void ScrollToItem(int index)
        {
            var target = scrollContent.GetChild(index);
            if (target)
            {
                var position = scrollContent.anchoredPosition;
                var newPosition = (Vector2)ScrollRect.transform.InverseTransformPoint(scrollContent.position)
                    - (Vector2)ScrollRect.transform.InverseTransformPoint(target.position);

                if (ScrollRect.horizontal)
                    position.x = newPosition.x;
                if (ScrollRect.vertical)
                    position.y = newPosition.y;
                scrollContent.anchoredPosition = position;
            }
        }

        private void OnEnable()
        {
            foreach (var item in Items)
                item.Check();
        }

        private void OnDestroy()
        {
            Clear();
        }

        public void Clear()
        {
            var scrollSnapSimple = GetComponent<SimpleScrollSnap>();
            if (scrollSnapSimple)
            {
                while (scrollSnapSimple.NumberOfPanels > 0)
                {
                    scrollSnapSimple.RemoveFromBack();
                }
            }
            foreach (var obj in prefabCache)
            {
                if (obj)
                    Destroy(obj.gameObject);
            }

            foreach (var item in Items)
            {
                if (item)
                    Destroy(item.gameObject);
            }

            prefabCache.Clear();
            Items.Clear();
        }

        public ScrollItemViewContent CreateContent(ScrollItemViewAdvanced container)
        {
            if (!prefabCache.Empty())
            {
                var result = prefabCache.Dequeue();
                result.gameObject.SetActive(true);
                result.transform.SetParent(container.Transform);
                result.transform.localPosition = Vector3.zero;
                result.transform.localScale = Vector3.one;
                return result;
            }

            var instance = Instantiate(contentPrefab, container.Transform);
            OnCreate?.Invoke(instance);

            return instance;
        }

        public void OnContentBecameInvisible(ScrollItemViewContent content)
        {
            content.gameObject.transform.SetParent(null);
            content.gameObject.SetActive(false);
            prefabCache.Enqueue(content);
        }

        private ScrollItemViewContent GetFromCache(object data) { return prefabCache.FirstOrDefault(x => x.Data == data); }
    }
}