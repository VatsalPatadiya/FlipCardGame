using System.Collections.Generic;
using System.IO;
using Eclipse.Core;
using Eclipse.Rendering;
using Eclipse.Themes;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Eclipse.EditorTools
{
    /// <summary>
    /// Assembles the Premium Set Sun table mockup scene: a table background, a draw pile,
    /// a discard pile, and the five-card hand fan (§4.5), all using the Premium Set theme asset.
    /// </summary>
    public static class TableMockupSceneBuilder
    {
        const string ThemePath = "Assets/Eclipse/Themes/Premium Set.asset";
        const string FontPath = "Assets/Eclipse/Fonts/Bungee SDF.asset";
        const string ScenesFolder = "Assets/Eclipse/Scenes";
        const string ScenePath = ScenesFolder + "/TableMockup_PremiumSun.unity";

        static readonly Vector2 CenterAnchor = new Vector2(0.5f, 0.5f);

        /// <summary>The five sample faces placed in the hand (§Interfaces), index 4 (Wild) is the playable one.</summary>
        static readonly (CardKind kind, int color, int number)[] HandFaces =
        {
            (CardKind.Number, 0, 4),
            (CardKind.Number, 2, 9),
            (CardKind.DrawOne, 1, 0),
            (CardKind.Jump, 3, 0),
            (CardKind.Wild, CardFace.NoColor, 0),
        };
        const int PlayableHandIndex = 4;

        [MenuItem("Eclipse/Build Table Mockup Scene")]
        public static void Build()
        {
            var theme = LoadOrCreateTheme();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var canvasRect = BuildCanvas();
            BuildTable(canvasRect, theme);
            BuildDrawPile(canvasRect, theme);
            BuildDiscardPile(canvasRect, theme);

            var handArea = new GameObject("HandArea", typeof(RectTransform)).GetComponent<RectTransform>();
            handArea.SetParent(canvasRect, false);
            handArea.anchorMin = handArea.anchorMax = CenterAnchor;
            handArea.pivot = CenterAnchor;
            handArea.anchoredPosition = new Vector2(0f, -700f);
            BuildHandFan(handArea, theme);

            if (!AssetDatabase.IsValidFolder(ScenesFolder))
                AssetDatabase.CreateFolder("Assets/Eclipse", "Scenes");

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"Eclipse table mockup: scene saved to {ScenePath}.");
        }

        /// <summary>Ensures the Premium Set theme asset exists on disk and has its number font assigned.</summary>
        static CardTheme LoadOrCreateTheme()
        {
            if (!File.Exists(ThemePath))
                ThemeAssetCreator.CreateThemeAssets();

            var theme = AssetDatabase.LoadAssetAtPath<CardTheme>(ThemePath);
            if (theme.numberFont == null)
            {
                theme.numberFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
                EditorUtility.SetDirty(theme);
                AssetDatabase.SaveAssets();
            }
            return theme;
        }

        static RectTransform BuildCanvas()
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            return canvasGo.GetComponent<RectTransform>();
        }

        static void BuildTable(RectTransform canvasRect, CardTheme theme)
        {
            var rect = new GameObject("Table", typeof(RectTransform), typeof(RawImage)).GetComponent<RectTransform>();
            rect.SetParent(canvasRect, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = rect.GetComponent<RawImage>();
            image.texture = FillRasterizer.Bake(theme.sun.table, Color.white, 1080, 1920);
        }

        /// <summary>
        /// No card-back design exists yet (§"What's left to the builder"), so the draw pile is rendered as a
        /// small stack of plain, slightly offset Number cards rather than a face-down back.
        /// </summary>
        static void BuildDrawPile(RectTransform canvasRect, CardTheme theme)
        {
            var pile = new GameObject("DrawPile", typeof(RectTransform)).GetComponent<RectTransform>();
            pile.SetParent(canvasRect, false);
            pile.anchorMin = pile.anchorMax = CenterAnchor;
            pile.pivot = CenterAnchor;
            pile.anchoredPosition = new Vector2(-220f, 100f);

            const int stackCount = 3;
            for (int i = 0; i < stackCount; i++)
            {
                var card = CreateCard(pile, $"DrawPileCard{i}", theme, Side.Sun, CardKind.Number, 0, 1, playable: false);
                var rect = card.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(i * 4f, -i * 4f);
            }
        }

        static void BuildDiscardPile(RectTransform canvasRect, CardTheme theme)
        {
            var pile = new GameObject("DiscardPile", typeof(RectTransform)).GetComponent<RectTransform>();
            pile.SetParent(canvasRect, false);
            pile.anchorMin = pile.anchorMax = CenterAnchor;
            pile.pivot = CenterAnchor;
            pile.anchoredPosition = new Vector2(0f, 100f);

            CreateCard(pile, "DiscardPileCard", theme, Side.Sun, CardKind.Number, 2, 5, playable: false);
        }

        /// <summary>
        /// The five-card hand fan (§4.5): pivot/rotation/offset per card come straight from
        /// <see cref="CardLayoutSpec"/>, with the one playable card (the Wild, index 4) raised.
        /// Factored out from <see cref="Build"/> so it's testable without building the whole scene.
        /// </summary>
        public static List<CardView> BuildHandFan(RectTransform parent, CardTheme theme)
        {
            var cards = new List<CardView>(HandFaces.Length);
            for (int i = 0; i < HandFaces.Length; i++)
            {
                var (kind, color, number) = HandFaces[i];
                bool playable = i == PlayableHandIndex;
                var card = CreateCard(parent, $"HandCard{i}", theme, Side.Sun, kind, color, number, playable);

                var rect = card.GetComponent<RectTransform>();
                rect.pivot = CardLayoutSpec.FanPivot;
                float y = CardLayoutSpec.FanOffsetsY[i] - (playable ? CardLayoutSpec.PlayableRaise : 0f);
                rect.anchoredPosition = new Vector2((i - 2) * CardLayoutSpec.FanSpacing, y);
                rect.localEulerAngles = new Vector3(0f, 0f, CardLayoutSpec.FanRotations[i]);

                cards.Add(card);
            }
            return cards;
        }

        static CardView CreateCard(RectTransform parent, string name, CardTheme theme, Side side, CardKind kind, int color, int number, bool playable)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = CenterAnchor;
            rect.pivot = CenterAnchor;
            rect.sizeDelta = new Vector2(CardLayoutSpec.CardWidth, CardLayoutSpec.CardHeight);

            var card = go.AddComponent<CardView>();
            card.theme = theme;
            card.side = side;
            card.kind = kind;
            card.color = color;
            card.number = number;
            card.playable = playable;
            card.Configure();
            return card;
        }
    }
}
