using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Eclipse.EditorTools
{
    /// <summary>Generates the Bungee SDF TMP font asset used for card value/label text.</summary>
    public static class BungeeFontSetup
    {
        const string SourcePath = "Assets/Eclipse/Fonts/Bungee-Regular.ttf";
        const string AssetPath = "Assets/Eclipse/Fonts/Bungee SDF.asset";
        const string AtlasPath = "Assets/Eclipse/Fonts/Bungee SDF Atlas.asset";
        const string MaterialPath = "Assets/Eclipse/Fonts/Bungee SDF Material.mat";

        /// <summary>The exact glyph set the premium set table mockup's sample cards need.</summary>
        const string RequiredCharacters = "0123456789+»WILD";

        [MenuItem("Eclipse/Build Bungee Font Asset")]
        public static void Build()
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(SourcePath);
            if (font == null)
            {
                AssetDatabase.ImportAsset(SourcePath);
                font = AssetDatabase.LoadAssetAtPath<Font>(SourcePath);
            }

            var fontAsset = TMP_FontAsset.CreateFontAsset(font, samplingPointSize: 90, atlasPadding: 5, GlyphRenderMode.SDFAA, atlasWidth: 512, atlasHeight: 512);
            fontAsset.TryAddCharacters(RequiredCharacters, out _);

            AssetDatabase.CreateAsset(fontAsset, AssetPath);

            // Persist the atlas texture and material as their own standalone assets rather than embedding them
            // via AssetDatabase.AddObjectToAsset: embedding them as sub-assets of the font asset was observed
            // to silently wipe the font asset's serialized character/glyph tables on disk (verified empirically
            // across several orderings), so a plain cross-asset reference is used instead.
            AssetDatabase.CreateAsset(fontAsset.atlasTextures[0], AtlasPath);
            AssetDatabase.CreateAsset(fontAsset.material, MaterialPath);

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            Debug.Log($"Eclipse font: Bungee SDF generated at {AssetPath} with glyphs for \"{RequiredCharacters}\".");
        }
    }
}
