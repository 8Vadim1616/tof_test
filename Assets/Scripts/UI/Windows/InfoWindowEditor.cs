using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.WindowsSystem;
using Assets.Scripts.UI.WindowsSystem.Components;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Windows
{
	public class InfoWindowEditor : AbstractWindow
    {
        public RectTransform buttonPlace;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI mainText;

        public WindowCloser closer;

        private string _title;

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

        public void InitEmpty(string title = null, string main = null)
        {
            _title = titleText.text = string.IsNullOrEmpty(title) ? "" : title;
            mainText.text = string.IsNullOrEmpty(main) ? "" : main;
        }

        public InfoWindowEditor AddButton(ButtonPrefab type, string text = null, Item itemIcon = null, Action onClick = null, bool isEnabled = true)
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

        public InfoWindowEditor FinishAddingButtons()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(buttonPlace);
            return this;
        }

        public static InfoWindowEditor Of(string title = null, string main = null, bool closeAllOther = false, bool minimize = true, Dictionary<string, object> addLogParams = null)
        {
            return Game.Windows.ScreenChange<InfoWindowEditor>(closeAllOther, w => w.InitEmpty(title, main), minimize, addLogParams);
        }
    }
}