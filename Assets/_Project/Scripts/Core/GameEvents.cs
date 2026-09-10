using System;

namespace OrbitDash.Core
{
    /// <summary>
    /// Decoupled event hub for cross-system gameplay signals, so gameplay and UI systems don't need
    /// direct references to each other (e.g. an Obstacle doesn't need to know about a GameManager,
    /// and GameManager doesn't need to know the UI exists).
    /// </summary>
    public static class GameEvents
    {
        /// <summary>Raised when the player collides with an obstacle.</summary>
        public static event Action PlayerHit;
        public static void RaisePlayerHit() => PlayerHit?.Invoke();

        /// <summary>Raised whenever the game state changes, so UI screens can show/hide themselves.</summary>
        public static event Action<GameState> StateChanged;
        public static void RaiseStateChanged(GameState state) => StateChanged?.Invoke(state);

        /// <summary>Raised when a brand new run begins (from Menu or a replay), before Playing starts ticking.</summary>
        public static event Action GameStarted;
        public static void RaiseGameStarted() => GameStarted?.Invoke();

        /// <summary>Raised whenever the live score changes during a run.</summary>
        public static event Action<int> ScoreChanged;
        public static void RaiseScoreChanged(int score) => ScoreChanged?.Invoke(score);
    }
}
