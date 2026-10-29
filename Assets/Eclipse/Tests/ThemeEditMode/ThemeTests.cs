using Eclipse.Core;
using NUnit.Framework;
using UnityEngine;

namespace Eclipse.Themes.Tests
{
    public class ThemeTests
    {
        [Test]
        public void AllThreeThemesBuildWithFourNamedColorsPerSide()
        {
            var themes = ThemeLibrary.CreateAll();
            Assert.AreEqual(3, themes.Length);
            foreach (var theme in themes)
            {
                foreach (var side in new[] { Side.Sun, Side.Moon })
                {
                    var skin = theme.Skin(side);
                    Assert.AreEqual(4, skin.colors.Length, theme.name);
                    Assert.AreEqual(4, skin.colorNames.Length, theme.name);
                    Assert.IsNotNull(skin.table, theme.name);
                    Assert.IsNotNull(skin.frame, theme.name);
                    Assert.IsNotNull(skin.panel, theme.name);
                    Assert.IsNotNull(skin.number.fill, theme.name);
                    Assert.Greater(skin.playableGlow.Length, 0, theme.name);
                    Assert.AreEqual(side == Side.Sun ? "WILD" : "CHASE", skin.wild.label);
                }
                Object.DestroyImmediate(theme);
            }
        }

        [Test]
        public void LightenAndDarkenMixWithWhiteAndBlack()
        {
            var c = new Color(0.2f, 0.4f, 0.6f, 1f);
            var l = Palette.Lighten(c, 0.5f);
            var d = Palette.Darken(c, 0.5f);
            Assert.AreEqual(0.6f, l.r, 1e-4f);
            Assert.AreEqual(0.8f, l.b, 1e-4f);
            Assert.AreEqual(0.1f, d.r, 1e-4f);
            Assert.AreEqual(0.3f, d.b, 1e-4f);
        }

        [Test]
        public void CardRelativeColorsResolvePerCard()
        {
            var tangerine = Palette.Hex("#FF7A1A");
            var r = ColorRef.CardDarken(0.35f).Resolve(tangerine);
            Assert.AreEqual(tangerine.r * 0.65f, r.r, 1e-4f);
            Assert.AreEqual(1f, r.a, 1e-4f);
            Assert.AreEqual(0.45f, ColorRef.Card(0.45f).Resolve(tangerine).a, 1e-4f);
        }

        [Test]
        public void SpecEasingStartsAndEndsCleanly()
        {
            Assert.AreEqual(0f, CardLayoutSpec.Ease(0f), 1e-3f);
            Assert.AreEqual(1f, CardLayoutSpec.Ease(1f), 1e-3f);
            Assert.Greater(CardLayoutSpec.Ease(0.3f), 0.6f, "fast start, gentle settle");
        }

        [Test]
        public void PremiumSunJumpUsesInkOuterStrokeAndCardColorInnerStroke()
        {
            var theme = ThemeLibrary.CreatePremiumSet();
            var jump = theme.sun.jump;
            var ink = Palette.Hex("#1B1530");
            Assert.AreEqual(ink, jump.outerStroke.Resolve(Color.white));
            Assert.IsTrue(jump.innerStroke.useCardColor);
            Assert.AreEqual(CardLayoutSpec.SymbolOuterStroke, jump.outerStrokeWidth, 1e-3f);
            Assert.AreEqual(CardLayoutSpec.SymbolInnerStroke, jump.innerStrokeWidth, 1e-3f);
            Object.DestroyImmediate(theme);
        }
    }
}
