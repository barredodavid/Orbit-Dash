using OrbitDash.Data;
using UnityEngine;

namespace OrbitDash.Gameplay
{
    /// <summary>
    /// Draws each orbit ring as a thin circle outline so the player can see the tracks.
    /// Purely visual and driven entirely by <see cref="OrbitLayoutConfig"/> - easy to swap for a
    /// sprite-based ring/track once real art is imported (see README, step 7).
    /// </summary>
    public class OrbitRingVisualizer : MonoBehaviour
    {
        [SerializeField] private OrbitLayoutConfig orbitLayout;
        [SerializeField] private Color ringColor = new(1f, 1f, 1f, 0.25f);
        [SerializeField] private float lineWidth = 0.03f;
        [SerializeField] private int segments = 64;

        private void Awake()
        {
            for (int ringIndex = 0; ringIndex < orbitLayout.RingCount; ringIndex++)
            {
                CreateRingLine(orbitLayout.GetRingRadius(ringIndex));
            }
        }

        private void CreateRingLine(float radius)
        {
            var ringObject = new GameObject($"Ring_{radius:0.00}");
            ringObject.transform.SetParent(transform, worldPositionStays: false);

            var lineRenderer = ringObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.positionCount = segments;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.startColor = ringColor;
            lineRenderer.endColor = ringColor;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.sortingOrder = -1;

            for (int i = 0; i < segments; i++)
            {
                float angleRad = i * Mathf.PI * 2f / segments;
                lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f) * radius);
            }
        }
    }
}
