using System.Linq;
using System.IO;
using Assets.Scripts.BuildSettings;
using Assets.Scripts.User;
using Microsoft.Win32;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Assets.Scripts.Network.Queries.Operations.Api.StaticData;
using Assets.Scripts.Static;

using Assets.Scripts.GameServiceProvider;using System;

namespace Assets.Scripts.UI.WindowsSystem.Editor
{
    public class EditorUI : ScriptableObject, IPreprocessBuildWithReport, IActiveBuildTargetChanged
    {
        private static string BUILD_HUAWEI = "BUILD_HUAWEI";
        private static string BUILD_GOOGLE = "BUILD_GOOGLE";
        private static string BUILD_AMAZON = "BUILD_AMAZON";
		
		private static string BUILD_CHEAT = "BUILD_CHEAT";

		private static string BUILD_NEURAL_BOT = nameof(BUILD_NEURAL_BOT);

		private const int BACK_INDEX = 0;
        private const int CONTENT_INDEX = 1;

        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report)
        {
            
        }

		private static void ToggleDefine(string define)
		{
			var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup)?
				.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
				.ToList();

			if (!defines.Remove(define))
				defines.Add(define);

			var symbols = string.Join(";", defines);
			PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
		}

		private static bool HasDefine(string define)
		{
			return PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup)?
				.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
				.ToList()
				.Contains(define)
				?? false;
		}

		[MenuItem("Build/Neural bot build", priority = 0)]
		private static void SwitchNNBot()
		{
			ToggleDefine(BUILD_NEURAL_BOT);
		}

		[MenuItem("Build/Neural bot build", true)]
		private static bool IsNNBot()
		{
			Menu.SetChecked("Build/Neural bot build", HasDefine(BUILD_NEURAL_BOT));
			return true;
		}


		[MenuItem("Build/Set Cheat Build/On")]
		private static void SwitchCheatOn()
		{
			string platform = GetCurrentPlatform();
			ChangePlatformDefine(string.IsNullOrWhiteSpace(platform) ? BUILD_CHEAT : platform + ";" + BUILD_CHEAT);
		}

		[MenuItem("Build/Set Cheat Build/Off")]
		private static void SwitchCheatOff()
		{
			string platform = GetCurrentPlatform();
			ChangePlatformDefine(platform);
		}

		[MenuItem("Build/Set Cheat Build/On", true)]
		private static bool IsCheatOn()
		{
#if BUILD_CHEAT
		    return false;
#endif
			return true;
		}

		[MenuItem("Build/Set Cheat Build/Off", true)]
		private static bool IsCheatOff()
		{
#if BUILD_CHEAT
		    return true;
#endif
			return false;
		}
		
		private static string GetCurrentPlatform()
		{
#if BUILD_GOOGLE
            return BUILD_GOOGLE;
#elif BUILD_AMAZON
            return BUILD_GOOGLE;
#elif BUILD_HUAWEI
            return BUILD_HUAWEI;
#endif
			return null;
		}
        
        [MenuItem("Build/Increase Both Versions &v", false, 1)]
        static void IncreaseBothVersions()
        {
            IncreaseBuild();
            IncreasePlatformVersion();
        }

        [MenuItem("Build/Edit Build Settings %J", false, 2)]
        static void EditBuildSettings()
        {
            var winds = Resources.FindObjectsOfTypeAll<EditorWindow>();
            var inspector = winds.Where(t => t.titleContent.text == "Inspector").ToList();

            if (inspector.Count == 0)
            {
                EditorUtility.DisplayDialog("Hint", "In order to edit Build setting open Inspector window", "ok");
            }

            BuildSettings.BuildSettings asset = Resources.Load<BuildSettings.BuildSettings>("BuildSettings");
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        [MenuItem("Build/Android Debug", false, 21)]
        static void AndroidDebug()
        {
            System.Diagnostics.Process.Start("adb", "logcat -c");
            System.Diagnostics.Process.Start("adb", "logcat -s Unity ActivityManager PackageManager dalvikvm DEBUG");
        }

        [MenuItem("Build/Open PlayerPrefs", false, 41)]
        static void OpenPlayerPrefs()
        {
            RegistryKey lastKey =
                            Registry.CurrentUser
                                    .OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit", true);


            var home = $@"HKEY_CURRENT_USER\SOFTWARE\Unity\UnityEditor\{Application.companyName}\{Application.productName}";
            lastKey?.SetValue("LastKey", home);

            System.Diagnostics.Process.Start("regedit.exe");
        }

        [MenuItem("Build/Kill User", false, 51)]
        static void RemovePlayerPrefs()
        {
            var keyName = $@"SOFTWARE\Unity\UnityEditor\{Application.companyName}";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName, true))
            {
                if (key != null)
                    key.DeleteSubKeyTree(Application.productName, false);
            }
			
			DirectoryInfo dataDir = new DirectoryInfo(Application.persistentDataPath);
			dataDir.Delete(true);
        }

        [MenuItem("Build/Kill UID", false, 51)]
        static void ClearUID()
        {
            PlayerPrefs.SetString(UserRegisterData.UID_KEY, null);
            PlayerPrefs.SetString(UserRegisterData.AUTH_KEY_KEY, null);
            PlayerPrefs.SetString(UserRegisterData.MOBILE_UID_KEY, null);
            PlayerPrefs.SetString(UserRegisterData.MOBILE_AUTH_KEY_KEY, null);
		}

		static void IncreaseBuild()
        {
            IncrementVersion(new[] { 0, 0, 1 });
        }

        static void IncreasePlatformVersion()
        {
            PlayerSettings.Android.bundleVersionCode += 1;
            //PlayerSettings.iOS.buildNumber = (int.Parse(PlayerSettings.iOS.buildNumber) + 1).ToString();

            Debug.Log($"PlayerSettings.Android.bundleVersionCode = {PlayerSettings.Android.bundleVersionCode}");
            Debug.Log($"PlayerSettings.iOS.buildNumber = {PlayerSettings.iOS.buildNumber}");
        }

        static void IncrementVersion(int[] version)
        {
            var lines = PlayerSettings.bundleVersion.Split('.');

            for (var i = lines.Length - 1; i >= 0; i--)
            {
                var isNumber = int.TryParse(lines[i], out var numberValue);

                if (!isNumber || version.Length - 1 < i) continue;

                if (i > 0 && version[i] + numberValue > 99)
                {
                    version[i - 1]++;
                    version[i] = 0;
                }
                else
                    version[i] += numberValue;
            }

            PlayerSettings.bundleVersion = $"{version[0]}.{version[1]}.{version[2]}";
            PlayerSettings.iOS.buildNumber = $"{version[0]}.{version[1]}.{version[2]}";
            Debug.Log($"PlayerSettings.bundleVersion = {PlayerSettings.bundleVersion}");
        }

        [MenuItem("Build/Switch Platform/Huawei")]
        private static void SwitchToHuawei()
        {
            SwitchFirebase(false);
            ChangePlatformDefine(BUILD_HUAWEI);
        }

        [MenuItem("Build/Switch Platform/Amazon")]
        private static void SwitchToAmazon()
        {
            SwitchFirebase(false);
            ChangePlatformDefine(BUILD_AMAZON);
        }

        [MenuItem("Build/Switch Platform/Google Play")]
        private static void SwitchToGooglePlay()
        {
            SwitchFirebase(true);
            ChangePlatformDefine(BUILD_GOOGLE);
        }

        private static void ChangePlatformDefine(string platform)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, platform);
        }

        [MenuItem("Build/Switch Platform/Huawei", true)]
		public static bool IsHuawei()
        {
#if BUILD_HUAWEI
		    return false;
#endif
            return true;
        }

        [MenuItem("Build/Switch Platform/Amazon", true)]
		public static bool IsAmazon()
        {
#if BUILD_AMAZON
		    return false;
#endif
            return true;
        }

        [MenuItem("Build/Switch Platform/Google Play", true)]
        public static bool IsGooglePlay()
        {
#if BUILD_GOOGLE
            return false;
#endif
            return true;
        }

        private static void SetRectTransformFullScreen(RectTransform t)
        {
            t.anchorMax = Vector2.one;
            t.anchorMin = Vector2.zero;
            t.localScale = Vector3.one;
            t.anchoredPosition = Vector2.zero;
            t.sizeDelta = Vector2.zero;
        }

        [MenuItem("Build/ManualSwitchFirebase/On")]
        private static void SwitchFirebaseOn()
        {
            SwitchFirebase(true);
            CompilationPipeline.RequestScriptCompilation();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log("SwitchFirebaseOn");
        }

        [MenuItem("Build/ManualSwitchFirebase/On", true)]
        private static bool IsSwitchFirebaseOn()
        {
            return !IsFirebaseOn();
        }

        [MenuItem("Build/ManualSwitchFirebase/Off")]
        private static void SwitchFirebaseOff()
        {
            SwitchFirebase(false);
            CompilationPipeline.RequestScriptCompilation();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log("SwitchFirebaseOff");
        }

        [MenuItem("Build/ManualSwitchFirebase/Off", true)]
        private static bool IsSwitchFirebaseOff()
        {
            return IsFirebaseOn();
        }

        private const string firebasePath = @"./Assets/Firebase";

        private static void SwitchFirebase(bool enabled)
        {
            bool isOn = IsFirebaseOn();
            if (enabled && !isOn)
            {
                string[] files = Directory.GetFiles(firebasePath, "*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    Match matchDisabledMeta = Regex.Match(file, @"(?<enabledName>.+)\.disabled\.meta$");
                    if (matchDisabledMeta.Success)
                        File.Delete(file);

                    Match matchDisabled = Regex.Match(file, @"(?<enabledName>.+)\.disabled");
                    if (matchDisabled.Success)
                    {
                        string enabledName = matchDisabled.Groups["enabledName"].Value;
                        if (File.Exists(enabledName))
                            File.Delete(file);
                        else File.Move(file, enabledName);
                    }
                }
            }
            else if (!enabled && isOn)
            {
                string[] files = Directory.GetFiles(firebasePath, "*", SearchOption.AllDirectories);
                foreach (string file in files)
                    if (Regex.IsMatch(file, @"(.+\.(?!meta).*$)") && !Regex.IsMatch(file, @".+(\d\.)+meta$"))
                        File.Move(file, file + ".disabled");
            }
        }

        private static bool IsFirebaseOn()
        {
            string[] files = Directory.GetFiles(firebasePath, "*", SearchOption.AllDirectories);
            foreach (string file in files)
                if (Regex.IsMatch(file, @".+\.disabled"))
                    return false;

            return true;
        }

        private BuildTarget[] FirebaseTargets = new BuildTarget[] { BuildTarget.Android, BuildTarget.iOS };

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            if (FirebaseTargets.Contains(newTarget))
            {
                bool enabled = true;
                if (newTarget == BuildTarget.Android)
                {
                    PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, out string[] defines);
                    if (defines.Contains(BUILD_AMAZON) || defines.Contains(BUILD_HUAWEI) )
                        enabled = false;
                }

                bool isOn = IsFirebaseOn();
                if (isOn && !enabled)
                    SwitchFirebaseOff();
                else if (!isOn && enabled)
                    SwitchFirebaseOn();
            }
        }
    }

}
