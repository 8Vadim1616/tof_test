using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif



public class iOSPlistPostProcess
{

    [PostProcessBuild(1)]
    public static void OnPostBuild(BuildTarget buildTarget, string path) {
        if (buildTarget == BuildTarget.iOS)
        {
            ChangeXcodePlist(buildTarget, path);
        }
      
    }
    public static void ChangeXcodePlist(BuildTarget buildTarget, string path)
    {
        #if UNITY_IOS
        string plistPath = path + "/Info.plist";
        UnityEditor.iOS.Xcode.PlistDocument plist = new UnityEditor.iOS.Xcode.PlistDocument();
        plist.ReadFromFile(plistPath);

        UnityEditor.iOS.Xcode.PlistElementDict rootDict = plist.root;

        Debug.Log(">> Automation, plist ... <<");

        if (rootDict.values.ContainsKey("LSApplicationQueriesSchemes"))
        {
            var LSApplicationQueriesSchemes = rootDict["LSApplicationQueriesSchemes"].AsArray().values;
            var vkAutorizeScemeIsPresent = false;
            foreach (var scheme in LSApplicationQueriesSchemes)
            {
                if (scheme.AsString().Contains("vkauthorize"))
                {
                    vkAutorizeScemeIsPresent = true;
                    break;
                }
            }
            if (!vkAutorizeScemeIsPresent)
            {
                rootDict["LSApplicationQueriesSchemes"].AsArray().AddString("vkauthorize");
            }
        }
        else
        {
            UnityEditor.iOS.Xcode.PlistElementArray LSApplicationQueriesSchemes = rootDict.CreateArray("LSApplicationQueriesSchemes");
            LSApplicationQueriesSchemes.AddString("vkauthorize");
        }
        File.WriteAllText(plistPath, plist.WriteToString());
        #endif
    }


}
    
