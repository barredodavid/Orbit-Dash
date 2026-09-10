using System.Collections;
using OrbitDash.Core;
using UnityEngine;

namespace OrbitDash.Gameplay
{
    /// <summary>Small camera kick on player hit. Purely cosmetic juice, entirely event-driven.</summary>
    [RequireComponent(typeof(Camera))]
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private float magnitude = 0.35f;

        private Vector3 _originalLocalPosition;
        private Coroutine _shakeRoutine;

        private void Awake()
        {
            _originalLocalPosition = transform.localPosition;
        }

        private void OnEnable()
        {
            GameEvents.PlayerHit += HandlePlayerHit;
        }

        private void OnDisable()
        {
            GameEvents.PlayerHit -= HandlePlayerHit;
        }

        private void HandlePlayerHit()
        {
            if (_shakeRoutine != null)
            {
                StopCoroutine(_shakeRoutine);
            }
            _shakeRoutine = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine()
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                // Unscaled time: GameOver freezes Time.timeScale the same frame the hit happens,
                // but the shake should still be visible as the game-over screen fades in.
                elapsed += Time.unscaledDeltaTime;
                float damper = 1f - Mathf.Clamp01(elapsed / duration);
                Vector2 offset = Random.insideUnitCircle * magnitude * damper;
                transform.localPosition = _originalLocalPosition + (Vector3)offset;
                yield return null;
            }

            transform.localPosition = _originalLocalPosition;
            _shakeRoutine = null;
        }
    }
}
