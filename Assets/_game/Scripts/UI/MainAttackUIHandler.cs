using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Littale {
    public class MainAttackUIHandler : MonoBehaviour {

        [Header("References")]
        [SerializeField] PlayerCardManager playerCardManager;
        [SerializeField] PlayerMovement playerMovement;
        [SerializeField] Image pulseRingImage; // A ring image on the cursor/player

        [Header("Settings")]
        [SerializeField] float baseScale = 1f;
        [SerializeField] float pulseScale = 1.5f;

        private Vector3 originalScale;

        void Start() {
            if (playerCardManager == null) playerCardManager = FindFirstObjectByType<PlayerCardManager>();
            if (playerMovement == null) {
                playerMovement = FindFirstObjectByType<PlayerMovement>();
                if (playerMovement != null) playerMovement.OnAimModeChanged += gameObject.SetActive;
            }
            if (pulseRingImage != null) {
                originalScale = pulseRingImage.transform.localScale;
            }
        }

        void OnDestroy() {
            playerMovement.OnAimModeChanged -= gameObject.SetActive;
        }

        void Update() {
            UpdatePosition();
            UpdateRhythm();
        }

        void UpdatePosition() {
            if (pulseRingImage == null && !pulseRingImage.gameObject.activeSelf) return;

            Vector3 mousePos = Mouse.current.position.ReadValue();
            Vector3 offset = Vector3.up * 50f; // Slight offset below cursor
            pulseRingImage.transform.position = mousePos + offset; // UI Overlay usually works with Screen Space
        }

        void UpdateRhythm() {
            if (playerCardManager == null || playerCardManager.anchorCard == null || pulseRingImage == null) return;

            // We need access to the actual timer in PlayerCardManager.
            // Since it's private, we might need to expose it or estimate it.
            // For now, let's assume we can get the cooldown duration.

            // Ideally, PlayerCardManager should expose "AttackProgress" (0 to 1).
            // Let's assume we added a property or event for this, OR we just simulate the pulse based on cooldown.

            float cooldown = playerCardManager.anchorCard.cooldown;

            // Simple Pulse Effect based on Time
            // Pulse every 'cooldown' seconds
            float t = Mathf.Repeat(Time.time, cooldown) / cooldown;

            // Visual: Ring fills up or scales
            pulseRingImage.fillAmount = t;

            // Pulse at the moment of attack (t near 1 or 0)
            if (t < 0.1f) {
                pulseRingImage.transform.localScale = originalScale * pulseScale;
            } else {
                pulseRingImage.transform.localScale = Vector3.Lerp(pulseRingImage.transform.localScale, originalScale * baseScale, Time.deltaTime * 10f);
            }
        }
    }
}
