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

        [Tooltip("How long before a spawn the warning indicator appears, in seconds.")]
        [SerializeField] private float spawnWarningLeadSeconds = 0.35f;

        [Header("Near miss")]
        [Tooltip("Leaving a ring at most this long before an obstacle sweeps through the player's spot counts as a near miss.")]
        [SerializeField] private float nearMissWindowSeconds = 0.35f;
        [Tooltip("A near miss within this delay of the previous one grows the combo; otherwise it restarts at 1.")]
        [SerializeField] private float nearMissComboSeconds = 3f;
        [Tooltip("Bonus points of a near miss = this x combo.")]
        [SerializeField] private int nearMissBasePoints = 2;

        private ObjectPool<Obstacle> _pool;
        private readonly List<Obstacle> _activeObstacles = new();
        private float _spawnTimer;
        private float _elapsedRunSeconds;

        private int _pendingRingIndex;
        private float _pendingDirection;
        private bool _hasPendingSpawn;
        private SpriteRenderer _warningIndicator;

        private int _playerRing;
        private int _lastLeftRing = -1;
        private float _lastRingLeaveTime = float.NegativeInfinity;
        private float _lastNearMissTime = float.NegativeInfinity;
        private int _nearMissCombo;

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
            GameEvents.PlayerRingChanged += HandlePlayerRingChanged;

            CreateWarningIndicator();
        }

        private void CreateWarningIndicator()
        {
            var indicatorGo = new GameObject("SpawnWarningIndicator");
            indicatorGo.transform.SetParent(transform, worldPositionStays: false);
            _warningIndicator = indicatorGo.AddComponent<SpriteRenderer>();
            _warningIndicator.sprite = RuntimeCircleSpriteFactory.CreateCircleSprite(0.45f, new Color(1f, 1f, 1f, 0.5f));
            _warningIndicator.sortingOrder = -1;
            indicatorGo.SetActive(false);
        }

        private void OnEnable()
        {
            // Avoid an instant spawn burst carried over from a previous run/pause.
            _spawnTimer = 0f;
            _hasPendingSpawn = false;
            _warningIndicator.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            GameEvents.GameStarted -= HandleGameStarted;
            GameEvents.PlayerRingChanged -= HandlePlayerRingChanged;
        }

        private void HandleGameStarted()
        {
            _elapsedRunSeconds = 0f;
            _playerRing = 0;
            _lastLeftRing = -1;
            _nearMissCombo = 0;
            _lastNearMissTime = float.NegativeInfinity;
        }

        private void HandlePlayerRingChanged(int fromRing, int toRing)
        {
            _lastLeftRing = fromRing;
            _playerRing = toRing;
            _lastRingLeaveTime = Time.time;
        }

        /// <summary>
        /// Near miss = the obstacle sweeps through the spot the player just left. Pure timing check,
        /// like the rest of the game rules (no extra colliders).
        /// </summary>
        private void HandlePassedPlayerAngle(Obstacle obstacle)
        {
            bool justLeftThisRing = obstacle.RingIndex == _lastLeftRing && obstacle.RingIndex != _playerRing;
            if (!justLeftThisRing || Time.time - _lastRingLeaveTime > nearMissWindowSeconds)
            {
                return;
            }

            _nearMissCombo = Time.time - _lastNearMissTime <= nearMissComboSeconds ? _nearMissCombo + 1 : 1;
            _lastNearMissTime = Time.time;
            GameEvents.RaiseNearMiss(_nearMissCombo, nearMissBasePoints * _nearMissCombo);
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
            float timeUntilSpawn = spawnInterval - _spawnTimer;

            if (!_hasPendingSpawn && timeUntilSpawn <= spawnWarningLeadSeconds)
            {
                BeginPendingSpawn();
            }

            if (_hasPendingSpawn)
            {
                UpdateWarningIndicator(timeUntilSpawn);
            }

            if (_spawnTimer < spawnInterval)
            {
                return;
            }

            _spawnTimer = 0f;
            SpawnObstacle();
            _hasPendingSpawn = false;
            _warningIndicator.gameObject.SetActive(false);
        }

        /// <summary>
        /// Locks in the next obstacle's ring/direction a short moment before it actually spawns, and
        /// shows a fading indicator there - a quick readability cue for where danger is about to appear.
        /// </summary>
        private void BeginPendingSpawn()
        {
            _pendingRingIndex = Random.Range(0, orbitLayout.RingCount);
            _pendingDirection = Random.value < 0.5f ? 1f : -1f;
            _hasPendingSpawn = true;

            float radius = orbitLayout.GetRingRadius(_pendingRingIndex);
            float angleRad = (orbitLayout.playerAngleDegrees + 180f) * Mathf.Deg2Rad;
            _warningIndicator.transform.localPosition = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f) * radius;
            _warningIndicator.gameObject.SetActive(true);
        }

        private void UpdateWarningIndicator(float timeUntilSpawn)
        {
            float t = Mathf.Clamp01(1f - timeUntilSpawn / spawnWarningLeadSeconds);
            float alpha = Mathf.Lerp(0.25f, 0.85f, t); // brightens as the spawn gets closer
            Color color = _warningIndicator.color;
            _warningIndicator.color = new Color(color.r, color.g, color.b, alpha);
        }

        private void SpawnObstacle()
        {
            float radius = orbitLayout.GetRingRadius(_pendingRingIndex);

            // Spawn opposite the player's fixed angle so every obstacle has a full lap to travel
            // before it could possibly reach the player.
            float startAngle = orbitLayout.playerAngleDegrees + 180f;
            float speed = obstacleConfig.angularSpeedDegreesPerSecond * GetSpeedMultiplier();

            Obstacle obstacle = _pool.Get();
            obstacle.transform.SetParent(transform, worldPositionStays: false);
            obstacle.Configure(
                ringIndex: _pendingRingIndex,
                ringRadius: radius,
                startAngleDegrees: startAngle,
                angularSpeedDegreesPerSecond: speed * _pendingDirection,
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
            obstacle.PassedPlayerAngle += HandlePassedPlayerAngle;
            return obstacle;
        }

        private void HandleLapCompleted(Obstacle obstacle)
        {
            _activeObstacles.Remove(obstacle);
            _pool.Release(obstacle);
            GameEvents.RaiseObstacleDodged();
        }
    }
}
