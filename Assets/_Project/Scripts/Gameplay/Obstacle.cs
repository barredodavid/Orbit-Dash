using System;
using OrbitDash.Core;
using UnityEngine;

namespace OrbitDash.Gameplay
{
    /// <summary>
    /// A single obstacle moving at a constant angular speed along one orbit ring.
    /// Pooled by <see cref="ObstacleSpawner"/> instead of being instantiated/destroyed per spawn.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class Obstacle : MonoBehaviour
    {
        [SerializeField] private CircleCollider2D circleCollider;

        private float _radius;
        private float _angleDegrees;
        private float _angularSpeedDegreesPerSecond;
        private float _traveledDegrees;
        private float _spinDegreesPerSecond;

        // Every sprite dropped in Resources/Sprites/Gameplay/Meteors is a possible look (same size, so
        // the collider stays fair). Loaded once for all pooled obstacles.
        private static Sprite[] _meteorSprites;

        /// <summary>Index of the ring this obstacle travels on.</summary>
        public int RingIndex { get; private set; }

        /// <summary>Raised when this obstacle has completed a full lap and should return to the pool.</summary>
        public event Action<Obstacle> LapCompleted;

        /// <summary>Raised once per lap, when the obstacle sweeps through the player's angle (it spawns opposite the player).</summary>
        public event Action<Obstacle> PassedPlayerAngle;

        private void Reset()
        {
            circleCollider = GetComponent<CircleCollider2D>();
        }

        /// <summary>Places the obstacle on a ring and starts its orbit.</summary>
        public void Configure(int ringIndex, float ringRadius, float startAngleDegrees, float angularSpeedDegreesPerSecond, float colliderDiameter)
        {
            RingIndex = ringIndex;
            _radius = ringRadius;
            _angleDegrees = startAngleDegrees;
            _angularSpeedDegreesPerSecond = angularSpeedDegreesPerSecond;
            _traveledDegrees = 0f;

            if (circleCollider != null)
            {
                circleCollider.radius = colliderDiameter * 0.5f;
            }

            ApplyRandomLook();
            ApplyPosition();
        }

        private void ApplyRandomLook()
        {
            _meteorSprites ??= Resources.LoadAll<Sprite>("Sprites/Gameplay/Meteors");
            if (_meteorSprites.Length > 0 && TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.sprite = _meteorSprites[UnityEngine.Random.Range(0, _meteorSprites.Length)];
            }

            _spinDegreesPerSecond = UnityEngine.Random.Range(60f, 200f) * (UnityEngine.Random.value < 0.5f ? 1f : -1f);
            transform.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));
        }

        private void Update()
        {
            float deltaDegrees = _angularSpeedDegreesPerSecond * Time.deltaTime;
            _angleDegrees += deltaDegrees;
            float previousTraveled = _traveledDegrees;
            _traveledDegrees += Mathf.Abs(deltaDegrees);
            ApplyPosition();
            transform.Rotate(0f, 0f, _spinDegreesPerSecond * Time.deltaTime); // spin is visual only: the collider is a circle

            if (previousTraveled < 180f && _traveledDegrees >= 180f)
            {
                PassedPlayerAngle?.Invoke(this);
            }

            if (_traveledDegrees >= 360f)
            {
                LapCompleted?.Invoke(this);
            }
        }

        private void ApplyPosition()
        {
            float angleRad = _angleDegrees * Mathf.Deg2Rad;
            transform.localPosition = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f) * _radius;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerMarker>(out _))
            {
                GameEvents.RaisePlayerHit();
            }
        }
    }
}
