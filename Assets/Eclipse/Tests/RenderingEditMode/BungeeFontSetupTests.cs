using NUnit.Framework;
using TMPro;
using UnityEditor;

namespace Eclipse.Rendering.Tests
{
    public class BungeeFontSetupTests
    {
        [Test]
        public void AtlasHasEveryGlyphTheFiveSampleCardsNeed()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Eclipse/Fonts/Bungee SDF.asset");
            Assert.IsNotNull(font, "run Eclipse > Build Bungee Font Asset first");
            foreach (var c in "0123456789+»WILD")
                Assert.IsTrue(font.HasCharacter(c), $"missing glyph '{c}'");
        }
    }
}
