using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.General;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace Scripts.UI.General
{
    public class ItemCountViewWithName : ItemCountView
    {
        [SerializeField]
        private TextMeshProUGUI item_name;
        public TextMeshProUGUI ItemName { get => item_name; }

        public override IPromise<Sprite> SetItem(Item item, string replaceIconPath = null)
        {
            if (item_name)
                item_name.text = item?.Name;
            return base.SetItem(item, replaceIconPath);
        }

		public void DisableItemName()
		{
			item_name.gameObject.SetActive(false);
		}
	}
}