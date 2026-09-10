using UnityEngine;

namespace OrbitDash.Utils
{
    /// <summary>
    /// Generates simple filled-circle sprites at runtime so gameplay objects have a visible
    /// placeholder shape before real art is imported (see project README, step 7).
    /// Soft-edged (antialiased) so placeholders don't look too programmer-art-jagged.
    /// </summary>
    public static class RuntimeCircleSpriteFactory
    {
        private const int PixelsPerUnit = 100;

        public static Sprite CreateCircleSprite(float worldDiameter, Color color)
        {
            int diameterPx = Mathf.Max(8, Mathf.RoundToInt(worldDiameter * PixelsPerUnit));
            var texture = new Texture2D(diameterPx, diameterPx, TextureFormat.RGBA32, mipChain: false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };

            float radiusPx = diameterPx * 0.5f;
            Vector2 center = new(radiusPx, radiusPx);
            var pixels = new Color[diameterPx * diameterPx];

            for (int y = 0; y < diameterPx; y++)
            {
                for (int x = 0; x < diameterPx; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    // Antialias the last pixel of the edge instead of a hard cutoff.
                    float alpha = Mathf.Clamp01(radiusPx - distance);
                    pixels[y * diameterPx + x] = new Color(color.r, color.g, color.b, color.a * alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            var rect = new Rect(0, 0, diameterPx, diameterPx);
            var pivot = new Vector2(0.5f, 0.5f);
            return Sprite.Create(texture, rect, pivot, PixelsPerUnit);
        }
    }
}
