using UnityEngine;
using static Eclipse.Themes.ColorRef;

namespace Eclipse.Themes
{
    /// <summary>
    /// The three confirmed themes (§5), built from the spec values. Use these to create the
    /// theme assets (Eclipse → Create Theme Assets) or directly at runtime.
    /// Where the spec leaves a value out, the closest stated value is reused and marked "not in spec".
    /// </summary>
    public static class ThemeLibrary
    {
        public const string PremiumId = "premium";
        public const string GlassId = "glass";
        public const string HoloId = "holo";

        public static CardTheme[] CreateAll() => new[] { CreatePremiumSet(), CreateGlassAurora(), CreatePrismHolo() };

        static ColorStop S(ColorRef c, float position) => new ColorStop(c, position);
        static ColorStop[] Even(params ColorRef[] colors) => Fill.Even(colors);
        static ColorRef[] HexList(params string[] hexes)
        {
            var refs = new ColorRef[hexes.Length];
            for (int i = 0; i < hexes.Length; i++) refs[i] = Hex(hexes[i]);
            return refs;
        }

        static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        /// <summary>A soft color blob for aurora tables: the color fading to nothing.</summary>
        static Fill Blob(Vector2 at, ColorRef c)
        {
            var clear = c;
            clear.alpha = 0f;
            return Fill.Radial(at, S(c, 0f), S(clear, 1f));
        }

        static ColorRef[] CardColorsAsRefs(SideSkin skin)
        {
            var refs = new ColorRef[skin.colors.Length];
            for (int i = 0; i < refs.Length; i++) refs[i] = new ColorRef { color = skin.colors[i], alpha = 1f };
            return refs;
        }

        // ---------------------------------------------------------------- A. Premium Set

        public static CardTheme CreatePremiumSet()
        {
            var t = ScriptableObject.CreateInstance<CardTheme>();
            t.name = "Premium Set";
            t.themeId = PremiumId;
            t.displayName = "Premium Set";
            t.numberFontName = "Bungee";
            t.numberScale = 1f;
            t.headingFontName = "Bricolage Grotesque";

            var ink = Hex("#1B1530");
            var cream = Hex("#FFF6E4");
            var panelCenter = new Vector2(0.3f, 0.18f);

            // Sun: warm, glossy, green felt.
            var sun = t.sun;
            sun.colorNames = new[] { "Tangerine", "Berry", "Aqua", "Lime" };
            sun.colors = Palette.Hexes("#FF7A1A", "#E8327C", "#13B5C4", "#8CC63F");
            sun.table = Fill.Radial(Center, S(Hex("#22503F"), 0f), S(Hex("#163629"), 0.55f), S(Hex("#0E241B"), 1f));
            sun.frame = Fill.Linear(160f, Even(Hex("#2E2554"), Hex("#120D24")));
            sun.framePadding = 9f;
            sun.outerRadius = 22f;
            sun.innerRadius = 15f;
            sun.panel = Fill.Radial(panelCenter, S(CardLighten(0.35f), 0f), S(Card(), 0.52f), S(CardDarken(0.35f), 1f));
            sun.patternOpacity = 0.16f;
            sun.patternSize = 400f;
            sun.patternRotation = 8f;
            sun.emblem = new EmblemStyle
            {
                shape = EmblemShape.Starburst, size = 184f, fill = cream,
                innerOutline = true, innerOutlineColor = Card(0.45f), innerOutlineScale = 0.86f,
                shadows = new[] { ShadowSpec.Drop(0f, 8f, 8f, Rgba(0, 0, 0, 0.3f)) },
            };
            sun.jump = new SymbolStyle
            {
                outerStroke = ink, outerStrokeWidth = CardLayoutSpec.SymbolOuterStroke,
                innerStroke = Card(), innerStrokeWidth = CardLayoutSpec.SymbolInnerStroke,
            };
            sun.number = new NumberStyle
            {
                fill = Fill.Linear(180f, S(CardLighten(0.42f), 0f), S(Card(), 0.55f), S(CardDarken(0.15f), 1f)),
                outline = ink, outlineWidth = 16f,
                shadows = new[] { ShadowSpec.Drop(0f, 8f, 0f, Rgba(27, 21, 48, 0.35f)) },
            };
            sun.chip = new ChipStyle
            {
                background = cream, text = ink, radius = 10f,
                shadows = new[] { ShadowSpec.Drop(0f, 3f, 0f, Rgba(27, 21, 48, 0.4f)) },
            };
            sun.gloss = Fill.Linear(180f, S(White(0.32f), 0f), S(White(0f), 0.46f));
            sun.cardShadow = ShadowSpec.Drop(0f, 24f, 40f, Rgba(0, 0, 0, 0.45f));
            sun.playableGlow = new[] { ShadowSpec.Ring(3f, Hex("#FFE9A8")), ShadowSpec.Glow(44f, Rgba(255, 220, 140, 0.65f)) };
            sun.wild = new WildStyle
            {
                label = "WILD",
                frame = Fill.Linear(150f, S(Hex("#FBE3A0"), 0f), S(Hex("#D9A441"), 0.35f), S(Hex("#A8741F"), 0.6f), S(Hex("#F6D98B"), 1f)),
                panel = Fill.Radial(Center, Even(Hex("#3A2E66"), Hex("#120D24"))),
                nestedAsOutlines = false,
                labelColor = cream, labelOutline = ink,
            };
            sun.uiHeading = cream.color;      // not in spec
            sun.uiMuted = new Color(1f, 0.965f, 0.894f, 0.7f);
            sun.uiEyebrow = Palette.Hex("#FFE9A8");

            // Moon: midnight with neon.
            var moon = t.moon;
            moon.colorNames = new[] { "Violet", "Rose", "Ice", "Ember" };
            moon.colors = Palette.Hexes("#8F6BFF", "#FF4F86", "#3FD6E8", "#FF8A3D");
            moon.table = Fill.Radial(Center, Even(Hex("#2A2160"), Hex("#151036"), Hex("#08061A")));
            moon.frame = Fill.Linear(160f, Even(Hex("#241E4A"), Hex("#07051A")));
            moon.framePadding = 9f;
            moon.outerRadius = 22f;
            moon.innerRadius = 15f;
            moon.panel = Fill.Radial(panelCenter, S(CardDarken(0.45f), 0f), S(CardDarken(0.72f), 0.55f), S(Hex("#07051A"), 1f));
            moon.starfieldOpacity = 0.55f;
            moon.emblem = new EmblemStyle
            {
                shape = EmblemShape.Starburst, size = 184f, fill = White(0.06f), stroke = Card(), strokeWidth = 5f,
                shadows = new[] { ShadowSpec.Glow(10f, Card()) },
            };
            moon.number = new NumberStyle
            {
                fill = Fill.Linear(180f, S(White(), 0f), S(White(), 0.4f), S(CardLighten(0.45f), 1f)),
                outline = CardDarken(0.6f), outlineWidth = 14f,
                shadows = new[] { ShadowSpec.Glow(24f, Card()), ShadowSpec.Glow(8f, Card()) },
            };
            moon.chip = new ChipStyle { background = Hex("#0E0B22"), border = Card(), borderWidth = 1.5f, text = White(), radius = 10f };
            moon.gloss = Fill.Linear(180f, S(White(0.14f), 0f), S(White(0f), 0.4f));
            moon.cardShadow = sun.cardShadow; // not in spec
            moon.playableGlow = new[] { ShadowSpec.Ring(3f, Card()), ShadowSpec.Glow(48f, Card()) };
            moon.wild = new WildStyle
            {
                label = "CHASE",
                frame = Fill.Linear(150f, Even(Hex("#F2F4FF"), Hex("#9AA3C7"), Hex("#5D6488"), Hex("#E3E7FF"))),
                panel = Fill.Radial(Center, Even(Hex("#241E4A"), Hex("#07051A"))), // not in spec
                nestedAsOutlines = true, nestedStrokeWidth = 5f,
                labelColor = White(), labelOutline = Hex("#07051A"),
            };
            moon.uiHeading = Color.white;
            moon.uiMuted = new Color(1f, 1f, 1f, 0.65f);
            moon.uiEyebrow = Palette.Hex("#8F6BFF");
            return t;
        }

        // ---------------------------------------------------------------- B. Glass Aurora

        public static CardTheme CreateGlassAurora()
        {
            var t = ScriptableObject.CreateInstance<CardTheme>();
            t.name = "Glass Aurora";
            t.themeId = GlassId;
            t.displayName = "Glass Aurora";
            t.numberFontName = "Righteous";
            t.numberScale = 1.04f;
            t.headingFontName = "Unbounded";

            var deep = Hex("#1E1640");
            var glowCenter = new Vector2(0.5f, 0.52f);

            // Sun: airy frosted glass.
            var sun = t.sun;
            sun.colorNames = new[] { "Coral", "Violet", "Jade", "Azure" };
            sun.colors = Palette.Hexes("#FF5E6C", "#6C5CE7", "#00A884", "#0984E3");
            sun.table = Fill.Solid(Hex("#F4F0FF"));
            sun.tableBlobs = new[]
            {
                Blob(new Vector2(0.15f, 0.12f), Hex("#FFB8CC")),
                Blob(new Vector2(0.85f, 0.12f), Hex("#A6DCFF")),
                Blob(new Vector2(0.5f, 0.95f), Hex("#CDBDFF")),
            };
            sun.frame = Fill.Linear(160f, Even(White(0.95f), White(0.4f)));
            sun.framePadding = 2f;
            sun.outerRadius = 24f;
            sun.innerRadius = 22f;
            sun.panel = Fill.Linear(160f, Even(White(0.8f), White(0.35f)));
            sun.panelOverlay = Fill.Radial(glowCenter, S(Card(0.75f), 0f), S(Card(0f), 0.72f));
            sun.backgroundBlur = true;
            sun.emblem = new EmblemStyle
            {
                shape = EmblemShape.Circle, size = 184f, fill = White(0.4f), stroke = White(0.95f), strokeWidth = 3f,
                shadows = new[] { ShadowSpec.Drop(0f, 10f, 18f, Rgba(30, 22, 64, 0.18f)) },
            };
            sun.number = new NumberStyle
            {
                fill = Fill.Solid(Card()), outline = White(), outlineWidth = 14f,
                shadows = new[] { ShadowSpec.Drop(0f, 6f, 10f, Rgba(30, 22, 64, 0.28f)) },
            };
            sun.chip = new ChipStyle
            {
                background = White(0.75f), border = White(0.95f), borderWidth = 1f, text = CardDarken(0.35f), radius = 12f,
                shadows = new[] { ShadowSpec.Drop(0f, 4f, 10f, Rgba(30, 22, 64, 0.12f)) },
            };
            sun.gloss = Fill.Linear(135f, S(White(0.6f), 0f), S(White(0f), 0.42f));
            sun.cardShadow = ShadowSpec.Drop(0f, 30f, 50f, Rgba(60, 40, 140, 0.25f));
            sun.playableGlow = new[] { ShadowSpec.Ring(2f, White()), ShadowSpec.Glow(60f, White(0.95f)) };
            sun.wild = new WildStyle
            {
                label = "WILD",
                frame = Fill.Linear(135f, Even(CardColorsAsRefs(sun))),
                panel = Fill.Solid(White()), // plus four soft color blobs, drawn from the side colors
                nestedAsOutlines = true, nestedStroke = White(), nestedStrokeWidth = 3f,
                labelColor = White(), labelOutline = deep,
            };
            sun.uiHeading = deep.color;
            sun.uiMuted = Palette.Hex("#4A3F6E");
            sun.uiEyebrow = Palette.Hex("#5B3FD0");

            // Moon: northern lights.
            var moon = t.moon;
            moon.colorNames = new[] { "Amethyst", "Magenta", "Cyan", "Volt" };
            moon.colors = Palette.Hexes("#A27BFF", "#FF4FA3", "#2FF3E0", "#E7FF5A");
            moon.table = Fill.Solid(Hex("#070A1F"));
            moon.tableBlobs = new[]
            {
                Blob(new Vector2(0.2f, 0.15f), Rgba(130, 70, 255, 0.55f)),
                Blob(new Vector2(0.85f, 0.35f), Rgba(0, 220, 200, 0.35f)),
                Blob(new Vector2(0.4f, 0.9f), Rgba(255, 60, 140, 0.35f)),
            };
            moon.frame = Fill.Linear(160f, Even(White(0.45f), White(0.08f)));
            moon.framePadding = 2f;
            moon.outerRadius = 24f;
            moon.innerRadius = 22f;
            moon.panel = Fill.Linear(160f, Even(Rgba(40, 44, 90, 0.85f), Rgba(10, 12, 32, 0.92f)));
            moon.panelOverlay = Fill.Radial(glowCenter, S(Card(0.5f), 0f), S(Card(0f), 0.72f));
            moon.starfieldOpacity = 0.55f; // not in spec
            moon.backgroundBlur = true;
            moon.emblem = new EmblemStyle
            {
                shape = EmblemShape.Circle, size = 184f, fill = White(0f), stroke = Card(), strokeWidth = 3f,
                shadows = new[] { ShadowSpec.Glow(10f, Card()) },
            };
            moon.number = new NumberStyle
            {
                fill = Fill.Solid(White()), outline = CardDarken(0.6f), outlineWidth = 14f,
                shadows = new[] { ShadowSpec.Glow(14f, Card()) },
            };
            moon.chip = new ChipStyle { background = Rgba(10, 12, 32, 0.7f), border = Card(), borderWidth = 1f, text = White(), radius = 12f };
            moon.gloss = Fill.Linear(135f, S(White(0.25f), 0f), S(White(0f), 0.42f)); // not in spec
            moon.cardShadow = ShadowSpec.Drop(0f, 30f, 50f, Rgba(0, 0, 0, 0.45f)); // not in spec
            moon.playableGlow = new[] { ShadowSpec.Ring(2f, Card()), ShadowSpec.Glow(60f, Card()) };
            moon.wild = new WildStyle
            {
                label = "CHASE",
                frame = Fill.Linear(135f, Even(CardColorsAsRefs(moon))),
                panel = moon.panel,
                nestedAsOutlines = true, nestedStrokeWidth = 3f,
                labelColor = White(), labelOutline = Hex("#070A1F"),
            };
            moon.uiHeading = Color.white;
            moon.uiMuted = new Color(1f, 1f, 1f, 0.65f);
            moon.uiEyebrow = Palette.Hex("#2FF3E0");
            return t;
        }

        // ---------------------------------------------------------------- C. Prism Holo

        public static CardTheme CreatePrismHolo()
        {
            var t = ScriptableObject.CreateInstance<CardTheme>();
            t.name = "Prism Holo";
            t.themeId = HoloId;
            t.displayName = "Prism Holo";
            t.numberFontName = "Orbitron";
            t.numberScale = 0.84f;
            t.headingFontName = "Syne";
            t.holoTilt = true;

            var chrome = Hex("#0B0C12");

            // Sun: collector's foil on pearl.
            var sun = t.sun;
            sun.colorNames = new[] { "Ruby", "Cobalt", "Emerald", "Amber" };
            sun.colors = Palette.Hexes("#F23E6B", "#4C6BFF", "#12A884", "#F29A0C");
            sun.table = Fill.Radial(Center, S(Hex("#FAFAFD"), 0f), S(Hex("#E6E7F0"), 0.6f), S(Hex("#C9CBDA"), 1f));
            sun.frame = Fill.Conic(210f, Even(HexList("#FFC9E3", "#C3D4FF", "#BFFBEA", "#FFF1B8", "#E3C9FF", "#FFC9E3")));
            sun.framePadding = 7f;
            sun.outerRadius = 22f;
            sun.innerRadius = 16f;
            sun.panel = Fill.Linear(160f, Even(Hex("#FFFFFF"), Hex("#EFF0F7")));
            sun.panelHairline = Rgba(20, 21, 31, 0.08f);
            sun.emblem = new EmblemStyle
            {
                shape = EmblemShape.Hexagon, size = 184f, fill = Card(), stroke = White(), strokeWidth = 6f,
                shadows = new[] { ShadowSpec.Drop(0f, 10f, 14f, Rgba(20, 21, 31, 0.22f)) },
            };
            sun.number = new NumberStyle
            {
                fill = Fill.Linear(160f, Even(Hex("#FFFFFF"), Hex("#FFE3F1"), Hex("#DCEBFF"))),
                outline = CardDarken(0.45f), outlineWidth = 11f,
                shadows = new[] { ShadowSpec.Drop(0f, 3f, 0f, Rgba(0, 0, 0, 0.18f)) },
            };
            sun.chip = new ChipStyle
            {
                background = Card(), text = White(), radius = 8f,
                shadows = new[] { ShadowSpec.Drop(0f, 2f, 6f, Rgba(20, 21, 31, 0.2f)) },
            };
            sun.gloss = HoloSheen(0.7f);
            sun.glossSweeps = true;
            sun.cardShadow = ShadowSpec.Drop(0f, 24f, 40f, Rgba(20, 21, 31, 0.25f));
            sun.playableGlow = new[] { ShadowSpec.Ring(3f, White()), ShadowSpec.Ring(6f, Card()), ShadowSpec.Glow(50f, Card(0.5f)) };
            sun.wild = new WildStyle
            {
                label = "WILD",
                frame = Fill.Conic(210f, Even(HexList("#FF7AB8", "#7A9BFF", "#5FFFD2", "#FFE36F", "#C98BFF", "#FF7AB8"))),
                panel = Fill.Linear(160f, Even(Hex("#1D1F2E"), chrome)),
                nestedAsOutlines = false, nestedStroke = White(), nestedStrokeWidth = 4f,
                labelColor = White(), labelOutline = chrome,
            };
            sun.uiHeading = Palette.Hex("#14151F");
            sun.uiMuted = Palette.Hex("#45475A");
            sun.uiEyebrow = Palette.Hex("#5A49D6");

            // Moon: black chrome with a thin rainbow rim.
            var moon = t.moon;
            moon.colorNames = new[] { "Hot Pink", "Periwinkle", "Mint", "Gold" };
            moon.colors = Palette.Hexes("#FF4D8D", "#6E7BFF", "#2DE3B4", "#FFC23D");
            moon.table = Fill.Radial(Center, Even(Hex("#22243A"), Hex("#0F1019"), Hex("#050509")));
            moon.frame = Fill.Conic(210f, Even(HexList("#FF6FB5", "#6F8BFF", "#4FFFD2", "#FFE16F", "#C77BFF", "#FF6FB5")));
            moon.framePadding = 3f;
            moon.outerRadius = 22f;
            moon.innerRadius = 19f;
            moon.panel = Fill.Linear(160f, Even(Hex("#1C1D2A"), chrome));
            moon.starfieldOpacity = 0.55f; // not in spec
            moon.emblem = new EmblemStyle
            {
                shape = EmblemShape.Hexagon, size = 184f, fill = CardDarken(0.62f), stroke = Card(), strokeWidth = 4f,
                shadows = new[] { ShadowSpec.Glow(10f, Card()) },
            };
            moon.number = new NumberStyle
            {
                fill = Fill.Linear(160f, Even(Hex("#FFB8DC"), Hex("#B8C8FF"), Hex("#B8FFE9"))),
                outline = chrome, outlineWidth = 11f,
                shadows = new[] { ShadowSpec.Glow(10f, Card(0.8f)) },
            };
            moon.chip = new ChipStyle { background = chrome, border = Card(), borderWidth = 1f, text = Card(), radius = 8f };
            moon.gloss = HoloSheen(0.2f);
            moon.glossSweeps = true;
            moon.cardShadow = ShadowSpec.Drop(0f, 24f, 40f, Rgba(0, 0, 0, 0.5f)); // not in spec
            moon.playableGlow = new[] { ShadowSpec.Ring(2f, Card()), ShadowSpec.Glow(54f, Card()) };
            moon.wild = new WildStyle
            {
                label = "CHASE",
                frame = moon.frame,
                panel = Fill.Linear(160f, Even(Hex("#16171F"), Hex("#050509"))),
                nestedAsOutlines = true, nestedStrokeWidth = 4f,
                labelColor = White(), labelOutline = chrome,
            };
            moon.uiHeading = Color.white;
            moon.uiMuted = new Color(1f, 1f, 1f, 0.65f);
            moon.uiEyebrow = Palette.Hex("#C77BFF");
            return t;
        }

        /// <summary>The holo sheen band at 115°; <paramref name="whiteAlpha"/> is 0.7 on Sun, 0.2 on Moon.</summary>
        static Fill HoloSheen(float whiteAlpha) => Fill.Linear(115f,
            S(White(0f), 0f), S(White(0f), 0.28f), S(White(whiteAlpha), 0.44f),
            S(Rgba(200, 220, 255, 0.35f * whiteAlpha / 0.7f), 0.5f), S(White(0f), 0.62f), S(White(0f), 1f));
    }
}
