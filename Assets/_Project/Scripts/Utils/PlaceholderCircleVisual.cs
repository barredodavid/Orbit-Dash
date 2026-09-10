using UnityEngine;

namespace OrbitDash.Utils
{
    /// <summary>
    /// Draws a gameplay object's visual (player, sun, obstacle...). Falls back to a generated
    /// circle until real art is available; picks it up automatically via <see cref="resourceSpritePath"/>
    /// (a sprite under a Resources/Sprites folder) or, failing that, <see cref="overrideSprite"/>
    /// (an explicit Inspector reference) - see README, step 7 for the art integration notes.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlaceholderCircleVisual : MonoBehaviour
    {
        [Tooltip("Diameter of the placeholder circle, in world units. Ignored once real art is used.")]
        [SerializeField] private float worldDiameter = 0.6f;

        [SerializeField] private Color color = Color.white;

        [SerializeField] private int sortingOrder;

        [Tooltip("If set, this sprite is used instead of the generated placeholder circle.")]
        [SerializeField] private Sprite overrideSprite;

        [Tooltip("If overrideSprite is empty, tries loading a sprite from this Resources path first (e.g. \"Sprites/Gameplay/player\").")]
        [SerializeField] private string resourceSpritePath;

        private void Awake()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = ResolveSprite();
            spriteRenderer.color = Color.white; // any tint lives in the sprite itself, not this component
            spriteRenderer.sortingOrder = sortingOrder;

            if (spriteRenderer.sharedMaterial == null)
            {
                spriteRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            }
        }

        private Sprite ResolveSprite()
        {
            if (overrideSprite != null)
            {
                return overrideSprite;
            }

            if (!string.IsNullOrEmpty(resourceSpritePath))
            {
                Sprite loaded = Resources.Load<Sprite>(resourceSpritePath);
                if (loaded != null)
                {
                    return loaded;
                }
            }

            return RuntimeCircleSpriteFactory.CreateCircleSprite(worldDiameter, color);
        }
    }
}
