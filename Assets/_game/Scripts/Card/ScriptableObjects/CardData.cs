using System.Collections.Generic;
using UnityEngine;

namespace Littale {
    public enum CardType {
        Anchor,    // Main Attack (Auto, VS Style)
        Revolver,  // Skill Cards (Active, Mana, 2 Slots)
        Ultimate,  // Signature Move (High Cooldown)
        Artifact   // Passive (Always On)
    }

    [CreateAssetMenu(fileName = "New Card", menuName = "Littale/Card Data")]
    public class CardData : ScriptableObject {
        [Header("Identity")]
        public string cardName;
        public Sprite icon;
        public CardType type;
        [TextArea] public string description;

        [Header("Stats")]
        public float cooldown;      // Untuk Anchor & Ultimate
        public int manaCost;        // Khusus tipe Revolver (Skill)

        [Header("Mekanik Serangan (Opsional)")]
        // Kosongkan jika ini kartu Artifact/Pasif
        public GameObject projectilePrefab;

        [Header("Daftar Efek")]
        // Jika ada Projectile -> Efek ini dijalankan saat 'Hit' musuh
        // Jika Artifact -> Efek ini dijalankan saat 'Start' game
        public List<CardEffect> effects;
    }
}