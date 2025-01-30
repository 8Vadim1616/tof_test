using Assets.Scripts.Static.Skills;
using UnityEngine;

namespace Assets.Scripts.UI.HUD.Widgets
{
	public class SkillHint : TextHint
	{
		[SerializeField] private SkillEmblems _emblems;

		public void Show(Transform target, Skill skill)
		{
			_emblems.Init(skill);
			
			Show(target, skill.Name, skill.Desc);
		}
	}
}