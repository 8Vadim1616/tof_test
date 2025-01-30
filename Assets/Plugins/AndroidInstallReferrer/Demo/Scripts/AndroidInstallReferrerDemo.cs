using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace AndroidInstallReferrer.Demo
{
    public class AndroidInstallReferrerDemo : MonoBehaviour
    {
        public GameObject InfoBlock;
        public GameObject ErrorBlock;
        public Text InstallReferrer;
        public string InstallReferrerFormat = "Referrer: {0}";
        public Text InstallVersion;
        public string InstallVersionFormat = "Version: {0}";
        public Text GooglePlayInstant;
        public string GooglePlayInstantFormat = "GooglePlay Instant: {0}";
        public Text InstallBeginTimestampSeconds;
        public string InstallBeginTimestampSecondsFormat = "Install Begin Time: {0}";
        public Text InstallBeginTimestampServerSeconds;
        public string InstallBeginTimestampServerSecondsFormat = "Install Begin Server Time: {0}";
        public Text ReferrerClickTimestampSeconds;
        public string ReferrerClickTimestampSecondsFormat = "Referrer Click Time: {0}";
        public Text ReferrerClickTimestampServerSeconds;
        public string ReferrerClickTimestampServerSecondsFormat = "Referrer Click Server Time: {0}";
        public Text Error;
        public string ErrorFormat = "Error: {0}";

        private const string Unknown = "Unknown";
        
        void Awake()
        {
            AndroidInstallReferrer.InstallReferrer.GetReferrer(OnGetData);
            
            //Emulating install referrer data in Unity Editor:
#if UNITY_EDITOR
            OnGetData(new InstallReferrerData(
                "utm_source=google&utm_medium=cpc&utm_term=1&utm_content=2&utm_campaign=3&anid=admob", 
                "1.0", false, 
                DateTime.Now, DateTime.Now, 
                DateTime.Now, DateTime.Now));
#endif
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        private void OnGetData(InstallReferrerData data)
        {
            InfoBlock.SetActive(data.IsSuccess);
            ErrorBlock.SetActive(!data.IsSuccess);
            if (data.IsSuccess)
            {
                InstallReferrer.text = string.Format(InstallReferrerFormat, data.InstallReferrer);
                InstallVersion.text = string.Format(InstallVersionFormat, string.IsNullOrEmpty(data.InstallVersion) ? Unknown : data.InstallVersion);
                GooglePlayInstant.text = string.Format(GooglePlayInstantFormat, data.GooglePlayInstant);
                var installTime = data.InstallBeginTime.Ticks == 0 ? Unknown : data.InstallBeginTime.ToString(CultureInfo.InvariantCulture);
                InstallBeginTimestampSeconds.text = string.Format(InstallBeginTimestampSecondsFormat, installTime);
                var installServerTime = data.InstallBeginServerTime.Ticks == 0 ? Unknown : data.InstallBeginServerTime.ToString(CultureInfo.InvariantCulture);
                InstallBeginTimestampServerSeconds.text = string.Format(InstallBeginTimestampServerSecondsFormat, installServerTime);
                var referrerClickTime = data.ReferrerClickTime.Ticks == 0 ? Unknown : data.ReferrerClickTime.ToString(CultureInfo.InvariantCulture);
                ReferrerClickTimestampSeconds.text = string.Format(ReferrerClickTimestampSecondsFormat, referrerClickTime);
                var referrerClickServerTime = data.ReferrerClickServerTime.Ticks == 0 ? Unknown : data.ReferrerClickServerTime.ToString(CultureInfo.InvariantCulture);
                ReferrerClickTimestampServerSeconds.text = string.Format(ReferrerClickTimestampServerSecondsFormat, referrerClickServerTime);
            }
            else
            {
                Error.text = string.Format(ErrorFormat, data.Error);
            }
        }
    }
}