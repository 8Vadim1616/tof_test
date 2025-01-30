using System.Collections;
using Assets.Scripts.Network.Logs;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#elif UNITY_ANDROID
using Google.Play.Review;
#endif

namespace Assets.Scripts.UI
{
	public class RateUsController : MonoBehaviour
	{
		private bool _showRateUsOnNextWin;
		public bool ShowRateUsOnNextWin
		{
			get => _showRateUsOnNextWin;
			set
			{
				_showRateUsOnNextWin = value;
				if (_showRateUsOnNextWin)
					PrepareRateUs();
			}
		}

		private bool LevelCondition => true;

#if UNITY_ANDROID
	    private ReviewManager reviewManager;
	    private PlayReviewInfo playReviewInfo;
#endif

		public void PrepareRateUs()
		{
#if UNITY_ANDROID
			StartCoroutine(PrepareReview());
#endif
		}

#if UNITY_ANDROID
		private IEnumerator PrepareReview()
		{
			reviewManager = new ReviewManager();

			var requestFlowOperation = reviewManager.RequestReviewFlow();

			yield return requestFlowOperation;

			if (requestFlowOperation.Error != ReviewErrorCode.NoError)
			{
				Debug.LogError($"Cant load requestInfo: {requestFlowOperation.Error}");
				// Log error. For example, using requestFlowOperation.Error.ToString().
				yield break;
			}

			playReviewInfo = requestFlowOperation.GetResult();
		}
#endif

		public void ShowRateUs()
		{
			if (!LevelCondition)
				return;
			
			ServerLogs.SendLog("RateUs: call");

#if UNITY_ANDROID
			StartCoroutine(StartReview());
#elif UNITY_IOS
			var res = Device.RequestStoreReview();
			ServerLogs.SendLog($"RateUs: IPhone {(res ? "no error" : "FAIL")}");
#endif
		}

#if UNITY_ANDROID
		private IEnumerator StartReview()
		{
			if (playReviewInfo == null)
				yield return PrepareReview();

			if (playReviewInfo == null)
				yield break;

			var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
			
			yield return launchFlowOperation;

			playReviewInfo = null;

			if (launchFlowOperation.Error != ReviewErrorCode.NoError)
			{
				Debug.LogError($"Cant load requestInfo: {launchFlowOperation.Error}");
				// ERROR
				yield break;
			}
			ServerLogs.SendLog("RateUs: Android, no errors");
		}
#endif
	}
}