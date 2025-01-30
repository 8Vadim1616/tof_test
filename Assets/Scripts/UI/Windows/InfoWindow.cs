using System;
using System.Collections.Generic;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.WindowsSystem;
using Assets.Scripts.UI.WindowsSystem.Components;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Windows
{
	public enum ButtonPrefab
    {
        GreenSquareButtonWithText,
        GreenSquareButtonWithIconAndText,

        RedSquareButtonWithText,
        RedSquareButtonWithIconAndText,
    }

    public class InfoWindow : AbstractWindow
    {
        public RectTransform buttonPlace;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI mainText;

        public BasicButton closeButton;
        public WindowCloser closer;

        private string _title;

		private const string CONNECTION_ERROR_WIN_LOCK = "OpenConnectionErrorWindow";

		public override string ClassName => GetType().Name + (_title.IsNullOrEmpty() ? "" : " " + _title);

        public void Init(string title = null, string main = null, Action OnClick = null, BasicButton buttonInstance = null)
        {
            _title = titleText.text = string.IsNullOrEmpty(title) ? "" : title;
            mainText.text = string.IsNullOrEmpty(main) ? "" : main;

            if (buttonInstance)
            {
                buttonInstance.transform.parent = buttonPlace;
                buttonInstance.transform.localPosition = Vector3.zero;

                buttonInstance.onClick.RemoveAllListeners();
                buttonInstance.onClick.AddListener(() =>
                {
                    Close();
                    OnClick?.Invoke();
                });
            }
        }

        public void InitEmpty(string title = null, string main = null, bool canClose = true, bool showCloseButton = false)
        {
            _title = titleText.text = string.IsNullOrEmpty(title) ? "" : title;
            mainText.text = string.IsNullOrEmpty(main) ? "" : main;

            closeButton.SetActive(showCloseButton);
            closer.enabled = canClose; 
        }

        public InfoWindow AddButton(ButtonPrefab type, string text = null, Item itemIcon = null, Action onClick = null, bool isEnabled = true)
        {
            var btn = Game.BasePrefabs.Buttons.InstantiateButton(type, text, itemIcon?.IconPath, buttonPlace);
            btn.transform.localPosition = Vector3.zero;
            btn.SetActive(true);
            btn.onClick.AddListener(() =>
            {
                Close();
                onClick?.Invoke();
            });
            btn.Enabled = isEnabled;

            return this;
        }

        public InfoWindow FinishAddingButtons()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(buttonPlace);
            return this;
        }

        public static InfoWindow Of(string title = null, string main = null, bool canClose = true, bool showCloseButton = false, bool closeAllOther = false, bool minimize = true, Dictionary<string, object> addLogParams = null)
        {
            return Game.Windows.ScreenChange<InfoWindow>(closeAllOther, w => w.InitEmpty(title, main, canClose, showCloseButton), minimize, addLogParams);
        }

		public static void OfError(string key, string message, Action callback, Dictionary<string, object> addLogParams)
		{
			Game.Windows.CloseAllScreensPromise(CONNECTION_ERROR_WIN_LOCK)
				.Then(() =>
				 {
					 InfoWindow window = null;

					 var title = Game.Localization.LocalizationLoaded.IsPending ? "Error" : $"{key}_title".Localize();
					 var desc = Game.Localization.LocalizationLoaded.IsPending ? message : $"{key}_desc".Localize();
					 if (string.IsNullOrEmpty(desc))
						 desc = "Server error!";
					 var btnOk = Game.Localization.LocalizationLoaded.IsPending ? "Ok" : "ok".Localize();

					 window = Of(title, desc, false, true, addLogParams: addLogParams);

					 Game.Windows.AddNewWindowsCreationAvailableLockOnce(CONNECTION_ERROR_WIN_LOCK);
					 //Game.Windows.NewWindowsCreationAvailable = false;

					 if (!window)
						 return;

					 window.AddButton(ButtonPrefab.GreenSquareButtonWithText, btnOk, null,
												   () =>
												   {
													   if (window != null)
													   {
														   window.CanClose.Value = true;
														   window.Close();
													   }

													   callback();
									  })
						   .FinishAddingButtons();

					 window.SetAdditionalLogParams(new Dictionary<string, object> { { "key", key } });

					 window.CanClose.Value = false;
					 window.CanCloseByBackButton = false;
				 });
		}
    }

    public static class InfoScreenExtensions
    {
		public static bool WasShowInCurrentSession;
		public static void OfNoFreeSpace()
		{
			InfoWindow.Of("attention".Localize(), "no_free_space".Localize())
					  .AddButton(ButtonPrefab.GreenSquareButtonWithText, "ok".Localize())
					  .FinishAddingButtons();
		}
	}
}