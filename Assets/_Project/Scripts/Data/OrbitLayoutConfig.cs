using UnityEngine;

namespace OrbitDash.Data
{
    /// <summary>
    /// Tunable geometry for the concentric orbit rings, shared by the player controller,
    /// the ring visuals and (later) the obstacle spawner so they always agree on where the rings are.
    /// Editing this asset does not require touching or recompiling any code.
    /// </summary>
    [CreateAssetMenu(fileName = "OrbitLayoutConfig", menuName = "OrbitDash/Orbit Layout Config")]
    public class OrbitLayoutConfig : ScriptableObject
    {
        [Tooltip("Radius of each ring in world units, ordered from innermost to outermost.")]
        public float[] ringRadii = { 1.6f, 2.7f, 3.8f };

        [Tooltip("Fixed angular position of the player on the circle, in degrees (0 = right, 90 = top).")]
        public float playerAngleDegrees = 90f;

        public int RingCount => ringRadii.Length;

        public float GetRingRadius(int ringIndex) => ringRadii[ringIndex];
    }
}
