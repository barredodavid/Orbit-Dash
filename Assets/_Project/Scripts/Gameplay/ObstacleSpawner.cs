using System.Collections.Generic;
using OrbitDash.Core;
using OrbitDash.Data;
using OrbitDash.Utils;
using UnityEngine;

namespace OrbitDash.Gameplay
{
    /// <summary>
    /// Periodically spawns obstacles on a random ring using a pool (see <see cref="ObjectPool{T}"/>)
    /// instead of repeated Instantiate/Destroy calls. Enabled/disabled by GameManager based on state.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        [SerializeField] private Obstacle obstaclePrefab;
        [SerializeField] private OrbitLayoutConfig orbitLayout;
        [SerializeField] private ObstacleConfig obstacleConfig;

        [Tooltip("Optional. If unassigned, the spawn rate and obstacle speed stay constant for the whole run.")]
        [SerializeField] private DifficultyCurveConfig difficultyCurve;

        private ObjectPool<Obstacle> _pool;
        private readonly List<Obstacle> _activeObstacles = new();
        private float _spawnTimer;
        private float _elapsedRunSeconds;

        private void Awake()
        {
            _pool = new ObjectPool<Obstacle>(
                createInstance: CreateObstacleInstance,
                onGet: obstacle => obstacle.gameObject.SetActive(true),
                onRelease: obstacle => obstacle.gameObject.SetActive(false),
                prewarmCount: 4);

            // Subscribed independently of enabled/disabled (unlike OnEnable/OnDisable below) so a
            // fresh run resets the difficulty ramp even though GameManager raises GameStarted just
            // before this component becomes enabled.
            GameEvents.GameStarted += HandleGameStarted;
        }

        private void OnEnable()
        {
            // Avoid an instant spawn burst carried over from a previous run/pause.
            _spawnTimer = 0f;
        }

        private void OnDestroy()
        {
            GameEvents.GameStarted -= HandleGameStarted;
        }

        private void HandleGameStarted()
        {
            _elapsedRunSeconds = 0f;
        }

        /// <summary>Returns every obstacle currently in play back to the pool. Called on a fresh run.</summary>
        public void ClearActiveObstacles()
        {
            // Iterate a copy since HandleLapCompleted (invoked indirectly via Release) is not called here,
            // so we release directly and mutate _activeObstacles ourselves.
            for (int i = _activeObstacles.Count - 1; i >= 0; i--)
            {
                _pool.Release(_activeObstacles[i]);
            }
            _activeObstacles.Clear();
        }

        private void Update()
        {
            _elapsedRunSeconds += Time.deltaTime;
            _spawnTimer += Time.deltaTime;

            float spawnInterval = obstacleConfig.spawnInterval * GetSpawnIntervalMultiplier();
            if (_spawnTimer < spawnInterval)
            {
                return;
            }

            _spawnTimer = 0f;
            SpawnObstacle();
        }

        private void SpawnObstacle()
        {
            int ringIndex = Random.Range(0, orbitLayout.RingCount);
            float radius = orbitLayout.GetRingRadius(ringIndex);

            // Spawn opposite the player's fixed angle so every obstacle has a full lap to travel
            // before it could possibly reach the player.
            float startAngle = orbitLayout.playerAngleDegrees + 180f;
            float direction = Random.value < 0.5f ? 1f : -1f;
            float speed = obstacleConfig.angularSpeedDegreesPerSecond * GetSpeedMultiplier();

            Obstacle obstacle = _pool.Get();
            obstacle.transform.SetParent(transform, worldPositionStays: false);
            obstacle.Configure(
                ringRadius: radius,
                startAngleDegrees: startAngle,
                angularSpeedDegreesPerSecond: speed * direction,
                colliderDiameter: obstacleConfig.obstacleColliderDiameter);
            _activeObstacles.Add(obstacle);
        }

        private float GetSpawnIntervalMultiplier() =>
            difficultyCurve != null ? difficultyCurve.EvaluateSpawnIntervalMultiplier(_elapsedRunSeconds) : 1f;

        private float GetSpeedMultiplier() =>
            difficultyCurve != null ? difficultyCurve.EvaluateSpeedMultiplier(_elapsedRunSeconds) : 1f;

        private Obstacle CreateObstacleInstance()
        {
            Obstacle obstacle = Instantiate(obstaclePrefab, transform);
            obstacle.LapCompleted += HandleLapCompleted;
            return obstacle;
        }

        private void HandleLapCompleted(Obstacle obstacle)
        {
            _activeObstacles.Remove(obstacle);
            _pool.Release(obstacle);

            // Simplification: any obstacle that survives its full lap counts as "dodged" for audio
            // feedback purposes, rather than a strict close-proximity check - see README design notes.
            GameEvents.RaiseObstacleDodged();
        }
    }
}
