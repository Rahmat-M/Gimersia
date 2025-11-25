using UnityEngine;

namespace Littale {
    [RequireComponent(typeof(Animator),
    typeof(SpriteRenderer))]
    public class PlayerAnimator : MonoBehaviour {

        const string ANIM_PARAM_ISMOVING = "IsMoving";

        PlayerMovement charController;
        Animator animator;
        SpriteRenderer spriteRenderer;

        void Awake() {
            charController = FindFirstObjectByType<PlayerMovement>(); // for get direction input
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Update() {
            // Update animator parameters
            float horizontal = charController.MovementInput.x;
            bool isMoving = charController.MovementInput.magnitude > 0.1f;

            animator.SetBool(ANIM_PARAM_ISMOVING, isMoving);

            if (isMoving) spriteRenderer.flipX = horizontal > 0;
        }

    }
}