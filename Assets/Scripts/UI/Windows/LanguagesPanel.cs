using Assets.Scripts.Localization;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Windows
{
	public class LanguagesPanel : MonoBehaviour
	{
		[SerializeField] TMP_Text _title;
		[SerializeField] Transform _btnsRoot;
		[SerializeField] BasicToggle _btnPrefab;
		[SerializeField] BasicButton _btnClose;

		private string _selectedLanguage;
		
		
		public void Init()
		{
			SelectLanguage(GameLocalization.Locale);
			GenerateButtons();

			_title.text = "language".Localize();
			_btnClose.SetOnClick(() => this.SetActive(false));
			
			this.SetActive(false);
		}

		private void GenerateButtons()
		{
			foreach (var lang in GameLocalization.AvailableLangs)
			{
				var btn = Instantiate(_btnPrefab, _btnsRoot);

				btn.Text = GetLanguageTranslation(lang);
				btn.IsOn = _selectedLanguage.Equals(lang);
				
				btn.SetOnClick(() =>
				{
					SelectLanguage(lang);
					Confirm();
				});
			}
		}

		private void SelectLanguage(string language)
		{
			_selectedLanguage = language;
		}

		private string GetLanguageTranslation(string language)
		{
			return $"language_{language}".Localize();
		}

		private void Confirm()
		{
			this.SetActive(false);
			
			if (_selectedLanguage.Equals(GameLocalization.Locale))
				return;

			GameLocalization.SetLocale(_selectedLanguage);
		}
	}
}