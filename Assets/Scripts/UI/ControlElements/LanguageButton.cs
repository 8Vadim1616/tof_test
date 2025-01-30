using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Utils;

namespace Assets.Scripts.UI.ControlElements
{
	public class LanguageButton : ButtonCheckboxText
	{
		[SerializeField] Image _checked;
		[SerializeField] Image _unchecked;
		public string Language { get; protected set; }

		public void Init(string language)
		{
			Language = language;
			Text = $"language_{Language}".Localize();
		}

		public override void OnChecked()
		{
			_checked.SetActive(IsChecked);
			_unchecked.SetActive(!IsChecked);
		}
	}
}
