using System;
using Eclipse.Themes;
using UnityEngine;

namespace Eclipse.Rendering
{
    /// <summary>Bakes a theme <see cref="EmblemStyle"/> (currently just the starburst) into a Texture2D.</summary>
    public static class EmblemRasterizer
    {
        const float ReferenceBox = 200f;

        public static Texture2D Bake(EmblemStyle style, Color cardColor, int size)
        {
            if (style.shape != EmblemShape.Starburst)
                throw new NotSupportedException($"EmblemRasterizer does not support {style.shape} yet.");

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float scale = size / ReferenceBox;
            var center = new Vector2(size / 2f, size / 2f);

            var outerPoints = StarburstPoints(center, scale, 1f);
            var fillColor = style.fill.Resolve(cardColor);

            Vector2[] innerOutlinePoints = null;
            Color innerOutlineColor = Color.clear;
            if (style.innerOutline)
            {
                innerOutlinePoints = StarburstPoints(center, scale, style.innerOutlineScale);
                innerOutlineColor = style.innerOutlineColor.Resolve(cardColor);
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    bool inside = IsInsidePolygon(p, outerPoints);
                    var pixel = Color.clear;

                    if (!inside && style.shadows != null)
                        pixel = ShadowHalo(p, outerPoints, style.shadows, cardColor, scale);

                    if (inside)
                        pixel = fillColor;

                    if (innerOutlinePoints != null && IsOnPolygonEdge(p, innerOutlinePoints, 1.5f * scale))
                        pixel = innerOutlineColor;

                    tex.SetPixel(x, y, pixel);
                }
            }

            tex.Apply();
            return tex;
        }

        /// <summary>The 2*StarburstPoints-vertex star polygon, scaled by <paramref name="radiusScale"/> on top of the box scale.</summary>
        static Vector2[] StarburstPoints(Vector2 center, float scale, float radiusScale)
        {
            int vertexCount = CardLayoutSpec.StarburstPoints * 2;
            var points = new Vector2[vertexCount];
            float outer = CardLayoutSpec.StarburstOuterRadius * scale * radiusScale;
            float inner = CardLayoutSpec.StarburstInnerRadius * scale * radiusScale;

            for (int i = 0; i < vertexCount; i++)
            {
                float radius = (i % 2 == 0) ? outer : inner;
                float angle = i * (360f / vertexCount) * Mathf.Deg2Rad;
                points[i] = center + new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * radius;
            }

            return points;
        }

        /// <summary>
        /// Soft darkened halo around the shape, approximating a blurred shadow/glow: alpha falls off
        /// linearly from the polygon edge out to <c>spread + blur</c>. Not pixel-matched to CSS blur.
        /// </summary>
        static Color ShadowHalo(Vector2 p, Vector2[] outerPolygon, ShadowSpec[] shadows, Color cardColor, float scale)
        {
            var result = Color.clear;
            float edgeDistance = DistanceToPolygon(p, outerPolygon);

            foreach (var shadow in shadows)
            {
                float spread = shadow.spread * scale;
                float blur = Mathf.Max(shadow.blur * scale, 0.001f);
                float beyondSpread = edgeDistance - spread;
                if (beyondSpread > blur) continue;

                float falloff = 1f - Mathf.Clamp01(beyondSpread / blur);
                var color = shadow.color.Resolve(cardColor);
                color.a *= falloff;
                if (color.a > result.a) result = color;
            }

            return result;
        }

        static float DistanceToPolygon(Vector2 p, Vector2[] polygon)
        {
            float min = float.MaxValue;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
                min = Mathf.Min(min, DistanceToSegment(p, polygon[j], polygon[i]));
            return min;
        }

        /// <summary>Standard ray-casting point-in-polygon test.</summary>
        static bool IsInsidePolygon(Vector2 p, Vector2[] polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                var a = polygon[i];
                var b = polygon[j];
                if (((a.y > p.y) != (b.y > p.y)) &&
                    (p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        /// <summary>True when the pixel is within <paramref name="thickness"/> of the polygon's boundary.</summary>
        static bool IsOnPolygonEdge(Vector2 p, Vector2[] polygon, float thickness)
        {
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (DistanceToSegment(p, polygon[j], polygon[i]) <= thickness)
                    return true;
            }
            return false;
        }

        static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float lenSq = ab.sqrMagnitude;
            float t = lenSq > 0f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / lenSq) : 0f;
            var closest = a + ab * t;
            return Vector2.Distance(p, closest);
        }
    }
}
