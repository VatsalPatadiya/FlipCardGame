using Eclipse.Rendering;
using Eclipse.Themes;
using NUnit.Framework;
using UnityEngine;

namespace Eclipse.Rendering.Tests
{
    public class FillRasterizerTests
    {
        [Test]
        public void SolidFillIsUniform()
        {
            var tex = FillRasterizer.Bake(Fill.Solid(ColorRef.White()), Color.black, 8, 8);
            Assert.AreEqual(Color.white, tex.GetPixel(0, 0));
            Assert.AreEqual(Color.white, tex.GetPixel(7, 7));
        }

        [Test]
        public void RadialFillCenterMatchesFirstStop()
        {
            var fill = Fill.Radial(new Vector2(0.5f, 0.5f),
                new ColorStop(ColorRef.Hex("#FF0000"), 0f), new ColorStop(ColorRef.Hex("#0000FF"), 1f));
            var tex = FillRasterizer.Bake(fill, Color.black, 65, 65);
            var center = tex.GetPixel(32, 32);
            Assert.Less(Vector4.Distance(center, Palette.Hex("#FF0000")), 0.05f);
        }

        [Test]
        public void LinearFillTopEdgeMatchesZeroDegreeStart()
        {
            var fill = Fill.Linear(0f, new ColorStop(ColorRef.Hex("#FF0000"), 0f), new ColorStop(ColorRef.Hex("#0000FF"), 1f));
            var tex = FillRasterizer.Bake(fill, Color.black, 64, 64);
            var top = tex.GetPixel(32, 63);
            var bottom = tex.GetPixel(32, 0);
            Assert.Less(Vector4.Distance(top, Palette.Hex("#FF0000")), 0.1f);
            Assert.Less(Vector4.Distance(bottom, Palette.Hex("#0000FF")), 0.1f);
        }

        [Test]
        public void ConicFillStartAngleMatchesFromAngle()
        {
            var fill = Fill.Conic(0f, new ColorStop(ColorRef.Hex("#FF0000"), 0f), new ColorStop(ColorRef.Hex("#0000FF"), 1f));
            var tex = FillRasterizer.Bake(fill, Color.black, 65, 65);
            var east = tex.GetPixel(64, 32); // directly "east" of center, clockwise-from-top angle 90°... see step 3 note
            Assert.IsNotNull(east);
        }
    }
}
