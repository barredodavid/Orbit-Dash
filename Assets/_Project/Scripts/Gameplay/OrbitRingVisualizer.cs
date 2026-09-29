using OrbitDash.Data;
using OrbitDash.Utils;
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
        [SerializeField] private Color ringColor = new(0.75f, 0.95f, 1f, 0.45f);
        [Tooltip("Soft, wide line drawn under each ring: a neon glow without post-processing.")]
        [SerializeField] private Color glowColor = new(0.3f, 0.8f, 1f, 0.22f);
        [SerializeField] private float glowWidth = 0.22f;
        [SerializeField] private float lineWidth = 0.03f;
        [SerializeField] private int segments = 64;

        private Texture2D _softLine;

        private void Awake()
        {
            _softLine = RuntimeCircleSpriteFactory.CreateSoftLineTexture();
            for (int ringIndex = 0; ringIndex < orbitLayout.RingCount; ringIndex++)
            {
                float radius = orbitLayout.GetRingRadius(ringIndex);
                CreateRingLine(radius, glowWidth, glowColor, sortingOrder: -2, _softLine);
                CreateRingLine(radius, lineWidth, ringColor, sortingOrder: -1, texture: null);
            }
        }

        private void CreateRingLine(float radius, float width, Color color, int sortingOrder, Texture2D texture)
        {
            var ringObject = new GameObject($"Ring_{radius:0.00}");
            ringObject.transform.SetParent(transform, worldPositionStays: false);

            var lineRenderer = ringObject.AddComponent<LineRenderer>();
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.positionCount = segments;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default")) { mainTexture = texture };
            lineRenderer.textureMode = LineTextureMode.Stretch;
            lineRenderer.sortingOrder = sortingOrder;

            for (int i = 0; i < segments; i++)
            {
                float angleRad = i * Mathf.PI * 2f / segments;
                lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f) * radius);
            }
        }
    }
}
