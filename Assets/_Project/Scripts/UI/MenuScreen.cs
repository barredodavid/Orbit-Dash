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
        private const string SoundEnabledPrefKey = "OrbitDash.SoundEnabled";

        private TextMeshProUGUI _bestScoreText;
        private TextMeshProUGUI _soundToggleLabel;

        public RectTransform Root { get; private set; }

        public void Init(Transform canvasRoot)
        {
            (RectTransform rect, _, _) = UIFactory.CreateScreenPanel("MenuScreen", canvasRoot, new Color(0.03f, 0.03f, 0.08f, 0.95f));
            Root = rect;

            UIFactory.CreateText("Title", Root, "ORBIT DASH", 108f, Color.white,
                anchoredPosition: new Vector2(0f, 560f), sizeDelta: new Vector2(1000f, 220f));

            _bestScoreText = UIFactory.CreateText("BestScoreText", Root, "Best: 0", 52f, new Color(1f, 1f, 1f, 0.8f),
                anchoredPosition: new Vector2(0f, 340f), sizeDelta: new Vector2(800f, 100f));

            Button playButton = UIFactory.CreateButton("PlayButton", Root, "PLAY",
                new Color(0.3f, 0.9f, 1f, 1f), new Color(0.03f, 0.03f, 0.08f, 1f),
                anchoredPosition: new Vector2(0f, 0f), sizeDelta: new Vector2(560f, 170f), fontSize: 64f);
            playButton.onClick.AddListener(() => GameManager.Instance.StartGame());

            Button soundButton = UIFactory.CreateButton("SoundToggleButton", Root, string.Empty,
                new Color(1f, 1f, 1f, 0.12f), Color.white,
                anchoredPosition: new Vector2(0f, -520f), sizeDelta: new Vector2(420f, 110f), fontSize: 38f);
            soundButton.onClick.AddListener(ToggleSound);
            _soundToggleLabel = soundButton.GetComponentInChildren<TextMeshProUGUI>();

            RefreshSoundLabel();
        }

        public void SetVisible(bool visible) => Root.gameObject.SetActive(visible);

        public void SetBestScore(int bestScore) => _bestScoreText.text = $"Best: {bestScore}";

        private void ToggleSound()
        {
            bool isEnabled = PlayerPrefs.GetInt(SoundEnabledPrefKey, 1) != 0;
            PlayerPrefs.SetInt(SoundEnabledPrefKey, isEnabled ? 0 : 1);
            PlayerPrefs.Save();
            RefreshSoundLabel();
            // AudioManager (step 8) reads the same pref key to mute/unmute the mixer.
        }

        private void RefreshSoundLabel()
        {
            bool isEnabled = PlayerPrefs.GetInt(SoundEnabledPrefKey, 1) != 0;
            _soundToggleLabel.text = isEnabled ? "SOUND: ON" : "SOUND: OFF";
        }
    }
}
