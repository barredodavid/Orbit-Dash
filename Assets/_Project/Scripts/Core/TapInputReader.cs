using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OrbitDash.Core
{
    /// <summary>
    /// Reads a single tap/click input using the new Input System.
    /// Uses the generic Pointer device so it works with touch, mouse and pen
    /// without needing separate bindings per platform.
    /// </summary>
    public class TapInputReader : MonoBehaviour
    {
        /// <summary>Raised on the frame a tap/click is pressed down.</summary>
        public event Action Tapped;

        private InputAction _tapAction;

        private void Awake()
        {
            _tapAction = new InputAction(name: "Tap", type: InputActionType.Button, binding: "<Pointer>/press");
        }

        private void OnEnable()
        {
            _tapAction.performed += HandleTapPerformed;
            _tapAction.Enable();
        }

        private void OnDisable()
        {
            _tapAction.Disable();
            _tapAction.performed -= HandleTapPerformed;
        }

        private void OnDestroy()
        {
            _tapAction?.Dispose();
        }

        private void HandleTapPerformed(InputAction.CallbackContext context)
        {
            Tapped?.Invoke();
        }
    }
}
