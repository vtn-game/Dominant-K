using System;
using System.Collections.Generic;
using UnityEngine;
using DominantK.Data;

namespace DominantK.Systems.Economy
{
    /// <summary>
    /// 経済システム全体を統括するマネージャー
    /// FaithSystem, RevenueSystem, PurchaseSimulationSystemを統合管理
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private FaithSystem faithSystem;
        [SerializeField] private RevenueSystem revenueSystem;
        [SerializeField] private PurchaseSimulationSystem simulationSystem;

        [Header("Player Data")]
        [SerializeField] private int playerMoney = 1000;
        [SerializeField] private ChainType playerChain = ChainType.SevenEleban;

        [Header("Settings")]
        [SerializeField] private float incomeCollectionInterval = 10f;

        private float incomeTimer;

        public int PlayerMoney => playerMoney;
        public ChainType PlayerChain => playerChain;

        public event Action<int> OnPlayerMoneyChanged;
        public event Action<int, int> OnIncomeCollected; // storeId, amount

        private void Awake()
        {
            // サブシステムがない場合は作成
            if (faithSystem == null)
                faithSystem = gameObject.AddComponent<FaithSystem>();
            if (revenueSystem == null)
                revenueSystem = gameObject.AddComponent<RevenueSystem>();
            if (simulationSystem == null)
                simulationSystem = gameObject.AddComponent<PurchaseSimulationSystem>();
        }

        private void Start()
        {
            // シミュレーションティックを購読
            simulationSystem.OnSimulationTick += HandleSimulationTick;
            revenueSystem.OnRevenueGenerated += HandleRevenueGenerated;
        }

        private void OnDestroy()
        {
            if (simulationSystem != null)
                simulationSystem.OnSimulationTick -= HandleSimulationTick;
            if (revenueSystem != null)
                revenueSystem.OnRevenueGenerated -= HandleRevenueGenerated;
        }

        private void Update()
        {
            incomeTimer += Time.deltaTime;
            if (incomeTimer >= incomeCollectionInterval)
            {
                incomeTimer = 0f;
                CollectPlayerIncome();
            }
        }

        /// <summary>
        /// プレイヤーの店舗から収入を回収
        /// </summary>
        private void CollectPlayerIncome()
        {
            var playerStores = simulationSystem.GetPlayerStores();
            int totalIncome = 0;

            foreach (var store in playerStores)
            {
                int income = revenueSystem.GetLastTermRevenue(store.Id);
                if (income > 0)
                {
                    totalIncome += income;
                    OnIncomeCollected?.Invoke(store.Id, income);
                }
            }

            if (totalIncome > 0)
            {
                AddMoney(totalIncome);
                Debug.Log($"Collected income: {totalIncome} from {playerStores.Count} stores");
            }
        }

        /// <summary>
        /// コンビニを建設
        /// </summary>
        public int BuildStore(Vector3 position, ConvenienceStoreData data, bool isPlayerOwned)
        {
            ChainType chain = data.chainType;

            if (isPlayerOwned)
            {
                if (playerMoney < data.buildCost)
                {
                    Debug.LogWarning("Not enough money to build store");
                    return -1;
                }
                SpendMoney(data.buildCost);
            }

            return simulationSystem.AddStore(position, data, chain, isPlayerOwned);
        }

        /// <summary>
        /// コンビニを撤去
        /// </summary>
        public void DestroyStore(int storeId)
        {
            simulationSystem.RemoveStore(storeId);
        }

        /// <summary>
        /// 施設を追加
        /// </summary>
        public int AddFacility(Vector3 position, ResidentData residentData = null)
        {
            return simulationSystem.AddFacility(position, residentData);
        }

        /// <summary>
        /// 施設を削除
        /// </summary>
        public void RemoveFacility(int facilityId)
        {
            simulationSystem.RemoveFacility(facilityId);
        }

        /// <summary>
        /// お金を追加
        /// </summary>
        public void AddMoney(int amount)
        {
            playerMoney += amount;
            OnPlayerMoneyChanged?.Invoke(playerMoney);
        }

        /// <summary>
        /// お金を消費
        /// </summary>
        public bool SpendMoney(int amount)
        {
            if (playerMoney < amount) return false;
            playerMoney -= amount;
            OnPlayerMoneyChanged?.Invoke(playerMoney);
            return true;
        }

        /// <summary>
        /// コンビニの品質をアップグレード
        /// </summary>
        public bool UpgradeStoreQuality(int storeId, float qualityIncrease, int cost)
        {
            var store = simulationSystem.GetStore(storeId);
            if (!store.HasValue) return false;

            if (!store.Value.IsPlayerOwned)
            {
                Debug.LogWarning("Cannot upgrade enemy store");
                return false;
            }

            if (!SpendMoney(cost)) return false;

            float newQuality = store.Value.Quality + qualityIncrease;
            simulationSystem.SetStoreQuality(storeId, newQuality);
            return true;
        }

        /// <summary>
        /// 施設のコンビニへの信仰度を取得
        /// </summary>
        public float GetFaith(int facilityId, int storeId)
        {
            return faithSystem.GetFaith(facilityId, storeId);
        }

        /// <summary>
        /// コンビニの収入を取得
        /// </summary>
        public int GetStoreRevenue(int storeId)
        {
            return revenueSystem.GetTotalRevenue(storeId);
        }

        /// <summary>
        /// 全コンビニの統計を取得
        /// </summary>
        public Dictionary<int, StoreStats> GetAllStoreStats()
        {
            var result = new Dictionary<int, StoreStats>();
            foreach (var store in simulationSystem.GetAllStores())
            {
                result[store.Id] = new StoreStats
                {
                    TotalRevenue = revenueSystem.GetTotalRevenue(store.Id),
                    LastTermRevenue = revenueSystem.GetLastTermRevenue(store.Id),
                    DominantCount = store.DominantCount
                };
            }
            return result;
        }

        /// <summary>
        /// プレイヤーの総収入を取得
        /// </summary>
        public int GetPlayerTotalRevenue()
        {
            int total = 0;
            foreach (var store in simulationSystem.GetPlayerStores())
            {
                total += revenueSystem.GetTotalRevenue(store.Id);
            }
            return total;
        }

        /// <summary>
        /// チェーン別の支配状況を取得
        /// </summary>
        public Dictionary<ChainType, ChainStats> GetChainStats()
        {
            var stats = new Dictionary<ChainType, ChainStats>();

            foreach (ChainType chain in Enum.GetValues(typeof(ChainType)))
            {
                stats[chain] = new ChainStats();
            }

            foreach (var store in simulationSystem.GetAllStores())
            {
                var chainStat = stats[store.Chain];
                chainStat.StoreCount++;
                chainStat.TotalRevenue += revenueSystem.GetTotalRevenue(store.Id);
                stats[store.Chain] = chainStat;
            }

            // 信仰している施設数を集計
            foreach (var facility in simulationSystem.GetAllFacilities())
            {
                if (facility.PreferredStoreId >= 0)
                {
                    var store = simulationSystem.GetStore(facility.PreferredStoreId);
                    if (store.HasValue)
                    {
                        var chainStat = stats[store.Value.Chain];
                        chainStat.FaithfulFacilityCount++;
                        stats[store.Value.Chain] = chainStat;
                    }
                }
            }

            return stats;
        }

        private void HandleSimulationTick(SimulationTickResult result)
        {
            // シミュレーション結果の処理
        }

        private void HandleRevenueGenerated(int storeId, int revenue)
        {
            // 収入発生時の処理
        }

        /// <summary>
        /// ゲーム内時間を設定
        /// </summary>
        public void SetGameHour(int hour)
        {
            revenueSystem.SetCurrentHour(hour);
        }
    }

    /// <summary>
    /// チェーン別統計
    /// </summary>
    [Serializable]
    public struct ChainStats
    {
        public int StoreCount;
        public int TotalRevenue;
        public int FaithfulFacilityCount;
    }
}
