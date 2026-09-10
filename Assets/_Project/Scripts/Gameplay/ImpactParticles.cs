using OrbitDash.Core;
using UnityEngine;

namespace OrbitDash.Gameplay
{
    /// <summary>
    /// Spawns a small particle burst at the player's position on hit. Built entirely from the
    /// ParticleSystem script API (no imported VFX asset and no hand-authored component needed) -
    /// swap in a nicer texture/material later via the ParticleSystemRenderer once a particle pack
    /// is imported (see README, step 7).
    /// </summary>
    public class ImpactParticles : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Color color = new(1f, 0.4f, 0.3f, 1f);

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
            main.startSize = 0.15f;
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
            particleRenderer.material = new Material(Shader.Find("Sprites/Default"));
            particleRenderer.sortingOrder = 5;
        }

        private void OnEnable()
        {
            GameEvents.PlayerHit += HandlePlayerHit;
        }

        private void OnDisable()
        {
            GameEvents.PlayerHit -= HandlePlayerHit;
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
