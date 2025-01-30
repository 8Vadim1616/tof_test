using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Utils
{
    public static class ColorUtils
    {
        public static Color Set(this Color c, float? r = null, float? g = null, float? b = null, float? a = null) =>
            new Color(r ?? c.r, g ?? c.g, b ?? c.b, a ?? c.a);

        public static Color SetAlpha(this Color color, float alpha) =>
            new Color(color.r, color.g, color.b, alpha);

        public static Color Add(this Color c, float? r = null, float? g = null, float? b = null, float? a = null) =>
            new Color(
                r == null ? c.r : c.r + r.Value,
                g == null ? c.g : c.g + g.Value,
                b == null ? c.b : c.b + b.Value,
                a == null ? c.a : c.a + a.Value);

        public static Image SetColor(this Image i, float? r = null, float? g = null, float? b = null, float? a = null)
        {
            i.color = i.color.Set(r, g, b, a);
            return i;
        }
        
        public static TextMeshProUGUI SetColor(this TextMeshProUGUI i, float? r = null, float? g = null, float? b = null, float? a = null)
        {
            i.color = i.color.Set(r, g, b, a);
            return i;
        }

        public static SpriteRenderer SetColor(this SpriteRenderer i, float? r = null, float? g = null, float? b = null, float? a = null)
        {
            i.color = i.color.Set(r, g, b, a);
            return i;
        }

        public static Color NumberToColor(this int number) => new Color((number >> 24) % 256, (number >> 16) % 256, (number >> 8) % 256, number % 256);
        public static Color NumberToColor(this uint number) => new Color((number >> 24) % 256, (number >> 16) % 256, (number >> 8) % 256, number % 256);

        public static Color HexToColor(string hexColor) => Convert.ToInt32(hexColor, 16).NumberToColor();
		
		public static string ColorToHex(Color32 color)
		{
			string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
			return hex;
		}
    }
}