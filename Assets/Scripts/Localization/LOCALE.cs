namespace Assets.Scripts.Localization
{
	public class LOCALE
	{
		public const string RU = "ru";
		public const string EN = "en";
		public const string DE = "de";
		public const string ES = "es";
		public const string FR = "fr";
		public const string PL = "pl";
		public const string PT = "pt";
		public const string PT_BR = "pt-br";
		public const string NL = "nl";
		public const string BE = "be";
		public const string TG = "tg";
		public const string UA = "ua";
		public const string UK = "uk";
		public const string KA = "ka";
		public const string KK = "kk";
		public const string KY = "ky";
		public const string TK = "tk";
		public const string UZ = "uz";
		public const string TT = "tt";
		public const string BA = "ba";
		public const string HY = "hy";
		public const string AZ = "az";
		public const string JA = "ja";
		public const string IT = "it";
		public const string ZH = "zh";
		public const string KO = "ko";
		public const string TH = "th";
		public const string TR = "tr";
		public const string HI = "hi";

		public static bool NeedSpecialFont(string locale)
		{
			return locale == JA ||
					locale == ZH ||
					locale == KO ||
					locale == TH;
		}
	}
}