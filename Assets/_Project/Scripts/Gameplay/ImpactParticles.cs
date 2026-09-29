using OrbitDash.Core;
using UnityEngine;

namespace OrbitDash.Gameplay
{
    /// <summary>
    /// Particle bursts at the player's position: a big red one on hit, a small cyan one on a near miss.
    /// Built entirely from the ParticleSystem script API, with a generated soft-glow texture (no
    /// imported VFX asset and no hand-authored component needed).
    /// </summary>
    public class ImpactParticles : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Color color = new(1f, 0.4f, 0.3f, 1f);
        [SerializeField] private Color nearMissColor = new(0.35f, 0.9f, 1f, 1f);

        private ParticleSystem _particles;

        private void Awake()
        {
            // Adding ParticleSystem also auto-attaches its required ParticleSystemRenderer.
            _particles = gameObject.AddComponent<ParticleSystem>();

            // A freshly added ParticleSystem starts playing immediately (playOnAwake defaults to
            // true), and Unity refuses to change main-module values like duration while playing -
            // stop it first before touching any module.
            _particles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = _particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 0.6f;
            main.startLifetime = 0.5f;
            main.startSpeed = 3f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.useUnscaledTime = true; // keep animating even though GameOver freezes Time.timeScale

            ParticleSystem.EmissionModule emission = _particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 24) });

            ParticleSystem.ShapeModule shape = _particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.05f;

            var particleRenderer = GetComponent<ParticleSystemRenderer>();
            particleRenderer.material = new Material(Shader.Find("Sprites/Default")) { mainTexture = CreateSoftGlowTexture(64) };

            // Particles shrink and fade out instead of popping off.
            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = _particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = _particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = fade;
            particleRenderer.sortingOrder = 5;
        }

        private void OnEnable()
        {
            GameEvents.PlayerHit += HandlePlayerHit;
            GameEvents.NearMiss += HandleNearMiss;
        }

        private void OnDisable()
        {
            GameEvents.PlayerHit -= HandlePlayerHit;
            GameEvents.NearMiss -= HandleNearMiss;
        }

        private void HandleNearMiss(int combo, int bonus)
        {
            if (player == null)
            {
                return;
            }
            var emitParams = new ParticleSystem.EmitParams { position = player.position, startColor = nearMissColor, applyShapeToPosition = true };
            _particles.Emit(emitParams, 8 + 4 * Mathf.Min(combo, 4));
        }

        /// <summary>Radial white-to-transparent gradient: reads as a soft glow instead of a hard dot.</summary>
        private static Texture2D CreateSoftGlowTexture(int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false) { wrapMode = TextureWrapMode.Clamp };
            float center = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center)) / center;
                    float alpha = Mathf.Clamp01(1f - distance);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha));
                }
            }
            texture.Apply();
            return texture;
        }

        private void HandlePlayerHit()
        {
            if (player != null)
            {
                transform.position = player.position;
            }
            _particles.Play();
        }
    }
}
