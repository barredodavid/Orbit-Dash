using System.Collections;
using OrbitDash.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OrbitDash.Core
{
    /// <summary>
    /// Central state machine for the game (Menu / Countdown / Playing / Paused / GameOver). Owns
    /// transitions and enables/disables the relevant gameplay systems; everything else reacts via
    /// <see cref="GameEvents"/> instead of holding a direct reference to this manager (the UI is the
    /// one exception - it calls into GameManager directly for button actions, the usual mediator pattern).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private const int CountdownStartValue = 3;

        public static GameManager Instance { get; private set; }

        [SerializeField] private OrbitController playerOrbitController;
        [SerializeField] private TapInputReader playerInputReader;
        [SerializeField] private ObstacleSpawner obstacleSpawner;

        public GameState CurrentState { get; private set; } = GameState.Menu;

        private InputAction _pauseKeyAction;

        private void Awake()
        {
            Instance = this;

            // Desktop convenience alongside the tap-only mobile flow: Escape toggles pause.
            _pauseKeyAction = new InputAction(name: "TogglePauseKey", type: InputActionType.Button, binding: "<Keyboard>/escape");
            _pauseKeyAction.performed += HandlePauseKeyPressed;
        }

        private void OnEnable()
        {
            GameEvents.PlayerHit += HandlePlayerHit;
            _pauseKeyAction.Enable();
        }

        private void OnDisable()
        {
            GameEvents.PlayerHit -= HandlePlayerHit;
            _pauseKeyAction.Disable();
        }

        private void OnDestroy()
        {
            _pauseKeyAction?.Dispose();
        }

        private void Start()
        {
            EnterState(GameState.Menu);
        }

        /// <summary>Starts a brand new run after a short countdown. Called by Play and Replay buttons.</summary>
        public void StartGame()
        {
            if (CurrentState == GameState.Countdown)
            {
                return;
            }
            StartCoroutine(CountdownThenPlay());
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

        private IEnumerator CountdownThenPlay()
        {
            // Reset the player/obstacles right away so they're visible at their starting layout
            // behind the "3 2 1" overlay instead of jumping there when Playing actually begins.
            EnterState(GameState.Countdown, isFreshStart: true);

            for (int secondsRemaining = CountdownStartValue; secondsRemaining >= 1; secondsRemaining--)
            {
                GameEvents.RaiseCountdownTick(secondsRemaining);
                yield return new WaitForSecondsRealtime(1f);

                if (CurrentState != GameState.Countdown)
                {
                    yield break; // state was forced elsewhere while counting down
                }
            }

            GameEvents.RaiseCountdownTick(0);
            GameEvents.RaiseGameStarted();
            EnterState(GameState.Playing);
        }

        private void HandlePlayerHit()
        {
            if (CurrentState != GameState.Playing)
            {
                return;
            }
            EnterState(GameState.GameOver);
        }

        private void HandlePauseKeyPressed(InputAction.CallbackContext context)
        {
            if (CurrentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (CurrentState == GameState.Paused)
            {
                ResumeGame();
            }
        }

        private void EnterState(GameState newState, bool isFreshStart = false)
        {
            CurrentState = newState;
            bool isPlaying = newState == GameState.Playing;

            playerOrbitController.enabled = isPlaying;
            playerInputReader.enabled = isPlaying;
            obstacleSpawner.enabled = isPlaying;

            // Paused and GameOver freeze the scene behind the UI; every other state runs normally.
            Time.timeScale = newState is GameState.Paused or GameState.GameOver ? 0f : 1f;

            if (isFreshStart)
            {
                playerOrbitController.ResetToStart();
                obstacleSpawner.ClearActiveObstacles();
            }

            GameEvents.RaiseStateChanged(newState);
        }
    }
}
