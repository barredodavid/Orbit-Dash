using System.Collections.Generic;
using System.Text;
using OrbitDash.Core;
using OrbitDash.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitDash.UI
{
    /// <summary>
    /// Game over screen: run score, best score, top 5 leaderboard, replay/menu buttons.
    /// Purely a view - shown/hidden and refreshed by UIManager.
    /// </summary>
    public class GameOverScreen : MonoBehaviour
    {
        private TextMeshProUGUI _scoreText;
        private TextMeshProUGUI _bestScoreText;
        private TextMeshProUGUI _topScoresText;

        public RectTransform Root { get; private set; }
        public CanvasGroup Group { get; private set; }

        public void Init(Transform canvasRoot)
        {
            (RectTransform rect, CanvasGroup group, _) = UIFactory.CreateScreenPanel("GameOverScreen", canvasRoot, new Color(0.03f, 0.03f, 0.08f, 0.95f));
            Root = rect;
            Group = group;

            UIFactory.CreateText("Title", Root, "GAME OVER", 84f, Color.white,
                anchoredPosition: new Vector2(0f, 700f), sizeDelta: new Vector2(1000f, 160f));

            _scoreText = UIFactory.CreateText("ScoreText", Root, "Score: 0", 64f, Color.white,
                anchoredPosition: new Vector2(0f, 560f), sizeDelta: new Vector2(900f, 120f));

            _bestScoreText = UIFactory.CreateText("BestScoreText", Root, "Best: 0", 46f, new Color(1f, 1f, 1f, 0.8f),
                anchoredPosition: new Vector2(0f, 460f), sizeDelta: new Vector2(900f, 100f));

            _topScoresText = UIFactory.CreateText("TopScoresText", Root, string.Empty, 40f, new Color(1f, 1f, 1f, 0.7f),
                anchoredPosition: new Vector2(0f, 180f), sizeDelta: new Vector2(700f, 360f));

            Button replayButton = UIFactory.CreateButton("ReplayButton", Root, "REPLAY",
                new Color(0.3f, 0.9f, 1f, 1f), new Color(0.03f, 0.03f, 0.08f, 1f),
                anchoredPosition: new Vector2(0f, -260f), sizeDelta: new Vector2(560f, 150f), fontSize: 52f);
            replayButton.onClick.AddListener(() => GameManager.Instance.StartGame());

            Button menuButton = UIFactory.CreateButton("MenuButton", Root, "MENU",
                new Color(1f, 1f, 1f, 0.15f), Color.white,
                anchoredPosition: new Vector2(0f, -440f), sizeDelta: new Vector2(560f, 150f), fontSize: 52f);
            menuButton.onClick.AddListener(() => GameManager.Instance.ReturnToMenu());
        }

        public void SetVisible(bool visible) => Root.gameObject.SetActive(visible);

        public void SetScore(int score) => _scoreText.text = $"Score: {score}";

        public void SetBestScore(int bestScore) => _bestScoreText.text = $"Best: {bestScore}";

        public void SetTopScores(IReadOnlyList<int> topScores)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < topScores.Count; i++)
            {
                builder.AppendLine($"{i + 1}. {topScores[i]}");
            }
            _topScoresText.text = builder.ToString();
        }
    }
}
