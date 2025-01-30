using System;
using UnityEngine;

namespace Assets.Scripts.Platform.Mobile.Share
{
	public class MobileAndroidShare : MobileShare
	{
		public override void ShareText(string title, string text, Action<NativeShare.ShareResult> callBack = null, Texture2D texture = null, string createdFileName = "Image.png")
		{
			if (_isProcessing)
				return;

			_isProcessing = true;

			var nativeShare = new NativeShare()
				.SetSubject(title)
				.SetText(text)
				.SetCallback((result, shareTarget) =>
				{
					callBack?.Invoke(result);

					_isProcessing = false;

					Debug.Log("Share result: " + result);
				});

			if (texture != null)
				nativeShare.AddFile(texture, createdFileName);

			nativeShare.Share();
		}
	}
}