using UnityEngine;

namespace OrbitDash.Data
{
    /// <summary>
    /// Tunable obstacle parameters. Kept separate from <see cref="DifficultyCurveConfig"/> (added later)
    /// so base obstacle feel and time-based difficulty ramp can be tuned independently.
    /// </summary>
    [CreateAssetMenu(fileName = "ObstacleConfig", menuName = "OrbitDash/Obstacle Config")]
    public class ObstacleConfig : ScriptableObject
    {
        [Tooltip("Seconds between two obstacle spawns.")]
        public float spawnInterval = 1.2f;

        [Tooltip("Angular speed of an obstacle along its ring, in degrees per second.")]
        public float angularSpeedDegreesPerSecond = 90f;

        [Tooltip("Collision diameter of an obstacle, in world units.")]
        public float obstacleColliderDiameter = 0.5f;
    }
}
