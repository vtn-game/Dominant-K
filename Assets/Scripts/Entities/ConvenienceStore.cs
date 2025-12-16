using UnityEngine;
using DominantK.Data;
using DominantK.Core;

namespace DominantK.Entities
{
    public class ConvenienceStore : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private ConvenienceStoreData storeData;

        [Header("State")]
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private ChainType ownerChain;
        [SerializeField] private bool isPlayerOwned;
        [SerializeField] private float currentFaith;
        [SerializeField] private int dominantCount;

        [Header("Visual")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private GameObject zocVisualizer;

        public ConvenienceStoreData Data => storeData;
        public Vector2Int GridPosition => gridPosition;
        public ChainType OwnerChain => ownerChain;
        public bool IsPlayerOwned => isPlayerOwned;
        public float CurrentFaith => currentFaith;
        public int DominantCount => dominantCount;

        public void Initialize(ConvenienceStoreData data, Vector2Int position, ChainType owner, bool playerOwned)
        {
            storeData = data;
            gridPosition = position;
            ownerChain = owner;
            isPlayerOwned = playerOwned;
            currentFaith = 0f;
            dominantCount = 0;

            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            if (meshRenderer != null && storeData != null)
            {
                meshRenderer.material.color = storeData.chainColor;
            }
        }

        public void SetDominantCount(int count)
        {
            dominantCount = count;
        }

        public void AddFaith(float amount)
        {
            float effectiveGain = storeData.GetEffectiveFaithGain(dominantCount);
            currentFaith += amount * effectiveGain;
            currentFaith = Mathf.Clamp(currentFaith, 0f, 100f);
        }

        public int CalculateRevenue()
        {
            float spendMultiplier = storeData.GetEffectiveSpendMultiplier(dominantCount);
            return Mathf.RoundToInt(storeData.baseRevenue * spendMultiplier);
        }

        public float GetZOCRadius()
        {
            return storeData.zocRadius;
        }

        public float GetDominantRadius()
        {
            return storeData.dominantRadius;
        }

        public void ShowZOC(bool show)
        {
            if (zocVisualizer != null)
            {
                zocVisualizer.SetActive(show);
                if (show)
                {
                    float diameter = storeData.zocRadius * 2f;
                    zocVisualizer.transform.localScale = new Vector3(diameter, 0.1f, diameter);
                }
            }
        }

        public void OnStoreDestroyed()
        {
            Debug.Log($"{storeData.displayName} at {gridPosition} destroyed!");
            // Trigger destruction effects
        }
    }
}
