using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Assets.Scripts.BuildSettings;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.Build.Reporting;
using UnityEditor.Purchasing;
using UnityEngine;
using UnityEngine.Purchasing;

public class PreBuildScript : UnityEditor.Editor, IPreprocessBuildWithReport
{
	/// <summary>
	/// Запускаем позже чем <seealso cref="CreateUI.callbackOrder"/>
	/// </summary>
	public int callbackOrder => 1;

	const string KEY = "!!BQNQZ5ZB6yBQNQZ5ZB6y&&";
	const string KEY_HUAWEI = "BQNQZ5ZB6y";

	public void OnPreprocessBuild(BuildReport report)
	{
		Debug.Log("[PreBuildScript] Server type: " + BuildSettings.ServerType);

		SetupPlayerSettings();
	}

	[MenuItem("Build/Setup Player Settings")]
	private static async void SetupPlayerSettings()
	{
		CheckSKAd();
		ChangeServerType();
		SetupEditorSettings();

		CopyBuildNameToClipboard();
		// SetupAddressableSettings();
		// RebuildAddressables();
		DisableUnityLogo();
		ChangePackageName();
		ChangeCompanyName();
		ChangePublishingVersion();
		await ChangeKeystore();
		ChangeStore();
		ChangeArchitecture();
		SelectAndroidManifest();
		
		PrepareFb();
		PrepareVk();
		PrepareGoogleSignIn();
		
		if (AutoBuildsScript.BuildAutoGitTagOption.IsEnabled)
			AddGitTag();
		Debug.Log("Player Setting is set up");
	}

	public static readonly List<string> SKADToAdd = new List<string>()
		{
			"5lm9lj6jb7.skadnetwork",
			"x44k69ngh6.skadnetwork",
			"mp6xlyr22a.skadnetwork",
			"f73kdq92p3.skadnetwork",
			"tl55sbb4fm.skadnetwork",
			"f7s53z58qe.skadnetwork",
			"32z4fx6l9h.skadnetwork",
			"mlmmfzh3r3.skadnetwork",
			"w9q455wk68.skadnetwork",
			"m8dbw4sv7c.skadnetwork",
			"v79kvwwj4g.skadnetwork",
			"wg4vff78zm.skadnetwork",
			"5tjdwbrq8w.skadnetwork",
			"glqzh8vgby.skadnetwork",
			"488r3q3dtq.skadnetwork",
			"44jx6755aq.skadnetwork",
			"3rd42ekr43.skadnetwork",
			"22mmun2rn5.skadnetwork",
			"zmvfpc5aq8.skadnetwork",
			"k674qkevps.skadnetwork",
			"4pfyvq9l8r.skadnetwork",
			"238da6jt44.skadnetwork",
			"lr83yxwka7.skadnetwork",
			// <!-- Audience Network. -->
			"v9wttpbfk9.skadnetwork",
			"n38lu8286q.skadnetwork",
			// <!-- Iron Source. -->
			"su67r6k2v3.skadnetwork"
		};

	[MenuItem("Build/CheckSKAd")]
	private static void CheckSKAd()
	{
		const string KEY_SK_ADNETWORK_ID = "SKAdNetworkIdentifier";
		const string SKADNETWORKS_RELATIVE_PATH = "GoogleMobileAds/Editor/GoogleMobileAdsSKAdNetworkItems.xml";

		List<string> skAdNetworkItems = new List<string>();

		string path = Path.Combine(Application.dataPath, SKADNETWORKS_RELATIVE_PATH);

		if (AssetDatabase.IsValidFolder("Packages/com.google.ads.mobile"))
		{
			path = Path.Combine("Packages/com.google.ads.mobile", SKADNETWORKS_RELATIVE_PATH);
		}

		try
		{
			if (!File.Exists(path))
			{
				return;
			}

			XmlDocument document = new XmlDocument();
			document.Load(path);

			XmlNode root = document.FirstChild;

			XmlNodeList nodes = root.SelectNodes(KEY_SK_ADNETWORK_ID);

			foreach (XmlNode node in nodes)
				skAdNetworkItems.Add(node.InnerText);

			bool hasNew = false;

			foreach (var addItem in SKADToAdd)
			{
				if (!skAdNetworkItems.Contains(addItem))
				{
					Debug.Log("[CheckSKAd] new " + addItem);
					skAdNetworkItems.Add(addItem);
					hasNew = true;
				}

			}

			if (hasNew)
			{
				root.RemoveAll();

				foreach (string str in skAdNetworkItems)
				{
					XmlElement elem = document.CreateElement(KEY_SK_ADNETWORK_ID);
					elem.InnerText = str;
					root.AppendChild(elem);
				}

				document.Save(path);
			}
		}
		catch (Exception e)
		{
			Debug.Log(e);
		}
	}

	private static void ChangeServerType()
	{
		if (BuildSettings.GameType == GameTypeEnum.Game)
			BuildSettings.ServerType = SelectServerTypeByVersion(PlayerSettings.bundleVersion);
		else
			BuildSettings.ServerType = ServerTypeEnum.Test;

		//BuildSettings.GameType = GameTypeEnum.Game;

			//if (BuildSettings.ServerType == ServerTypeEnum.Test)
			//	EditorUserBuildSettings.buildAppBundle = false;
	}

	private static void SetupEditorSettings()
	{
		if (BuildSettings.IsEditor)
		{
			PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
			PlayerSettings.resizableWindow = true;
			PlayerSettings.runInBackground = true;
			PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
		}
		else
		{
			PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
			PlayerSettings.resizableWindow = false;
			PlayerSettings.runInBackground = false;
		}
	}

	public static void AddGitTag()
	{
		var tag = $"{PlayerSettings.bundleVersion}_{GetPlatform()}{GetBranch()}";
		GitUtils.RunGitCommand("tag " + tag);
		GitUtils.RunGitCommand("push origin " + tag);
	}

	public static string GetMajorVersion()
	{
		var vers = PlayerSettings.bundleVersion.Split('.');
		return vers.Length > 0 ? vers[0] : "";
	}

	public static string GetMinorVersion()
	{
		var vers = PlayerSettings.bundleVersion.Split('.');
		return vers.Length > 1 ? vers[1] : "";
	}

	public static string GetMaintenanceVersion()
	{
		var vers = PlayerSettings.bundleVersion.Split('.');
		return vers.Length > 2 ? vers[2] : "";
	}

	public static string GetBranch()
	{
		var branch = GitUtils.RunGitCommand(" branch --show-current");
		branch = branch.Replace("\n", "");
		
		if (branch.Contains("BUILD") || branch.Equals("master"))
			branch = "";
		else
			branch = "_" + branch;
		return branch;
	}

	private static string GetPlatform()
	{
		var platform = "google";
		
#if BUILD_HUAWEI
		platform = "huawei";
#elif BUILD_AMAZON
		platform = "amazon";
#elif UNITY_IOS
		platform = "ios";
#elif UNITY_WSA
		platform = "wsa";
#endif
		return platform;
	}

	/** Прописывает нужные id в зависимости от билда **/
	public static void PrepareFb()
	{
		/*
		var id = GameConsts.FacebookAppId;
		Facebook.Unity.Settings.FacebookSettings.AppIds = new List<string>(new[] {id});
		Facebook.Unity.Settings.FacebookSettings.AndroidKeystorePath = PlayerSettings.Android.keystoreName;

		AddScheme("fb", $"fb{id}");
		*/
	}

	/** Прописывает нужные id в зависимости от билда **/
	public static void PrepareVk()
	{
		var id = GameConsts.VkAppId;

		var vkSettingses = FindAssetsWithType<VkSettings>();

		if (vkSettingses.Count > 0)
		{
			var vkSettings = vkSettingses[0];

			vkSettings.VkAppId = id;

			// Scopes
			vkSettings.friends = true;
			vkSettings.wall = true;
			// vkSettings.notify = true;
			vkSettings.groups = true;
		}

		AddScheme("vk", $"vk{id}");
	}

	public static void PrepareGoogleSignIn()
	{
		var id = GameConsts.GOOGLE_SIGN_IN_CLIENT_ID_IOS_REVERSE;

		AddScheme("com.googleusercontent.apps", id);
	}

	public static ServerTypeEnum SelectServerTypeByVersion(string bundleVersion)
	{
		ServerTypeEnum serverTypeDefault = ServerTypeEnum.Test;
		if (int.TryParse(bundleVersion.Substring(bundleVersion.Length - 1), out int lastDigit))
		{
			if (lastDigit % 2 == 0)
				serverTypeDefault = ServerTypeEnum.Release;
			else
				serverTypeDefault = ServerTypeEnum.Test;
		}

		return serverTypeDefault;
	}

	private static void SetupAddressableSettings()
	{
		AddressableAssetSettingsDefaultObject.Settings.DisableCatalogUpdateOnStartup = false;
	}

	public const string AndroidManifestPath = @"./Assets/Plugins/Android/AndroidManifest.xml";
	private static void SelectAndroidManifest()
	{
		string fromPath = @"./Assets/Plugins/Android/AndroidManifest_google.xml";
#if BUILD_AMAZON
		fromPath = @"./Assets/Plugins/Android/AndroidManifest_amazon.xml";
#elif BUILD_HUAWEI
		fromPath = @"./Assets/Plugins/Android/AndroidManifest_huawei.xml";
#endif

		File.Delete(AndroidManifestPath);
		File.Copy(fromPath, AndroidManifestPath);
	}

	private static void AddScheme(string startKey, string val)
	{
		var schemes = PlayerSettings.iOS.iOSUrlSchemes;
		var index = 0;
		var wasAdd = false;

		foreach (var scheme in schemes)
		{
			if (scheme.IndexOf(startKey) == 0)
			{
				schemes[index] = val;
				wasAdd = true;
				break;
			}

			index++;
		}

		if (!wasAdd)
			schemes = schemes.Append(val).ToArray();

		PlayerSettings.iOS.iOSUrlSchemes = schemes;
	}

	private static void DisableUnityLogo()
	{
		PlayerSettings.SplashScreen.showUnityLogo = false;
	}

	private static void ChangePackageName()
	{
#if BUILD_HUAWEI
		Change(GameConsts.HMS_PACKAGE_NAME);
		SetMinAPILevel(AndroidSdkVersions.AndroidApiLevel21);
		SetTargetAPILevel((AndroidSdkVersions) 31);
#elif BUILD_AMAZON
		Change(GameConsts.AMAZON_PACKAGE_NAME);
		SetMinAPILevel(AndroidSdkVersions.AndroidApiLevel19);
		SetTargetAPILevel((AndroidSdkVersions) 30);
#elif UNITY_IOS
		Change(GameConsts.IOS_PACKAGE_NAME);
		SetTargetAPILevel((AndroidSdkVersions) 30);
#else
		Change(GameConsts.DEF_PACKAGE_NAME);
		//SetMinAPILevel(AndroidSdkVersions.AndroidApiLevel21);
		//SetTargetAPILevel((AndroidSdkVersions) 33);
#endif
	}

	private static void ChangeCompanyName()
	{
#if UNITY_WSA
		PlayerSettings.companyName = "ПлейМи8";
#else
		PlayerSettings.companyName = "Playgenes";
#endif
	}

	private static void ChangePublishingVersion()
	{
		if (Version.TryParse(PlayerSettings.bundleVersion, out Version version))
		{
			// Microsoft
			if (version.Revision > 0)
				Debug.LogWarning($"Версия для Windows Store должна заканчиваться на .0, сейчас {version}");
			PlayerSettings.WSA.packageVersion = SetAllDigitsInVersion(version);

			// Android
			int bundleVersionCode = version.Major * 1000000 + version.Minor * 1000 + version.Build;
			PlayerSettings.Android.bundleVersionCode = bundleVersionCode;

			// iOS
			PlayerSettings.iOS.buildNumber = PlayerSettings.bundleVersion;
		}
		else
		{
			Debug.LogError($"Can't parse version {PlayerSettings.bundleVersion}");
		}
	}

	private static Version SetAllDigitsInVersion(Version version)
	{
		int major = version.Major >= 0 ? version.Major : 0;
		int minor = version.Minor >= 0 ? version.Minor : 0;
		int build = version.Build >= 0 ? version.Build : 0;
		int revision = version.Revision >= 0 ? version.Revision : 0;
		return new Version(major, minor, build, revision);
	}

	private static async Task ChangeKeystore()
	{
#if BUILD_HUAWEI
		PlayerSettings.Android.keystoreName = "doc/huawei.keystore";
#else
		PlayerSettings.Android.keystoreName = "doc/google_play_key.keystore";
#endif


		bool needKeySetup = true;
		const int maxAttempts = 3;
		int attemptCount = 0;
		const int millisecondsDelay = 300;

		var needKey = KEY;

#if BUILD_HUAWEI
		needKey = KEY_HUAWEI;
#endif
		
		while (needKeySetup)
		{
			await Task.Delay(millisecondsDelay);
			
			PlayerSettings.Android.keystorePass = needKey;
			
			await Task.Delay(millisecondsDelay);
			
#if BUILD_HUAWEI
			PlayerSettings.Android.keyaliasName = "PlayMe8";
#else
			PlayerSettings.Android.keyaliasName = "com.playgenes.solitaire";
#endif
			
			await Task.Delay(millisecondsDelay);
			
			PlayerSettings.Android.keyaliasPass = needKey;
			
			await Task.Delay(millisecondsDelay);
			
			if (PlayerSettings.Android.keystorePass == needKey && PlayerSettings.Android.keyaliasPass == needKey)
				needKeySetup = false;
			else
				Debug.LogWarning("Ключ не установлен, подождите");

			attemptCount++;

			if (attemptCount >= maxAttempts)
			{
				needKeySetup = false;
				Debug.LogError("Не удалось ввести ключ для Android->Publishing Settings->Custom Keystore");
			}
		}
	}

	private static void ChangeStore()
	{
#if BUILD_AMAZON
		UnityPurchasingEditor.TargetAndroidStore(AppStore.AmazonAppStore);
#else
		UnityPurchasingEditor.TargetAndroidStore(AppStore.GooglePlay);
#endif
	}

	private static void ChangeArchitecture()
	{
		PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
	}

	private static void Change(string package)
	{
#if UNITY_IOS
		PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, package);
#else
		PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, package);
#endif
	}

	private static void SetMinAPILevel(AndroidSdkVersions versions)
	{
		PlayerSettings.Android.minSdkVersion = versions;
	}

	private static void SetTargetAPILevel(AndroidSdkVersions versions)
	{
		PlayerSettings.Android.targetSdkVersion = versions;
	}

	private static void RebuildAddressables()
	{
		SetDefaultAddressableDataBuilder();
		CleanAddressables();
		CleanBuildCache();
		BuildAddressables();
	}

	private static void CleanBuildCache() =>
		BuildCache.PurgeCache(prompt: false);

	private static void SetDefaultAddressableDataBuilder()
	{
		if (!AddressableAssetSettingsDefaultObject.SettingsExists)
			return;

		const string targetDataBuilder = "BuildScriptPackedMode";

		for (int i = 0; i < AddressableAssetSettingsDefaultObject.Settings.DataBuilders.Count; i++)
		{
			if (AddressableAssetSettingsDefaultObject.Settings.DataBuilders[i].name == targetDataBuilder)
			{
				AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilderIndex = i;
				return;
			}
		}

		string message = $"Ошибка! Не найден подходящий сборщик адрессаблов = {targetDataBuilder}.";
		Debug.LogError(message);
	}

	private static void CopyBuildNameToClipboard()
	{
		EditorGUIUtility.systemCopyBuffer = GetBuildName();
	}

	public static string GetBuildName()
	{
		var appName = PlayerSettings.productName.Replace(' ', '_').ToLower();

		string bundleVersion = PlayerSettings.bundleVersion;
		ServerTypeEnum serverType = SelectServerTypeByVersion(bundleVersion);

		var serverTypeString = serverType switch
		{
			ServerTypeEnum.Release => "release",
			ServerTypeEnum.Test => "pred",
			_ => ""
		};

		string devPostFix = EditorUserBuildSettings.development ? "_dev" : "";

		string buildName = $"{appName}_{GetPlatform()}_{serverTypeString}_{bundleVersion}{GetBranch()}{devPostFix}";

		return buildName;
	}
	
	public static List<T> FindAssetsWithType<T>() where T : UnityEngine.Object
	{
		var result = new List<T>();

		var guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));

		for( int i = 0; i < guids.Length; i++ )
		{
			string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
			T asset = AssetDatabase.LoadAssetAtPath<T>( assetPath );
			if( asset != null )
			{
				result.Add(asset);
			}
		}

		return result;
	}

	private static void BuildAddressables() =>
		AddressableAssetSettings.BuildPlayerContent();

	private static void CleanAddressables() =>
		AddressableAssetSettings.CleanPlayerContent();
}