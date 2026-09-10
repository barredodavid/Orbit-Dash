using System;

namespace OrbitDash.Core
{
    /// <summary>
    /// Decoupled event hub for cross-system gameplay signals, so gameplay systems don't need
    /// direct references to each other (e.g. an Obstacle doesn't need to know about a GameManager).
    /// </summary>
    public static class GameEvents
    {
        /// <summary>Raised when the player collides with an obstacle.</summary>
        public static event Action PlayerHit;

        public static void RaisePlayerHit() => PlayerHit?.Invoke();
    }
}
