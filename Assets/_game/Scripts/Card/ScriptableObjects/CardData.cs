using System.Collections.Generic;
using UnityEngine;

namespace Littale {
    public enum CardType {
        Anchor,    // Main Attack (Auto, VS Style)
        Revolver,  // Skill Cards (Active, Mana, 2 Slots)
        Ultimate,  // Signature Move (High Cooldown)
        Artifact   // Passive (Always On)
    }

    public enum CardTier { Bronze, Silver, Gold }

    [CreateAssetMenu(fileName = "New Card", menuName = "Littale/Card Data")]
    public class CardData : ScriptableObject {
        [Header("Identity")]
        public string cardName;
        public Sprite icon;
        public CardType type;
        public CardTier tier;
        public CardData nextTierCard; // Reference to the next tier version
        [TextArea] public string description;

        [Header("Stats Base (Bronze)")]
        public float cooldown;      // Untuk Anchor & Ultimate
        public int manaCost;        // Khusus tipe Revolver (Skill)
        [Tooltip("Percentage of Player Base ATK (e.g., 1.0 = 100%)")]
        public float damageMultiplier = 1.0f; 
        [Tooltip("Force applied to enemies (Force - Weight)")]
        public float knockbackForce = 1.0f;

        [Header("Mekanik Serangan (Opsional)")]
        // Kosongkan jika ini kartu Artifact/Pasif
        public GameObject projectilePrefab; // Jika ada Projectile
        public GameObject secondaryPrefab; // Misal: Area Effect, Trap, dll.

        [Header("Daftar Efek")]
        // Jika ada Projectile -> Efek ini dijalankan saat 'Hit' musuh
        // Jika Artifact -> Efek ini dijalankan saat 'Start' game
        public List<CardEffect> effects;
    }
}