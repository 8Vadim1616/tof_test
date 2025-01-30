using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Localization
{
    public class GameLocalization
    {
		public const string DEFAULT = LOCALE.EN;

		public static string[] AvailableLangs =
		{
			LOCALE.EN,
		};

		public static string[] RU_LANG_AVAILABLE =
		{
			LOCALE.RU,
			LOCALE.BE,
			LOCALE.TG,
			LOCALE.UA,
			LOCALE.UK,
			LOCALE.KA,
			LOCALE.KK,
			LOCALE.KY,
			LOCALE.TK,
			LOCALE.UZ,
			LOCALE.TT,
			LOCALE.BA,
			LOCALE.HY,
			LOCALE.AZ
		};
		
        public static event EventHandler OnChangeLocaleEvent;

        public static string Locale { get; set; } = LOCALE.EN;

        public static bool IsLocaleEN => Locale == LOCALE.EN;
        public static bool IsLocaleRU => Locale == LOCALE.RU;
        public static bool IsLocaleZH => Locale == LOCALE.ZH;

		/// <summary>
		/// Для теста приложения на разных языках, переключалка на лету.
		/// !!! ДОЛЖЕН БЫТЬ NULL !!! Не менять !!!
		/// </summary>
		public static string DEBUG_LOCALE_KEY = null;

        public Dictionary<string, string> LocalizePairs { get; private set; } = new Dictionary<string, string>();
        private readonly Regex paramRegex = new Regex(@"@\d");

        public GameLocalization()
        {

        }
		
		public static void InitLocale()
		{
			Locale = GetApplicationLocale();
			GameLogger.debug("InitLocale: " + Locale);
		}

		private static string GetApplicationLocale()
		{
			if (!DEBUG_LOCALE_KEY.IsNullOrEmpty())
				return DEBUG_LOCALE_KEY;
			
			if (!UserRegisterData.GetLocale().IsNullOrEmpty())
				return UserRegisterData.GetLocale();
			
			// if (Game.Social.Adapter != null)
			// 	return Game.Social.Adapter.Locale;
			
			var locale = GetValidLanguageCode(CultureInfo.InstalledUICulture);
			return string.IsNullOrEmpty(locale) ? DEFAULT : locale;
		}

		private static string GetValidLanguageCode(CultureInfo cultureInfo)
		{
			var systemLanguage = cultureInfo.ToString().ToLower();

			for (int i = 0; i < 2; i++)
			{
				if (AvailableLangs.Contains(systemLanguage))
					return systemLanguage;

				systemLanguage = cultureInfo.TwoLetterISOLanguageName;
			}

			return string.Empty;
		}
		
		public static void AddLangsFromServer(string[] arr)
		{
			if (arr != null)
				AvailableLangs = AvailableLangs
								.Concat(arr)
								.Distinct()
								.ToArray();
		}

        public void Load(Dictionary<string, string> data)
        {
            LocalizePairs = data;
            LocalizationLoaded.ResolveOnce();
        }

        public string Localize(GameObject target, string key, params string[] parameters)
        {
            if (key == null)
            {
                Debug.LogError("Localize key is null\n" + new Exception().StackTrace);
                return "@null";
            }

            string result = "@" + key;
            
            if (LocalizePairs.ContainsKey(key))
            {
                result = LocalizePairs[key];
            }

            if (parameters != null && parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    result = result.Replace("@" + (i + 1), parameters[i] ?? String.Empty);
                }
            }

            result = paramRegex.Replace(result, "");

            result = result.Replace("<br>", Environment.NewLine);
            result = result.Replace("\\n", Environment.NewLine);

            return result;
        }

        public bool ContainsKey(string key) => LocalizePairs.ContainsKey(key);

        public string Localize(string key, params string[] parameters)
        {
            return Localize(null, key, parameters);
        }

        public int GetKeysCount(string startWith)
        {
            if (startWith[startWith.Length - 1] != '_')
                startWith += '_';

            var result = 0;
            while (LocalizePairs.ContainsKey(startWith + (result + 1)))
                result++;

            return result;
        }
		
		public static string GetLocaleByCapabilities(string checkCurrent = null)
		{
			var locale = checkCurrent != null ? checkCurrent : Utils.Utils.To2LetterISOCode(Application.systemLanguage).ToLower();

			if (RU_LANG_AVAILABLE.Contains(locale))
				locale = LOCALE.RU;
			else if (!AvailableLangs.Contains(locale))
				locale = LOCALE.EN;

			return locale;
		}

		public static string GetValidLocale()
		{
			if (AvailableLangs.Contains(Locale, StringComparer.OrdinalIgnoreCase))
				return Locale;
			return LOCALE.EN;
		}

		public Promise LocalizationLoaded { get; } = new Promise();

		public static void SetLocale(string locale)
		{
			UserRegisterData.SaveLocale(locale);
			Game.User.Settings.LangPref = locale;

			Locale = locale;
			//TMP_Text.SetLocale(locale);

			UserData.SaveLocale();

			Game.Loader.Show();
			Game.Locker.Lock("GameLoaclization.SetLocal");
			Game.Windows.CloseAllScreensPromise("GameLocalization.SetLocal")
				.Then(() =>
				{
					Game.Loader.Hide();
					Game.Locker.Unlock("GameLoaclization.SetLocal");
				})
			    //.Then(Game.MapLoader.UnloadCurrentMap)
				.Then(() => GameReloader.Reload(false, true))
				.Finally(() =>
				{
					Game.Loader.Hide();
					Game.Locker.Unlock("GameLoaclization.SetLocal");
				});
		}
	}
}
