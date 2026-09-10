using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitDash.Utils
{
    /// <summary>
    /// Builds simple UGUI/TextMeshPro widgets entirely from code, so the whole UI hierarchy is
    /// generated at runtime instead of a hand-authored Canvas asset. Swap in real art later
    /// (see README, step 7) by assigning a sprite to the returned Image instead of touching this factory.
    /// </summary>
    public static class UIFactory
    {
        /// <summary>A rect, center-anchored to its parent by default (most convenient for stacked UI).</summary>
        private static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, worldPositionStays: false);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            return rect;
        }

        /// <summary>A rect stretched to fill its parent on all sides.</summary>
        public static RectTransform CreateStretchedRect(string name, Transform parent)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        /// <summary>Full-screen panel with a solid background and a CanvasGroup to fade/block raycasts as one unit.</summary>
        public static (RectTransform rect, CanvasGroup group, Image background) CreateScreenPanel(string name, Transform parent, Color backgroundColor)
        {
            RectTransform rect = CreateStretchedRect(name, parent);
            var group = rect.gameObject.AddComponent<CanvasGroup>();
            var image = rect.gameObject.AddComponent<Image>();
            image.color = backgroundColor;
            return (rect, group, image);
        }

        /// <summary>Text placed at a fixed offset from its parent's center, with a fixed size.</summary>
        public static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize, Color color,
            Vector2 anchoredPosition, Vector2 sizeDelta, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            return tmp;
        }

        /// <summary>Button placed at a fixed offset from its parent's center, with a fixed size.</summary>
        public static Button CreateButton(string name, Transform parent, string label, Color backgroundColor, Color labelColor,
            Vector2 anchoredPosition, Vector2 sizeDelta, float fontSize = 46f)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var image = rect.gameObject.AddComponent<Image>();
            image.color = backgroundColor;

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            TextMeshProUGUI text = CreateText($"{name}Label", rect, label, fontSize, labelColor, Vector2.zero, Vector2.zero);
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }
    }
}
