using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utils
{
    public static class CurrencyCodeMapper
    {
        private static readonly Dictionary<string, string> StringBySymbol;

		private static char[] SymbolsInAtlas =
		{
			'$', '€', 'K', 'č', '￥', 'z', 'ł', '₽', 'L', 'e', 'k', 'ë', '₴', '₹', 's', 'o', 'm', '¥', '£'
		};

        public static string GetSymbol(string code) { return StringBySymbol[code]; }

        public static string CheckCurrencies(this string inputString)
        {
			//return inputString;

			if (inputString == null) return null;
            
            var outputString = inputString;
            foreach (var pair in StringBySymbol)
                outputString = outputString.Replace(pair.Key, pair.Value);

            return outputString;
        }

        private static bool IsNormalSymbol(char c)
        {
            int code = c;

            if ((code > 96 && code < 123) || (code > 64 && code < 91) ||
                (code >= 1040 && code <= 1071)  || (code >= 1072 && code <= 1103) ||  
                char.IsDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c) || char.IsSeparator(c) ||
				SymbolsInAtlas.Contains(c))
                return true;

            return false;
        }

        static CurrencyCodeMapper()
        {
            StringBySymbol = new Dictionary<string, string>();

            var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(x => new RegionInfo(x.LCID));

            foreach (var region in regions)
                if (!region.CurrencySymbol.All(IsNormalSymbol) && !StringBySymbol.ContainsKey(region.CurrencySymbol))
                    StringBySymbol.Add(region.CurrencySymbol, region.ISOCurrencySymbol);

			StringBySymbol["грн"] = "₴";
			StringBySymbol["грн."] = "₴";
			//StringBySymbol["\u20BD"] = "руб.";
			//StringBySymbol.Remove("$");// = "$";
		}
	}
}
