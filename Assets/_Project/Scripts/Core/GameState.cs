namespace OrbitDash.Core
{
    /// <summary>The finite set of states the game can be in, driven by <see cref="GameManager"/>.</summary>
    public enum GameState
    {
        Menu,
        Countdown,
        Playing,
        Paused,
        GameOver,
    }
}
