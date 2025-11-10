using UnityEngine;

namespace Littale
{
    [CreateAssetMenu(fileName = "New Passive Card", menuName = "LittaleData/Passive Card")]
    public class PassiveCardSO : ScriptableObject
    {

        [SerializeField] float multiplier;
        public float Multiplier => multiplier;

        [SerializeField] int level;  //Not meant to be modified in game [Only in Editor]
        public int Level { get => level; private set => level = value; }

        [SerializeField] GameObject nextLevelPrefab;  //The prefab of the next level i.e. what the object becomes when it levels up
                                                      //Not to be confused with the prefab to be spawned at the next level
        public GameObject NextLevelPrefab { get => nextLevelPrefab; private set => nextLevelPrefab = value; }

        [SerializeField]
        Sprite icon;    //Not meant to be modified in game [Only in Editor]
        public Sprite Icon { get => icon; private set => icon = value; }

    }
}