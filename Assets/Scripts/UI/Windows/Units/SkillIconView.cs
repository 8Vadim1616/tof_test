using Assets.Scripts.Static.Skills;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.User.Units;
using Assets.Scripts.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Windows.Units
{
	public class SkillIconView : BasicButton
	{
		[SerializeField] private Image _backCommon;
		[SerializeField] private Image _backUlt;
		[SerializeField] private Image _icon;
		[SerializeField] private Image _lock;
		[SerializeField] private GameObject _select;
		
		public Skill Skill { get; private set; }

		public void Init(UserUnit unit, Skill skill)
		{
			Skill = skill;
			_icon.LoadFromAssets(skill.IconPath);
			_backCommon.SetActive(skill.Type != SkillType.Active);
			_backUlt.SetActive(skill.Type == SkillType.Active);
			unit.Level.Subscribe(_ =>
			{
				_lock.SetActive(unit.GetSkill(skill.Id) == null);
			}).AddTo(this);
		}

		public void Select() => _select.SetActive(true);
		public void UnSelect() => _select.SetActive(false);
	}
}