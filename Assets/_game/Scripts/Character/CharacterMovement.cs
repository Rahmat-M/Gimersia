using UnityEngine;
using UnityEngine.InputSystem;

namespace Littale {
    public class CharacterMovement : MonoBehaviour {

        // References
        [SerializeField] InputActionReference moveActions;

        public InputActionReference TriggerActions;

        CharacterStats characterStats;

        // Movement
        Vector2 movementInput;
        public Vector2 MovementInput { get { return movementInput; } }
        Vector2 lastMovementInput = Vector2.right; // Default to right
        public Vector2 LastMovementInput { get { return lastMovementInput; } }

        void Awake() {
            characterStats = GetComponent<CharacterStats>();
            TriggerActions.action.Enable();
        }

        void Update() {
            // Handle Input
            movementInput = moveActions.action.ReadValue<Vector2>();
            if (movementInput != Vector2.zero) lastMovementInput = movementInput; // Save last non-zero input for direction purposes
        }

        void FixedUpdate() {
            // Handle Movement
            Vector3 movementVector = Vector3.zero;
            movementVector += (Vector3)movementInput;
            transform.position += movementVector.normalized * Time.deltaTime * characterStats.Actual.moveSpeed;
        }

    }
}
