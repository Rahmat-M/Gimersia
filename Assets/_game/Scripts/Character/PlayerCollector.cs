using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Littale {
    [RequireComponent(typeof(Collider2D))]
    public class PlayerCollector : MonoBehaviour {

        public UnityEvent<float> OnCoinCollected;

        PlayerStats player;
        CircleCollider2D detector;
        public float pullSpeed = 10;

        int coins;

        void Start() {
            player = GetComponentInParent<PlayerStats>();
            coins = 0;
        }

        public void SetRadius(float r) {
            if (!detector) detector = GetComponent<CircleCollider2D>();
            detector.radius = r;
        }

        public int GetCoins() { return coins; }

        //Updates coins Display and information
        public int AddCoins(int amount) {
            coins += amount;
            OnCoinCollected?.Invoke(coins);
            return coins;
        }

        public int SpendCoins(int amount) {
            coins -= amount;
            OnCoinCollected?.Invoke(coins);
            return coins;
        }

        public void SaveCoinsToStash() {
            // SaveManager.LastLoadedGameData.coins += coins;
            // SaveManager.Save();
        }

        private void OnTriggerEnter2D(Collider2D col) {
            if (col.TryGetComponent(out Pickup p)) {
                p.Collect(player, pullSpeed);
            }
        }
    }
}