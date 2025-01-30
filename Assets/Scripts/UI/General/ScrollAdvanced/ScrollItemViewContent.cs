using Assets.Scripts.UI.Utils;
using UnityEngine;

namespace Scripts.UI.General
{
    /// <summary>
    /// Контент для скролла. При вызове метода SetData будет перерисовано всё
    /// </summary>
    public abstract class ScrollItemViewContent : MonoBehaviour
    {
        public object Data { get; internal set; }

        public abstract void SetData(object data);

        private ScrollViewAdvanced scroll; 
        public ScrollViewAdvanced Scroll
        {
            get
            {
				if (scroll == null)
					scroll = GetComponentInParent<ScrollViewAdvanced>();
                return scroll;
            }
        }
    }
}