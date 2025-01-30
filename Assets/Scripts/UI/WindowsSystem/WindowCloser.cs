using System;
using Assets.Scripts.UI.Utils;
using Assets.Scripts.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.WindowsSystem.Components
{
    [RequireComponent(typeof(Image))]
    public class WindowCloser : MonoBehaviour
    {
        private float lastPress = -1;
        private float pressDistance = .5f;
        [SerializeField] private bool ignoreOnceIfTipOpen;

        private const float INTERVAL_BEFORE_CLOSE = .2f;

		private bool _wasDisabled = false;
		private IDisposable _closer;
		private Button _btn;

        private AbstractWindow ParentWindow
        {
            get;
            set;
        }

		private void Awake()
		{
			_btn = GetComponent<Button>();
		}

		private void Start()
        {
			if (_btn != null)
            {
				_btn.onClick.AddListener(Close);
                return;
            }

			_closer = GetComponent<Image>().OnClickAsObservable(Close).AddTo(this);
        }

        private void Close()
        {
            if (!ParentWindow) 
				ParentWindow = transform.GetComponentInParents<AbstractWindow>();

            if (ParentWindow) 
				ParentWindow.Close();
            else
            {
                return;
                if (lastPress + pressDistance < Time.time)
                {
                    //Game.Windows.CloseCommand();
                    lastPress = Time.time;
                }
            }
        }

		private void OnEnable()
		{
			if (_wasDisabled)
				Start();

			_wasDisabled = false;
		}

		private void OnDisable()
		{
			_wasDisabled = true;
			var btn = GetComponent<Button>();
			if (btn)
			{
				btn.onClick.RemoveListener(Close);
				return;
			}

			_closer?.Dispose();
		}
	}
}