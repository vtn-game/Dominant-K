using System.Collections.Generic;
using UnityEngine;
using DominantK.Data;
using DominantK.Systems;

namespace DominantK.AI
{
    /// <summary>
    /// 複数のAIプレイヤーを管理
    /// </summary>
    public class AIManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool autoSpawnAI = true;
        [SerializeField] private List<AIPlayerConfig> aiConfigs = new List<AIPlayerConfig>();

        [Header("References")]
        [SerializeField] private GridSystem gridSystem;
        [SerializeField] private PlacementSystem placementSystem;
        [SerializeField] private DominantSystem dominantSystem;

        private List<AutoPlayer> activePlayers = new List<AutoPlayer>();

        public IReadOnlyList<AutoPlayer> ActivePlayers => activePlayers;

        private void Start()
        {
            if (autoSpawnAI && aiConfigs.Count > 0)
            {
                SpawnAllAI();
            }
        }

        /// <summary>
        /// 全AIをスポーン
        /// </summary>
        public void SpawnAllAI()
        {
            foreach (var config in aiConfigs)
            {
                SpawnAI(config);
            }
        }

        /// <summary>
        /// AIをスポーン
        /// </summary>
        public AutoPlayer SpawnAI(AIPlayerConfig config)
        {
            var aiGO = new GameObject($"AI_{config.chain}");
            aiGO.transform.SetParent(transform);

            var autoPlayer = aiGO.AddComponent<AutoPlayer>();

            // 設定を反映（SerializeFieldなのでリフレクションで設定）
            SetPrivateField(autoPlayer, "aiChain", config.chain);
            SetPrivateField(autoPlayer, "difficulty", config.difficulty);
            SetPrivateField(autoPlayer, "startingMoney", config.startingMoney);
            SetPrivateField(autoPlayer, "storeCost", config.storeCost);
            SetPrivateField(autoPlayer, "decisionInterval", config.decisionInterval);
            SetPrivateField(autoPlayer, "randomPlacementChance", config.randomPlacementChance);

            autoPlayer.Initialize(gridSystem, placementSystem, dominantSystem);
            autoPlayer.OnPlacement += OnAIPlacement;

            activePlayers.Add(autoPlayer);
            Debug.Log($"Spawned AI: {config.chain} with difficulty {config.difficulty}");

            return autoPlayer;
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }

        /// <summary>
        /// 特定のAIを停止
        /// </summary>
        public void StopAI(ChainType chain)
        {
            foreach (var player in activePlayers)
            {
                if (player.AIChain == chain)
                {
                    player.Stop();
                }
            }
        }

        /// <summary>
        /// 全AIを停止
        /// </summary>
        public void StopAllAI()
        {
            foreach (var player in activePlayers)
            {
                player.Stop();
            }
        }

        /// <summary>
        /// AIに収入を分配
        /// </summary>
        public void DistributeIncome(ChainType chain, int amount)
        {
            foreach (var player in activePlayers)
            {
                if (player.AIChain == chain)
                {
                    player.AddMoney(amount);
                }
            }
        }

        private void OnAIPlacement(PlacementAction action)
        {
            // AI配置時の処理（UI更新、エフェクトなど）
            Debug.Log($"AI placed at {action.GridPosition}");
        }

        /// <summary>
        /// AIの統計を取得
        /// </summary>
        public Dictionary<ChainType, AIStats> GetAIStats()
        {
            var stats = new Dictionary<ChainType, AIStats>();

            foreach (var player in activePlayers)
            {
                stats[player.AIChain] = new AIStats
                {
                    Chain = player.AIChain,
                    Money = player.CurrentMoney,
                    IsActive = player.IsActive
                };
            }

            return stats;
        }
    }

    /// <summary>
    /// AIプレイヤーの設定
    /// </summary>
    [System.Serializable]
    public class AIPlayerConfig
    {
        public ChainType chain = ChainType.Lawson;
        public AIDifficulty difficulty = AIDifficulty.Normal;
        public int startingMoney = 500;
        public int storeCost = 100;
        public float decisionInterval = 3f;
        [Range(0f, 1f)]
        public float randomPlacementChance = 0.15f;
    }

    /// <summary>
    /// AI統計
    /// </summary>
    public struct AIStats
    {
        public ChainType Chain;
        public int Money;
        public bool IsActive;
    }
}
