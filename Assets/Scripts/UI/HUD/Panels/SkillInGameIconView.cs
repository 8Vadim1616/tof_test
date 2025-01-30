using Assets.Scripts.Static.Skills;
using Assets.Scripts.Utils;
using TMPro;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.HUD.Panels
{
	public class SkillInGameIconView : MonoBehaviour
	{
		[SerializeField] private string _skillParameter;
		[SerializeField] private TMP_Text _value;

		private void Awake()
		{
			/** JAVA
			Game.Instance.Playfiled.Subscribe(_ =>
			{
				var playfield = Game.Instance.Playfiled.Value;

				gameObject.SetActive(Game.Instance.Playfiled.Value);

				if (!playfield)
					return;

				FloatReactiveProperty reactive = null;
				if (_skillParameter == SkillParameter.Slow)
					reactive = playfield.AbilitiesController.SlowMultiplayer;
				else if (_skillParameter == SkillParameter.AttackSpeed)
					reactive = playfield.AbilitiesController.AttackSpeedMultiplayer;
				else if (_skillParameter == SkillParameter.AttackDamage)
					reactive = playfield.AbilitiesController.AttackMultiplayer;

				reactive?.Subscribe(val =>
				{
					gameObject.SetActive(!val.Equals(1f));
					var value = val > 1f ? (1f - val) * 100f : (val - 1f) * 100f;
					_value.text = $"{value.ToString("0.#")}%";
				}).AddTo(playfield.gameObject);
				
			}).AddTo(this);
			*/
		}
	}
}