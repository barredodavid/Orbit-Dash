using UnityEngine;

namespace OrbitDash.Gameplay
{
    /// <summary>
    /// Identifies the player's game object for collision checks (see Obstacle.OnTriggerEnter2D),
    /// so gameplay systems can recognize the player without comparing tags or hard string names.
    /// </summary>
    public class PlayerMarker : MonoBehaviour
    {
    }
}
