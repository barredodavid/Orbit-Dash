using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

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

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                samplingPointSize: 90,
                atlasPadding: 9,
                renderMode: GlyphRenderMode.SDFAA,
                atlasWidth: 1024,
                atlasHeight: 1024,
                atlasPopulationMode: AtlasPopulationMode.Dynamic,
                enableMultiAtlasSupport: true);

            if (fontAsset == null)
            {
                Debug.LogError("GenerateFontAssets: TMP_FontAsset.CreateFontAsset returned null.");
                EditorApplication.Exit(1);
                return;
            }

            AssetDatabase.CreateAsset(fontAsset, OutputAssetPath);

            // CreateFontAsset builds the atlas texture(s) and material in memory but does not persist
            // them - without registering them as sub-assets here, they'd serialize as null references
            // and the font would render no glyphs at all.
            if (fontAsset.atlasTextures != null)
            {
                foreach (Texture2D atlasTexture in fontAsset.atlasTextures)
                {
                    if (atlasTexture == null)
                    {
                        continue;
                    }
                    atlasTexture.name = fontAsset.name + " Atlas";
                    AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);
                }
            }

            if (fontAsset.material != null)
            {
                fontAsset.material.name = fontAsset.name + " Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }
            else
            {
                Debug.LogWarning("GenerateFontAssets: fontAsset.material was null after creation.");
            }

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            int atlasCount = fontAsset.atlasTextures?.Length ?? 0;
            Debug.Log($"GenerateFontAssets: created {OutputAssetPath} (atlasTextures={atlasCount}, material={fontAsset.material != null})");
        }
    }
}
