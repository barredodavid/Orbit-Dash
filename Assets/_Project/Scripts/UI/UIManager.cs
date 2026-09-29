using System.Collections;
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
        private CountdownView _countdownView;
        private PauseScreen _pauseScreen;
        private GameOverScreen _gameOverScreen;

        private const float ScreenFadeDuration = 0.2f;

        private int _lastScore;
        private Coroutine _fadeRoutine;

        private void Awake()
        {
            Transform canvasRoot = BuildCanvas();
            BuildEventSystem();

            _hud = gameObject.AddComponent<HudView>();
            _hud.Init(canvasRoot);

            _menuScreen = gameObject.AddComponent<MenuScreen>();
            _menuScreen.Init(canvasRoot);

            _countdownView = gameObject.AddComponent<CountdownView>();
            _countdownView.Init(canvasRoot);

            _pauseScreen = gameObject.AddComponent<PauseScreen>();
            _pauseScreen.Init(canvasRoot);

            _gameOverScreen = gameObject.AddComponent<GameOverScreen>();
            _gameOverScreen.Init(canvasRoot);
        }

        private void OnEnable()
        {
            GameEvents.StateChanged += HandleStateChanged;
            GameEvents.ScoreChanged += HandleScoreChanged;
            GameEvents.NearMiss += HandleNearMiss;
            GameEvents.CountdownTick += HandleCountdownTick;
        }

        private void OnDisable()
        {
            GameEvents.StateChanged -= HandleStateChanged;
            GameEvents.ScoreChanged -= HandleScoreChanged;
            GameEvents.NearMiss -= HandleNearMiss;
            GameEvents.CountdownTick -= HandleCountdownTick;
        }

        private void HandleScoreChanged(int score)
        {
            _lastScore = score;
            _hud.SetScore(score);
        }

        private void HandleNearMiss(int combo, int bonus) => _hud.ShowNearMiss(combo, bonus);

        private void HandleCountdownTick(int secondsRemaining) => _countdownView.SetCount(secondsRemaining);

        private void HandleStateChanged(GameState state)
        {
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

            // Hide every screen instantly, then fade in only the one that matches the new state -
            // a small "animation de transition" without needing to crossfade two screens at once.
            _hud.SetVisible(false);
            _menuScreen.SetVisible(false);
            _countdownView.SetVisible(false);
            _pauseScreen.SetVisible(false);
            _gameOverScreen.SetVisible(false);

            CanvasGroup targetGroup = state switch
            {
                GameState.Playing => _hud.Group,
                GameState.Menu => _menuScreen.Group,
                GameState.Countdown => _countdownView.Group,
                GameState.Paused => _pauseScreen.Group,
                GameState.GameOver => _gameOverScreen.Group,
                _ => null,
            };

            if (targetGroup == null)
            {
                return;
            }

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }
            _fadeRoutine = StartCoroutine(FadeIn(targetGroup));
        }

        private static IEnumerator FadeIn(CanvasGroup group)
        {
            group.gameObject.SetActive(true);
            group.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < ScreenFadeDuration)
            {
                // Unscaled time: Paused and GameOver freeze Time.timeScale the moment they're entered,
                // but their own screen still needs to fade in.
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Clamp01(elapsed / ScreenFadeDuration);
                yield return null;
            }

            group.alpha = 1f;
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

            // All screens live under this safe-area-shrunk rect so notches/curved corners/system
            // bars on mobile never cover a button or piece of text.
            var safeAreaGo = new GameObject("SafeArea", typeof(RectTransform));
            var safeAreaRect = (RectTransform)safeAreaGo.transform;
            safeAreaRect.SetParent(canvasGo.transform, worldPositionStays: false);
            safeAreaRect.anchorMin = Vector2.zero;
            safeAreaRect.anchorMax = Vector2.one;
            safeAreaRect.offsetMin = Vector2.zero;
            safeAreaRect.offsetMax = Vector2.zero;
            safeAreaGo.AddComponent<SafeAreaFitter>();

            return safeAreaRect;
        }

        private void BuildEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystemGo.transform.SetParent(transform, worldPositionStays: false);
        }
    }
}
