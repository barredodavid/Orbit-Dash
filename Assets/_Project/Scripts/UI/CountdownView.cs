using OrbitDash.Utils;
using TMPro;
using UnityEngine;

namespace OrbitDash.UI
{
    /// <summary>Big centered "3 2 1 GO!" overlay shown during GameState.Countdown, right before a run starts.</summary>
    public class CountdownView : MonoBehaviour
    {
        private TextMeshProUGUI _numberText;

        public RectTransform Root { get; private set; }
        public CanvasGroup Group { get; private set; }

        public void Init(Transform canvasRoot)
        {
            (RectTransform rect, CanvasGroup group, _) = UIFactory.CreateScreenPanel("CountdownView", canvasRoot, Color.clear);
            Root = rect;
            Group = group;

            _numberText = UIFactory.CreateText("CountdownNumber", Root, "3", 220f, Color.white,
                anchoredPosition: Vector2.zero, sizeDelta: new Vector2(600f, 400f));
        }

        public void SetVisible(bool visible) => Root.gameObject.SetActive(visible);

        public void SetCount(int secondsRemaining) => _numberText.text = secondsRemaining > 0 ? secondsRemaining.ToString() : "GO!";
    }
}
