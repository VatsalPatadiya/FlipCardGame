using UnityEngine;

namespace Eclipse.Themes
{
    /// <summary>
    /// Measurements shared by all themes (§4), in reference pixels at 220 × 330 card size.
    /// Scale everything by (actual card width / <see cref="CardWidth"/>).
    /// </summary>
    public static class CardLayoutSpec
    {
        // Card geometry (§4.1)
        public const float CardWidth = 220f;
        public const float CardHeight = 330f;
        public const float ChipMinWidth = 36f;
        public const float ChipMinHeight = 34f;
        public const float ChipPaddingX = 8f;
        public const float ChipInset = 10f;
        public const float ChipFontSize = 17f;

        // Center number (§4.2). Multiply by the theme's numberScale.
        public const float NumberSingleDigit = 112f;
        public const float NumberSingleDigitPremium = 100f;
        public const float NumberTwoChars = 84f;
        public const float SubLabelNumberShift = 22f;
        public const float SubLabelFontSize = 13.5f;
        public const float SubLabelLetterSpacingEm = 0.3f;

        // Action symbols (§4.4), in a 200 × 200 box.
        public const string JumpPath = "M60 62 L98 100 L60 138 M104 62 L142 100 L104 138";
        public const string JumpAllPath = "M44 66 L76 100 L44 134 M80 66 L112 100 L80 134 M116 66 L148 100 L116 134";
        public const float SymbolOuterStroke = 30f;
        public const float SymbolInnerStroke = 15f;
        public static readonly float[] WildNestedScales = { 1f, 0.78f, 0.58f, 0.40f };
        public const float WildNestedRotationStarburst = 13f;
        public const float WildNestedRotationHexagon = 30f;

        // Emblem geometry, in a 200 × 200 box.
        public const int StarburstPoints = 14;
        public const float StarburstOuterRadius = 88f;
        public const float StarburstInnerRadius = 72f;
        public const float CircleRadius = 86f;
        public const float HexagonRadius = 88f;

        // Hand fan (§4.5)
        public const float FanSpacing = 150f;
        public static readonly float[] FanRotations = { -14f, -7f, 0f, 7f, 14f };
        public static readonly float[] FanOffsetsY = { 20f, 6f, -40f, 6f, 20f };
        /// <summary>Rotation pivot relative to the card: 50% across, 140% down (below the card).</summary>
        public static readonly Vector2 FanPivot = new Vector2(0.5f, 1.4f);
        public const float PlayableRaise = 40f;
        public const float HoverLift = 16f;
        public const float HoverDuration = 0.22f;
        public const float UnplayableBrightness = 0.55f;

        // Flip (§4.6)
        public const float FlipDuration = 0.8f;
        public const float FlipStaggerMin = 0.03f;
        public const float FlipStaggerMax = 0.04f;

        /// <summary>The spec's easing, cubic-bezier(0.2, 0.8, 0.2, 1), as an AnimationCurve-free evaluator.</summary>
        public static float Ease(float t) => CubicBezier(0.2f, 0.8f, 0.2f, 1f, t);

        /// <summary>CSS cubic-bezier timing function: solves x(u) = t, returns y(u).</summary>
        public static float CubicBezier(float x1, float y1, float x2, float y2, float t)
        {
            t = Mathf.Clamp01(t);
            float u = t;
            for (int i = 0; i < 8; i++)
            {
                float x = Bezier(x1, x2, u) - t;
                float dx = BezierSlope(x1, x2, u);
                if (Mathf.Abs(x) < 1e-5f || Mathf.Abs(dx) < 1e-6f) break;
                u = Mathf.Clamp01(u - x / dx);
            }
            return Bezier(y1, y2, u);
        }

        static float Bezier(float p1, float p2, float u)
        {
            float v = 1f - u;
            return 3f * v * v * u * p1 + 3f * v * u * u * p2 + u * u * u;
        }

        static float BezierSlope(float p1, float p2, float u)
        {
            float v = 1f - u;
            return 3f * v * v * p1 + 6f * v * u * (p2 - p1) + 3f * u * u * (1f - p2);
        }
    }
}
