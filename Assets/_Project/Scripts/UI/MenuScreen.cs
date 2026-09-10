using OrbitDash.Core;
using OrbitDash.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitDash.UI
{
    /// <summary>
    /// Main menu: title, Play button, best score, sound on/off toggle.
    /// Purely a view - visibility and content are driven by UIManager.
    /// </summary>
    public class MenuScreen : MonoBehaviour
    {
        private TextMeshProUGUI _bestScoreText;
        private Image _soundIcon;

        public RectTransform Root { get; private set; }
        public CanvasGroup Group { get; private set; }

        public void Init(Transform canvasRoot)
        {
            (RectTransform rect, CanvasGroup group, _) = UIFactory.CreateScreenPanel("MenuScreen", canvasRoot, new Color(0.03f, 0.03f, 0.08f, 0.95f));
            Root = rect;
            Group = group;

            TextMeshProUGUI title = UIFactory.CreateText("Title", Root, "ORBIT DASH", 108f, Color.white,
                anchoredPosition: Vector2.zero, sizeDelta: new Vector2(1000f, 220f));
            UIFactory.AnchorToTop(title.rectTransform, 400f);

            _bestScoreText = UIFactory.CreateText("BestScoreText", Root, "Best: 0", 52f, new Color(1f, 1f, 1f, 0.8f),
                anchoredPosition: new Vector2(0f, 340f), sizeDelta: new Vector2(800f, 100f));

            Button playButton = UIFactory.CreateButton("PlayButton", Root, "PLAY",
                new Color(0.3f, 0.9f, 1f, 1f), new Color(0.03f, 0.03f, 0.08f, 1f),
                anchoredPosition: new Vector2(0f, 0f), sizeDelta: new Vector2(560f, 170f), fontSize: 64f,
                backgroundSpritePath: UITheme.ButtonPrimary);
            playButton.onClick.AddListener(() => GameManager.Instance.StartGame());

            (Button soundButton, Image soundIcon) = UIFactory.CreateIconButton("SoundToggleButton", Root,
                UITheme.ButtonSecondary, new Color(1f, 1f, 1f, 0.12f), UITheme.IconAudioOn,
                anchoredPosition: Vector2.zero, sizeDelta: new Vector2(140f, 100f), iconSizeDelta: new Vector2(56f, 56f));
            UIFactory.AnchorToBottom((RectTransform)soundButton.transform, 440f);
            soundButton.onClick.AddListener(ToggleSound);
            _soundIcon = soundIcon;

            RefreshSoundIcon();
        }

        public void SetVisible(bool visible) => Root.gameObject.SetActive(visible);

        public void SetBestScore(int bestScore) => _bestScoreText.text = $"Best: {bestScore}";

        private void ToggleSound()
        {
            AudioPreferences.SetSoundEnabled(!AudioPreferences.IsSoundEnabled);
            RefreshSoundIcon();
        }

        private void RefreshSoundIcon()
        {
            Sprite icon = Resources.Load<Sprite>(AudioPreferences.IsSoundEnabled ? UITheme.IconAudioOn : UITheme.IconAudioOff);
            if (icon != null)
            {
                _soundIcon.sprite = icon;
                _soundIcon.color = Color.white;
            }
        }
    }
}
