using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Assets.Scripts.Static.Items;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public static class TextFormatting
    {
        /// <summary>
        /// Получить цену в валюте с ее международным названием
        /// </summary>
        /// <param name="???"></param>
        /// <returns></returns>
        public static string GetCurrencyText(float val,
                                            string currency,
                                            float userCurrencyExchange,
                                            bool needFract = true,
                                            bool needCurrencySymbol = true)
        {
            var result = "";
            var resultVal = "";
            var resultCount = 0f;

            if (val != 0)
            {
                resultCount = (float) Math.Round(GetCurrencyExchanged(val, userCurrencyExchange) * 100f) / 100f;
				resultVal = GetCurrencyExchanged(val, userCurrencyExchange).ToString();
			}

            if (!needFract)
            {
                resultVal = ((int) float.Parse(resultVal)).ToString();
            }
            else
            {
                var split = resultVal.Split('.');

                if (resultVal.Length > 3 && split.Length == 2)
                {
                    var intPart = split[0];
                    var fractionPart = split[1];

                    if (intPart.Length >= 3) // если у нас 3 или более целого числа
                    {
                        if (intPart.Length == 3 && fractionPart == "99")
                            resultVal = resultVal.Substring(0, intPart.Length + 1 + 2);
                        else
                            resultVal = ((int) Math.Round(float.Parse(resultVal))).ToString(); // просто округляем
                    }
                    else
                    {
                        resultVal = resultVal.Substring(0, intPart.Length + 1 + 2); // целое число, запятая и 2 знака после
                    }
                }
            }

            if (needCurrencySymbol)
            {
                if (Currencies.Info.ContainsKey(currency.ToUpper()))
                {
                    var currencyInfo = Currencies.Info[currency.ToUpper()];
                    
                    if (currencyInfo.pre)
                        result = currencyInfo.symbol + resultVal;
                    else
                        result = resultVal + currencyInfo.symbol;
                }
                else
                {
                    result = resultVal + " " + currency.ToUpper();
                }
            }
            else
            {
                result = resultVal;
            }
            
            return AsciiToUnicode(result);
        }
        
        public static string AsciiToUnicode(string inputString)
        {
            foreach (Match s in new Regex(@"&#\d+;").Matches(inputString))
            {
                var sub = s.Value.Substring(2, s.Value.Length - 3);
                var charCode = int.Parse(sub);
                var ch = (char) charCode;
                inputString = inputString.Replace(s.Value, ch.ToString());
            }

            return inputString;
        }
        
        /// <summary>
        /// Получить цену, переведенную в валюту пользователя
        /// </summary>
        /// <param name="???"></param>
        /// <returns></returns>
        public static float GetCurrencyExchanged(float val, float userCurrencyExchange)
        {
            return val * userCurrencyExchange;
        }
        
        public static string CountText(string key, int count, bool showCount = true)
        {
            var countString = count.ToString();
            int lastDigit = count % 10;
            var result = "";

            if (lastDigit == 1) result = (key + "_one").Localize();
            else if (lastDigit <= 4 && lastDigit > 1) result = (key + "_four").Localize();
            else result = (key + "_many").Localize();

            if (countString.Length > 1)
            {
                if (countString[countString.Length - 2] == '1')
                    result = (key + "_many").Localize();
            }
            
            return showCount ? count + " " + result : result ;
        }
        
        public static string GetNumericTime(this long time, bool needHours = true, bool needSeconds = true)
        {
            if (time == int.MaxValue)
            {
                return "∞";
            }

            var hours = (int) Math.Floor(time / 3600f);
            var minutes = (int) Math.Floor((time - hours*3600f) / 60f);
            var seconds = (int) time - hours* 3600 - minutes*60;

            if (!needSeconds && hours == 0 && minutes == 0 && seconds > 0)
            {
                minutes += 1;
            }

            if (needHours || hours > 0)
            {
                return fillDigits(hours) + ":" + fillDigits(minutes) + (needSeconds? ":" + fillDigits(seconds) : "");
            }
            else
            {
                return fillDigits(minutes) + ":" + (needSeconds? fillDigits(seconds) : "");
            }

            string fillDigits(int val)
            {
                string result = "";

                if (val == 0)
                    return "00";

                result = val.ToString();
                while (result.Length< 2)
                {
                    result = "0" + result;
                }

                return result;
            }
        }
		
		public static string GetCharNumericTime(this long time, bool needSeconds = true, int groupCount = -1, bool needZeroSeconds = false, bool needSecondsIfLessThanHour = false)
		{
			var result = "";

			int days = (int) Math.Floor(time / (24f * 60 * 60));
			var hours = (int) Math.Floor((time - days * (24 * 60 * 60)) / 3600f);
			var minutes = (int) Math.Floor((time - days * (24 * 60 * 60) - hours * 3600f) / 60);
			var seconds = (int) (time - days * (24 * 60 * 60) - hours * 3600 - minutes * 60);

			var strs = new List<string>();

			if (days > 0) strs.Add(Game.Localize("d", days.ToString()));
			if (hours > 0) strs.Add(Game.Localize("h", hours.ToString()));
			if (minutes > 0) strs.Add(Game.Localize("m", minutes.ToString()));
			if (seconds > 0 && needSeconds || needZeroSeconds && !needSecondsIfLessThanHour || needSecondsIfLessThanHour && days <= 0 && hours <= 0) strs.Add(Game.Localize("s", seconds.ToString()));
            
			var needGroups = groupCount == -1 ? strs.Count : groupCount;

			for (var i = 0; i < strs.Count; i++)
			{
				if (i >= needGroups)
					break;
                
				if (!result.Equals("")) result += " ";
				result += strs[i];
			}

			if (!result.Equals("")) return result;
            
			if (needSeconds)
				result = Game.Localize("s", seconds.ToString());
			else
				result = Game.Localize("m", minutes.ToString());

			return result;
		}

        public static string GetCharNumericTime(this float time, bool needSeconds = true, int groupCount = -1, bool needZeroSeconds = false, bool needSecondsIfLessThanHour = false)
        {
            var result = "";

            int days = (int) Math.Floor(time / (24f * 60 * 60));
            var hours = (int) Math.Floor((time - days * (24 * 60 * 60)) / 3600f);
            var minutes = (int) Math.Floor((time - days * (24 * 60 * 60) - hours * 3600f) / 60);
            var seconds = (time - days * (24 * 60 * 60) - hours * 3600 - minutes * 60);

            var strs = new List<string>();

            if (days > 0) strs.Add(Game.Localize("d", days.ToString()));
            if (hours > 0) strs.Add(Game.Localize("h", hours.ToString()));
            if (minutes > 0) strs.Add(Game.Localize("m", minutes.ToString()));
            if (seconds > 0 && needSeconds || needZeroSeconds && !needSecondsIfLessThanHour || needSecondsIfLessThanHour && days <= 0 && hours <= 0) strs.Add(Game.Localize("s", seconds.ToString()));
            
            var needGroups = groupCount == -1 ? strs.Count : groupCount;

            for (var i = 0; i < strs.Count; i++)
            {
                if (i >= needGroups)
                	break;
                
                if (!result.Equals("")) result += " ";
                result += strs[i];
            }

            if (!result.Equals("")) return result;
            
            if (needSeconds)
                result = Game.Localize("s", seconds.ToString());
            else
                result = Game.Localize("m", minutes.ToString());

            return result;
		}

		public static string GetSmartCharNumericTime(this long time, bool needSeconds = true, bool needHours = true, int groupCount = -1, bool needZeroSeconds = false, bool needSecondsIfLessThanHour = false, int? timeShowAsTextThreshold = null)
		{
			const int hundredHours = 100 * 60 * 60;
			var threshold = timeShowAsTextThreshold ?? hundredHours;

			if (time >= threshold)
				return GetCharNumericTime(time, groupCount: groupCount, needSeconds: needZeroSeconds,
										  needSecondsIfLessThanHour: needSecondsIfLessThanHour);
			else
				return GetNumericTime(time, needHours: needHours, needSeconds: needSeconds);
		}

		public static string ToKiloFormat(this long num)
		{
			return ToKiloFormat((float)num);
		}
		
		public static string ToKiloFormat(this float num)
		{
			if (num >= 100000000)
				return (num / 1000000D).ToString("0.#M");
			if (num >= 1000000)
				return (num / 1000000D).ToString("0.##M");
			if (num >= 100000)
				return (num / 1000D).ToString("0.#K");
			if (num >= 1000)
				return (num / 1000D).ToString("0.##K");

			if ((num * 100) % 100 > 0)
				return num.ToString("F");
			
			return ((long) num).ToString();
		}

		public static string GetColoredText(this string text, Color color)
		{
			var hexColor = ColorUtils.ColorToHex(color);
			return $"<color=#{hexColor}>{text}</color>";
		}
		
		public static string GetColoredText(this string text, string hexColor)
		{
			return $"<color=#{hexColor}>{text}</color>";
		}

		public static string GetLink(this string text, int index, string hexColor)
		{
			return $"<U color=#{hexColor}><link={index}>{text}</link></U>".GetColoredText(hexColor);
		}

		public static string GetColoredCount(Item item, float count, bool inKiloFormat = true)
		{
			var needColor = Color.white;
			
			if (Game.User != null && Game.User.Items != null)
			{
				if (item.UserAmount() < count)
					needColor = Game.BasePrefabs.TextRedColor;
			}

			var st = inKiloFormat ? ToKiloFormat(count) : count.ToString("F");

			return GetColoredText(st, needColor);
		}

		public static string GetColoredCount(this ItemCount itemCount, bool inKiloFormat = true)
		{
			return GetColoredCount(itemCount.Item, itemCount.Count, inKiloFormat);
		}
	}
}
