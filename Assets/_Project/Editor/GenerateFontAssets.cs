using TMPro;
using UnityEditor;
using UnityEngine;

namespace OrbitDash.EditorTools
{
    /// <summary>
    /// Generates the TMP Font Asset(s) used by the UI theme, so the SDF atlas doesn't need to be
    /// hand-authored or built interactively via Window > TextMeshPro > Font Asset Creator.
    /// Run via: Unity.exe -batchmode -quit -projectPath . -executeMethod OrbitDash.EditorTools.GenerateFontAssets.GenerateOrbitronFontAsset
    /// </summary>
    public static class GenerateFontAssets
    {
        private const string SourceFontPath = "Assets/_Project/Art/Fonts/Orbitron-VariableFont_wght.ttf";
        private const string OutputAssetPath = "Assets/_Project/Art/Fonts/Resources/Orbitron SDF.asset";

        public static void GenerateOrbitronFontAsset()
        {
            var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
            if (sourceFont == null)
            {
                Debug.LogError($"GenerateFontAssets: source font not found at {SourceFontPath}");
                EditorApplication.Exit(1);
                return;
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
            if (fontAsset == null)
            {
                Debug.LogError("GenerateFontAssets: TMP_FontAsset.CreateFontAsset returned null.");
                EditorApplication.Exit(1);
                return;
            }

            AssetDatabase.CreateAsset(fontAsset, OutputAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"GenerateFontAssets: created {OutputAssetPath}");
        }
    }
}
