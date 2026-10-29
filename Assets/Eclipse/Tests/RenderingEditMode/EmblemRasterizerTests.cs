using System;
using Eclipse.Rendering;
using Eclipse.Themes;
using NUnit.Framework;
using UnityEngine;

namespace Eclipse.Rendering.Tests
{
    public class EmblemRasterizerTests
    {
        [Test]
        public void StarburstCenterIsOpaqueFill()
        {
            var style = new EmblemStyle { shape = EmblemShape.Starburst, size = 184f, fill = ColorRef.Hex("#FFF6E4") };
            var tex = EmblemRasterizer.Bake(style, Color.black, 200);
            var center = tex.GetPixel(100, 100);
            Assert.Greater(center.a, 0.9f);
            Assert.Less(Vector4.Distance(center, Palette.Hex("#FFF6E4")), 0.05f);
        }

        [Test]
        public void StarburstCornerIsTransparent()
        {
            var style = new EmblemStyle { shape = EmblemShape.Starburst, size = 184f, fill = ColorRef.Hex("#FFF6E4") };
            var tex = EmblemRasterizer.Bake(style, Color.black, 200);
            Assert.Less(tex.GetPixel(2, 2).a, 0.05f);
        }

        [Test]
        public void HexagonShapeNotYetSupported()
        {
            var style = new EmblemStyle { shape = EmblemShape.Hexagon, fill = ColorRef.White() };
            Assert.Throws<NotSupportedException>(() => EmblemRasterizer.Bake(style, Color.black, 200));
        }
    }
}
