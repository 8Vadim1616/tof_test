#if UNITY_IOS
using System.IO;
using UnityEditor.iOS.Xcode;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

public class PostBuildIOS
{
	public static void OnBuildDone(string pathToBuiltProject)
	{
		AddPushNotifications(pathToBuiltProject);
		AddSwiftPath(pathToBuiltProject);
		RemoveSwiftSymbolsFromUnityFramework(pathToBuiltProject);
		ModifyInfoPList(pathToBuiltProject);
	}

	private static void ModifyInfoPList(string pathToBuiltProject)
	{
		string path = pathToBuiltProject + "/info.plist";
		var plist = new PlistDocument();
		plist.ReadFromFile(path);
		var root = plist.root;
		CFBundleLocalizations(root);
		ITSAppUsesNonExemptEncryption(root);
		CFBundleVersion(root);
		SetTrackingUsageDescription(root);
		AppsFlyerSwizzle(root);
		NSAdvertisingAttributionReportEndpoint(root);
		FirebaseMessagingAutoInitEnabled(root);
		AddSKADNetworks(root);
		AddNSAppTransportSecurity(root);
		plist.WriteToFile(path);
	}

	private static void CFBundleLocalizations(PlistElementDict root)
	{
		var array = root.CreateArray("CFBundleLocalizations");
		foreach (var localization in PostBuildScript.LANGS)
			array.AddString(localization);
	}

	private static void SetTrackingUsageDescription(PlistElementDict root)
	{
		root.SetString("NSUserTrackingUsageDescription",
			"Your data will be used to provide you a better and personalized ad experience.");
	}

	private static void CFBundleVersion(PlistElementDict root)
	{
		root.SetString("CFBundleVersion", PlayerSettings.bundleVersion);
	}

	/**Проставляет ITSAppUsesNonExemptEncryption = false, для автоокрытия версии в TestFlight*/
	private static void ITSAppUsesNonExemptEncryption(PlistElementDict root)
	{
		root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
	}

	/**Fix перехвата авторизаций в соц.сетях апсфлаером
	 * https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/blob/master/docs/iOS-Swizzling-Guide.md
	 */
	private static void AppsFlyerSwizzle(PlistElementDict root)
	{
		root.SetBoolean("AppsFlyerShouldSwizzle", true);
	}

	/** Проставляет NSAdvertisingAttributionReportEndpoint, для AppsFlyer*/
	private static void NSAdvertisingAttributionReportEndpoint(PlistElementDict root)
	{
		root.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");
	}

	/** Проставляет FirebaseMessagingAutoInitEnabled = false, для избегания автоинита Firebase*/
	private static void FirebaseMessagingAutoInitEnabled(PlistElementDict root)
	{
		root.SetBoolean("FirebaseMessagingAutoInitEnabled", false);
	}

	private static void AddPushNotifications(string pathToBuiltProject)
	{
		Debug.Log("[AddPushNotifications] ProcessPostBuild - iOS - Adding Push Notification capabilities.");

		// get XCode project path
		string pbxPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);

		// Add linked frameworks
		PBXProject pbxProject = new PBXProject();
		pbxProject.ReadFromString(File.ReadAllText(pbxPath));
		string targetGUID = pbxProject.GetUnityMainTargetGuid();
		pbxProject.AddFrameworkToProject(targetGUID, "UserNotifications.framework", false);
		File.WriteAllText(pbxPath, pbxProject.WriteToString());

		// Add required capabilities: Push Notifications, and Remote Notifications in Background Modes
		var isDevelopment = Debug.isDebugBuild;
		var capabilities = new ProjectCapabilityManager(pbxPath, "app.entitlements", "Unity-iPhone");
		capabilities.AddPushNotifications(isDevelopment);
		capabilities.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
		capabilities.AddSignInWithApple();
		capabilities.WriteToFile();
	}

	private static void AddSwiftPath(string pathToBuiltProject)
	{
		Debug.Log("[AddSwift] ProcessPostBuild - iOS - Adding Swift.");

		// get XCode project path
		string pbxPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);

		// Add linked frameworks
		PBXProject pbxProject = new PBXProject();
		pbxProject.ReadFromString(File.ReadAllText(pbxPath));
		pbxProject.AddBuildProperty(pbxProject.ProjectGuid(), "LIBRARY_SEARCH_PATHS", "$(SDKROOT)/usr/lib/swift");
		File.WriteAllText(pbxPath, pbxProject.WriteToString());
	}

	private static void RemoveSwiftSymbolsFromUnityFramework(string pathToBuiltProject)
	{
		Debug.Log("[AddSwift] ProcessPostBuild - iOS - Remove Swift Symbols From UnityFramework.");

		string pbxPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);

		PBXProject pbxProject = new PBXProject();
		pbxProject.ReadFromString(File.ReadAllText(pbxPath));
		pbxProject.SetBuildProperty(pbxProject.GetUnityFrameworkTargetGuid(), "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
		File.WriteAllText(pbxPath, pbxProject.WriteToString());
	}

	private static void AddSKADNetworks(PlistElementDict root)
	{
		Debug.Log("[AddSKADNetworks] AddSKADNetworks - iOS - Adding SKAD Networks.");

		// Check if NSAppTransportSecurity already exists
		PlistElementArray SKAdNetworkItems = null;
		if (root.values.ContainsKey("SKAdNetworkItems"))
		{
			try
			{
				SKAdNetworkItems = root.values["SKAdNetworkItems"] as PlistElementArray;
			}
			catch (Exception e)
			{
				Debug.LogWarning(string.Format("Could not obtain SKAdNetworkItems PlistElementDict: {0}", e.Message));
			}
		}

		// If not exists, create it
		if (SKAdNetworkItems == null)
			SKAdNetworkItems = root.CreateArray("SKAdNetworkItems");

		foreach (var network in PreBuildScript.SKADToAdd)
		{
			//if (!SKAdNetworkItems.values.Any(x => x is PlistElementString plistString && plistString.value == network))
			SKAdNetworkItems.AddString(network);
		}
	}

	private static void AddNSAppTransportSecurity(PlistElementDict root)
	{
		Debug.Log("[AddNSAppTransportSecurity] NSAppTransportSecurity - iOS - Add NSAllowsArbitraryLoads.");

		// Check if NSAppTransportSecurity already exists
		PlistElementDict NSAppTransportSecurity = null;
		if (root.values.ContainsKey("NSAppTransportSecurity"))
		{
			try
			{
				NSAppTransportSecurity = root.values["NSAppTransportSecurity"] as PlistElementDict;
			}
			catch (Exception e)
			{
				Debug.LogWarning(string.Format("Could not obtain SKAdNetworkItems PlistElementDict: {0}", e.Message));
			}
		}

		// If not exists, create it
		if (NSAppTransportSecurity == null)
			NSAppTransportSecurity = root.CreateDict("NSAppTransportSecurity");

		NSAppTransportSecurity.SetBoolean("NSAllowsArbitraryLoads", true);
	}
}
#endif