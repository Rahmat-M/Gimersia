using UnityEngine;
using UnityEngine.InputSystem;

namespace Littale {
    public class PlayerMovement : MonoBehaviour {

        // References
        [SerializeField] InputActionReference moveActions;
        public InputActionReference TriggerActions;

        [Header("Aiming Controls")]
        [SerializeField] Texture2D aimCursorTexture;
        [SerializeField] Vector2 cursorHotspot = Vector2.zero;

        bool isMouseAiming = false;
        Camera mainCamera;
        PlayerStats characterStats;

        // Movement
        Vector2 movementInput;
        public Vector2 MovementInput { get { return movementInput; } }
        Vector2 _cachedMovementDirection = Vector2.right;

        public Vector2 LastMovementInput {
            get {
                // Jika mode Aim aktif, kembalikan arah ke Mouse.
                // Jika tidak, kembalikan arah gerakan terakhir (WASD).
                if (isMouseAiming) {
                    return GetDirectionToMouse();
                }
                return _cachedMovementDirection;
            }
        }

        void Awake() {
            characterStats = GetComponent<PlayerStats>();
            mainCamera = Camera.main;
            if (TriggerActions != null) TriggerActions.action.Enable();
        }

        void Update() {
            // Handle Input
            movementInput = moveActions.action.ReadValue<Vector2>();
            if (movementInput != Vector2.zero) _cachedMovementDirection = movementInput.normalized;

            if (Mouse.current.rightButton.wasPressedThisFrame) ToggleAimMode();
        }

        void FixedUpdate() {
            // Handle Movement
            Vector3 movementVector = (Vector3)movementInput;
            transform.position += movementVector.normalized * Time.deltaTime * characterStats.Actual.moveSpeed;
        }

        void ToggleAimMode() {
            isMouseAiming = !isMouseAiming;

            if (isMouseAiming) {
                Cursor.SetCursor(aimCursorTexture, cursorHotspot, CursorMode.Auto);
                // Cursor.lockState = CursorLockMode.Confined; // Lock cursor within game window
            } else {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                // Cursor.lockState = CursorLockMode.None;
            }
        }

        Vector2 GetDirectionToMouse() {
            if (mainCamera == null) return _cachedMovementDirection;

            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, mainCamera.nearClipPlane));

            Vector2 direction = (mouseWorldPosition - transform.position).normalized;

            return direction;
        }

        void OnDestroy() {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

    }
}
