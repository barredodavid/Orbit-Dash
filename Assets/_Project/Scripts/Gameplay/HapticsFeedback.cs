using OrbitDash.Core;
using UnityEngine;

namespace OrbitDash.Gameplay
{
    /// <summary>
    /// Short device vibration on collision. Handheld.Vibrate is compiled out on non-mobile targets,
    /// so this has zero effect (and zero cost) in the Editor, WebGL or a desktop build.
    /// </summary>
    public class HapticsFeedback : MonoBehaviour
    {
        private void OnEnable() => GameEvents.PlayerHit += HandlePlayerHit;

        private void OnDisable() => GameEvents.PlayerHit -= HandlePlayerHit;

        private static void HandlePlayerHit()
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
