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

        /// <summary>
        /// White radial gradient, opaque at the center and transparent at the edge (alpha = (1-d)^2).
        /// Tinted and scaled, it fakes a bloom halo - URP post-processing doesn't run on WebGL here.
        /// </summary>
        public static Texture2D CreateSoftGlowTexture(int size) => CreateRadialTexture(size, d => { float a = Mathf.Clamp01(1f - d); return a * a; });

        /// <summary>1 world unit wide soft-glow sprite (scale the transform to size it).</summary>
        public static Sprite CreateSoftGlowSprite(int size = 64)
        {
            Texture2D texture = CreateSoftGlowTexture(size);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        /// <summary>Black, transparent in the middle and darker toward the corners: a cheap screen vignette.</summary>
        public static Sprite CreateVignetteSprite(int size = 128)
        {
            Texture2D texture = CreateRadialTexture(size, d => Mathf.SmoothStep(0f, 1f, (d - 0.55f) / 0.9f), Color.black);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        /// <summary>1-pixel-wide vertical gradient, opaque in the middle: gives a LineRenderer soft edges across its width.</summary>
        public static Texture2D CreateSoftLineTexture(int height = 32)
        {
            var texture = new Texture2D(1, height, TextureFormat.RGBA32, mipChain: false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < height; y++)
            {
                float alpha = Mathf.Sin(Mathf.PI * (y + 0.5f) / height);
                texture.SetPixel(0, y, new Color(1f, 1f, 1f, alpha * alpha));
            }
            texture.Apply();
            return texture;
        }

        /// <param name="alphaAtDistance">Alpha for a normalized distance from the center (0 = center, 1 = edge midpoint).</param>
        private static Texture2D CreateRadialTexture(int size, System.Func<float, float> alphaAtDistance, Color? tint = null)
        {
            Color color = tint ?? Color.white;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false) { wrapMode = TextureWrapMode.Clamp };
            float center = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center)) / center;
                    texture.SetPixel(x, y, new Color(color.r, color.g, color.b, Mathf.Clamp01(alphaAtDistance(distance))));
                }
            }
            texture.Apply();
            return texture;
        }
    }
}
