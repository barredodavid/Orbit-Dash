using OrbitDash.Core;
using OrbitDash.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitDash.UI
{
    /// <summary>Pause overlay: resume, restart, back to menu. Purely a view - shown/hidden by UIManager.</summary>
    public class PauseScreen : MonoBehaviour
    {
        private static readonly Color DarkLabel = new(0.03f, 0.03f, 0.08f, 1f);

        public RectTransform Root { get; private set; }
        public CanvasGroup Group { get; private set; }

        public void Init(Transform canvasRoot)
        {
            (RectTransform rect, CanvasGroup group, _) = UIFactory.CreateScreenPanel("PauseScreen", canvasRoot, new Color(0.03f, 0.03f, 0.08f, 0.85f));
            Root = rect;
            Group = group;

            TextMeshProUGUI title = UIFactory.CreateText("Title", Root, "PAUSED", 90f, Color.white,
                anchoredPosition: Vector2.zero, sizeDelta: new Vector2(900f, 180f));
            UIFactory.AnchorToTop(title.rectTransform, 580f);

            Button resumeButton = UIFactory.CreateButton("ResumeButton", Root, "RESUME",
                new Color(0.3f, 0.9f, 1f, 1f), DarkLabel,
                anchoredPosition: new Vector2(0f, 130f), sizeDelta: new Vector2(560f, 150f), fontSize: 52f,
                backgroundSpritePath: UITheme.ButtonPrimary);
            resumeButton.onClick.AddListener(() => GameManager.Instance.ResumeGame());

            Button restartButton = UIFactory.CreateButton("RestartButton", Root, "RESTART",
                new Color(1f, 1f, 1f, 0.15f), DarkLabel,
                anchoredPosition: new Vector2(0f, -80f), sizeDelta: new Vector2(560f, 150f), fontSize: 52f,
                backgroundSpritePath: UITheme.ButtonSecondary);
            restartButton.onClick.AddListener(() => GameManager.Instance.StartGame());

            Button menuButton = UIFactory.CreateButton("MenuButton", Root, "MENU",
                new Color(1f, 1f, 1f, 0.15f), DarkLabel,
                anchoredPosition: new Vector2(0f, -290f), sizeDelta: new Vector2(560f, 150f), fontSize: 52f,
                backgroundSpritePath: UITheme.ButtonSecondary);
            menuButton.onClick.AddListener(() => GameManager.Instance.ReturnToMenu());
        }

        public void SetVisible(bool visible) => Root.gameObject.SetActive(visible);
    }
}
