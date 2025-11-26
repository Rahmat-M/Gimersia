using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Littale {
    public class ManaUIHandler : MonoBehaviour {

        [Header("References")]
        [SerializeField] PlayerCardManager playerCardManager;
        [SerializeField] Image inkJarFillImage;

        [Header("Settings")]
        [SerializeField] Color normalColor = Color.cyan;
        [SerializeField] Color lowManaColor = Color.red;
        [SerializeField] float lowManaThreshold = 0.2f;

        void Start() {
            if (playerCardManager == null) playerCardManager = FindFirstObjectByType<PlayerCardManager>();

            if (playerCardManager != null) {
                playerCardManager.OnManaChanged += UpdateManaUI;
                // Initialize
                UpdateManaUI(playerCardManager.CurrentMana, playerCardManager.maxMana);
            }
        }

        void OnDestroy() {
            if (playerCardManager != null) {
                playerCardManager.OnManaChanged -= UpdateManaUI;
            }
        }

        void UpdateManaUI(float current, float max) {
            if (inkJarFillImage == null) return;

            float fillAmount = current / max;
            
            // Smooth Fill Animation
            Tween.Custom(inkJarFillImage.fillAmount, fillAmount, duration: 0.2f, onValueChange: val => inkJarFillImage.fillAmount = val);

            // Color Change based on Low Mana
            if (fillAmount <= lowManaThreshold) {
                inkJarFillImage.color = Color.Lerp(inkJarFillImage.color, lowManaColor, Time.deltaTime * 5f);
            } else {
                inkJarFillImage.color = Color.Lerp(inkJarFillImage.color, normalColor, Time.deltaTime * 5f);
            }
        }
    }
}
