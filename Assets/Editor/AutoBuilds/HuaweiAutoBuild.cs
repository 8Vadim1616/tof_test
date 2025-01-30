using Assets.Scripts.BuildSettings;
using Assets.Scripts.Libraries.RSG;
using System;
using UnityEditor;

public class HuaweiAutoBuild : AutoBuildsScript
{
	private const BuildTarget TARGET = BuildTarget.Android;
	private const BuildTargetGroup TARGET_GROUP = BuildTargetGroup.Android;
	private const string ManifestPath = PreBuildScript.AndroidManifestPath;

	[MenuItem(MENU_PATH + "Huawei/Test (apk)", priority = 10)]
	private static IPromise BuildHuaweiTestApk()
	{
		return Build(new Func<BuildPlayerOptions>[]
					 {
									 () => TestApk(),
					 });
	}

	[MenuItem(MENU_PATH + "Huawei/Release (apk)", priority = 100)]
	private static IPromise BuildHuaweiReleaseApk()
	{
		return Build(new Func<BuildPlayerOptions>[]
					 {
									 () => ReleaseApk(),
					 });
	}


	[MenuItem(MENU_PATH + "Huawei/All (test + release)", priority = 200)]
	private static IPromise BuildAndroidAll()
	{
		return Build(new Func<BuildPlayerOptions>[]
					 {
									 () => TestApk(),
									 () => ReleaseApk(),
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

	private static BuildPlayerOptions CreateOptions(string extension)
	{
		PlayerSettings.SetIl2CppCompilerConfiguration(TARGET_GROUP, Il2CppCompilerConfiguration.Release);
		EditorUserBuildSettings.androidETC2Fallback = AndroidETC2Fallback.Quality32Bit;
		return GetBuildOptions(TARGET, TARGET_GROUP, extension, BuildOptions.CompressWithLz4, manifestPath: ManifestPath);
	}
}