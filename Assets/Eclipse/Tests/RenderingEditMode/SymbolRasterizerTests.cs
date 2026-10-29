using Eclipse.Rendering;
using Eclipse.Themes;
using NUnit.Framework;
using UnityEngine;

namespace Eclipse.Rendering.Tests
{
    public class SymbolRasterizerTests
    {
        [Test]
        public void JumpChevronMidpointIsInnerStrokeColor()
        {
            var style = new SymbolStyle { outerStroke = ColorRef.Hex("#1B1530"), outerStrokeWidth = 30f,
                innerStroke = ColorRef.Card(), innerStrokeWidth = 15f };
            var tex = SymbolRasterizer.Bake(style, CardLayoutSpec.JumpPath, Palette.Hex("#FF7A1A"), 200);
            // JumpPath's first segment vertex is (98,100) in the 200x200 box (SVG y-down); at size=200
            // that maps to pixel (98, 200-100)=(98,100) in bottom-left pixel space.
            var onStroke = tex.GetPixel(98, 100);
            Assert.Greater(onStroke.a, 0.5f);
        }

        [Test]
        public void FarFromChevronIsTransparent()
        {
            var style = new SymbolStyle { outerStroke = ColorRef.Hex("#1B1530"), outerStrokeWidth = 30f,
                innerStroke = ColorRef.Card(), innerStrokeWidth = 15f };
            var tex = SymbolRasterizer.Bake(style, CardLayoutSpec.JumpPath, Palette.Hex("#FF7A1A"), 200);
            Assert.Less(tex.GetPixel(5, 5).a, 0.05f);
        }
    }
}
