using System;
using UnityEngine;

namespace Assets.Scripts.Platform.Mobile.Share
{
	public class MobileShare
	{
		protected bool _isFocus = false;
		protected bool _isProcessing = false;

		public void Init()
		{
			AddListeners();
		}

		public virtual void ShareText(string title, string text, Action<NativeShare.ShareResult> callBack = null, Texture2D texture = null, string createdFileName = "Image.png")
		{
			GameLogger.debug("No sharing set up for this platform.");
			callBack?.Invoke(NativeShare.ShareResult.Unknown);
		}

		private void OnApplicationFocus()
		{
			_isFocus = true;
		}

		private void OnApplicationUnFocus()
		{
			_isFocus = false;
		}

		private void AddListeners()
		{
			Game.GameReloader.ApplicationFocus += OnApplicationFocus;
			Game.GameReloader.ApplicationUnFocus += OnApplicationUnFocus;
		}

		private void RemoveListeners()
		{
			Game.GameReloader.ApplicationFocus -= OnApplicationFocus;
			Game.GameReloader.ApplicationUnFocus -= OnApplicationUnFocus;
		}

		public void Free()
		{
			RemoveListeners();
		}
	}
}