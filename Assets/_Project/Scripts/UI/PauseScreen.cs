using OrbitDash.Core;
using OrbitDash.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitDash.UI
{
    /// <summary>Pause overlay: resume, restart, back to menu. Purely a view - shown/hidden by UIManager.</summary>
    public class PauseScreen : MonoBehaviour
    {
        public RectTransform Root { get; private set; }

        public void Init(Transform canvasRoot)
        {
            (RectTransform rect, _, _) = UIFactory.CreateScreenPanel("PauseScreen", canvasRoot, new Color(0.03f, 0.03f, 0.08f, 0.85f));
            Root = rect;

            UIFactory.CreateText("Title", Root, "PAUSED", 90f, Color.white,
                anchoredPosition: new Vector2(0f, 380f), sizeDelta: new Vector2(900f, 180f));

            Button resumeButton = UIFactory.CreateButton("ResumeButton", Root, "RESUME",
                new Color(0.3f, 0.9f, 1f, 1f), new Color(0.03f, 0.03f, 0.08f, 1f),
                anchoredPosition: new Vector2(0f, 100f), sizeDelta: new Vector2(560f, 150f), fontSize: 52f);
            resumeButton.onClick.AddListener(() => GameManager.Instance.ResumeGame());

            Button restartButton = UIFactory.CreateButton("RestartButton", Root, "RESTART",
                new Color(1f, 1f, 1f, 0.15f), Color.white,
                anchoredPosition: new Vector2(0f, -80f), sizeDelta: new Vector2(560f, 150f), fontSize: 52f);
            restartButton.onClick.AddListener(() => GameManager.Instance.StartGame());

            Button menuButton = UIFactory.CreateButton("MenuButton", Root, "MENU",
                new Color(1f, 1f, 1f, 0.15f), Color.white,
                anchoredPosition: new Vector2(0f, -260f), sizeDelta: new Vector2(560f, 150f), fontSize: 52f);
            menuButton.onClick.AddListener(() => GameManager.Instance.ReturnToMenu());
        }

        public void SetVisible(bool visible) => Root.gameObject.SetActive(visible);
    }
}
