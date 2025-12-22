using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using DominantK.Data;

namespace DominantK.Systems.Economy
{
    /// <summary>
    /// 購買シミュレーションシステム
    /// 施設とコンビニの関係をシミュレートし、経済活動を統括
    /// </summary>
    public class PurchaseSimulationSystem : MonoBehaviour
    {
        [Header("Simulation Settings")]
        [SerializeField] private float simulationTickInterval = 1f;
        [SerializeField] private float faithInfluenceRadius = 30f;

        [Header("References")]
        [SerializeField] private FaithSystem faithSystem;
        [SerializeField] private RevenueSystem revenueSystem;

        // 登録されたエンティティ
        private Dictionary<int, StoreEntity> stores = new();
        private Dictionary<int, FacilityEntity> facilities = new();

        private int nextEntityId = 1;
        private float timer;

        public event Action<SimulationTickResult> OnSimulationTick;

        private void Awake()
        {
            if (faithSystem == null)
                faithSystem = GetComponent<FaithSystem>();
            if (revenueSystem == null)
                revenueSystem = GetComponent<RevenueSystem>();
        }

        private void Start()
        {
            // イベントを購読
            if (faithSystem != null)
            {
                faithSystem.OnFaithChanged += HandleFaithChanged;
            }
            if (revenueSystem != null)
            {
                revenueSystem.OnRevenueGenerated += HandleRevenueGenerated;
                revenueSystem.OnPurchase += HandlePurchase;
            }
        }

        private void OnDestroy()
        {
            if (faithSystem != null)
            {
                faithSystem.OnFaithChanged -= HandleFaithChanged;
            }
            if (revenueSystem != null)
            {
                revenueSystem.OnRevenueGenerated -= HandleRevenueGenerated;
                revenueSystem.OnPurchase -= HandlePurchase;
            }
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= simulationTickInterval)
            {
                timer = 0f;
                RunSimulationTick();
            }
        }

        /// <summary>
        /// コンビニを追加
        /// </summary>
        public int AddStore(Vector3 position, ConvenienceStoreData data, ChainType chain, bool isPlayerOwned)
        {
            int id = nextEntityId++;

            var store = new StoreEntity
            {
                Id = id,
                Position = position,
                Data = data,
                Chain = chain,
                IsPlayerOwned = isPlayerOwned,
                Quality = 50f,
                DominantCount = 0
            };

            stores[id] = store;

            // サブシステムに登録
            faithSystem?.RegisterStore(id, position, data, chain);
            revenueSystem?.RegisterStore(id, data, chain);

            // 周囲のコンビニ数を更新
            UpdateNearbyStoreCounts();

            return id;
        }

        /// <summary>
        /// コンビニを削除
        /// </summary>
        public void RemoveStore(int storeId)
        {
            stores.Remove(storeId);
            faithSystem?.UnregisterStore(storeId);
            revenueSystem?.UnregisterStore(storeId);

            UpdateNearbyStoreCounts();
        }

        /// <summary>
        /// 施設（住民）を追加
        /// </summary>
        public int AddFacility(Vector3 position, ResidentData residentData = null)
        {
            int id = nextEntityId++;

            var facility = new FacilityEntity
            {
                Id = id,
                Position = position,
                ResidentData = residentData,
                PreferredStoreId = -1
            };

            facilities[id] = facility;

            // サブシステムに登録
            faithSystem?.RegisterFacility(id, position, residentData);
            revenueSystem?.RegisterFacility(id, residentData);

            return id;
        }

        /// <summary>
        /// 施設を削除
        /// </summary>
        public void RemoveFacility(int facilityId)
        {
            facilities.Remove(facilityId);
            faithSystem?.UnregisterFacility(facilityId);
            revenueSystem?.UnregisterFacility(facilityId);
        }

        /// <summary>
        /// コンビニの品質を設定
        /// </summary>
        public void SetStoreQuality(int storeId, float quality)
        {
            if (stores.TryGetValue(storeId, out var store))
            {
                store.Quality = Mathf.Clamp(quality, 0f, 100f);
                stores[storeId] = store;
                faithSystem?.UpdateStoreQuality(storeId, quality);
            }
        }

        private void UpdateNearbyStoreCounts()
        {
            foreach (var storeKv in stores)
            {
                var store = storeKv.Value;
                int count = 0;

                foreach (var otherStore in stores.Values)
                {
                    if (otherStore.Id != store.Id)
                    {
                        float dist = Vector3.Distance(store.Position, otherStore.Position);
                        if (dist <= faithInfluenceRadius)
                        {
                            count++;
                        }
                    }
                }

                store.DominantCount = count;
                stores[storeKv.Key] = store;
            }
        }

        private void RunSimulationTick()
        {
            var result = new SimulationTickResult();

            // 各施設の状態を更新
            foreach (var facilityKv in facilities)
            {
                var facility = facilityKv.Value;

                // 最も信仰しているコンビニを更新
                int preferredStore = faithSystem?.GetMostFaithfulStore(facility.Id) ?? -1;
                facility.PreferredStoreId = preferredStore;
                facilities[facilityKv.Key] = facility;

                if (preferredStore >= 0)
                {
                    result.FacilityPreferences[facility.Id] = preferredStore;
                }
            }

            // 各コンビニの統計を集計
            foreach (var storeKv in stores)
            {
                var stats = new StoreStats
                {
                    TotalRevenue = revenueSystem?.GetTotalRevenue(storeKv.Key) ?? 0,
                    LastTermRevenue = revenueSystem?.GetLastTermRevenue(storeKv.Key) ?? 0,
                    DominantCount = storeKv.Value.DominantCount,
                    FaithfulFacilityCount = CountFaithfulFacilities(storeKv.Key)
                };

                result.StoreStats[storeKv.Key] = stats;
            }

            OnSimulationTick?.Invoke(result);
        }

        private int CountFaithfulFacilities(int storeId)
        {
            int count = 0;
            foreach (var facility in facilities.Values)
            {
                if (facility.PreferredStoreId == storeId)
                {
                    count++;
                }
            }
            return count;
        }

        private void HandleFaithChanged(int facilityId, int storeId, float newFaith)
        {
            // 信仰度変化時の処理（デバッグログなど）
        }

        private void HandleRevenueGenerated(int storeId, int revenue)
        {
            // 収入発生時の処理
            Debug.Log($"Store {storeId} generated revenue: {revenue}");
        }

        private void HandlePurchase(int facilityId, int storeId, int amount)
        {
            // 購買発生時の処理
        }

        /// <summary>
        /// コンビニ情報を取得
        /// </summary>
        public StoreEntity? GetStore(int storeId)
        {
            return stores.TryGetValue(storeId, out var store) ? store : null;
        }

        /// <summary>
        /// 施設情報を取得
        /// </summary>
        public FacilityEntity? GetFacility(int facilityId)
        {
            return facilities.TryGetValue(facilityId, out var facility) ? facility : null;
        }

        /// <summary>
        /// 全コンビニを取得
        /// </summary>
        public IEnumerable<StoreEntity> GetAllStores() => stores.Values;

        /// <summary>
        /// 全施設を取得
        /// </summary>
        public IEnumerable<FacilityEntity> GetAllFacilities() => facilities.Values;

        /// <summary>
        /// 指定チェーンのコンビニを取得
        /// </summary>
        public List<StoreEntity> GetStoresByChain(ChainType chain)
        {
            var result = new List<StoreEntity>();
            foreach (var store in stores.Values)
            {
                if (store.Chain == chain)
                {
                    result.Add(store);
                }
            }
            return result;
        }

        /// <summary>
        /// プレイヤー所有のコンビニを取得
        /// </summary>
        public List<StoreEntity> GetPlayerStores()
        {
            var result = new List<StoreEntity>();
            foreach (var store in stores.Values)
            {
                if (store.IsPlayerOwned)
                {
                    result.Add(store);
                }
            }
            return result;
        }
    }

    /// <summary>
    /// コンビニエンティティ
    /// </summary>
    [Serializable]
    public struct StoreEntity
    {
        public int Id;
        public Vector3 Position;
        public ConvenienceStoreData Data;
        public ChainType Chain;
        public bool IsPlayerOwned;
        public float Quality;
        public int DominantCount;
    }

    /// <summary>
    /// 施設エンティティ
    /// </summary>
    [Serializable]
    public struct FacilityEntity
    {
        public int Id;
        public Vector3 Position;
        public ResidentData ResidentData;
        public int PreferredStoreId;
    }

    /// <summary>
    /// シミュレーションティック結果
    /// </summary>
    public class SimulationTickResult
    {
        public Dictionary<int, int> FacilityPreferences = new();
        public Dictionary<int, StoreStats> StoreStats = new();
    }

    /// <summary>
    /// コンビニ統計
    /// </summary>
    public struct StoreStats
    {
        public int TotalRevenue;
        public int LastTermRevenue;
        public int DominantCount;
        public int FaithfulFacilityCount;
    }
}
