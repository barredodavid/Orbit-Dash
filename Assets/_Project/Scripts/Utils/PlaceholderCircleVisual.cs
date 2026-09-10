using UnityEngine;

namespace OrbitDash.Utils
{
    /// <summary>
    /// Draws a simple circular placeholder for a gameplay object (player, sun, obstacle...)
    /// until real art is imported. Assign <see cref="overrideSprite"/> later to swap in real art
    /// without touching any other script (see README, step 7 - art integration).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlaceholderCircleVisual : MonoBehaviour
    {
        [Tooltip("Diameter of the placeholder circle, in world units.")]
        [SerializeField] private float worldDiameter = 0.6f;

        [SerializeField] private Color color = Color.white;

        [SerializeField] private int sortingOrder;

        [Tooltip("If set, this sprite is used instead of the generated placeholder circle.")]
        [SerializeField] private Sprite overrideSprite;

        private void Awake()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = overrideSprite != null
                ? overrideSprite
                : RuntimeCircleSpriteFactory.CreateCircleSprite(worldDiameter, color);
            spriteRenderer.color = Color.white; // the placeholder color is baked into its texture
            spriteRenderer.sortingOrder = sortingOrder;

            if (spriteRenderer.sharedMaterial == null)
            {
                spriteRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            }
        }
    }
}
