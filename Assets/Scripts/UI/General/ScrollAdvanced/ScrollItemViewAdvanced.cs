using UnityEngine;

namespace Scripts.UI.General
{
    /// <summary>
    /// Просто RectTransform, по которому опредялем видимость и перерисовывем контент
    /// </summary>
    public class ScrollItemViewAdvanced : MonoBehaviour, VisibilityControlTarget
    {
        [SerializeField] private RectTransform container;
        
        public object Data { get; private set; }
        public ScrollViewAdvanced Scroll { get; private set; }

        public ScrollItemViewContent Content { get; private set; }
        private VisibilityController VisibilityController;

        private bool isVisible = false;

        public void Init(ScrollViewAdvanced scroll, object data)
        {
            Data = data;
            Scroll = scroll;
            
            VisibilityController = gameObject.AddComponent<VisibilityController>();
            VisibilityController.Init(this, scroll.ScrollRect);
            Check();
        }

        public void Check()
        {
            if (!VisibilityController) return;
            VisibilityController.Check();
        }

        public bool IsVisible()
        {
            return isVisible;
        }

        public void OnVisible()
        {
            if (isVisible) return;
            if (!Content)
                Content = Scroll.CreateContent(this);

            Content.Data = Data;
            Content.SetData(Data);
            isVisible = true;
        }

        public void OnInvisible()
        {
            if (!isVisible) return;
            if (Content)
                Scroll.OnContentBecameInvisible(Content);
            Content = null;
            isVisible = false;
        }

        public RectTransform Transform => container;
    }
}