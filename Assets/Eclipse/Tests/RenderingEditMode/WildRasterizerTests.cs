using Eclipse.Rendering;
using Eclipse.Themes;
using NUnit.Framework;
using UnityEngine;

namespace Eclipse.Rendering.Tests
{
    public class WildRasterizerTests
    {
        [Test]
        public void FourNestedCopiesAreAllOpaqueAtCenter()
        {
            var baseEmblem = new EmblemStyle { shape = EmblemShape.Starburst, size = 184f, fill = ColorRef.Card() };
            var style = new WildStyle { nestedAsOutlines = false };
            var colors = Palette.Hexes("#FF7A1A", "#E8327C", "#13B5C4", "#8CC63F");
            var tex = WildRasterizer.BakeNestedEmblems(style, baseEmblem, colors, 200);
            Assert.Greater(tex.GetPixel(100, 100).a, 0.9f, "smallest (topmost) nested copy must cover the center");
        }
    }
}
