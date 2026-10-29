using Eclipse.Core;
using UnityEngine;

namespace Eclipse.Themes
{
    /// <summary>
    /// One full skin (§5): colors, frames, emblem, number style, chips, glows and table for both sides.
    /// Swappable at runtime, so themes can be unlocked or sold as cosmetics.
    /// Create the three spec themes with Eclipse → Create Theme Assets, or in code with <see cref="ThemeLibrary"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Eclipse/Card Theme", fileName = "CardTheme")]
    public class CardTheme : ScriptableObject
    {
        public string themeId;
        public string displayName;

        [Header("Fonts (Google Fonts, SIL OFL). Assign the generated TMP SDF assets.")]
        public string numberFontName;
        public float numberScale = 1f;
        public string headingFontName;
        public string bodyFontName = "Manrope";
        /// <summary>TMP_FontAsset for numbers and chips. Typed as Object so this assembly doesn't depend on TextMeshPro.</summary>
        public Object numberFont;
        public Object headingFont;
        public Object bodyFont;

        /// <summary>Prism Holo: tilt the iridescent frame and sheen with the gyroscope.</summary>
        public bool holoTilt;

        public SideSkin sun = new SideSkin();
        public SideSkin moon = new SideSkin();

        public SideSkin Skin(Side side) => side == Side.Sun ? sun : moon;

        public Color CardColor(Side side, int colorIndex) => Skin(side).CardColor(colorIndex);
    }
}
