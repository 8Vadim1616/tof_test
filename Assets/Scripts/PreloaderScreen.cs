using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Localization;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.Utils;
using DG.Tweening;
using System.Collections.Generic;
using Assets.Scripts.Platform.Adapter;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
	[RequireComponent(typeof(CanvasGroup))]
	public class PreloaderScreen : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI version;
		[SerializeField] private TextMeshProUGUI progressText;
		[SerializeField] private TextMeshProUGUI message;
		[SerializeField] private GameObject messageContainer;
		[SerializeField] private GameObject test;
		[SerializeField] private GameObject cheats;
		[SerializeField] private GameObject editor;
		[SerializeField] public Slider ProgressBar;
		[SerializeField] private TextMeshProUGUI uid;
		[SerializeField] private RectTransform _bottomImage;
		[SerializeField] private ButtonText updateBtn;

		[Header("Fade out")]
		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private float fadeDuration = 1f;


		[Header("Save progress")]
		[SerializeField] private ButtonText saveButton;
		[SerializeField] private ButtonText playButton;
		[SerializeField] private BasicButton languageButton;

		private bool IsSocialLogged => Game.Social.Adapter.IsLoggedIn.Value;
		private Promise _checkSavePromise;

		public void Initialize()
		{
			if (!canvasGroup)
				canvasGroup = GetComponent<CanvasGroup>();

			if (canvasGroup)
				canvasGroup.alpha = 1;

			progress = 0;
			Progress = 0;

			if (uid)
				uid.text = null;

			if (version)
				version.text = $"ver: {Application.version}";

			if (test)
				test.SetActive(BuildSettings.BuildSettings.IsTest);

			if (cheats)
			{
#if BUILD_CHEAT
				cheats.SetActive(true);
#else
				cheats.SetActive(false);
#endif
			}

			if (editor)
				editor.SetActive(BuildSettings.BuildSettings.IsEditor);

			ProgressBar.SetActive(true);
			playButton.SetActive(false);

			messageContainer.SetActive(false);
			updateBtn.SetActive(false);

			updateBtn.Text = "update".Localize();
			updateBtn.onClick.RemoveAllListeners();
			updateBtn.onClick.AddListener(Game.UpdateBuild);

			//Game.Instance.OnScreenResize += OnScreenResize;
			//OnScreenResize();
		}

		/*private void OnScreenResize()
		{
			if ((float) Screen.width / Screen.height < 0.8f)
			{
				_bottomImage.anchorMax = new Vector2(0.5f, 0);
				_bottomImage.anchorMin = new Vector2(0.5f, 0);
				_bottomImage.anchoredPosition = new Vector2(0f, 50f);
			}
			else
			{
				_bottomImage.anchorMax = new Vector2(0.5f, 1);
				_bottomImage.anchorMin = new Vector2(0.5f, 1);
				_bottomImage.anchoredPosition = new Vector2(0, -1422);
			}
		}*/

		public int Group
		{
			set
			{
				if (version)
					version.text = $"{Application.version} ({value}/{Game.User?.Group})";
			}
		}

		public string UID
		{
			set
			{
				if (uid)
					uid.text = $"Uid: {value ?? ""}";
			}
		}

		/*private void OnDestroy()
		{
			Game.Instance.OnScreenResize -= OnScreenResize;
		}*/

		private int progress;
		public int Progress
		{
			get => progress;
			set
			{
				if (value > progress)
					progress = value;

				if (progressText)
					progressText.text = progress + "%";
				if (ProgressBar)
					ProgressBar.value = progress / 100f;
			}
		}

		internal IPromise FadeOut()
		{
			var promise = new Promise();

			if (canvasGroup)
				canvasGroup.DOFade(0, fadeDuration)
					.SetLink(gameObject)
					.OnComplete(OnComplete);
			else
				OnComplete();

			return promise;

			void OnComplete()
			{
				gameObject.SetActive(false);
				promise.ResolveOnce();
			}
		}

		public void OnVersionError()
		{
			var desc = new Dictionary<string, string>()
			{
				{"ru" , "Необходимо обновить игру до последней версии!"},
				{"en", "Please update to the latest version to get the best new features."},
				{"pt", "Atualize para a última versão para obter as melhores novas funcionalidades."}
			};

			var refresh = new Dictionary<string, string>()
			{
				{"ru", "Обновить"},
				{"en", "Update"},
				{"pt", "Atualizar"}
			};

			if (message && messageContainer)
			{
				messageContainer.SetActive(true);
				message.text = getText();
			}

			ProgressBar.SetActive(false);

			updateBtn.Text = getRefresh();
			updateBtn.SetActive(true);
			updateBtn.onClick.RemoveAllListeners();
			updateBtn.onClick.AddListener(Game.UpdateBuild);

			string getText()
			{
				if (desc.ContainsKey(GameLocalization.Locale))
					return desc[GameLocalization.Locale];

				return desc["en"];
			}

			string getRefresh()
			{
				if (refresh.ContainsKey(GameLocalization.Locale))
					return refresh[GameLocalization.Locale];

				return refresh["en"];
			}
		}

		public IPromise CheckSocialSave()
		{
			var canSaveProgress = SocialNetwork.CanSaveProgress;

			if (!canSaveProgress)
				return Promise.Resolved();

// #if (!BUILD_GOOGLE || BUILD_CHINA) && !UNITY_EDITOR
// 			return Promise.Resolved();
// #endif

            if (BuildSettings.BuildSettings.IsEditor)
                return Promise.Resolved();

            if (((Game.User.WasRegisterFromServer == true && Game.Settings.NEED_SHOW_SAVE_PROGRESS_IN_PRELOADER_REG) 
				|| (Game.User.WasRegisterFromServer == false && Game.Settings.NEED_SHOW_SAVE_PROGRESS_IN_PRELOADER))
						&& IsSocialLogged == false)
			{
				ServerLogs.SendLog("preloader_show_save_buttons");

				saveButton.Text = "save_progress".Localize();
				saveButton.onClick.RemoveAllListeners();
				saveButton.onClick.AddListener(OnSaveButtonClick);

				playButton.Text = "play".Localize();
				playButton.onClick.RemoveAllListeners();
				playButton.onClick.AddListener(OnPlayButtonClick);

				languageButton.onClick.RemoveAllListeners();
				//languageButton.onClick.AddListener(() => LanguageChangeWindow.Of());

				_checkSavePromise = new Promise();

				ProgressBar.SetActive(false);

				playButton.SetActive(true);
			}
			else
				return Promise.Resolved();


			return _checkSavePromise;
		}

		private void OnSaveButtonClick()
		{
			//ChooseSocialWindow.Of();
		}

		private void OnPlayButtonClick()
		{
			_checkSavePromise.ResolveOnce();
		}

		private void OnDisable()
		{
			ProgressBar.SetActive(true);
		}
	}
}
