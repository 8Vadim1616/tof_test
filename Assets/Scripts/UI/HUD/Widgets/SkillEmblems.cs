using Assets.Scripts.Static.Skills;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.UI.HUD.Widgets
{
	public class SkillEmblems : MonoBehaviour
	{
		[SerializeField] private TextHolder _passive;
		[SerializeField] private TextHolder _skill;
		[SerializeField] private TextHolder _ult;
		[SerializeField] private TextHolder _mp;
		
		public void Init(Skill skill)
		{
			_passive.Text = "skill_type_passive".Localize();
			_skill.Text = "skill_type_skill".Localize();
			_ult.Text = "skill_type_ult".Localize();
			_mp.Text = "skill_type_mp".Localize();
			
			_passive.SetActive(false/*skill.Type == SkillType.Initialize || skill.Type == SkillType.Chance*/);
			_skill.SetActive(skill.Type == SkillType.Attack || skill.Type == SkillType.Initialize || skill.Type == SkillType.Chance);
			_ult.SetActive(skill.Type == SkillType.Active);
			_mp.SetActive(skill.Type == SkillType.Active);
		}
	}
}