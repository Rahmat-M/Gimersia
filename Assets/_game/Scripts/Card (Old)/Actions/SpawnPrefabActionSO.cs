using UnityEngine;

namespace Littale {
    [CreateAssetMenu(fileName = "New Spawn Prefab Action", menuName = "Littale Data/Card/Actions/Spawn Prefab")]
    public class SpawnPrefabActionSO : CardActionSO {
        
        [Header("Spawning Settings")]
        [Tooltip("If true, the spawned object will be a child of the owner.")]
        public bool parentToOwner = false;
        
        [Tooltip("If true, the object will be rotated to face the movement direction.")]
        public bool useMovementDirection = true;

        [Tooltip("Offset from the owner's position.")]
        public Vector3 spawnOffset = Vector3.zero;

        public override void PerformAction(BaseCard controller, BaseCardSO data) {
            if (data.BehaviourPrefab == null) {
                Debug.LogWarning($"Card {data.name} has no BehaviourPrefab assigned!");
                return;
            }

            // Calculate spawn position
            Vector3 spawnPos = controller.transform.position + spawnOffset;
            
            // Instantiate
            GameObject spawnedObj = Instantiate(data.BehaviourPrefab, spawnPos, Quaternion.identity);
            
            // Handle Parenting
            if (parentToOwner) {
                spawnedObj.transform.SetParent(controller.transform);
            }

            // Handle Direction/Rotation
            Vector2 direction = controller.GetMovementInput(); 
            
            // Option 1: SendMessage (Flexible but slow/unsafe)
            spawnedObj.SendMessage("DirectionChecker", direction, SendMessageOptions.DontRequireReceiver);
            
            // Option 2: Check for known types (Safe but coupled)
            if (spawnedObj.TryGetComponent(out MeleeBehaviour melee)) {
                melee.DirectionChecker(direction);
            } else if (spawnedObj.TryGetComponent(out ProjectileBehaviour proj)) {
                proj.DirectionChecker(direction);
            }
        }
    }
}
