using UnityEngine;

namespace DominantK.Data
{
    public enum ChainType
    {
        SevenEleban,
        Lawson,
        Lawson100,
        NaturalLawson,
        Famoma
    }

    [CreateAssetMenu(fileName = "NewConvenienceStore", menuName = "DominantK/Convenience Store Data")]
    public class ConvenienceStoreData : ScriptableObject
    {
        [Header("Basic Info")]
        public string id;
        public string displayName;
        public ChainType chainType;
        public Sprite icon;
        public GameObject prefab;

        [Header("Cost")]
        public int buildCost = 100;

        [Header("Zone of Control")]
        public float zocRadius = 3f;
        public float dominantRadius = 4f;

        [Header("Revenue")]
        public int baseRevenue = 10;
        public float customerSpendMultiplier = 1f;

        [Header("Faith")]
        public float baseFaithGain = 1f;

        [Header("Special Effects")]
        [Tooltip("SevenEleban: Corruption rate per dominant count")]
        public float corruptionRate = 0.05f;
        [Tooltip("SevenEleban: Minimum corruption multiplier")]
        public float minCorruptionMultiplier = 0.3f;

        [Tooltip("Famoma: Brainwash speed bonus")]
        public float brainwashSpeedBonus = 0.3f;
        [Tooltip("Famoma: Movement suppression rate")]
        public float movementSuppression = 0.2f;

        [Header("Visual")]
        public Color chainColor = Color.white;

        /// <summary>
        /// Calculate the corruption multiplier for SevenEleban based on dominant count
        /// </summary>
        public float GetCorruptionMultiplier(int dominantCount)
        {
            if (chainType != ChainType.SevenEleban)
                return 1f;

            float multiplier = 1f - (dominantCount * corruptionRate);
            return Mathf.Max(multiplier, minCorruptionMultiplier);
        }

        /// <summary>
        /// Get effective customer spend multiplier
        /// </summary>
        public float GetEffectiveSpendMultiplier(int dominantCount)
        {
            return customerSpendMultiplier * GetCorruptionMultiplier(dominantCount);
        }

        /// <summary>
        /// Get effective faith gain
        /// </summary>
        public float GetEffectiveFaithGain(int dominantCount)
        {
            return baseFaithGain * GetCorruptionMultiplier(dominantCount);
        }
    }
}
