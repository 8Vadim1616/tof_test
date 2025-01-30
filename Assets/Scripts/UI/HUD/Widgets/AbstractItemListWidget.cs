using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.HUD.Widgets
{
	public abstract class AbstractItemListWidget<TItem, TView> : MonoBehaviour where TView : Component
	{
		private event Action<TView> InstanceCreated;

		[SerializeField] protected RectTransform _content;
		[SerializeField] TMP_Text _caption;
		[SerializeField] TView _itemPrefab;

		public readonly List<TView> Views = new List<TView>();
		public RectTransform GetContent() =>
			_content ? _content : (RectTransform) transform;

		public void Init(string caption, IEnumerable<TItem> items, Action<Transform, TItem> onItemClick = null)
		{
			_caption.text = caption;
			Init(items, onItemClick);
		}

		public void Init(IEnumerable<TItem> items, Action<Transform, TItem> onItemClick = null)
		{
			if (!this)
				return;

			if (!Views.IsNullOrEmpty())
			{
				Views.ForEach(x => Destroy(x.gameObject));
				Views.Clear();
			}

			if (!items.IsNullOrEmpty())
			{
				_itemPrefab.SetActive(true);

				var container = GetContent();
				foreach (var item in items)
				{
					var view = Instantiate(_itemPrefab, container);
					OnViewCreate(view, item);
					InstanceCreated?.Invoke(view);
					Views.Add(view);

					if (onItemClick != null)
					{
						var button = view.gameObject.GetOrAddComponent<BasicButton>();
						button.SetOnClick(() => onItemClick(button.transform, item));
					}
				}
			}

			_itemPrefab.SetActive(false);

			OnInited();
		}

		protected virtual void OnInited()
		{

		}

		public void SubscribeInstance(Action<TView> action)
		{
			InstanceCreated = action;
		}

		public void SubscribeClear()
		{
			InstanceCreated = null;
		}

		protected abstract void OnViewCreate(TView view, TItem item);
	}
}