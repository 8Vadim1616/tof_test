using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.General
{
	public class ItemDecriptionView : MonoBehaviour
    {
		protected const string BASE_PATH = "img/";

        public Image icon;
        public TextMeshProUGUI description;

        public bool HideIfNull = false;

        protected Item _item;
		protected string _replaceIconPath;

        public Item Item
        {
            get => _item;
            set => SetItem(value);
        }

        public virtual IPromise<Sprite> SetItem(Item item, string replaceIconPath = null)
        {
            Promise<Sprite> promise = new Promise<Sprite>();

			if (_item != item)
				_replaceIconPath = replaceIconPath;

            if (item == null)
            {
                if (icon && HideIfNull)
                    icon.SetActive(false);

				description.text = "";

				promise.Resolve(null);
            }
            else
            {
                if (icon)
                {
                    icon.SetActive(true);
                    if (_item != item)
                    {
                        var wasAlphaIcon = icon.color.a;
						IPromise<Sprite> loadPromise = null;

						if (_replaceIconPath != null)
							loadPromise = icon.LoadFromAssets(BASE_PATH + _replaceIconPath);
						else
							loadPromise = icon.LoadItemImage(item);

						loadPromise
                            .Then(sprite =>
                            {
                                promise.Resolve(sprite);

								if (icon)
									icon.color = icon.color.Set(a: wasAlphaIcon);
							});
                    }
                    else
                    {
                        promise.Resolve(icon.sprite);
                    }
                }

				description.text = item.GetDescription();
			}

            _item = item;
            return promise;
        }

        //public void ForceCountRebuildLayout()
        //{
        //    if (description.transform.parent is RectTransform rectTransform)
        //        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        //}

		//public void RemoveReplaceIcon() => _replaceIconPath = null;
    }
}