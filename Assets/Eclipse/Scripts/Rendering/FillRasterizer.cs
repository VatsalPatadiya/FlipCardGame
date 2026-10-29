using Eclipse.Themes;
using UnityEngine;

namespace Eclipse.Rendering
{
    /// <summary>Bakes a theme <see cref="Fill"/> (solid/linear/radial/conic) into a Texture2D, pixel by pixel.</summary>
    public static class FillRasterizer
    {
        public static Texture2D Bake(Fill fill, Color cardColor, int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            if (fill.kind == FillKind.Solid)
            {
                var solid = fill.Evaluate(0f, cardColor);
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        tex.SetPixel(x, y, solid);
                tex.Apply();
                return tex;
            }

            // Linear: CSS angle 0deg points "up" (top edge), increasing clockwise.
            float rad = fill.angle * Mathf.Deg2Rad;
            float dirX = Mathf.Sin(rad);
            float dirY = Mathf.Cos(rad);
            float halfW = width / 2f;
            float halfH = height / 2f;
            // Box corners project onto [-maxProj, +maxProj] along the angle direction for any angle,
            // so t=0/t=1 always land exactly on a pair of opposite corners.
            float maxProj = halfW * Mathf.Abs(dirX) + halfH * Mathf.Abs(dirY);

            // Radial: farthest UV corner from center defines t=1.
            float maxCornerDistance = 0f;
            if (fill.kind == FillKind.Radial)
            {
                maxCornerDistance = Mathf.Max(
                    Vector2.Distance(Vector2.zero, fill.center),
                    Mathf.Max(
                        Vector2.Distance(Vector2.right, fill.center),
                        Mathf.Max(Vector2.Distance(Vector2.up, fill.center), Vector2.Distance(Vector2.one, fill.center))));
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float t;
                    switch (fill.kind)
                    {
                        case FillKind.Linear:
                            // Pixel position relative to box center; angle direction points toward t=0.
                            float px = x + 0.5f - halfW;
                            float py = y + 0.5f - halfH;
                            float proj = px * dirX + py * dirY;
                            t = maxProj > 0f ? 0.5f - proj / (2f * maxProj) : 0f;
                            break;

                        case FillKind.Radial:
                            var radialUv = new Vector2((x + 0.5f) / width, (y + 0.5f) / height);
                            t = maxCornerDistance > 0f ? Vector2.Distance(radialUv, fill.center) / maxCornerDistance : 0f;
                            break;

                        case FillKind.Conic:
                            var conicUv = new Vector2((x + 0.5f) / width, (y + 0.5f) / height);
                            float dx = conicUv.x - fill.center.x;
                            float dy = conicUv.y - fill.center.y;
                            float degreesClockwiseFromUp = Mathf.Atan2(dx, dy) * Mathf.Rad2Deg;
                            t = Mathf.Repeat((degreesClockwiseFromUp - fill.angle) / 360f, 1f);
                            break;

                        default:
                            t = 0f;
                            break;
                    }
                    tex.SetPixel(x, y, fill.Evaluate(t, cardColor));
                }
            }

            tex.Apply();
            return tex;
        }
    }
}
