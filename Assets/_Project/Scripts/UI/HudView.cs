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
        private TextMeshProUGUI _scoreText;

        public RectTransform Root { get; private set; }

        public void Init(Transform canvasRoot)
        {
            (RectTransform rect, _, Image background) = UIFactory.CreateScreenPanel("HUD", canvasRoot, Color.clear);
            Root = rect;
            background.raycastTarget = false; // the HUD background must never block gameplay taps

            _scoreText = UIFactory.CreateText("ScoreText", Root, "0", 96f, Color.white,
                anchoredPosition: new Vector2(0f, -170f), sizeDelta: new Vector2(700f, 160f));
            var scoreRect = _scoreText.rectTransform;
            scoreRect.anchorMin = new Vector2(0.5f, 1f);
            scoreRect.anchorMax = new Vector2(0.5f, 1f);

            Button pauseButton = UIFactory.CreateButton("PauseButton", Root, "II",
                new Color(1f, 1f, 1f, 0.15f), Color.white,
                anchoredPosition: new Vector2(-120f, -140f), sizeDelta: new Vector2(140f, 140f), fontSize: 44f);
            pauseButton.onClick.AddListener(() => GameManager.Instance.PauseGame());
            var pauseRect = (RectTransform)pauseButton.transform;
            pauseRect.anchorMin = new Vector2(1f, 1f);
            pauseRect.anchorMax = new Vector2(1f, 1f);
        }

        public void SetVisible(bool visible) => Root.gameObject.SetActive(visible);

        public void SetScore(int score) => _scoreText.text = score.ToString();
    }
}
