namespace Littale {
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "CardDatabase", menuName = "Cards/Card Database")]
    public class CardDatabase : ScriptableObject {
        public List<CardData> allCards;

        /// <summary>
        /// Mengambil beberapa kartu acak yang unik dari database.
        /// </summary>
        /// <param name="count">Jumlah kartu yang ingin diambil</param>
        /// <returns>Daftar kartu acak</returns>
        public List<CardData> GetRandomCards(int count) {
            List<CardData> result = new List<CardData>();
            if (count > allCards.Count) {
                Debug.LogWarning("Meminta lebih banyak kartu daripada yang ada di database!");
                return allCards;
            }

            List<CardData> tempList = new List<CardData>(allCards);

            for (int i = 0; i < count; i++) {
                int randomIndex = Random.Range(0, tempList.Count);
                CardData randomCard = tempList[randomIndex];

                result.Add(randomCard);

                tempList.RemoveAt(randomIndex);
            }

            return result;
        }

        // Helper lainnya bisa ditambahkan di sini

#if UNITY_EDITOR
        [ContextMenu("Find All Cards in Project")]
        private void FindAllCards() {
            // Fungsi ini HANYA berjalan di Editor
            // Dia akan mencari semua file .asset yang tipenya CardData

            allCards = new List<CardData>();
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CardData");

            if (guids.Length == 0) {
                Debug.LogWarning("Tidak ada 'CardData' asset yang ditemukan di project.");
                return;
            }

            foreach (string guid in guids) {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                CardData card = UnityEditor.AssetDatabase.LoadAssetAtPath<CardData>(path);
                if (card != null) {
                    allCards.Add(card);
                }
            }

            Debug.Log($"[CardDatabase] Berhasil menemukan dan mendaftarkan {allCards.Count} kartu.");

            // Tandai bahwa aset ini sudah berubah, agar Unity menyimpannya
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}