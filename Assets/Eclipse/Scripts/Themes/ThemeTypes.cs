using System;
using UnityEngine;

namespace Eclipse.Themes
{
    public enum EmblemShape
    {
        /// <summary>14-point starburst (Premium Set).</summary>
        Starburst,
        /// <summary>Circle (Glass Aurora).</summary>
        Circle,
        /// <summary>Pointy-top hexagon (Prism Holo).</summary>
        Hexagon,
    }

    public enum FillKind
    {
        Solid,
        Linear,
        Radial,
        Conic,
    }

    /// <summary>
    /// A color that may depend on the card's own color, so one spec covers all four colors:
    /// e.g. "lighten(c, .35)" is <c>useCardColor = true, lighten = 0.35</c>.
    /// Alpha 0 means "not used" (no border, no hairline), so set it to 1 for an opaque color.
    /// </summary>
    [Serializable]
    public struct ColorRef
    {
        public bool useCardColor;
        public Color color;
        /// <summary>Mix with white by this fraction (spec: lighten(c, t)).</summary>
        [Range(0f, 1f)] public float lighten;
        /// <summary>Mix with black by this fraction (spec: darken(c, t)).</summary>
        [Range(0f, 1f)] public float darken;
        [Range(0f, 1f)] public float alpha;

        public Color Resolve(Color cardColor)
        {
            var c = useCardColor ? cardColor : color;
            if (lighten > 0f) c = Palette.Lighten(c, lighten);
            if (darken > 0f) c = Palette.Darken(c, darken);
            c.a *= alpha;
            return c;
        }

        public static ColorRef Hex(string hex, float alpha = 1f) =>
            new ColorRef { color = Palette.Hex(hex), alpha = alpha };

        public static ColorRef White(float alpha = 1f) => new ColorRef { color = Color.white, alpha = alpha };

        public static ColorRef Card(float alpha = 1f) => new ColorRef { useCardColor = true, alpha = alpha };
        public static ColorRef CardLighten(float t, float alpha = 1f) => new ColorRef { useCardColor = true, lighten = t, alpha = alpha };
        public static ColorRef CardDarken(float t, float alpha = 1f) => new ColorRef { useCardColor = true, darken = t, alpha = alpha };

        public static ColorRef Rgba(int r, int g, int b, float a) =>
            new ColorRef { color = new Color32((byte)r, (byte)g, (byte)b, 255), alpha = a };
    }

    [Serializable]
    public struct ColorStop
    {
        public ColorRef color;
        [Range(0f, 1f)] public float position;

        public ColorStop(ColorRef color, float position)
        {
            this.color = color;
            this.position = position;
        }
    }

    /// <summary>A CSS-style gradient: solid, linear (angle in CSS degrees), radial (center in 0-1 card space) or conic.</summary>
    [Serializable]
    public class Fill
    {
        public FillKind kind = FillKind.Solid;
        public float angle;
        public Vector2 center = new Vector2(0.5f, 0.5f);
        public ColorStop[] stops = new ColorStop[0];

        public static Fill Solid(ColorRef c) => new Fill { kind = FillKind.Solid, stops = new[] { new ColorStop(c, 0f) } };

        public static Fill Linear(float angle, params ColorStop[] stops) =>
            new Fill { kind = FillKind.Linear, angle = angle, stops = stops };

        public static Fill Radial(Vector2 center, params ColorStop[] stops) =>
            new Fill { kind = FillKind.Radial, center = center, stops = stops };

        public static Fill Conic(float fromAngle, params ColorStop[] stops) =>
            new Fill { kind = FillKind.Conic, angle = fromAngle, stops = stops };

        /// <summary>Stops spread evenly from 0 to 1.</summary>
        public static ColorStop[] Even(params ColorRef[] colors)
        {
            var stops = new ColorStop[colors.Length];
            for (int i = 0; i < colors.Length; i++)
                stops[i] = new ColorStop(colors[i], colors.Length == 1 ? 0f : i / (float)(colors.Length - 1));
            return stops;
        }

        /// <summary>Sample the gradient at t (0-1) for a given card color.</summary>
        public Color Evaluate(float t, Color cardColor)
        {
            if (stops == null || stops.Length == 0) return Color.clear;
            if (t <= stops[0].position) return stops[0].color.Resolve(cardColor);
            for (int i = 1; i < stops.Length; i++)
            {
                if (t <= stops[i].position)
                {
                    float span = stops[i].position - stops[i - 1].position;
                    float k = span <= 0f ? 1f : (t - stops[i - 1].position) / span;
                    return Color.Lerp(stops[i - 1].color.Resolve(cardColor), stops[i].color.Resolve(cardColor), k);
                }
            }
            return stops[stops.Length - 1].color.Resolve(cardColor);
        }
    }

    /// <summary>A CSS box-shadow / text-shadow: drop shadow when offset, glow when not. Spread draws a solid ring.</summary>
    [Serializable]
    public struct ShadowSpec
    {
        public Vector2 offset;
        public float blur;
        public float spread;
        public ColorRef color;

        public static ShadowSpec Drop(float x, float y, float blur, ColorRef color) =>
            new ShadowSpec { offset = new Vector2(x, y), blur = blur, color = color };

        public static ShadowSpec Glow(float blur, ColorRef color) => new ShadowSpec { blur = blur, color = color };

        public static ShadowSpec Ring(float width, ColorRef color) => new ShadowSpec { spread = width, color = color };
    }

    [Serializable]
    public class EmblemStyle
    {
        public EmblemShape shape;
        /// <summary>Width of the emblem's 200 × 200 design box in reference pixels (spec: about 182-186). Shapes sit inside it (see CardLayoutSpec).</summary>
        public float size = 184f;
        public ColorRef fill;
        public ColorRef stroke;
        public float strokeWidth;
        /// <summary>Optional inner outline (Premium Sun: card color at 45%, scaled 0.86).</summary>
        public bool innerOutline;
        public ColorRef innerOutlineColor;
        public float innerOutlineScale = 0.86f;
        public ShadowSpec[] shadows = new ShadowSpec[0];
    }

    /// <summary>The big center number, drawn twice: back layer (outline + shadow/glow) then front fill (§4.3).</summary>
    [Serializable]
    public class NumberStyle
    {
        public Fill fill;
        public ColorRef outline;
        /// <summary>CSS stroke width; only half of it shows outside the glyph. In TMP: Outline thickness.</summary>
        public float outlineWidth;
        /// <summary>In TMP: Underlay (one) or a second glow layer.</summary>
        public ShadowSpec[] shadows = new ShadowSpec[0];
    }

    [Serializable]
    public class ChipStyle
    {
        public ColorRef background;
        public ColorRef border;
        public float borderWidth;
        public ColorRef text;
        public float radius = 10f;
        public ShadowSpec[] shadows = new ShadowSpec[0];
    }

    /// <summary>The Wild (Sun) or Wild Chase (Moon) treatment: foil frame, own panel, four nested emblems.</summary>
    [Serializable]
    public class WildStyle
    {
        public string label = "WILD";
        public Fill frame;
        public Fill panel;
        /// <summary>Nested emblem copies use the side's four colors as strokes (neon) or fills.</summary>
        public bool nestedAsOutlines;
        public ColorRef nestedStroke;
        public float nestedStrokeWidth = 4f;
        public ColorRef labelColor;
        public ColorRef labelOutline;
    }

    /// <summary>Everything that differs between the Sun and the Moon side of one theme.</summary>
    [Serializable]
    public class SideSkin
    {
        public string[] colorNames = new string[4];
        public Color[] colors = new Color[4];

        [Header("Table")]
        public Fill table;
        /// <summary>Extra soft radial blobs over the table (Glass Aurora).</summary>
        public Fill[] tableBlobs = new Fill[0];

        [Header("Card")]
        public Fill frame;
        public float framePadding = 9f;
        public float outerRadius = 22f;
        public float innerRadius = 15f;
        public Fill panel;
        /// <summary>Second panel layer drawn on top (Glass: color glow over frosted glass).</summary>
        public Fill panelOverlay;
        /// <summary>Hairline inner border on the panel (Prism Holo Sun).</summary>
        public ColorRef panelHairline;
        public float starfieldOpacity;
        /// <summary>Premium Sun: two concentric starburst outlines in white.</summary>
        public float patternOpacity;
        public float patternSize = 400f;
        public float patternRotation = 8f;
        /// <summary>Glass Aurora: blur what's behind the card.</summary>
        public bool backgroundBlur;

        public EmblemStyle emblem = new EmblemStyle();
        public NumberStyle number = new NumberStyle();
        public ChipStyle chip = new ChipStyle();
        public Fill gloss;
        /// <summary>Prism Holo: sweep the gloss band across the card on idle and on select.</summary>
        public bool glossSweeps;
        public ShadowSpec cardShadow;
        public ShadowSpec[] playableGlow = new ShadowSpec[0];
        public WildStyle wild = new WildStyle();

        [Header("UI text")]
        public Color uiHeading = Color.white;
        public Color uiMuted = new Color(1f, 1f, 1f, 0.7f);
        public Color uiEyebrow = Color.white;

        public Color CardColor(int index) => index >= 0 && index < colors.Length ? colors[index] : Color.white;
    }
}
