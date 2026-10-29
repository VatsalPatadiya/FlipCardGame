using Eclipse.Core;
using Eclipse.Themes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Eclipse.Rendering
{
    /// <summary>
    /// A single card face. Reads <see cref="CardTheme"/> data for <see cref="side"/>/<see cref="kind"/>/<see cref="color"/>
    /// and builds every visual layer (frame, panel, emblem/symbol, number, chips, gloss) as child RectTransforms (§4).
    /// Call <see cref="Configure"/> whenever the fields change; it tears down and rebuilds its children each time.
    /// </summary>
    public class CardView : MonoBehaviour
    {
        public CardTheme theme;
        public Side side;
        public CardKind kind;
        /// <summary>0-3, or <see cref="CardFace.NoColor"/> for Wild/Wild+2/Wild Chase.</summary>
        public int color;
        /// <summary>1-9 for Number kind, else 0.</summary>
        public int number;
        public bool playable;

        static readonly Vector2 CenterAnchor = new Vector2(0.5f, 0.5f);

        public void Configure()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);

            var skin = theme.Skin(side);
            var cardColor = color == CardFace.NoColor ? Color.white : skin.CardColor(color);
            int width = Mathf.RoundToInt(CardLayoutSpec.CardWidth);
            int height = Mathf.RoundToInt(CardLayoutSpec.CardHeight);
            bool isWild = CardFace.IsWildKind(kind);

            if (playable)
                AddPlayableGlow(skin, cardColor, width, height);

            // Wild frame/panel use the theme's own foil fills, not the card color (§4.4).
            var frameFill = isWild ? skin.wild.frame : skin.frame;
            var panelFill = isWild ? skin.wild.panel : skin.panel;
            var frameColor = isWild ? Color.white : cardColor;

            AddImage("Frame", FillRasterizer.Bake(frameFill, frameColor, width, height), new Vector2(width, height));

            float pad = skin.framePadding;
            int panelWidth = Mathf.Max(1, Mathf.RoundToInt(width - 2f * pad));
            int panelHeight = Mathf.Max(1, Mathf.RoundToInt(height - 2f * pad));
            AddImage("Panel", FillRasterizer.Bake(panelFill, frameColor, panelWidth, panelHeight), new Vector2(panelWidth, panelHeight));

            if (isWild)
            {
                var wildEmblemTex = WildRasterizer.BakeNestedEmblems(skin.wild, skin.emblem, skin.colors, width);
                AddImage("WildEmblem", wildEmblemTex, new Vector2(width, width), new Vector2(0f, height * 0.08f));
                AddLabel("WildLabel", skin.wild.label, CardLayoutSpec.ChipFontSize * 1.6f,
                    skin.wild.labelColor.Resolve(cardColor), new Vector2(width, 60f), new Vector2(0f, -height * 0.32f));
            }
            else if (kind == CardKind.Jump)
            {
                var symbolTex = SymbolRasterizer.Bake(skin.jump, CardLayoutSpec.JumpPath, cardColor, width);
                AddImage("Symbol", symbolTex, new Vector2(width, width));
            }
            else
            {
                var emblemTex = EmblemRasterizer.Bake(skin.emblem, cardColor, width);
                AddImage("Emblem", emblemTex, new Vector2(width, width));

                float fontSize = (kind == CardKind.Number ? CardLayoutSpec.NumberSingleDigitPremium : CardLayoutSpec.NumberTwoChars)
                    * theme.numberScale;
                string label = new CardFace(side, kind, color, number).Label;
                AddLabel("Number", label, fontSize, skin.number.fill.Evaluate(0.5f, cardColor), new Vector2(width, height));
            }

            AddChip("ChipTopLeft", skin, cardColor, new Vector2(-1f, 1f));
            AddChip("ChipBottomRight", skin, cardColor, new Vector2(1f, -1f));

            AddImage("Gloss", FillRasterizer.Bake(skin.gloss, cardColor, width, height), new Vector2(width, height));
        }

        void AddChip(string name, SideSkin skin, Color cardColor, Vector2 cornerSign)
        {
            float halfW = CardLayoutSpec.CardWidth / 2f;
            float halfH = CardLayoutSpec.CardHeight / 2f;
            float chipHalfW = CardLayoutSpec.ChipMinWidth / 2f;
            float chipHalfH = CardLayoutSpec.ChipMinHeight / 2f;
            var position = new Vector2(
                cornerSign.x * (halfW - CardLayoutSpec.ChipInset - chipHalfW),
                cornerSign.y * (halfH - CardLayoutSpec.ChipInset - chipHalfH));

            string label = new CardFace(side, kind, color, number).Label;
            var chip = AddLabel(name, label, CardLayoutSpec.ChipFontSize, skin.chip.text.Resolve(cardColor),
                new Vector2(CardLayoutSpec.ChipMinWidth, CardLayoutSpec.ChipMinHeight), position);

            // Bottom-right chip is drawn upside down so it reads correctly from the opposite hand (§4.1).
            if (cornerSign.y < 0f)
                chip.rectTransform.localEulerAngles = new Vector3(0f, 0f, 180f);
        }

        void AddPlayableGlow(SideSkin skin, Color cardColor, int width, int height)
        {
            if (skin.playableGlow == null || skin.playableGlow.Length == 0)
                return;

            var glowShadow = skin.playableGlow[0];
            foreach (var shadow in skin.playableGlow)
                if (shadow.blur > glowShadow.blur) glowShadow = shadow;

            var glowColor = glowShadow.color.Resolve(cardColor);
            var fill = Fill.Radial(CenterAnchor,
                new ColorStop(new ColorRef { color = glowColor, alpha = 1f }, 0f),
                new ColorStop(new ColorRef { color = glowColor, alpha = 0f }, 1f));

            int glowWidth = Mathf.RoundToInt(width * 1.3f);
            int glowHeight = Mathf.RoundToInt(height * 1.3f);
            AddImage("PlayableGlow", FillRasterizer.Bake(fill, cardColor, glowWidth, glowHeight), new Vector2(glowWidth, glowHeight));
        }

        RectTransform CreateChild(string name)
        {
            var child = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)child.transform;
            rect.SetParent(transform, false);
            rect.anchorMin = rect.anchorMax = CenterAnchor;
            rect.pivot = CenterAnchor;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            return rect;
        }

        RawImage AddImage(string name, Texture2D texture, Vector2 size, Vector2? anchoredPosition = null)
        {
            var rect = CreateChild(name);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition ?? Vector2.zero;
            var image = rect.gameObject.AddComponent<RawImage>();
            image.texture = texture;
            return image;
        }

        TextMeshProUGUI AddLabel(string name, string text, float fontSize, Color color, Vector2 size, Vector2? anchoredPosition = null)
        {
            var rect = CreateChild(name);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition ?? Vector2.zero;
            var tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            var font = theme.numberFont as TMP_FontAsset;
            if (font != null)
                tmp.font = font;
            return tmp;
        }
    }
}
