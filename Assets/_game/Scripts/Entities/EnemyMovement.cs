using UnityEngine;

namespace Littale {
    public class EnemyMovement : Sortable {
        protected EnemyStats stats;
        protected Transform player;
        protected Rigidbody2D rb;

        protected Vector2 knockbackVelocity;
        protected float knockbackDuration;

        public enum OutOfFrameAction { none, respawnAtEdge, despawn }
        public OutOfFrameAction outOfFrameAction = OutOfFrameAction.respawnAtEdge;

        [System.Flags]
        public enum KnockbackVariance { duration = 1, velocity = 2 }
        public KnockbackVariance knockbackVariance = KnockbackVariance.velocity;

        protected bool spawnedOutOfFrame = false;

        float defaultScaleX;

        protected override void Start() {
            base.Start();
            defaultScaleX = transform.localScale.x;

            rb = GetComponent<Rigidbody2D>();
            spawnedOutOfFrame = !SpawnManager.IsWithinBoundaries(transform);
            stats = GetComponent<EnemyStats>();

            // Picks a random player on the screen, instead of always picking the 1st player.
            PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
            player = allPlayers[Random.Range(0, allPlayers.Length)].transform;
        }

        protected virtual void Update() {
            // If we are currently being knocked back, then process the knockback.
            if (knockbackDuration > 0) {
                transform.position += (Vector3)knockbackVelocity * Time.deltaTime;
                knockbackDuration -= Time.deltaTime;
            } else {
                Move();
                HandleOutOfFrameAction();
            }
        }

        protected virtual void FlipSprite() {
            if (!player) return;

            Vector3 scale = transform.localScale;
            scale.x = player.position.x > transform.position.x ? -defaultScaleX : defaultScaleX;
            transform.localScale = scale;
        }

        // If the enemy falls outside of the frame, handle it.
        protected virtual void HandleOutOfFrameAction() {
            // Handle the enemy when it is out of frame.
            if (!SpawnManager.IsWithinBoundaries(transform)) {
                switch (outOfFrameAction) {
                    case OutOfFrameAction.none:
                    default:
                        break;
                    case OutOfFrameAction.respawnAtEdge:
                        // If the enemy is outside the camera frame, teleport it back to the edge of the frame.
                        transform.position = SpawnManager.GeneratePosition();
                        break;
                    case OutOfFrameAction.despawn:
                        // Don't destroy if it is spawned outside the frame.
                        if (!spawnedOutOfFrame) {
                            Destroy(gameObject);
                        }
                        break;
                }
            } else spawnedOutOfFrame = false;
        }

        // This is meant to be called from other scripts to create knockback.
        public virtual void Knockback(Vector2 velocity, float duration) {
            // Simply apply the knockback.
            // EnemyStats.cs already handles the "Force vs Weight" calculation.
            knockbackVelocity = velocity;
            knockbackDuration = duration;
        }

        public virtual void Move() {
            // if (rb) {
            //     rb.MovePosition(Vector2.MoveTowards(
            //         rb.position,
            //         player.transform.position,
            //         stats.Actual.moveSpeed * Time.deltaTime
            //     ));
            // } else {
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.transform.position,
                stats.Actual.moveSpeed * Time.deltaTime
            );
            // }

            FlipSprite(); // Flip sprite based on direction
        }
    }
}