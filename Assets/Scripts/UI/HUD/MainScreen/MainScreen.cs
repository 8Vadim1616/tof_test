using System.Collections.Generic;
using Assets.Scripts.UI.HUD.Panels;
using Assets.Scripts.Utils;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.HUD.MainScreen
{
	public class MainScreen : MonoBehaviour
	{
		private const int TAB_SHOP = 0;
		private const int TAB_CARDS = 1;
		private const int TAB_MAIN = 2;
		private const int TAB_BOOSTS = 3;
		private const int TAB_LOCK = 4;
		
		public Money1Panel Money1Panel;
		public Money2Panel Money2Panel;
		
		[SerializeField] private List<MainScreenTab> _tabs;
		[SerializeField] private List<MainScreenBase> _screens;
		
		public ReactiveProperty<MainScreenBase> CurrentScreen { get; } = new ();

		private void Awake()
		{
			for (var tabIndex = 0; tabIndex < _tabs.Count; tabIndex++)
			{
				var t = tabIndex;
				_tabs[tabIndex].SetTitle($"main_tab_name_{tabIndex}".Localize());
				_tabs[tabIndex].SetOnClick(() => SelectTab(t, true));
			}

			SelectTab(TAB_MAIN, false);
		}

		private void ShowTab(int index)
		{
			foreach (var s in _screens)
			{
				if (_screens[index] == s)
				{
					s.Show();
					CurrentScreen.Value = s;
				}
				else
					s.Hide();
			}
		}

		private void SelectTab(int tabIndex, bool anim)
		{
			ShowTab(tabIndex);
			
			foreach (var t in _tabs)
			{
				if (t == _tabs[tabIndex])
					t.Select(anim);
				else
					t.Unselect(anim);
			}
		}
	}
}