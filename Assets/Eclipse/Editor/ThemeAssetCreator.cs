using System.IO;
using Eclipse.Themes;
using UnityEditor;
using UnityEngine;

namespace Eclipse.EditorTools
{
    /// <summary>Writes the three spec themes as editable assets in Assets/Eclipse/Themes.</summary>
    public static class ThemeAssetCreator
    {
        const string Folder = "Assets/Eclipse/Themes";

        [MenuItem("Eclipse/Create Theme Assets")]
        public static void CreateThemeAssets()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/Eclipse", "Themes");

            int created = 0, skipped = 0;
            foreach (var theme in ThemeLibrary.CreateAll())
            {
                string path = $"{Folder}/{theme.name}.asset";
                if (File.Exists(path))
                {
                    // Never overwrite: designers may have tuned the asset by hand.
                    Object.DestroyImmediate(theme);
                    skipped++;
                    continue;
                }
                AssetDatabase.CreateAsset(theme, path);
                created++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"Eclipse themes: {created} created, {skipped} already existed (left unchanged) in {Folder}.");
        }
    }
}
