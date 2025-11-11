using UnityEngine;

namespace Littale {
    public class BasicProjectileCard : CardController {
        public override void Attack() {
            GameObject projectile = Instantiate(cardData.Prefab);
            projectile.transform.position = transform.position; //Set the position to be the same as the player
            projectile.GetComponent<ProjectileBehaviour>().DirectionChecker(characterMovement.LastMovementInput);   //Reference and set the direction
        }
    }
}
