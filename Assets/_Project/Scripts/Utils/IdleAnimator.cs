using UnityEngine;

namespace OrbitDash.Utils
{
    /// <summary>Generic idle motion for background/decorative objects: constant rotation and/or a gentle scale pulse.</summary>
    public class IdleAnimator : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private float rotationDegreesPerSecond = 6f;

        [Header("Pulse")]
        [SerializeField] private bool pulse = true;
        [SerializeField] private float pulseAmplitude = 0.04f;
        [SerializeField] private float pulseSpeed = 1.2f;

        private Vector3 _baseScale;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void Update()
        {
            transform.Rotate(Vector3.forward, rotationDegreesPerSecond * Time.deltaTime);

            if (pulse)
            {
                float scaleFactor = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
                transform.localScale = _baseScale * scaleFactor;
            }
        }
    }
}
