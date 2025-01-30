using AppsFlyerSDK;
using Assets.Scripts.Network.Logs;
using UnityEngine;

public class AppsFlyerCustomTrackerCallbacks : MonoBehaviour, IAppsFlyerConversionData
{
    public const string TAG = "[AppsFlyerPartner] ";
    
    public void onConversionDataSuccess(string conversionData)
    {
        Debug.Log(TAG + "onConversionDataSuccess " + conversionData);

        ServerLogs.SendAppsFlyerConversionData(conversionData);
    }

    public void onConversionDataFail(string error)
    {
        Debug.Log(TAG + "onConversionDataFail " + error);
    }

    public void onAppOpenAttribution(string attributionData)
    {
        Debug.Log(TAG + "onAppOpenAttribution " + attributionData);
    }

    public void onAppOpenAttributionFailure(string error)
    {
        Debug.Log(TAG + "onAppOpenAttributionFailure " + error);
    }
}
