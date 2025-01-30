using Assets.Scripts.BuildSettings;
using Assets.Scripts.Localization;
using Assets.Scripts.Platform.Adapter;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Core
{
	public class LocalSocialSettings
	{
		public static string SOCIAL_NETWORK = SocialNetwork.VKONTAKTE;

		public static string LocalUid = "23";
		public static string LocalAuthKey = "32b2c23ec38c0ec415fa9240d6ae73ee";

		public static string Locale = LOCALE.RU;


		public static JObject GetFakeInitData()
		{
			var result = new JObject();

			result["sn"] = SOCIAL_NETWORK;
			result["viewer_id"] = LocalUid;
			result["auth_key"] = LocalAuthKey;
			result["locale"] = Locale;
			result["isLocal"] = true;
			result["server"] = GameConsts.ServerEntryPoint;

			// result["sn"] = "ok";
			// result["api_server"] = "https://api.ok.ru/";
			// result["application_key"] = "CBAIOEGLEBABABABA";
			// result["auth_sig"] = "5ccd960488f1ba4036b2986129eea48f";
			// result["authorized"] = "1";
			// result["logged_user_id"] = "564969551729";
			// result["session_key"] = "-s-2m-RF7ZuNn3PK6VTpLU.FA3vpjd0p83yMHbRrh3WuHcQK7z.Ro8xJkUzRMXPHhVSNpVQD9X-qh8PI7ZvMMbVGiV3";
			// result["session_secret_key"] = "824fa41864763af6feb648fd822e7005";
			// result["commun"] = "53192795422859";
			// result["server"] = "https://develop2.playme8.ru/farm3/";
			// result["multi_uid"] = "230";
			// result["multi_auth_key"] = "f962fd198098fe39ffa43b1318774dca";

			return result;
		}
	}
}