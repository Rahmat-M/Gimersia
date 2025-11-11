using System.Collections.Generic;
using UnityEngine;

namespace Littale {
    [CreateAssetMenu(fileName = "New Character", menuName = "Littale Data/Character")]
    public class CharacterSO : ScriptableObject {

        [Header("Character Info")]
        [SerializeField] new string name;
        public string Name { get => name; private set => name = value; }

        [SerializeField] string fullName;
        public string FullName { get => fullName; private set => fullName = value; }

        [SerializeField, TextArea] string characterDescription;
        public string CharacterDescription { get => characterDescription; private set => characterDescription = value; }

        [Header("Stats")]
        [SerializeField] List<CardController> startingCards;
        public List<CardController> StartingCards { get => startingCards; }

        [System.Serializable]
        public struct Stats {
            public float maxHealth, recovery, armor;
            [Range(-1, 10)] public float moveSpeed, might;
            [Range(-1, 5)] public float attackSpeed, projectileSpeed;
            [Min(-1)] public float luck;
            public float magnet;
            [Range(-1, 8)] public int handSize;

            public static Stats operator +(Stats s1, Stats s2) {
                s1.maxHealth += s2.maxHealth;
                s1.recovery += s2.recovery;
                s1.armor += s2.armor;
                s1.moveSpeed += s2.moveSpeed;
                s1.might += s2.might;
                s1.attackSpeed += s2.attackSpeed;
                s1.projectileSpeed += s2.projectileSpeed;
                s1.luck += s2.luck;
                s1.magnet += s2.magnet;
                s1.handSize += s2.handSize;
                return s1;
            }

            public static Stats operator *(Stats s1, Stats s2) {
                s1.maxHealth *= s2.maxHealth;
                s1.recovery *= s2.recovery;
                s1.armor *= s2.armor;
                s1.moveSpeed *= s2.moveSpeed;
                s1.might *= s2.might;
                s1.attackSpeed *= s2.attackSpeed;
                s1.projectileSpeed *= s2.projectileSpeed;
                s1.luck *= s2.luck;
                s1.magnet *= s2.magnet;
                s1.handSize = Mathf.RoundToInt(s1.handSize * s2.handSize);
                return s1;
            }
        }

        public Stats stats = new Stats {
            maxHealth = 100f,
            recovery = 0f,
            armor = 0f,
            moveSpeed = 1f,
            might = 1f,
            attackSpeed = 1f,
            projectileSpeed = 2f,
            luck = 1f,
            magnet = 1f,
            handSize = 3
        };
    }
}