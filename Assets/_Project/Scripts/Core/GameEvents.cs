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

        /// <summary>Raised whenever a gameplay tap successfully switches the player's ring.</summary>
        public static event Action RingSwitched;
        public static void RaiseRingSwitched() => RingSwitched?.Invoke();

        /// <summary>Raised when the player leaves a ring: (ring left, ring entered).</summary>
        public static event Action<int, int> PlayerRingChanged;
        public static void RaisePlayerRingChanged(int fromRing, int toRing) => PlayerRingChanged?.Invoke(fromRing, toRing);

        /// <summary>
        /// Raised when the player leaves a ring just before an obstacle sweeps through their spot:
        /// (combo count, bonus points granted). Chaining near misses quickly grows the combo.
        /// </summary>
        public static event Action<int, int> NearMiss;
        public static void RaiseNearMiss(int combo, int bonus) => NearMiss?.Invoke(combo, bonus);

        /// <summary>Raised whenever an obstacle completes its lap without hitting the player.</summary>
        public static event Action ObstacleDodged;
        public static void RaiseObstacleDodged() => ObstacleDodged?.Invoke();

        /// <summary>Raised whenever the sound on/off preference is toggled from the menu.</summary>
        public static event Action<bool> SoundSettingChanged;
        public static void RaiseSoundSettingChanged(bool isEnabled) => SoundSettingChanged?.Invoke(isEnabled);

        /// <summary>Raised once per second during the pre-run countdown; 0 means "GO!".</summary>
        public static event Action<int> CountdownTick;
        public static void RaiseCountdownTick(int secondsRemaining) => CountdownTick?.Invoke(secondsRemaining);
    }
}
