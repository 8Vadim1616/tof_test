using IngameDebugConsole;
using UnityEngine.Scripting;
using Assets.Scripts;
using UnityEngine;
using Assets.Scripts.Localization;

public class DebugConsoleCommands : MonoBehaviour
{
	#region SETTINGS
	[ConsoleMethod("lang", "Изменить язык приложения", "/ Код страны в формате ISO: ru, en, es, de, ja, ..."), Preserve]
	public static void SetLocale(string locale)
	{
		GameLocalization.DEBUG_LOCALE_KEY = locale;
		GameReloader.Reload();
	}
	#endregion

}