using UnityEngine;

namespace OrbitDash.Data
{
    /// <summary>
    /// Ramps difficulty over a run's survival time by scaling ObstacleConfig's base values.
    /// Curves are evaluated on normalized time (0 = run start, 1 = curveDurationSeconds elapsed,
    /// clamped beyond that), so the whole ramp is tunable in the Inspector without touching code.
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyCurveConfig", menuName = "OrbitDash/Difficulty Curve Config")]
    public class DifficultyCurveConfig : ScriptableObject
    {
        [Tooltip("Survival time, in seconds, over which the curves below ramp from start to end.")]
        public float curveDurationSeconds = 60f;

        [Tooltip("Multiplies ObstacleConfig.spawnInterval over time (below 1 = obstacles spawn faster).")]
        public AnimationCurve spawnIntervalMultiplier = AnimationCurve.Linear(0f, 1f, 1f, 0.4f);

        [Tooltip("Multiplies ObstacleConfig.angularSpeedDegreesPerSecond over time (above 1 = obstacles move faster).")]
        public AnimationCurve speedMultiplier = AnimationCurve.Linear(0f, 1f, 1f, 1.8f);

        public float EvaluateSpawnIntervalMultiplier(float elapsedSeconds) => spawnIntervalMultiplier.Evaluate(NormalizeTime(elapsedSeconds));

        public float EvaluateSpeedMultiplier(float elapsedSeconds) => speedMultiplier.Evaluate(NormalizeTime(elapsedSeconds));

        private float NormalizeTime(float elapsedSeconds) =>
            curveDurationSeconds > 0f ? Mathf.Clamp01(elapsedSeconds / curveDurationSeconds) : 1f;
    }
}
