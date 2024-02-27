using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace UnityEngine.UI
{
    public partial class DynamicText
    {
        internal static class Utils
        {
            const float LINE_MIN_OFFSET = 0.1f;
            const float BOUNDS_MIN_AREA = 1f;

            public static bool InLine(Vector3 position1, Vector3 position2)
            {
                return Math.Abs(position1.y - position2.y) <= LINE_MIN_OFFSET;
            }
            public static bool IsValidBounds(Rect bounds)
            {
                return bounds.size.magnitude >= BOUNDS_MIN_AREA;
            }

            static Dictionary<string, Color> buildinColors;

            public static Color HexToColor(string colorHex)
            {
                return HexToColor(colorHex, Color.white);
            }
            public static Color HexToColor(string colorHex, Color defaultValue)
            {
                if (colorHex == null)
                    return defaultValue;
                if (buildinColors == null) InitBuildinColors();

                Color color;
                if (buildinColors.TryGetValue(colorHex, out color))
                {
                    return color;
                }

                if (colorHex.Length < 7 || !colorHex.StartsWith("#"))
                    return defaultValue;

                return new Color(
                    TryParse(colorHex.Substring(1, 2)) / 255f,
                    TryParse(colorHex.Substring(3, 2)) / 255f,
                    TryParse(colorHex.Substring(5, 2)) / 255f
                );
            }

            static void InitBuildinColors()
            {
                if (buildinColors != null)
                    return;
                buildinColors = new Dictionary<string, Color>();
                foreach (var property in typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.Public))
                {
                    if (!property.CanRead || !typeof(Color).IsAssignableFrom(property.PropertyType))
                        continue;
                    buildinColors.Add(property.Name, (Color)property.GetValue(null));
                }
            }

            static int TryParse(string hex)
            {
                try
                {
                    return int.Parse(hex, NumberStyles.HexNumber);
                }
                catch (Exception) { }
                return 0;
            }
        }
    }
}