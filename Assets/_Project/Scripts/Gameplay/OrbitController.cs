using System;
using OrbitDash.Core;
using OrbitDash.Data;
using UnityEngine;

namespace OrbitDash.Gameplay
{
    /// <summary>
    /// Moves the player along a fixed angular position across a set of concentric orbit rings
    /// (gameplay variant A: ring switching). A tap advances the player to the next ring outward,
    /// cycling back to the innermost ring after the outermost one.
    /// </summary>
    [RequireComponent(typeof(TapInputReader))]
    public class OrbitController : MonoBehaviour
    {
        [SerializeField] private OrbitLayoutConfig orbitLayout;

        [Header("Movement feel")]
        [SerializeField] private float ringTransitionDuration = 0.18f;
        [SerializeField] private AnimationCurve transitionEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

        /// <summary>Raised whenever the player finishes moving to a new ring, with the new ring index.</summary>
        public event Action<int> RingChanged;

        private TapInputReader _inputReader;
        private int _currentRingIndex;
        private float _currentRadius;
        private float _transitionTimer;
        private float _fromRadius;
        private float _toRadius;
        private bool _isTransitioning;

        public int CurrentRingIndex => _currentRingIndex;
        public int RingCount => orbitLayout.RingCount;

        private void Awake()
        {
            _inputReader = GetComponent<TapInputReader>();
        }

        private void OnEnable()
        {
            _inputReader.Tapped += HandleTapped;
        }

        private void OnDisable()
        {
            _inputReader.Tapped -= HandleTapped;
        }

        private void Start()
        {
            ResetToStart();
        }

        /// <summary>Snaps the player back to the innermost ring. Called by GameManager on a fresh run.</summary>
        public void ResetToStart()
        {
            _currentRingIndex = 0;
            SnapToRing(_currentRingIndex);
        }

        private void Update()
        {
            if (!_isTransitioning)
            {
                return;
            }

            _transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_transitionTimer / ringTransitionDuration);
            float easedT = transitionEase.Evaluate(t);
            ApplyRadius(Mathf.Lerp(_fromRadius, _toRadius, easedT));

            if (t >= 1f)
            {
                _isTransitioning = false;
            }
        }

        private void HandleTapped()
        {
            int nextRingIndex = (_currentRingIndex + 1) % orbitLayout.RingCount;
            BeginTransitionTo(nextRingIndex);
        }

        private void BeginTransitionTo(int targetRingIndex)
        {
            GameEvents.RaisePlayerRingChanged(_currentRingIndex, targetRingIndex);
            _currentRingIndex = targetRingIndex;
            _fromRadius = _currentRadius;
            _toRadius = orbitLayout.GetRingRadius(targetRingIndex);
            _transitionTimer = 0f;
            _isTransitioning = true;
            RingChanged?.Invoke(_currentRingIndex);
            GameEvents.RaiseRingSwitched();
        }

        private void SnapToRing(int ringIndex)
        {
            _isTransitioning = false;
            ApplyRadius(orbitLayout.GetRingRadius(ringIndex));
        }

        private void ApplyRadius(float radius)
        {
            _currentRadius = radius;
            float angleRad = orbitLayout.playerAngleDegrees * Mathf.Deg2Rad;
            transform.localPosition = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f) * radius;
        }
    }
}
