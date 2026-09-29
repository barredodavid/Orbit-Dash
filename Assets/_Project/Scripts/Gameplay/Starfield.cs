using UnityEngine;

namespace OrbitDash.Gameplay
{
    /// <summary>
    /// A static field of small background stars, built from the ParticleSystem script API (no
    /// imported texture needed) so the play area doesn't look like empty black space.
    /// </summary>
    public class Starfield : MonoBehaviour
    {
        [SerializeField] private int starCount = 80;
        [SerializeField] private Vector2 areaSize = new(11f, 19f);
        [SerializeField] private float minSize = 0.03f;
        [SerializeField] private float maxSize = 0.09f;

        private void Awake()
        {
            CreateBackdrop();

            var particles = gameObject.AddComponent<ParticleSystem>();
            particles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 1f;
            main.startLifetime = Mathf.Infinity;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, 0.3f), new Color(1f, 1f, 1f, 0.9f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)starCount) });

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Rectangle;
            shape.scale = new Vector3(areaSize.x, areaSize.y, 1f);

            var particleRenderer = GetComponent<ParticleSystemRenderer>();
            particleRenderer.material = new Material(Shader.Find("Sprites/Default"));
            particleRenderer.sortingOrder = -10; // behind the rings, sun, player and obstacles

            particles.Play();
        }

        /// <summary>
        /// Tiles the nebula texture (Resources/Sprites/Background/nebula) behind everything, over an
        /// area larger than any phone aspect ratio. Stays plain camera background if the file is missing.
        /// </summary>
        private void CreateBackdrop()
        {
            var nebula = Resources.Load<Sprite>("Sprites/Background/nebula");
            if (nebula == null)
            {
                return;
            }

            var backdrop = new GameObject("Backdrop").AddComponent<SpriteRenderer>();
            backdrop.transform.SetParent(transform, worldPositionStays: false);
            backdrop.sprite = nebula;
            backdrop.drawMode = SpriteDrawMode.Tiled;
            backdrop.size = areaSize * 1.5f;
            backdrop.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            backdrop.sortingOrder = -20;
        }
    }
}
