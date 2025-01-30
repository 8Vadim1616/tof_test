using System;
using Assets.Scripts.Static.UnitUpgrades;
using Assets.Scripts.User.Units;
using Assets.Scripts.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Windows.Units
{
	public class UnitUpgradeByLevelItemView : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField] private TMP_Text _lvl;
		[SerializeField] private TMP_Text _desc;
		[SerializeField] private GameObject _availableBack;
		[SerializeField] private GameObject _unavailableBack;

		public Action OnLinkClick;

		public void Init(UnitUpgradeByLevel upgradeByLevel, UserUnit userUnit)
		{
			_lvl.text = "lvl".Localize(upgradeByLevel.Level.ToString());
			_desc.text = upgradeByLevel.Desc;

			userUnit.Level.Subscribe(_ =>
			{
				_availableBack.SetActive(userUnit.Level.Value >= upgradeByLevel.Level);
				_unavailableBack.SetActive(userUnit.Level.Value < upgradeByLevel.Level);
			}).AddTo(this);
		}
		
		public void OnPointerClick(PointerEventData eventData)
		{
			int linkIndex = TMP_TextUtilities.FindIntersectingLink(_desc, eventData.position, null);
			if (linkIndex != -1)
				OnLinkClick?.Invoke();
		}
	}
}