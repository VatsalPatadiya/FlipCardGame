using System;
using System.Collections.Generic;
using Eclipse.Themes;
using UnityEngine;

namespace Eclipse.Rendering
{
    /// <summary>Bakes a theme action symbol (e.g. the Jump chevron) — an SVG path of M/L commands only — into a Texture2D.</summary>
    public static class SymbolRasterizer
    {
        const float ReferenceBox = 200f;

        public static Texture2D Bake(SymbolStyle style, string svgPath, Color cardColor, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float scale = size / ReferenceBox;
            var polylines = ParsePath(svgPath, size, scale);

            var outerColor = style.outerStroke.Resolve(cardColor);
            var innerColor = style.innerStroke.Resolve(cardColor);
            float outerHalfWidth = style.outerStrokeWidth * scale / 2f;
            float innerHalfWidth = style.innerStrokeWidth * scale / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    float dist = DistanceToPolylines(p, polylines);

                    var pixel = Color.clear;
                    if (dist <= outerHalfWidth) pixel = outerColor;
                    if (dist <= innerHalfWidth) pixel = innerColor;

                    tex.SetPixel(x, y, pixel);
                }
            }

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Parses an SVG path using only M (moveto, starts a new polyline) and L (lineto, extends the
        /// current polyline) commands, converting each point from the 200x200 SVG box (y-down) to
        /// pixel space (y-up, scaled by size/ReferenceBox). The command letter is glued to its x
        /// coordinate with no space (e.g. "M60 62 L98 100"), so each token's first character is the
        /// command and the rest (if any) is the leading number.
        /// </summary>
        static List<List<Vector2>> ParsePath(string svgPath, int size, float scale)
        {
            var polylines = new List<List<Vector2>>();
            var tokens = svgPath.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            List<Vector2> current = null;

            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i];
                char command = token[0];
                if (command != 'M' && command != 'L')
                    continue;

                float svgX = float.Parse(token.Substring(1));
                float svgY = float.Parse(tokens[++i]);
                var point = new Vector2(svgX * scale, size - svgY * scale);

                if (command == 'M')
                {
                    current = new List<Vector2> { point };
                    polylines.Add(current);
                }
                else
                {
                    current.Add(point);
                }
            }

            return polylines;
        }

        static float DistanceToPolylines(Vector2 p, List<List<Vector2>> polylines)
        {
            float min = float.MaxValue;
            foreach (var polyline in polylines)
                for (int i = 1; i < polyline.Count; i++)
                    min = Mathf.Min(min, DistanceToSegment(p, polyline[i - 1], polyline[i]));
            return min;
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
