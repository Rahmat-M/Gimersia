using UnityEngine;
using UnityEngine.InputSystem;

namespace Littale {
    public class PlayerMovement : MonoBehaviour {

        public System.Action<bool> OnAimModeChanged;

        // References
        [SerializeField] InputActionReference moveActions;

        public bool IsAiming { get; private set; }
        Camera mainCamera;
        PlayerStats characterStats;
        Rigidbody2D rb;

        // Movement
        Vector2 movementInput;
        public Vector2 MovementInput { get { return movementInput; } }
        Vector2 _cachedMovementDirection = Vector2.right;

        public Vector2 LastMovementInput {
            get {
                // Flow Mode: Attack direction = Movement direction
                // Focus Mode: Attack direction = Mouse direction
                if (IsAiming) {
                    return GetDirectionToMouse();
                }

                // If moving, use movement input. If stopped, use cached direction.
                if (movementInput != Vector2.zero) return movementInput.normalized;
                return _cachedMovementDirection;
            }
        }

        void Awake() {
            characterStats = GetComponent<PlayerStats>();
            mainCamera = Camera.main;
            rb = GetComponent<Rigidbody2D>();
        }

        void Update() {
            // Handle Input
            movementInput = moveActions.action.ReadValue<Vector2>();
            if (movementInput != Vector2.zero) _cachedMovementDirection = movementInput.normalized;
        }

        public void OnToggleAimMode(InputAction.CallbackContext context) {
            if (context.performed) {
                SetAimMode(!IsAiming);
            }
        }

        void FixedUpdate() {
            // Handle Movement
            Vector3 movementVector = (Vector3)movementInput;
            transform.position += movementVector.normalized * Time.deltaTime * characterStats.Actual.moveSpeed;

        }

        void HandleVisualFlip() {
            // Determine facing direction based on Aim Mode
            Vector2 lookDir = Vector2.zero;

            if (IsAiming) {
                // Focus Mode: Face Mouse
                lookDir = GetDirectionToMouse();
            } else {
                // Flow Mode: Face Movement
                if (movementInput != Vector2.zero) {
                    lookDir = movementInput.normalized;
                } else {
                    lookDir = _cachedMovementDirection;
                }
            }

            // Flip Sprite based on X direction
            if (lookDir.x > 0.1f) {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x);
                transform.localScale = scale;
            } else if (lookDir.x < -0.1f) {
                Vector3 scale = transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }

        void SetAimMode(bool active) {
            IsAiming = active;
            Cursor.visible = !active;
            OnAimModeChanged?.Invoke(active);
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
