using System.Collections;
using OrbitDash.Core;
using UnityEngine;

namespace OrbitDash.Gameplay
{
    /// <summary>Small scale punch on the player whenever it switches rings - purely cosmetic juice.</summary>
    public class PlayerPulseEffect : MonoBehaviour
    {
        [SerializeField] private float punchScale = 1.35f;
        [SerializeField] private float duration = 0.18f;

        private Vector3 _baseScale;
        private Coroutine _pulseRoutine;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            GameEvents.RingSwitched += HandleRingSwitched;
        }

        private void OnDisable()
        {
            GameEvents.RingSwitched -= HandleRingSwitched;
        }

        private void HandleRingSwitched()
        {
            if (_pulseRoutine != null)
            {
                StopCoroutine(_pulseRoutine);
            }
            _pulseRoutine = StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Punch out then back in: a simple triangle wave peaking at the midpoint.
                float scaleFactor = 1f + (punchScale - 1f) * (1f - Mathf.Abs(t * 2f - 1f));
                transform.localScale = _baseScale * scaleFactor;
                yield return null;
            }

            transform.localScale = _baseScale;
            _pulseRoutine = null;
        }
    }
}
