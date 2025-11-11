using UnityEngine;

namespace Littale {
    public class ProjectileCardBehaviour : ProjectileBehaviour {

        void Update() {
            transform.position += direction * currentSpeed * Time.deltaTime;    //Set the movement of the projectile
        }

    }
}