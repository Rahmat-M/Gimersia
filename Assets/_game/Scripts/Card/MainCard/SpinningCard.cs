using PrimeTween;
using UnityEngine;

namespace Littale {
    public class SpinningCard : CardController {

        public override void Attack() {
            GameObject meleeHitbox = Instantiate(cardData.BehaviourPrefab);
            meleeHitbox.transform.position = transform.position; //Set the position to be the same as the player
            meleeHitbox.transform.parent = transform;    //Set the hitbox to be a child of the player
        }

    }
}
