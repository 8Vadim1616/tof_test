using System;
using System.Linq;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.UI.ControlElements
{
	[Serializable]
	public class TabPages
	{
		[Serializable]
		public class Tab
		{
			public BasicButton Button;
			public GameObject Page;
		}

		[SerializeField] Tab[] _tabs;
		[SerializeField] int _defaultTabIndex;

		public delegate void ButtonSelectFunction(BasicButton button, bool isSelected, int index);
		public delegate void PageSelectFunction(GameObject page, bool isSelected, int index);
		public ButtonSelectFunction ButtonSelect { get; set; }
		public PageSelectFunction PageSelect { get; set; }
		public Tab[] Tabs => _tabs;

		public int DefaultIndex
		{
			get => _defaultTabIndex;
			set => _defaultTabIndex = value;
		}

		/// <summary>Типа конструктор (обязательный метод при инициализации View)</summary>
		/// <param name="buttonSelect">Функция состояния табулятора. По умолчанию - Button.SetLock</param>
		/// <param name="pageSelect">Функция состояния страницы. По умолчанию - GameObject.SetActive</param>
		/// <param name="needSelect">Нужно ли вызывать buttonSelect при инициализации</param>
		public void Init(ButtonSelectFunction buttonSelect = null, PageSelectFunction pageSelect = null, bool needSelect = true)
		{
			ButtonSelect = buttonSelect ?? SetButtonSelected;
			PageSelect = pageSelect ?? SetPageSelected;

			_tabs.ForEach(x => x.Button.SetOnClick(() => OnTabSelected(x)));

			if (needSelect)
				OnTabSelected(DefaultIndex);
		}

		public void SelectTab(int index)
		{
			OnTabSelected(index);
		}

		private void OnTabSelected(int index)
		{
			OnTabSelected(_tabs.ElementAtOrDefault(index));
		}

		private void OnTabSelected(Tab tab)
		{
			var index = 0;

			_tabs.ForEach(x =>
			{
				var isSelected = x == tab;
				PageSelect(x.Page, isSelected, index);
				ButtonSelect(x.Button, isSelected, index);
				index++;
			});
		}

		private void SetButtonSelected(BasicButton button, bool isSelected, int index) => button.SetLock(isSelected);

		private void SetPageSelected(GameObject gameObject, bool isSelected, int index)
		{
			if (gameObject)
				gameObject.SetActive(isSelected);
		}
	}
}