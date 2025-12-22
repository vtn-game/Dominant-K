using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using DominantK.Data;

namespace DominantK.Systems.Economy
{
    /// <summary>
    /// 施設（住民）がコンビニに対して持つ信仰度を管理するシステム
    /// </summary>
    public class FaithSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float faithUpdateInterval = 1f;
        [SerializeField] private float maxFaith = 100f;
        [SerializeField] private float baseFaithChangeRate = 5f;

        [Header("Faith Modifiers")]
        [SerializeField] private float qualityWeight = 0.4f;
        [SerializeField] private float competitionWeight = 0.3f;
        [SerializeField] private float randomWeight = 0.3f;
        [SerializeField] private float distanceDecayFactor = 0.1f;

        // 施設ID -> (コンビニID -> 信仰度)
        private Dictionary<int, Dictionary<int, FaithData>> facilityFaithMap = new();

        // 登録されたコンビニ
        private Dictionary<int, StoreInfo> registeredStores = new();

        // 登録された施設
        private Dictionary<int, FacilityInfo> registeredFacilities = new();

        private float timer;
        private System.Random random;

        public event Action<int, int, float> OnFaithChanged; // facilityId, storeId, newFaith

        private void Awake()
        {
            random = new System.Random();
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= faithUpdateInterval)
            {
                timer = 0f;
                UpdateAllFaith();
            }
        }

        /// <summary>
        /// コンビニを登録
        /// </summary>
        public void RegisterStore(int storeId, Vector3 position, ConvenienceStoreData data, ChainType chain)
        {
            registeredStores[storeId] = new StoreInfo
            {
                Id = storeId,
                Position = position,
                Data = data,
                Chain = chain,
                Quality = CalculateBaseQuality(data)
            };

            // 既存の施設に対してこのコンビニへの信仰度を初期化
            foreach (var facility in registeredFacilities.Values)
            {
                InitializeFaithForStore(facility.Id, storeId, facility.Position, position);
            }
        }

        /// <summary>
        /// コンビニを登録解除
        /// </summary>
        public void UnregisterStore(int storeId)
        {
            registeredStores.Remove(storeId);

            // 全施設からこのコンビニへの信仰度を削除
            foreach (var facilityFaith in facilityFaithMap.Values)
            {
                facilityFaith.Remove(storeId);
            }
        }

        /// <summary>
        /// 施設（住民）を登録
        /// </summary>
        public void RegisterFacility(int facilityId, Vector3 position, ResidentData residentData = null)
        {
            registeredFacilities[facilityId] = new FacilityInfo
            {
                Id = facilityId,
                Position = position,
                ResidentData = residentData,
                SpendingPower = residentData?.spendingPower ?? 1f,
                FaithResistance = residentData?.faithResistance ?? 1f
            };

            facilityFaithMap[facilityId] = new Dictionary<int, FaithData>();

            // 全コンビニに対する信仰度を初期化
            foreach (var store in registeredStores.Values)
            {
                InitializeFaithForStore(facilityId, store.Id, position, store.Position);
            }
        }

        /// <summary>
        /// 施設を登録解除
        /// </summary>
        public void UnregisterFacility(int facilityId)
        {
            registeredFacilities.Remove(facilityId);
            facilityFaithMap.Remove(facilityId);
        }

        private void InitializeFaithForStore(int facilityId, int storeId, Vector3 facilityPos, Vector3 storePos)
        {
            if (!facilityFaithMap.ContainsKey(facilityId))
            {
                facilityFaithMap[facilityId] = new Dictionary<int, FaithData>();
            }

            float distance = Vector3.Distance(facilityPos, storePos);
            float initialFaith = CalculateInitialFaith(distance);

            facilityFaithMap[facilityId][storeId] = new FaithData
            {
                CurrentFaith = initialFaith,
                Distance = distance
            };
        }

        private float CalculateInitialFaith(float distance)
        {
            // 距離に応じて初期信仰度を設定（近いほど高い）
            float normalizedDistance = Mathf.Clamp01(distance / 50f);
            return maxFaith * 0.5f * (1f - normalizedDistance * 0.5f);
        }

        private float CalculateBaseQuality(ConvenienceStoreData data)
        {
            // コンビニの品質スコアを計算
            float quality = 50f; // 基本値

            quality += data.baseFaithGain * 10f;
            quality += data.customerSpendMultiplier * 20f;

            // チェーン固有ボーナス
            quality += data.chainType switch
            {
                ChainType.SevenEleban => 15f,
                ChainType.Lawson => 10f,
                ChainType.NaturalLawson => 20f,
                ChainType.Lawson100 => -5f, // 安さ重視で品質やや低め
                ChainType.Famoma => 5f,
                _ => 0f
            };

            return Mathf.Clamp(quality, 0f, 100f);
        }

        private void UpdateAllFaith()
        {
            foreach (var facilityKv in facilityFaithMap)
            {
                int facilityId = facilityKv.Key;
                if (!registeredFacilities.TryGetValue(facilityId, out var facility))
                    continue;

                var faithMap = facilityKv.Value;
                int nearbyStoreCount = CountNearbyStores(facility.Position, 30f);

                foreach (var storeId in new List<int>(faithMap.Keys))
                {
                    if (!registeredStores.TryGetValue(storeId, out var store))
                        continue;

                    var faithData = faithMap[storeId];
                    float faithChange = CalculateFaithChange(facility, store, faithData, nearbyStoreCount);

                    faithData.CurrentFaith = Mathf.Clamp(
                        faithData.CurrentFaith + faithChange,
                        0f,
                        maxFaith
                    );

                    faithMap[storeId] = faithData;
                    OnFaithChanged?.Invoke(facilityId, storeId, faithData.CurrentFaith);
                }
            }
        }

        private float CalculateFaithChange(FacilityInfo facility, StoreInfo store, FaithData faithData, int nearbyStoreCount)
        {
            // 1. 品質による影響
            float qualityFactor = (store.Quality - 50f) / 50f; // -1 ~ 1

            // 2. 競合による影響（周囲のコンビニが多いと分散）
            float competitionFactor = nearbyStoreCount > 1
                ? -0.5f * (nearbyStoreCount - 1) / 10f
                : 0.1f;

            // 3. 距離による減衰
            float distanceFactor = 1f / (1f + faithData.Distance * distanceDecayFactor);

            // 4. ランダム要素
            float randomFactor = (float)(random.NextDouble() * 2 - 1); // -1 ~ 1

            // 5. 住民の信仰抵抗
            float resistanceFactor = 1f / facility.FaithResistance;

            // 総合計算
            float change = baseFaithChangeRate * (
                qualityWeight * qualityFactor +
                competitionWeight * competitionFactor +
                randomWeight * randomFactor
            ) * distanceFactor * resistanceFactor;

            return change;
        }

        private int CountNearbyStores(Vector3 position, float radius)
        {
            int count = 0;
            foreach (var store in registeredStores.Values)
            {
                if (Vector3.Distance(position, store.Position) <= radius)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// 施設の特定コンビニへの信仰度を取得
        /// </summary>
        public float GetFaith(int facilityId, int storeId)
        {
            if (facilityFaithMap.TryGetValue(facilityId, out var faithMap))
            {
                if (faithMap.TryGetValue(storeId, out var faithData))
                {
                    return faithData.CurrentFaith;
                }
            }
            return 0f;
        }

        /// <summary>
        /// 施設の全コンビニへの信仰度を取得
        /// </summary>
        public Dictionary<int, float> GetAllFaith(int facilityId)
        {
            var result = new Dictionary<int, float>();
            if (facilityFaithMap.TryGetValue(facilityId, out var faithMap))
            {
                foreach (var kv in faithMap)
                {
                    result[kv.Key] = kv.Value.CurrentFaith;
                }
            }
            return result;
        }

        /// <summary>
        /// 施設が最も信仰しているコンビニを取得
        /// </summary>
        public int GetMostFaithfulStore(int facilityId)
        {
            int bestStoreId = -1;
            float maxFaithValue = 0f;

            if (facilityFaithMap.TryGetValue(facilityId, out var faithMap))
            {
                foreach (var kv in faithMap)
                {
                    if (kv.Value.CurrentFaith > maxFaithValue)
                    {
                        maxFaithValue = kv.Value.CurrentFaith;
                        bestStoreId = kv.Key;
                    }
                }
            }
            return bestStoreId;
        }

        /// <summary>
        /// コンビニの品質を更新
        /// </summary>
        public void UpdateStoreQuality(int storeId, float newQuality)
        {
            if (registeredStores.TryGetValue(storeId, out var store))
            {
                store.Quality = Mathf.Clamp(newQuality, 0f, 100f);
                registeredStores[storeId] = store;
            }
        }

        /// <summary>
        /// 信仰度データ
        /// </summary>
        private struct FaithData
        {
            public float CurrentFaith;
            public float Distance;
        }

        /// <summary>
        /// コンビニ情報
        /// </summary>
        private struct StoreInfo
        {
            public int Id;
            public Vector3 Position;
            public ConvenienceStoreData Data;
            public ChainType Chain;
            public float Quality;
        }

        /// <summary>
        /// 施設情報
        /// </summary>
        private struct FacilityInfo
        {
            public int Id;
            public Vector3 Position;
            public ResidentData ResidentData;
            public float SpendingPower;
            public float FaithResistance;
        }
    }
}
