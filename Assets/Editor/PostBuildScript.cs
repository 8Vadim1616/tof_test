using Assets.Scripts.Localization;
using UnityEditor;
using UnityEditor.Callbacks;

public class PostBuildScript
{
	public static readonly string[] LANGS = new[]
	{
		LOCALE.EN,
		LOCALE.DE,
		//LOCALE.ES,
		//LOCALE.FR,
		//LOCALE.IT,
		LOCALE.RU,
		//LOCALE.NL,
		//LOCALE.PL,
		//LOCALE.PT,
		//LOCALE.JA
	};

	[PostProcessBuild(int.MaxValue)]
	private static void OnBuildDone(BuildTarget target, string pathToBuiltProject)
	{
#if UNITY_ANDROID
		//PostBuildAndroid.OnBuildDone(pathToBuiltProject);
#elif UNITY_IOS
		PostBuildIOS.OnBuildDone(pathToBuiltProject);
// #elif UNITY_WSA
// 		PostBuildWSA.OnBuildDone(pathToBuiltProject);
#elif UNITY_WEBGL
		PostBuildWebGl.OnBuildDone(pathToBuiltProject);
#endif
	}
}