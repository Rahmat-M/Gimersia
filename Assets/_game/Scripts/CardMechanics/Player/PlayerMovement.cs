using UnityEngine;

namespace Littale {
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour {
        public float moveSpeed = 5f;
        private Rigidbody2D rb;
        private Vector2 moveInput;
        private Vector2 lastMoveDir = Vector2.down; // default menghadap bawah
        private Vector2 aimDir;

        private Camera mainCam;
        private Animator anim;

        public Animator animator;

        void Start() {
            rb = GetComponent<Rigidbody2D>();
            mainCam = Camera.main;
            anim = GetComponent<Animator>();
        }

        void Update() {
            // --- WASD Movement ---
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
            moveInput = moveInput.normalized;

            if (moveInput != Vector2.zero)
                lastMoveDir = moveInput;

            // --- Aim Direction ke Mouse (tanpa rotasi player) ---
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            aimDir = ((Vector2)(mousePos - transform.position)).normalized;

            UpdateAnimator();
        }

        void FixedUpdate() {
            rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        }

        void UpdateAnimator() {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
            animator.SetBool("IsMoving", moveInput != Vector2.zero);

            if (moveInput != Vector2.zero) {
                lastMoveDir = moveInput;
                animator.SetFloat("LastMoveX", lastMoveDir.x);
                animator.SetFloat("LastMoveY", lastMoveDir.y);
            }
        }

        public Vector2 GetAimDirection() => aimDir;
        public Vector2 GetLastMoveDirection() => lastMoveDir;

        //  Tambahkan ini untuk memperbaiki error di PlayerCombat.cs
        public void SetLastMoveDirection(Vector2 dir) {
            lastMoveDir = dir.normalized;
            animator.SetFloat("LastMoveX", lastMoveDir.x);
            animator.SetFloat("LastMoveY", lastMoveDir.y);
        }
    }
}
