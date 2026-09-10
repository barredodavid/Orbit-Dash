using UnityEngine;

namespace OrbitDash.Core
{
    /// <summary>
    /// TEMP stand-in for the real game-over flow. Just logs when the player is hit so the
    /// obstacle/collision pipeline can be verified end to end before the state machine exists.
    /// Removed once GameManager's state machine takes over this responsibility (step 5).
    /// </summary>
    public class TemporaryGameOverLogger : MonoBehaviour
    {
        private void OnEnable()
        {
            GameEvents.PlayerHit += HandlePlayerHit;
        }

        private void OnDisable()
        {
            GameEvents.PlayerHit -= HandlePlayerHit;
        }

        private static void HandlePlayerHit()
        {
            Debug.Log("Game Over - player hit an obstacle.");
        }
    }
}
