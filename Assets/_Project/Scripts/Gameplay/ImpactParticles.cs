using System.Collections;
using OrbitDash.Core;
using OrbitDash.Utils;
using UnityEngine;

namespace OrbitDash.Gameplay
{
    /// <summary>
    /// Feedback at the player's position: a red glow burst on hit; on a near miss, a spray of star
    /// sparks plus a quick shield flash around the ship. Built from the ParticleSystem script API; the
    /// star and shield sprites come from Resources (Kenney Space Shooter), with generated fallbacks.
    /// </summary>
    public class ImpactParticles : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Color color = new(1f, 0.4f, 0.3f, 1f);
        [SerializeField] private Color nearMissColor = new(0.35f, 0.9f, 1f, 1f);

        private const float ShieldFlashDuration = 0.35f;

        private ParticleSystem _hitBurst;
        private ParticleSystem _nearMissSparks;
        private SpriteRenderer _shield;
        private Coroutine _shieldRoutine;

        private void Awake()
        {
            Texture2D glow = RuntimeCircleSpriteFactory.CreateSoftGlowTexture(64);
            Texture2D star = Resources.Load<Sprite>("Sprites/Gameplay/Effects/star")?.texture;

            _hitBurst = CreateBurstSystem(gameObject, glow, color, burstCount: 24, speed: 3f, size: new ParticleSystem.MinMaxCurve(0.12f, 0.3f));

            var sparksHost = new GameObject("NearMissSparks");
            sparksHost.transform.SetParent(transform, worldPositionStays: false);
            _nearMissSparks = CreateBurstSystem(sparksHost, star != null ? star : glow, nearMissColor, burstCount: 0, speed: 2.2f, size: new ParticleSystem.MinMaxCurve(0.1f, 0.22f));

            Sprite shieldSprite = Resources.Load<Sprite>("Sprites/Gameplay/Effects/shield");
            if (shieldSprite != null)
            {
                _shield = new GameObject("NearMissShield").AddComponent<SpriteRenderer>();
                _shield.transform.SetParent(transform, worldPositionStays: false);
                _shield.sprite = shieldSprite;
                _shield.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                _shield.sortingOrder = 6;
                _shield.enabled = false;
            }
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

        private void HandlePlayerHit()
        {
            if (player != null)
            {
                transform.position = player.position;
            }
            _hitBurst.Play();
        }

        private void HandleNearMiss(int combo, int bonus)
        {
            if (player == null)
            {
                return;
            }

            var emitParams = new ParticleSystem.EmitParams { position = player.position, applyShapeToPosition = true };
            _nearMissSparks.Emit(emitParams, 8 + 4 * Mathf.Min(combo, 4));

            if (_shield != null)
            {
                if (_shieldRoutine != null)
                {
                    StopCoroutine(_shieldRoutine);
                }
                _shieldRoutine = StartCoroutine(FlashShield());
            }
        }

        /// <summary>The shield pops around the ship, grows and fades: reads as "that was close".</summary>
        private IEnumerator FlashShield()
        {
            _shield.enabled = true;
            float elapsed = 0f;
            while (elapsed < ShieldFlashDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / ShieldFlashDuration);
                _shield.transform.position = player.position;
                _shield.transform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.1f, t);
                _shield.color = new Color(nearMissColor.r, nearMissColor.g, nearMissColor.b, 1f - t);
                yield return null;
            }

            _shield.enabled = false;
            _shieldRoutine = null;
        }

        private static ParticleSystem CreateBurstSystem(GameObject host, Texture texture, Color startColor, short burstCount, float speed, ParticleSystem.MinMaxCurve size)
        {
            // Adding ParticleSystem also auto-attaches its required ParticleSystemRenderer.
            var particles = host.AddComponent<ParticleSystem>();

            // A freshly added ParticleSystem starts playing immediately (playOnAwake defaults to
            // true), and Unity refuses to change main-module values like duration while playing -
            // stop it first before touching any module.
            particles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 0.6f;
            main.startLifetime = 0.5f;
            main.startSpeed = speed;
            main.startSize = size;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 2f * Mathf.PI);
            main.startColor = startColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.useUnscaledTime = true; // keep animating even though GameOver freezes Time.timeScale

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(burstCount > 0 ? new[] { new ParticleSystem.Burst(0f, burstCount) } : new ParticleSystem.Burst[0]);

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.05f;

            // Particles shrink and fade out instead of popping off.
            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var fade = new Gradient();
            fade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colorOverLifetime.color = fade;

            var particleRenderer = host.GetComponent<ParticleSystemRenderer>();
            particleRenderer.material = new Material(Shader.Find("Sprites/Default")) { mainTexture = texture };
            particleRenderer.sortingOrder = 5;
            return particles;
        }
    }
}
