using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Littale {
    public class AimingUIHandler : MonoBehaviour {

        [Header("References")]
        [SerializeField] PlayerCardManager playerCardManager;
        [SerializeField] DeckManager deckManager;
        [SerializeField] PlayerMovement playerMovement;

        [Header("Visuals")]
        [SerializeField] LineRenderer aimingLine;
        [SerializeField] Transform crosshair;
        [SerializeField] Transform arrowIndicator; // New: Arrow at feet
        [SerializeField] Transform aoeMarker;
        [SerializeField] float lineLength = 10f;

        private Camera mainCamera;

        void Start() {
            mainCamera = Camera.main;
            if (playerCardManager == null) playerCardManager = FindFirstObjectByType<PlayerCardManager>();
            if (deckManager == null) deckManager = FindFirstObjectByType<DeckManager>();
            if (playerMovement == null) playerMovement = FindFirstObjectByType<PlayerMovement>();

            if (aimingLine) aimingLine.enabled = false;
            if (aoeMarker) aoeMarker.gameObject.SetActive(false);
        }

        void Update() {
            HandleAimingModeVisuals();
            HandleAimingLine();
        }

        void HandleAimingModeVisuals() {
            bool isAiming = playerMovement != null && playerMovement.IsAiming;

            if (isAiming) {
                // Focus Mode: Show Crosshair, Hide Arrow
                if (crosshair) {
                    crosshair.gameObject.SetActive(true);
                    UpdateCrosshairPosition();
                }
                if (arrowIndicator) arrowIndicator.gameObject.SetActive(false);
            } else {
                // Flow Mode: Hide Crosshair, Show Arrow
                if (crosshair) crosshair.gameObject.SetActive(false);

                if (arrowIndicator) {
                    arrowIndicator.gameObject.SetActive(true);
                    UpdateArrowPositionAndRotation();
                }
            }
        }

        void UpdateCrosshairPosition() {
            if (crosshair == null) return;

            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = 10f; // Distance
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            worldPos.z = 0;

            crosshair.position = worldPos;
        }

        void UpdateArrowPositionAndRotation() {
            if (arrowIndicator == null || playerMovement == null) return;

            // Position: At player's feet (assumed player position)
            arrowIndicator.position = playerMovement.transform.position;

            // Rotation: Match player's movement direction
            Vector2 moveDir = playerMovement.MovementInput;
            if (moveDir != Vector2.zero) {
                float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
                arrowIndicator.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }

        void HandleAimingLine() {
            // Check if holding Q or E
            bool isHoldingQ = Keyboard.current.qKey.isPressed;
            bool isHoldingE = Keyboard.current.eKey.isPressed;

            if (isHoldingQ || isHoldingE) {
                // Determine which card
                CardData card = isHoldingQ ? deckManager.slot1 : deckManager.slot2;

                if (card != null) {
                    ShowAimingVisuals(card);
                } else {
                    HideAimingVisuals();
                }
            } else {
                HideAimingVisuals();
            }
        }

        void ShowAimingVisuals(CardData card) {
            Vector3 playerPos = playerCardManager.transform.position;

            // Direction depends on mode
            Vector3 targetPos = Vector3.zero;
            if (playerMovement.IsAiming) {
                if (crosshair) targetPos = crosshair.position;
            } else {
                // In Flow mode, aim forward
                targetPos = playerPos + (Vector3)playerMovement.LastMovementInput * 5f;
            }

            Vector3 dir = (targetPos - playerPos).normalized;

            // Line Renderer
            if (aimingLine) {
                aimingLine.enabled = true;
                aimingLine.SetPosition(0, playerPos);
                aimingLine.SetPosition(1, playerPos + dir * lineLength);
            }

            // AoE Marker
            if (aoeMarker) {
                bool isAoE = card.cardName.Contains("Bomb") || card.cardName.Contains("Trap") || card.cardName.Contains("Bucket");

                if (isAoE) {
                    aoeMarker.gameObject.SetActive(true);
                    // In Flow mode, maybe lock distance? For now, just use targetPos
                    aoeMarker.position = targetPos;
                } else {
                    aoeMarker.gameObject.SetActive(false);
                }
            }
        }

        void HideAimingVisuals() {
            if (aimingLine) aimingLine.enabled = false;
            if (aoeMarker) aoeMarker.gameObject.SetActive(false);
        }
    }
}
