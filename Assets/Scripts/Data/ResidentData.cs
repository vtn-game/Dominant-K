using UnityEngine;

namespace DominantK.Data
{
    public enum ResidentType
    {
        Salaryman,
        Student,
        Homemaker,
        Elder,
        Engineer,
        IndianResident
    }

    [CreateAssetMenu(fileName = "NewResident", menuName = "DominantK/Resident Data")]
    public class ResidentData : ScriptableObject
    {
        [Header("Basic Info")]
        public string id;
        public string displayName;
        public ResidentType residentType;
        public Sprite icon;
        public GameObject prefab;

        [Header("Movement")]
        public float moveSpeed = 1f;

        [Header("Economy")]
        [Tooltip("Contribution to revenue when visiting a store")]
        public float spendingPower = 1f;

        [Header("Faith")]
        [Tooltip("Higher values make this resident harder to brainwash")]
        public float faithResistance = 1f;

        [Header("Spawn")]
        [Tooltip("Relative spawn weight (higher = more common)")]
        public int spawnWeight = 10;

        [Header("Time-based Spawn Bonus")]
        public bool hasTimeBonus;
        public int peakStartHour;
        public int peakEndHour;
        public float peakSpawnMultiplier = 2f;

        [Header("Special Effects")]
        [Tooltip("Student: Reduced chain switching rate")]
        public float chainLoyaltyBonus = 0f;

        [Tooltip("Homemaker: Preference for low-cost chains")]
        public float lowCostPreference = 0f;

        [Tooltip("Elder: Lock faith when maxed")]
        public bool lockFaithWhenMaxed = false;

        [Tooltip("Engineer: Sound wave resistance")]
        public float soundWaveResistance = 0f;

        [Tooltip("Indian: Community faith bonus when near same type")]
        public float communityFaithBonus = 0f;

        [Header("Visual")]
        public Color residentColor = Color.white;
    }
}
