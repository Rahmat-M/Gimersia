using UnityEngine;
using UnityEngine.InputSystem;

namespace Littale {
    public class ShopInteractor : MonoBehaviour {
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private InputActionReference interactAction;

        private bool canInteract = false;
        private int currentWave = 1;

        // void Start() {
        //     interactionPrompt.SetActive(false);
        //     interactAction.action.Enable();
        //     interactAction.action.performed += ctx => OnInteract();
        // }

        // private void OnInteract() {
        //     if (canInteract && Time.timeScale > 0) {
        //         ShopManager.Instance.OpenShop();
        //     }
        // }

        void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Player")) {
                interactionPrompt.SetActive(true);
                canInteract = true;
            }
        }

        void OnTriggerExit(Collider other) {
            if (other.CompareTag("Player")) {
                interactionPrompt.SetActive(false);
                canInteract = false;
                // ShopManager.Instance.CloseShop(); // Opsi: tutup otomatis
            }
        }

        public void SetWave(int wave) {
            currentWave = wave;
        }
    }
}