using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitDash.Utils
{
    /// <summary>
    /// Builds simple UGUI/TextMeshPro widgets entirely from code, so the whole UI hierarchy is
    /// generated at runtime instead of a hand-authored Canvas asset. Buttons/icons pick up real art
    /// automatically from a Resources path (see SpriteImportPostprocessor for import settings) and
    /// fall back to a flat color if the sprite isn't there yet.
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

        private static Sprite LoadSprite(string resourcePath) =>
            string.IsNullOrEmpty(resourcePath) ? null : Resources.Load<Sprite>(resourcePath);

        private static TMP_FontAsset _themeFont;
        private static bool _themeFontLookedUp;

        /// <summary>The theme's TMP font asset if it's been generated (see Editor/GenerateFontAssets.cs), else null.</summary>
        private static TMP_FontAsset ThemeFont
        {
            get
            {
                if (!_themeFontLookedUp)
                {
                    _themeFont = Resources.Load<TMP_FontAsset>("Orbitron SDF");
                    _themeFontLookedUp = true;
                }
                return _themeFont;
            }
        }

        /// <summary>
        /// Re-anchors a widget to its parent's top edge, offset downward by <paramref name="offsetFromTop"/>.
        /// Elements placed far from center (titles, etc.) should use this instead of a large center
        /// offset, so they stay on-screen at aspect ratios narrower/shorter than the 1080x1920 reference.
        /// </summary>
        public static void AnchorToTop(RectTransform rect, float offsetFromTop)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -offsetFromTop);
        }

        /// <summary>Same as <see cref="AnchorToTop"/> but relative to the parent's bottom edge, offset upward.</summary>
        public static void AnchorToBottom(RectTransform rect, float offsetFromBottom)
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, offsetFromBottom);
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
            if (ThemeFont != null)
            {
                tmp.font = ThemeFont;
            }
            return tmp;
        }

        /// <summary>
        /// Text-labeled button placed at a fixed offset from its parent's center. Uses
        /// <paramref name="backgroundSpritePath"/> (9-sliced) if it resolves, else a flat
        /// <paramref name="backgroundColor"/> rectangle.
        /// </summary>
        public static Button CreateButton(string name, Transform parent, string label, Color backgroundColor, Color labelColor,
            Vector2 anchoredPosition, Vector2 sizeDelta, float fontSize = 46f, string backgroundSpritePath = null)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var image = rect.gameObject.AddComponent<Image>();
            ApplyButtonBackground(image, backgroundSpritePath, backgroundColor);

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

        /// <summary>
        /// Icon-only button (e.g. pause, sound toggle). Returns the icon Image so the caller can
        /// swap its sprite later (e.g. toggling between "sound on"/"sound off").
        /// </summary>
        public static (Button button, Image icon) CreateIconButton(string name, Transform parent,
            string backgroundSpritePath, Color fallbackBackgroundColor, string iconSpritePath,
            Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 iconSizeDelta)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var backgroundImage = rect.gameObject.AddComponent<Image>();
            ApplyButtonBackground(backgroundImage, backgroundSpritePath, fallbackBackgroundColor);

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = backgroundImage;

            RectTransform iconRect = CreateRect($"{name}Icon", rect);
            iconRect.sizeDelta = iconSizeDelta;
            var iconImage = iconRect.gameObject.AddComponent<Image>();
            iconImage.sprite = LoadSprite(iconSpritePath);
            iconImage.raycastTarget = false;
            iconImage.color = iconImage.sprite != null ? Color.white : Color.clear; // hide cleanly if art is missing

            return (button, iconImage);
        }

        private static void ApplyButtonBackground(Image image, string backgroundSpritePath, Color fallbackColor)
        {
            Sprite sprite = LoadSprite(backgroundSpritePath);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
                image.color = Color.white;
            }
            else
            {
                image.color = fallbackColor;
            }
        }
    }
}
