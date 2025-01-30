using Assets.Scripts.BuildSettings;
using Assets.Scripts.Libraries.RSG;
using System;
using UnityEditor;

public class AndroidAutoBuild : AutoBuildsScript
{
	private const BuildTarget TARGET = BuildTarget.Android;
	private const BuildTargetGroup TARGET_GROUP = BuildTargetGroup.Android;
	private const string ManifestPath = PreBuildScript.AndroidManifestPath;

	[MenuItem(MENU_PATH + "Android/Test (apk)", priority = 10)]
	private static IPromise BuildAndroidTestApk()
	{
		return Build(new Func<BuildPlayerOptions>[]
		{
			() => TestApk(),
		});
	}

	[MenuItem(MENU_PATH + "Android/Release (apk)", priority = 100)]
	private static IPromise BuildAndroidReleaseApk()
	{
		return Build(new Func<BuildPlayerOptions>[]
		{
			() => ReleaseApk(),
		});
	}

	[MenuItem(MENU_PATH + "Android/Release (aab)", priority = 101)]
	private static IPromise BuildAndroidReleaseAab()
	{
		return Build(new Func<BuildPlayerOptions>[]
		{
			() => ReleaseAab(),
		});
	}

	[MenuItem(MENU_PATH + "Android/Release (apk + aab)", false, 102)]
	private static IPromise BuildAndroidReleaseApkAab()
	{
		CheckServerType(ServerTypeEnum.Release);
		return Build(new Func<BuildPlayerOptions>[]
		{
			() => ReleaseApk(),
			() => ReleaseAab(),
		});
	}


	[MenuItem(MENU_PATH + "Android/All (test + release + aab)", priority = 200)]
	private static IPromise BuildAndroidAll()
	{
		return Build(new Func<BuildPlayerOptions>[]
		{
			() => TestApk(),
			() => ReleaseApk(),
			() => ReleaseAab(),
		});
	}

	private static BuildPlayerOptions TestApk()
	{
		CheckServerType(ServerTypeEnum.Test);
		EditorUserBuildSettings.buildAppBundle = false;
		EditorUserBuildSettings.androidCreateSymbolsZip = false;
		return CreateOptions(".apk");
	}

	private static BuildPlayerOptions ReleaseApk()
	{
		CheckServerType(ServerTypeEnum.Release);
		EditorUserBuildSettings.buildAppBundle = false;
		EditorUserBuildSettings.androidCreateSymbolsZip = false;
		return CreateOptions(".apk");
	}

	private static BuildPlayerOptions ReleaseAab()
	{
		CheckServerType(ServerTypeEnum.Release);
		EditorUserBuildSettings.buildAppBundle = true;
		EditorUserBuildSettings.androidCreateSymbolsZip = CreateSymbolsZipTagOption.IsEnabled;
		return CreateOptions(".aab");
	}

	private static BuildPlayerOptions CreateOptions(string extension)
	{
		PlayerSettings.SetIl2CppCompilerConfiguration(TARGET_GROUP, Il2CppCompilerConfiguration.Release);
		EditorUserBuildSettings.androidETC2Fallback = AndroidETC2Fallback.Quality32Bit;
		return GetBuildOptions(TARGET, TARGET_GROUP, extension, BuildOptions.CompressWithLz4, manifestPath: ManifestPath);
	}
}