using Assets.Scripts.Static.Items;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.ControlElements
{
	[RequireComponent(typeof(Image))]
	public class ItemTapeSelector : MonoBehaviour
	{
		[SerializeField] private Sprite moneyTape;
		[SerializeField] private Sprite itemsTape;

		private Image _img;
		private Image IMG
		{
			get
			{
				if (_img) return _img;
				_img = GetComponent<Image>();
				return _img;
			}
		}

		public void Set(ItemCount i, bool setVisibility = true)
		{
			var isMoney = i.Item.IsMoney1;
			var needHide = i.Count <= 1 && i.Item.Value == default;

			IMG.sprite = isMoney ? moneyTape : itemsTape;

			if (setVisibility)
				gameObject.SetActive(!needHide);
		}
	}
}