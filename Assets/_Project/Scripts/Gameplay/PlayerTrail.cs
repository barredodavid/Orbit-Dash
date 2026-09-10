using UnityEngine;

namespace OrbitDash.Gameplay
{
    /// <summary>Adds a short fading trail behind the player as it moves between rings.</summary>
    public class PlayerTrail : MonoBehaviour
    {
        [SerializeField] private Color color = new(0.3f, 0.9f, 1f, 0.6f);
        [SerializeField] private float time = 0.25f;
        [SerializeField] private float width = 0.12f;

        private void Awake()
        {
            var trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = time;
            trail.startWidth = width;
            trail.endWidth = 0f;
            trail.minVertexDistance = 0.02f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = color;
            trail.endColor = new Color(color.r, color.g, color.b, 0f);
            trail.sortingOrder = 1;
        }
    }
}
