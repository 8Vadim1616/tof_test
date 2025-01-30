using System.Collections.Generic;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.UI.HUD.Panels
{
	public class SomeResourcesPanel : MonoBehaviour
	{
		[SerializeField] private SomeResourcePanel _itemPrefab;

		private List<SomeResourcePanel> _items = new List<SomeResourcePanel>();

		public void Init(IEnumerable<Item> items)
		{
			_itemPrefab.SetActive(true);

			foreach (var item in _items)
				Destroy(item.gameObject);
			_items.Clear();
			
			foreach (var item in items)
			{
				var itemView = Instantiate(_itemPrefab, transform);
				itemView.Init(item);
				_items.Add(itemView);
			}
			
			_itemPrefab.SetActive(false);
		}
	}
}