using UnityEngine;

namespace Eclipse.Themes
{
    /// <summary>Color helpers used by the spec (§4.7).</summary>
    public static class Palette
    {
        /// <summary>Mix with white by fraction t.</summary>
        public static Color Lighten(Color c, float t) => new Color(
            c.r + (1f - c.r) * t, c.g + (1f - c.g) * t, c.b + (1f - c.b) * t, c.a);

        /// <summary>Mix with black by fraction t.</summary>
        public static Color Darken(Color c, float t) => new Color(c.r * (1f - t), c.g * (1f - t), c.b * (1f - t), c.a);

        /// <summary>"#RRGGBB" or "#RRGGBBAA".</summary>
        public static Color Hex(string hex)
        {
            if (!ColorUtility.TryParseHtmlString(hex, out var c))
                throw new System.ArgumentException("Bad color " + hex);
            return c;
        }

        public static Color[] Hexes(params string[] hexes)
        {
            var colors = new Color[hexes.Length];
            for (int i = 0; i < hexes.Length; i++) colors[i] = Hex(hexes[i]);
            return colors;
        }
    }
}
