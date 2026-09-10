using System.Collections;
using OrbitDash.Core;
using OrbitDash.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitDash.UI
{
    /// <summary>
    /// Always-present gameplay overlay: live score top-center, pause button top-right.
    /// Purely a view - visibility and content are driven by UIManager, which is the single place
    /// reacting to GameEvents so screens don't each duplicate their own subscriptions.
    /// </summary>
    public class HudView : MonoBehaviour
    {
        private const float ScorePunchScale = 1.25f;
        private const float ScorePunchDuration = 0.15f;

        private TextMeshProUGUI _scoreText;
        private Vector3 _scoreBaseScale;
        private Coroutine _scorePunchRoutine;

        public RectTransform Root { get; private set; }
        public CanvasGroup Group { get; private set; }

        public void Init(Transform canvasRoot)
        {
            (RectTransform rect, CanvasGroup group, Image background) = UIFactory.CreateScreenPanel("HUD", canvasRoot, Color.clear);
            Root = rect;
            Group = group;
            background.raycastTarget = false; // the HUD background must never block gameplay taps

            _scoreText = UIFactory.CreateText("ScoreText", Root, "0", 96f, Color.white,
                anchoredPosition: new Vector2(0f, -170f), sizeDelta: new Vector2(700f, 160f));
            var scoreRect = _scoreText.rectTransform;
            scoreRect.anchorMin = new Vector2(0.5f, 1f);
            scoreRect.anchorMax = new Vector2(0.5f, 1f);
            _scoreBaseScale = scoreRect.localScale;

            (Button pauseButton, _) = UIFactory.CreateIconButton("PauseButton", Root,
                UITheme.ButtonSecondary, new Color(1f, 1f, 1f, 0.15f), UITheme.IconPause,
                anchoredPosition: new Vector2(-120f, -140f), sizeDelta: new Vector2(120f, 120f), iconSizeDelta: new Vector2(48f, 48f));
            pauseButton.onClick.AddListener(() => GameManager.Instance.PauseGame());
            var pauseRect = (RectTransform)pauseButton.transform;
            pauseRect.anchorMin = new Vector2(1f, 1f);
            pauseRect.anchorMax = new Vector2(1f, 1f);
        }

        public void SetVisible(bool visible) => Root.gameObject.SetActive(visible);

        public void SetScore(int score)
        {
            _scoreText.text = score.ToString();

            if (_scorePunchRoutine != null)
            {
                StopCoroutine(_scorePunchRoutine);
            }
            _scorePunchRoutine = StartCoroutine(PunchScoreText());
        }

        private IEnumerator PunchScoreText()
        {
            Transform scoreTransform = _scoreText.transform;
            float elapsed = 0f;
            while (elapsed < ScorePunchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / ScorePunchDuration);
                float scaleFactor = 1f + (ScorePunchScale - 1f) * (1f - Mathf.Abs(t * 2f - 1f));
                scoreTransform.localScale = _scoreBaseScale * scaleFactor;
                yield return null;
            }

            scoreTransform.localScale = _scoreBaseScale;
            _scorePunchRoutine = null;
        }
    }
}
