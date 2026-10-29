using System;
using Eclipse.Themes;
using UnityEngine;

namespace Eclipse.Rendering
{
    /// <summary>Bakes a Wild card's four nested starburst copies (one per side color, §4.4) onto one Texture2D.</summary>
    public static class WildRasterizer
    {
        public static Texture2D BakeNestedEmblems(WildStyle style, EmblemStyle baseEmblem, Color[] sideColors, int size)
        {
            if (style.nestedAsOutlines)
                throw new NotSupportedException("WildRasterizer does not support outline nested copies yet.");

            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            var scales = CardLayoutSpec.WildNestedScales;
            int count = Mathf.Min(scales.Length, sideColors.Length);

            // Largest-to-smallest so the smaller copies composite on top (spec §4.4).
            for (int i = 0; i < count; i++)
            {
                var copyStyle = CloneWithFill(baseEmblem, sideColors[i], scales[i]);
                int copySize = Mathf.Max(1, Mathf.RoundToInt(size * scales[i]));
                var baked = EmblemRasterizer.Bake(copyStyle, sideColors[i], copySize);
                baked.wrapMode = TextureWrapMode.Clamp; // avoid edge bleed when GetPixelBilinear samples near a border

                float angle = i * CardLayoutSpec.WildNestedRotationStarburst;
                CompositeRotatedCentered(pixels, size, baked, angle);
            }

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        static EmblemStyle CloneWithFill(EmblemStyle baseEmblem, Color color, float scale) => new EmblemStyle
        {
            shape = baseEmblem.shape,
            size = baseEmblem.size * scale,
            fill = new ColorRef { color = color, alpha = 1f },
            stroke = baseEmblem.stroke,
            strokeWidth = baseEmblem.strokeWidth,
            innerOutline = baseEmblem.innerOutline,
            innerOutlineColor = baseEmblem.innerOutlineColor,
            innerOutlineScale = baseEmblem.innerOutlineScale,
            shadows = baseEmblem.shadows,
        };

        /// <summary>
        /// Alpha-composites <paramref name="src"/> onto <paramref name="dest"/>, both centered on the same point,
        /// by sampling <paramref name="src"/> with a UV rotated by <paramref name="degrees"/> (rotate-then-sample,
        /// i.e. each dest pixel looks up the un-rotated source pixel at its position rotated by -degrees).
        /// </summary>
        static void CompositeRotatedCentered(Color[] dest, int destSize, Texture2D src, float degrees)
        {
            float rad = -degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            float destCenter = destSize / 2f;
            var srcCenter = new Vector2(src.width / 2f, src.height / 2f);

            for (int y = 0; y < destSize; y++)
            {
                for (int x = 0; x < destSize; x++)
                {
                    float px = x + 0.5f - destCenter;
                    float py = y + 0.5f - destCenter;
                    var samplePos = new Vector2(px * cos - py * sin, px * sin + py * cos) + srcCenter;

                    float u = samplePos.x / src.width;
                    float v = samplePos.y / src.height;
                    if (u < 0f || u >= 1f || v < 0f || v >= 1f) continue;

                    int idx = y * destSize + x;
                    dest[idx] = AlphaOver(src.GetPixelBilinear(u, v), dest[idx]);
                }
            }
        }

        /// <summary>Standard "source over destination" compositing for straight (non-premultiplied) alpha.</summary>
        static Color AlphaOver(Color src, Color dst)
        {
            float outA = src.a + dst.a * (1f - src.a);
            if (outA <= 0f) return Color.clear;
            float k = 1f - src.a;
            return new Color(
                (src.r * src.a + dst.r * dst.a * k) / outA,
                (src.g * src.a + dst.g * dst.a * k) / outA,
                (src.b * src.a + dst.b * dst.a * k) / outA,
                outA);
        }
    }
}
