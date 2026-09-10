using UnityEditor;
using UnityEngine;

namespace OrbitDash.EditorTools
{
    /// <summary>
    /// Auto-configures any texture imported under Art/Resources/Sprites as a "Sprite (2D and UI)"
    /// with sensible defaults, so drag-and-drop art (or art fetched by a script) never needs manual
    /// per-file Inspector tweaking. Buttons additionally get a 9-slice border so they don't stretch
    /// out of shape at different sizes across screens.
    /// </summary>
    public class SpriteImportPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            string path = assetPath.Replace('\\', '/');
            if (!path.Contains("/_Project/Art/Resources/Sprites/"))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;

            if (path.Contains("/UI/Buttons/"))
            {
                // Keeps the pill-shaped rounded corners crisp regardless of the button's final size.
                importer.spriteBorder = new Vector4(20f, 20f, 20f, 20f);
            }
        }
    }
}
