using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.HUD.Widgets;
using Assets.Scripts.UI.WindowsSystem;
using Assets.Scripts.User.Units;
using Assets.Scripts.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Windows.Units
{
	public class UnitInfoWindow : AbstractWindow
	{
		[SerializeField] private UnitUpgradeByLevelItemView _upgradeItemViewPrefab;
		[SerializeField] private ButtonTextIcon _btnUpgrade;
		[SerializeField] private TMP_Text _btnUpgradeText;
		[SerializeField] private TMP_Text _unitTypeName;
		[SerializeField] private Image _unitTypeNameBack;
		[SerializeField] private TMP_Text _unitAttackType;
		[SerializeField] private UnitCardView _unitCard;
		[SerializeField] private TMP_Text _desc;
		[SerializeField] private SkillIconView _skillPrefab;
		[SerializeField] private List<SkillIconView> _skillItems;
		[SerializeField] private SkillHint _skillHint;

		[Header("Atk+Spd")]
		[SerializeField] private TMP_Text _attackName;
		[SerializeField] private TMP_Text _attackValue;
		[SerializeField] private TMP_Text _attackSpeedName;
		[SerializeField] private TMP_Text _attackSpeedValue;
		
		private List<UnitUpgradeByLevelItemView> _upgradeItems;

		private UserUnit _userUnit;
		
		public static UnitInfoWindow Of(UserUnit userUnit) =>
						Game.Windows.ScreenChange<UnitInfoWindow>(false, w => w.Init(userUnit));

		private void Init(UserUnit userUnit)
		{
			_userUnit = userUnit;
			
			_unitCard.Init(_userUnit);

			_desc.text = _userUnit.Data.Desc;
			_attackName.text = "attack".Localize();
			_attackSpeedName.text = "attack_speed".Localize();
			
			_userUnit.Attack.Subscribe(_ =>
			{
				_attackValue.text = _userUnit.Attack.Value.ToKiloFormat();
			}).AddTo(this);
			
			_userUnit.AttackSpeed.Subscribe(_ =>
			{
				_attackSpeedValue.text = _userUnit.AttackSpeed.Value.ToKiloFormat();
			}).AddTo(this);

			_unitTypeName.text = _userUnit.Data.UnitType.ModelId.Localize();
			_unitTypeNameBack.color = Game.BasePrefabs.UnitTypeColors[_userUnit.Data.UnitType.ModelId];

			_unitAttackType.text = _userUnit.Data.AttackType.ToString().Localize();
			
			_btnUpgradeText.text = "upgrade_card_text".Localize();

			_userUnit.Level.Subscribe(_ =>
			{
				_btnUpgrade.SetItemCount(_userUnit.GetUpgradeMoney1(), new List<ItemCount> {_userUnit.GetUpgradeCards()});
			}).AddTo(this);
			
			_btnUpgrade.SetOnClick(() =>
			{
				if (Game.Checks.EnoughItems(_userUnit.GetUpgradeCost()))
					_userUnit.Upgrade();
			});

			_upgradeItemViewPrefab.SetActive(true);
			var upgrades = Game.Static.UnitUpgrades.GetByUnit(_userUnit.Data);
			foreach (var up in upgrades.All)
			{
				if (up.Level == 1)
					continue;
				
				var item = Instantiate(_upgradeItemViewPrefab, _upgradeItemViewPrefab.transform.parent);
				item.Init(up, userUnit);
				var skill = up.SkillUpgrade?.Skill;
				if (skill != null)
				{
					item.OnLinkClick = () =>
					{
						var skillItem = _skillItems.FirstOrDefault(s => s.Skill == skill);
						if (skillItem)
							skillItem.onClick?.Invoke();
					};
				}
			}
			_upgradeItemViewPrefab.SetActive(false);
			
			_skillPrefab.SetActive(true);
			_skillItems = new List<SkillIconView>();
			foreach (var skill in upgrades.Skills)
			{
				var skillItem = Instantiate(_skillPrefab, _skillPrefab.transform.parent);
				skillItem.Init(_userUnit, skill);
				skillItem.SetOnClick(() => _skillHint.Show(skillItem.transform, skillItem.Skill));
				_skillItems.Add(skillItem);
			}
			_skillPrefab.SetActive(false);
		}
	}
}