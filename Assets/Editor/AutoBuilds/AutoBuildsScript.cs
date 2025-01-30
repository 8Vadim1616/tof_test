using Assets.Scripts.BuildSettings;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.UI.WindowsSystem.Editor;
using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class AutoBuildsScript
{
	protected const string MENU_PATH = "Build/Auto/";
	private const string TAG = "<b><color=green>[BUILD]</color></b> ";
	private static readonly string[] BUILD_SCENES = new[] { "Assets/Scenes/Main.unity" };
	private static string GetBuildPath(string directory) => $"./bin/auto/{directory}/{PreBuildScript.GetBuildName()}";
	private static ServerTypeEnum ServerType => PreBuildScript.SelectServerTypeByVersion(PlayerSettings.bundleVersion);

	protected static void CheckServerType(ServerTypeEnum serverType, bool forceAutoIncrement = false)
	{
		var autoIncrement = forceAutoIncrement || BuildAutoIncrementOption.IsEnabled;

		if (!autoIncrement && ServerType != serverType)
			throw new BuildVersionException(serverType);

		var version = new BuildVersionHandler(PlayerSettings.bundleVersion);
		if (serverType != ServerType)
		{
			version.Build++;
			PlayerSettings.bundleVersion = version.ToString();
			CheckServerType(serverType, false);
		}
	}

	protected static IPromise Build(Func<BuildPlayerOptions>[] optionList, bool forceUpdateStaticData = false)
	{
		Debug.Log(TAG + $"Start build items: {optionList.Length}, force update static: {forceUpdateStaticData}");
		return CheckStatic()
			.Then(BuildPlayers);

		IPromise CheckStatic()
		{
			if (forceUpdateStaticData || BuildAutoStaticUpdateOption.IsEnabled)
				return EditorStaticDataUpdater.UpdateStaticData();
			return Promise.Resolved();
		}

		IPromise BuildPlayers()
		{
			for (int i = 0; i < optionList.Length; i++)
			{
				var options = optionList[i]();
				EditorUserBuildSettings.SwitchActiveBuildTarget(options.targetGroup, options.target);
				var build = BuildPipeline.BuildPlayer(options);
				var summary = build.summary;
				if (summary.result == BuildResult.Succeeded)
				{
					Debug.Log(TAG + $"{i + 1}/{optionList.Length} Build {summary.result}: {summary.outputPath} / {summary.totalSize} bytes / {summary.totalTime}");
				}
				else
				{
					Debug.LogError(TAG + $"{i + 1}/{optionList.Length} Build {summary.result}: (total errors: {summary.totalErrors})");
					return Promise.Rejected(null);
				}
			}

			Debug.Log(TAG + $"Build ended");
			return Promise.Resolved();
		}
	}

	protected static BuildPlayerOptions GetBuildOptions(BuildTarget target, BuildTargetGroup targetGroup, string fileExt, BuildOptions options = BuildOptions.None, string path = null, string manifestPath = null) =>
		new BuildPlayerOptions
		{
			scenes = BUILD_SCENES,
			locationPathName = (path ?? GetBuildPath(target.ToString())) + fileExt,
			target = target,
			targetGroup = targetGroup,
			options = options,
			assetBundleManifestPath = manifestPath
		};

	public static class BuildAutoIncrementOption
	{
		private const string PATH = MENU_PATH + "Авто коррекция номера версии";
		private const string PREF_KEY = "ab_inc";

		public static bool IsEnabled
		{
			get => EditorPrefs.GetBool(PREF_KEY, true);
			set => EditorPrefs.SetBool(PREF_KEY, value);
		}

		[MenuItem(PATH, false, 0)]
		private static void Toggle() => IsEnabled = !IsEnabled;

		[MenuItem(PATH, true)]
		private static bool Validate()
		{
			Menu.SetChecked(PATH, IsEnabled);
			return true;
		}
	}

	public static class BuildAutoStaticUpdateOption
	{
		private const string PATH = MENU_PATH + "Авто обновление статики перед сборкой";
		private const string PREF_KEY = "ab_static_upd";

		public static bool IsEnabled
		{
			get => EditorPrefs.GetBool(PREF_KEY, true);
			set => EditorPrefs.SetBool(PREF_KEY, value);
		}

		[MenuItem(PATH, false, 1)]
		private static void Toggle() => IsEnabled = !IsEnabled;

		[MenuItem(PATH, true)]
		private static bool Validate()
		{
			Menu.SetChecked(PATH, IsEnabled);
			return true;
		}
	}

	public static class BuildAutoGitTagOption
	{
		private const string PATH = MENU_PATH + "Проставлять тег с версией сборки в Git";
		private const string PREF_KEY = "ab_git_tag";

		public static bool IsEnabled
		{
			get => EditorPrefs.GetBool(PREF_KEY, true);
			set => EditorPrefs.SetBool(PREF_KEY, value);
		}

		[MenuItem(PATH, false, 2)]
		private static void Toggle() => IsEnabled = !IsEnabled;

		[MenuItem(PATH, true)]
		private static bool Validate()
		{
			Menu.SetChecked(PATH, IsEnabled);
			return true;
		}
	}

	public static class CreateSymbolsZipTagOption
	{
		private const string PATH = MENU_PATH + "Создать symbols.zip для рабочей версии";
		private const string PREF_KEY = "create_symb_tag";

		public static bool IsEnabled
		{
			get => EditorPrefs.GetBool(PREF_KEY, true);
			set => EditorPrefs.SetBool(PREF_KEY, value);
		}

		[MenuItem(PATH, false, 2)]
		private static void Toggle() => IsEnabled = !IsEnabled;

		[MenuItem(PATH, true)]
		private static bool Validate()
		{
			Menu.SetChecked(PATH, IsEnabled);
			return true;
		}
	}
}