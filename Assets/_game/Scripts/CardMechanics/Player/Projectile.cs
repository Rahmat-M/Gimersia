using UnityEngine;

namespace Littale {
    public class Projectile : MonoBehaviour {
        public float speed = 8f;
        int damage = 1;

        private Vector2 direction = Vector2.right;

        public void SetDamage(int d) { damage = d; }


        void Start() { Destroy(gameObject, 4f); } // auto destroy

        public void SetDirection(Vector2 d) { direction = d.normalized; }



        void Update() {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }

        void OnTriggerEnter2D(Collider2D other) {
            // jika kena enemy: deal damage (stub)
            Debug.Log("[Projectile] hit: " + other.name + " dmg: " + damage);
            Destroy(gameObject);
        }
    }
}
