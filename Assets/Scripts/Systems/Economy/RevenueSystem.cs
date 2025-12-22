using System;
using System.Collections.Generic;
using UnityEngine;
using DominantK.Data;

namespace DominantK.Systems.Economy
{
    /// <summary>
    /// コンビニの収入計算システム
    /// 信仰度に基づいて一定間隔で収入を計算
    /// </summary>
    public class RevenueSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float revenueInterval = 5f;
        [SerializeField] private float faithToRevenueRate = 0.01f;
        [SerializeField] private float minimumFaithThreshold = 10f;

        [Header("Modifiers")]
        [SerializeField] private float peakHourBonus = 1.5f;
        [SerializeField] private int peakStartHour = 7;
        [SerializeField] private int peakEndHour = 9;
        [SerializeField] private int eveningPeakStart = 17;
        [SerializeField] private int eveningPeakEnd = 20;

        [Header("References")]
        [SerializeField] private FaithSystem faithSystem;

        // コンビニID -> 収入データ
        private Dictionary<int, StoreRevenueData> storeRevenueMap = new();

        // 施設ID -> 施設データ
        private Dictionary<int, FacilityRevenueData> facilityDataMap = new();

        private float timer;
        private int currentHour = 12;

        public event Action<int, int> OnRevenueGenerated; // storeId, revenue
        public event Action<int, int, int> OnPurchase; // facilityId, storeId, amount

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= revenueInterval)
            {
                timer = 0f;
                CalculateAllRevenue();
            }
        }

        /// <summary>
        /// コンビニを登録
        /// </summary>
        public void RegisterStore(int storeId, ConvenienceStoreData data, ChainType chain)
        {
            storeRevenueMap[storeId] = new StoreRevenueData
            {
                Id = storeId,
                Data = data,
                Chain = chain,
                TotalRevenue = 0,
                LastTermRevenue = 0
            };
        }

        /// <summary>
        /// コンビニを登録解除
        /// </summary>
        public void UnregisterStore(int storeId)
        {
            storeRevenueMap.Remove(storeId);
        }

        /// <summary>
        /// 施設を登録
        /// </summary>
        public void RegisterFacility(int facilityId, ResidentData residentData = null)
        {
            facilityDataMap[facilityId] = new FacilityRevenueData
            {
                Id = facilityId,
                SpendingPower = residentData?.spendingPower ?? 1f,
                LowCostPreference = residentData?.lowCostPreference ?? 0f
            };
        }

        /// <summary>
        /// 施設を登録解除
        /// </summary>
        public void UnregisterFacility(int facilityId)
        {
            facilityDataMap.Remove(facilityId);
        }

        /// <summary>
        /// 現在のゲーム内時間を設定
        /// </summary>
        public void SetCurrentHour(int hour)
        {
            currentHour = hour % 24;
        }

        private void CalculateAllRevenue()
        {
            if (faithSystem == null) return;

            // 各コンビニの今期収入をリセット
            foreach (var storeId in storeRevenueMap.Keys)
            {
                var data = storeRevenueMap[storeId];
                data.LastTermRevenue = 0;
                storeRevenueMap[storeId] = data;
            }

            // 各施設から各コンビニへの貢献を計算
            foreach (var facilityKv in facilityDataMap)
            {
                int facilityId = facilityKv.Key;
                var facilityData = facilityKv.Value;

                // この施設の全コンビニへの信仰度を取得
                var allFaith = faithSystem.GetAllFaith(facilityId);

                // 信仰度に基づいて購買行動を決定
                ProcessFacilityPurchases(facilityId, facilityData, allFaith);
            }

            // 収入イベントを発火
            foreach (var storeKv in storeRevenueMap)
            {
                if (storeKv.Value.LastTermRevenue > 0)
                {
                    OnRevenueGenerated?.Invoke(storeKv.Key, storeKv.Value.LastTermRevenue);
                }
            }
        }

        private void ProcessFacilityPurchases(int facilityId, FacilityRevenueData facilityData, Dictionary<int, float> allFaith)
        {
            if (allFaith.Count == 0) return;

            // 信仰度が閾値を超えるコンビニを選択
            var candidates = new List<(int storeId, float faith)>();
            float totalFaith = 0f;

            foreach (var kv in allFaith)
            {
                if (kv.Value >= minimumFaithThreshold)
                {
                    candidates.Add((kv.Key, kv.Value));
                    totalFaith += kv.Value;
                }
            }

            if (candidates.Count == 0 || totalFaith <= 0) return;

            // 信仰度に基づく確率で購買先を決定
            float roll = UnityEngine.Random.Range(0f, totalFaith);
            float cumulative = 0f;
            int selectedStore = candidates[0].storeId;

            foreach (var (storeId, faith) in candidates)
            {
                cumulative += faith;
                if (roll <= cumulative)
                {
                    selectedStore = storeId;
                    break;
                }
            }

            // 購買額を計算
            int purchaseAmount = CalculatePurchaseAmount(facilityId, facilityData, selectedStore);

            if (purchaseAmount > 0)
            {
                AddRevenue(selectedStore, purchaseAmount);
                OnPurchase?.Invoke(facilityId, selectedStore, purchaseAmount);
            }
        }

        private int CalculatePurchaseAmount(int facilityId, FacilityRevenueData facilityData, int storeId)
        {
            if (!storeRevenueMap.TryGetValue(storeId, out var storeData))
                return 0;

            // 基本収入
            float baseAmount = storeData.Data.baseRevenue;

            // 消費力による補正
            baseAmount *= facilityData.SpendingPower;

            // 顧客支出乗数
            baseAmount *= storeData.Data.customerSpendMultiplier;

            // ピーク時間ボーナス
            if (IsPeakHour())
            {
                baseAmount *= peakHourBonus;
            }

            // 低価格志向の施設は100円ローソンを好む
            if (facilityData.LowCostPreference > 0 && storeData.Chain == ChainType.Lawson100)
            {
                baseAmount *= 1f + facilityData.LowCostPreference;
            }

            // 信仰度に基づく追加購入
            float faith = faithSystem.GetFaith(facilityId, storeId);
            baseAmount *= 1f + (faith * faithToRevenueRate);

            return Mathf.RoundToInt(baseAmount);
        }

        private bool IsPeakHour()
        {
            return (currentHour >= peakStartHour && currentHour < peakEndHour) ||
                   (currentHour >= eveningPeakStart && currentHour < eveningPeakEnd);
        }

        private void AddRevenue(int storeId, int amount)
        {
            if (storeRevenueMap.TryGetValue(storeId, out var data))
            {
                data.TotalRevenue += amount;
                data.LastTermRevenue += amount;
                storeRevenueMap[storeId] = data;
            }
        }

        /// <summary>
        /// コンビニの累計収入を取得
        /// </summary>
        public int GetTotalRevenue(int storeId)
        {
            return storeRevenueMap.TryGetValue(storeId, out var data) ? data.TotalRevenue : 0;
        }

        /// <summary>
        /// コンビニの前回ターム収入を取得
        /// </summary>
        public int GetLastTermRevenue(int storeId)
        {
            return storeRevenueMap.TryGetValue(storeId, out var data) ? data.LastTermRevenue : 0;
        }

        /// <summary>
        /// 全コンビニの収入サマリーを取得
        /// </summary>
        public Dictionary<int, int> GetAllRevenueSummary()
        {
            var result = new Dictionary<int, int>();
            foreach (var kv in storeRevenueMap)
            {
                result[kv.Key] = kv.Value.TotalRevenue;
            }
            return result;
        }

        private struct StoreRevenueData
        {
            public int Id;
            public ConvenienceStoreData Data;
            public ChainType Chain;
            public int TotalRevenue;
            public int LastTermRevenue;
        }

        private struct FacilityRevenueData
        {
            public int Id;
            public float SpendingPower;
            public float LowCostPreference;
        }
    }
}
