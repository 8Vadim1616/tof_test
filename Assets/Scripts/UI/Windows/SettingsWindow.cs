using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.WindowsSystem;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Windows
{
	public class SettingsWindow : AbstractWindow
	{
		[SerializeField] TextMeshProUGUI _title;
		[SerializeField] ButtonText _toggleMusic;
		[SerializeField] ButtonText _toggleSound;
		[SerializeField] ButtonText _toggleVibro;
		[Space]
		[SerializeField] ButtonText _btnPolicy;
		[SerializeField] ButtonText _btnTermsOfUse;
		[SerializeField] ButtonText _btnConsentSettings;
		[Space]
		[SerializeField] TextMeshProUGUI _uid;
		[SerializeField] TextMeshProUGUI _version;
		[Space]
		[SerializeField] ButtonText _btnLanguage;
		[SerializeField] private LanguagesPanel _languagesPanel;

		public override bool HideHudAll => false;

		public static SettingsWindow Of() =>
						Game.Windows.ScreenChange<SettingsWindow>(true, w => w.Init());
		
		private void Init()
		{
			_title.text = "settings".Localize();

			_btnPolicy.Text = "gdpr_term2".Localize();
			_btnPolicy.onClick.AddListener(GDPRWindow.OpenPrivacyPolicy);

			_btnTermsOfUse.Text = "gdpr_term1".Localize();
			_btnTermsOfUse.onClick.AddListener(GDPRWindow.OpenTermsOfService);
			_btnConsentSettings.onClick.AddListener(() => Game.Instance.ConsentController.ShowPrivacyOptionsForm());
			
			_uid.text = $"uid: {Game.User.Uid}";

			_version.text = UserSettings.VersionInfo;

			_toggleMusic.Text = "music".Localize();
			_toggleMusic.SetOnClick(() =>
			{
				Game.User.Settings.IsMusic = !Game.User.Settings.IsMusic;
				UpdateValues();
			});
			
			_toggleSound.Text = "sound".Localize();
			_toggleSound.SetOnClick(() =>
			{
				Game.User.Settings.IsSound = !Game.User.Settings.IsSound;
				UpdateValues();
			});
			
			_toggleVibro.Text = "vibro".Localize();
			_toggleVibro.SetOnClick(() =>
			{
				Game.User.Settings.IsVibration = !Game.User.Settings.IsVibration;
				UpdateValues();
			});
			
			_btnLanguage.Text = "language".Localize();
			_btnLanguage.SetOnClick(() => _languagesPanel.SetActive(true));

			_languagesPanel.Init();
			
			UpdateValues();
		}

		private void UpdateValues()
		{
			_toggleSound.SetLock(!Game.User.Settings.IsSound, true);
			_toggleMusic.SetLock(!Game.User.Settings.IsMusic, true);
			_toggleVibro.SetLock(!Game.User.Settings.IsVibration, true);
		}
	}
}