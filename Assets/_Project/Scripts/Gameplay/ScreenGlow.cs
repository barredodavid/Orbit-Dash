using OrbitDash.Utils;
using UnityEngine;

namespace OrbitDash.Gameplay
{
    /// <summary>
    /// Neon look without post-processing: URP disables its post-processing passes on WebGL here
    /// (the FSR upscaling shader isn't supported, which turns bloom off entirely), so the glow is
    /// faked with soft halo sprites behind the bright elements plus a screen vignette. Built at
    /// startup from code, like the rest of the scene dressing - no scene edits needed.
    /// </summary>
    public static class ScreenGlow
    {
        private static readonly Color PlayerGlow = new(0.3f, 0.8f, 1f, 0.55f);
        private static readonly Color SunGlow = new(1f, 0.45f, 0.2f, 0.45f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Setup()
        {
            Sprite glow = RuntimeCircleSpriteFactory.CreateSoftGlowSprite();

            PlayerMarker player = Object.FindFirstObjectByType<PlayerMarker>();
            if (player != null)
            {
                AddHalo(player.transform, glow, PlayerGlow, worldSize: 1.6f);
            }

            GameObject sun = GameObject.Find("Sun");
            if (sun != null)
            {
                AddHalo(sun.transform, glow, SunGlow, worldSize: 4.5f);
            }

            Camera camera = Camera.main;
            if (camera != null && camera.orthographic)
            {
                AddVignette(camera);
            }
        }

        /// <summary>Soft tinted disc drawn just behind <paramref name="target"/>, following it (child transform).</summary>
        private static void AddHalo(Transform target, Sprite glow, Color color, float worldSize)
        {
            var halo = new GameObject("Glow").AddComponent<SpriteRenderer>();
            halo.transform.SetParent(target, worldPositionStays: false);
            // Counter the parent's scale so worldSize stays in world units.
            Vector3 parentScale = target.lossyScale;
            halo.transform.localScale = new Vector3(worldSize / parentScale.x, worldSize / parentScale.y, 1f);
            halo.sprite = glow;
            halo.color = color;
            halo.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

            int targetOrder = target.TryGetComponent(out SpriteRenderer targetRenderer) ? targetRenderer.sortingOrder : 0;
            halo.sortingOrder = targetOrder - 1;
        }

        /// <summary>Darkens the screen corners, parented to the camera so it always covers the view.</summary>
        private static void AddVignette(Camera camera)
        {
            var vignette = new GameObject("Vignette").AddComponent<SpriteRenderer>();
            vignette.transform.SetParent(camera.transform, worldPositionStays: false);
            vignette.transform.localPosition = new Vector3(0f, 0f, 1f); // just in front of the camera's near plane
            float height = camera.orthographicSize * 2f;
            // Wide enough for any phone aspect ratio: the texture is transparent in the middle anyway.
            vignette.transform.localScale = new Vector3(height * 1.2f, height * 1.2f, 1f);
            vignette.sprite = RuntimeCircleSpriteFactory.CreateVignetteSprite();
            vignette.color = new Color(1f, 1f, 1f, 0.65f);
            vignette.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            vignette.sortingOrder = 50; // above gameplay; the UI canvas is an overlay and stays on top
        }
    }
}
