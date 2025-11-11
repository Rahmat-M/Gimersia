using UnityEngine;

namespace Littale {
    public class MeleeCard : CardController {
        public override void Attack() {
            GameObject meleeHitbox = Instantiate(cardData.Prefab);
            meleeHitbox.transform.position = transform.position + (Vector3)characterMovement.LastMovementInput; //Set the position to be the same as the player
            meleeHitbox.transform.parent = transform;    //Set the hitbox to be a child of the player
        }
    }
}
