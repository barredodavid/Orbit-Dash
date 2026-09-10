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

        /// <summary>Raised when this obstacle has completed a full lap and should return to the pool.</summary>
        public event Action<Obstacle> LapCompleted;

        private void Reset()
        {
            circleCollider = GetComponent<CircleCollider2D>();
        }

        /// <summary>Places the obstacle on a ring and starts its orbit.</summary>
        public void Configure(float ringRadius, float startAngleDegrees, float angularSpeedDegreesPerSecond, float colliderDiameter)
        {
            _radius = ringRadius;
            _angleDegrees = startAngleDegrees;
            _angularSpeedDegreesPerSecond = angularSpeedDegreesPerSecond;
            _traveledDegrees = 0f;

            if (circleCollider != null)
            {
                circleCollider.radius = colliderDiameter * 0.5f;
            }

            ApplyPosition();
        }

        private void Update()
        {
            float deltaDegrees = _angularSpeedDegreesPerSecond * Time.deltaTime;
            _angleDegrees += deltaDegrees;
            _traveledDegrees += Mathf.Abs(deltaDegrees);
            ApplyPosition();

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
