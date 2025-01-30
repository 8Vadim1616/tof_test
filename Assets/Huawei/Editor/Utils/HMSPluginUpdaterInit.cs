#if UNITY_EDITOR && BUILD_HUAWEI
using UnityEditor;

[InitializeOnLoad]
public class HMSPluginUpdaterInit : AssetPostprocessor
{
    static HMSPluginUpdaterInit()
    {
        HMSPluginUpdater.Request();
    }
}

#endif