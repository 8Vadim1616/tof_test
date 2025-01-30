using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Static.Units;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.Windows.Units;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.HUD.MainScreen
{
	public class UnitsScreen : MainScreenBase
	{
		enum SortDirection
		{
			UP,
			DOWN
		}
		
		enum SortType
		{
			TIER,
			LEVEL
		}

		enum Bookmark
		{
			BOOKMARK_ALL,
			BOOKMARK_MYTHIC
		}

		[SerializeField] private ScrollRect _scroll;
		[SerializeField] private ButtonText _bookmarkAllBtn;
		[SerializeField] private ButtonText _bookmarkMythicBtn;
		[SerializeField] private ButtonTextIcon _sortTierBtn;
		[SerializeField] private ButtonTextIcon _sortLevelBtn;
		[SerializeField] private RectTransform _groupOwned;
		[SerializeField] private RectTransform _groupNotOwned;
		[SerializeField] private TextHolder _notOwndedDevider;
		
		[SerializeField] private UnitCardView _unitPrefab;
		private List<UnitCardView> _all = new List<UnitCardView>();

		private Bookmark _bookmark = Bookmark.BOOKMARK_ALL;
		private SortDirection _tierSort = SortDirection.UP;
		private SortDirection _levelSort = SortDirection.UP;
		
		protected override void Init()
		{
			_bookmarkAllBtn.Text = "bookmark_all".Localize();
			_bookmarkMythicBtn.Text = "bookmark_mythic".Localize();
			_sortTierBtn.Text = "sort_tier".Localize();
			_sortLevelBtn.Text = "sort_level".Localize();
			_notOwndedDevider.Text = "not_owned".Localize();

			_unitPrefab.SetActive(true);
			foreach (var unit in Game.Static.Units.All.Values)
			{
				var userUnit = Game.User.Units.Get(unit);
				var unitCardView = Instantiate(_unitPrefab, userUnit.IsOwned ? _groupOwned : _groupNotOwned);
				unitCardView.Init(userUnit);
				_all.Add(unitCardView);
			}
			_unitPrefab.SetActive(false);

			ApplyBookmark(_bookmark, false);
			ApplySort(SortType.TIER, _tierSort, false);
			ApplySort(SortType.LEVEL, _levelSort, false);
			Sort();
			
			_sortTierBtn.SetOnClick(() =>
			{
				ApplySort(SortType.TIER, _tierSort == SortDirection.UP ? SortDirection.DOWN : SortDirection.UP);
			});
			
			_sortLevelBtn.SetOnClick(() =>
			{
				ApplySort(SortType.LEVEL, _levelSort == SortDirection.UP ? SortDirection.DOWN : SortDirection.UP);
			});
			
			_bookmarkAllBtn.SetOnClick(() =>
			{
				ApplyBookmark(Bookmark.BOOKMARK_ALL);
			});
			
			_bookmarkMythicBtn.SetOnClick(() =>
			{
				ApplyBookmark(Bookmark.BOOKMARK_MYTHIC);
			});
			
			base.Init();
		}

		private void ApplyBookmark(Bookmark bookmark, bool needSort = true)
		{
			_bookmark = bookmark;
			_bookmarkAllBtn.SetLock(_bookmark != Bookmark.BOOKMARK_ALL, true);
			_bookmarkMythicBtn.SetLock(_bookmark != Bookmark.BOOKMARK_MYTHIC, true);
			
			if (needSort)
				Sort();
		}

		private void ApplySort(SortType type, SortDirection direction, bool needSort = true)
		{
			var btn = type == SortType.TIER ? _sortTierBtn : _sortLevelBtn;
			btn.Icon.transform.localScale = btn.Icon.transform.localScale.Set(y: direction == SortDirection.DOWN ? 1 : -1);
			if (type == SortType.TIER)
				_tierSort = direction;
			else
				_levelSort = direction;

			if (needSort)
				Sort();
		}

		private void Sort()
		{
			_all.Each(u => u.transform.SetParent(u.Unit.IsOwned ? _groupOwned : _groupNotOwned));
			
			var byBookmark = _all.Where(u => _bookmark == Bookmark.BOOKMARK_ALL ||
										 u.Unit.Data.UnitType.ModelId == UnitType.MYTHICAL);
			var groups = byBookmark.GroupBy(u => u.Unit.IsOwned);
			
			foreach (var group in groups)
			{
				IOrderedEnumerable<UnitCardView> ordered = null;
				
				if (_tierSort == SortDirection.DOWN)
					ordered = group.OrderByDescending(u => u.Unit.Data.UnitType.Id);
				else
					ordered = group.OrderBy(u => u.Unit.Data.UnitType.Id);

				if (_levelSort == SortDirection.DOWN)
					ordered = ordered.ThenByDescending(u => u.Unit.Level.Value);
				else
					ordered = ordered.ThenBy(u => u.Unit.Level.Value);

				var i = 0;
				foreach (var item in ordered)
				{
					if (byBookmark.Contains(item))
					{
						(item.transform as RectTransform).SetSiblingIndex(i);
						i++;
					}
				}
			}
			
			_all.Each(u => u.SetActive(byBookmark.Contains(u)));
			LayoutRebuilder.ForceRebuildLayoutImmediate(_scroll.content.transform as RectTransform);
		}
	}
}