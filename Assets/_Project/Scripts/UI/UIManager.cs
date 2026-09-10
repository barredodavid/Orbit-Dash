using OrbitDash.Core;
using OrbitDash.Save;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace OrbitDash.UI
{
    /// <summary>
    /// Builds the entire UI hierarchy at runtime (Canvas, EventSystem, HUD and the three screens) and
    /// is the single place reacting to GameEvents to show/hide/refresh them. Screens themselves stay
    /// "dumb" views whose only GameManager coupling is their own button callbacks.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private HudView _hud;
        private MenuScreen _menuScreen;
        private PauseScreen _pauseScreen;
        private GameOverScreen _gameOverScreen;

        private int _lastScore;

        private void Awake()
        {
            Transform canvasRoot = BuildCanvas();
            BuildEventSystem();

            _hud = gameObject.AddComponent<HudView>();
            _hud.Init(canvasRoot);

            _menuScreen = gameObject.AddComponent<MenuScreen>();
            _menuScreen.Init(canvasRoot);

            _pauseScreen = gameObject.AddComponent<PauseScreen>();
            _pauseScreen.Init(canvasRoot);

            _gameOverScreen = gameObject.AddComponent<GameOverScreen>();
            _gameOverScreen.Init(canvasRoot);
        }

        private void OnEnable()
        {
            GameEvents.StateChanged += HandleStateChanged;
            GameEvents.ScoreChanged += HandleScoreChanged;
        }

        private void OnDisable()
        {
            GameEvents.StateChanged -= HandleStateChanged;
            GameEvents.ScoreChanged -= HandleScoreChanged;
        }

        private void HandleScoreChanged(int score)
        {
            _lastScore = score;
            _hud.SetScore(score);
        }

        private void HandleStateChanged(GameState state)
        {
            _hud.SetVisible(state == GameState.Playing);
            _menuScreen.SetVisible(state == GameState.Menu);
            _pauseScreen.SetVisible(state == GameState.Paused);
            _gameOverScreen.SetVisible(state == GameState.GameOver);

            if (state == GameState.Menu)
            {
                _menuScreen.SetBestScore(SaveSystem.GetBestScore());
            }
            else if (state == GameState.GameOver)
            {
                _gameOverScreen.SetScore(_lastScore);
                _gameOverScreen.SetBestScore(SaveSystem.GetBestScore());
                _gameOverScreen.SetTopScores(SaveSystem.LoadTopScores());
            }
        }

        private Transform BuildCanvas()
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, worldPositionStays: false);

            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvasGo.transform;
        }

        private void BuildEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemGo.transform.SetParent(transform, worldPositionStays: false);
        }
    }
}
