using OrbitDash.Gameplay;
using UnityEngine;

namespace OrbitDash.Core
{
    /// <summary>
    /// Central state machine for the game (Menu / Playing / Paused / GameOver). Owns transitions and
    /// enables/disables the relevant gameplay systems; everything else reacts via <see cref="GameEvents"/>
    /// instead of holding a direct reference to this manager (the UI is the one exception - it calls
    /// into GameManager directly for button actions, which is the usual mediator/facade pattern).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private OrbitController playerOrbitController;
        [SerializeField] private TapInputReader playerInputReader;
        [SerializeField] private ObstacleSpawner obstacleSpawner;

        public GameState CurrentState { get; private set; } = GameState.Menu;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.PlayerHit += HandlePlayerHit;
        }

        private void OnDisable()
        {
            GameEvents.PlayerHit -= HandlePlayerHit;
        }

        private void Start()
        {
            EnterState(GameState.Menu);
        }

        /// <summary>Starts a brand new run. Called by the Menu's Play button and the Game Over Replay button.</summary>
        public void StartGame()
        {
            GameEvents.RaiseGameStarted();
            EnterState(GameState.Playing, isFreshStart: true);
        }

        public void PauseGame()
        {
            if (CurrentState != GameState.Playing)
            {
                return;
            }
            EnterState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused)
            {
                return;
            }
            EnterState(GameState.Playing);
        }

        public void ReturnToMenu()
        {
            EnterState(GameState.Menu);
        }

        private void HandlePlayerHit()
        {
            if (CurrentState != GameState.Playing)
            {
                return;
            }
            EnterState(GameState.GameOver);
        }

        private void EnterState(GameState newState, bool isFreshStart = false)
        {
            CurrentState = newState;
            bool isPlaying = newState == GameState.Playing;

            playerOrbitController.enabled = isPlaying;
            playerInputReader.enabled = isPlaying;
            obstacleSpawner.enabled = isPlaying;

            // Menu and Playing run at normal speed; Paused and GameOver freeze the scene behind the UI.
            Time.timeScale = (newState == GameState.Playing || newState == GameState.Menu) ? 1f : 0f;

            if (isFreshStart)
            {
                playerOrbitController.ResetToStart();
                obstacleSpawner.ClearActiveObstacles();
            }

            GameEvents.RaiseStateChanged(newState);
        }
    }
}
