using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using DominantK.Data;
using DominantK.Systems;

namespace DominantK.AI
{
    /// <summary>
    /// 自動でコンビニを配置するAIプレイヤー
    /// ドミナント戦略に従いつつ、たまにランダム配置も行う
    /// </summary>
    public class AutoPlayer : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private ChainType aiChain = ChainType.Lawson;
        [SerializeField] private float decisionInterval = 3f;
        [SerializeField] private int startingMoney = 500;
        [SerializeField] private int storeCost = 100;

        [Header("Strategy Settings")]
        [SerializeField, Range(0f, 1f)] private float randomPlacementChance = 0.15f;
        [SerializeField] private int mctsIterations = 500;
        [SerializeField] private int mctsDepth = 4;
        [SerializeField] private int maxCandidates = 25;

        [Header("Difficulty")]
        [SerializeField] private AIDifficulty difficulty = AIDifficulty.Normal;

        [Header("References")]
        [SerializeField] private GridSystem gridSystem;
        [SerializeField] private PlacementSystem placementSystem;
        [SerializeField] private DominantSystem dominantSystem;

        // AI状態
        private int currentMoney;
        private float decisionTimer;
        private BoardState currentBoardState;
        private MCTSPlanner mcts;
        private DominantStrategy strategy;
        private System.Random random;

        public ChainType AIChain => aiChain;
        public int CurrentMoney => currentMoney;
        public bool IsActive { get; private set; }

        public event Action<PlacementAction> OnPlacement;
        public event Action<int> OnMoneyChanged;

        private void Awake()
        {
            random = new System.Random();
            ApplyDifficulty();
            mcts = new MCTSPlanner(mctsIterations, mctsDepth);
            strategy = new DominantStrategy();
        }

        private void ApplyDifficulty()
        {
            switch (difficulty)
            {
                case AIDifficulty.Easy:
                    randomPlacementChance = 0.4f;
                    mctsIterations = 200;
                    decisionInterval = 5f;
                    break;
                case AIDifficulty.Normal:
                    randomPlacementChance = 0.15f;
                    mctsIterations = 500;
                    decisionInterval = 3f;
                    break;
                case AIDifficulty.Hard:
                    randomPlacementChance = 0.05f;
                    mctsIterations = 1000;
                    decisionInterval = 2f;
                    break;
                case AIDifficulty.Expert:
                    randomPlacementChance = 0.02f;
                    mctsIterations = 2000;
                    decisionInterval = 1.5f;
                    break;
            }
        }

        /// <summary>
        /// AIを初期化して開始
        /// </summary>
        public void Initialize(GridSystem grid, PlacementSystem placement, DominantSystem dominant)
        {
            gridSystem = grid;
            placementSystem = placement;
            dominantSystem = dominant;

            currentMoney = startingMoney;
            IsActive = true;
            decisionTimer = 0f;

            Debug.Log($"AutoPlayer initialized: Chain={aiChain}, Money={currentMoney}");
        }

        /// <summary>
        /// AIを停止
        /// </summary>
        public void Stop()
        {
            IsActive = false;
        }

        /// <summary>
        /// 収入を追加
        /// </summary>
        public void AddMoney(int amount)
        {
            currentMoney += amount;
            OnMoneyChanged?.Invoke(currentMoney);
        }

        private void Update()
        {
            if (!IsActive) return;
            if (gridSystem == null || placementSystem == null) return;

            decisionTimer += Time.deltaTime;
            if (decisionTimer >= decisionInterval)
            {
                decisionTimer = 0f;
                MakeDecision();
            }
        }

        private void MakeDecision()
        {
            if (currentMoney < storeCost)
            {
                Debug.Log($"AutoPlayer: Not enough money ({currentMoney}/{storeCost})");
                return;
            }

            // 盤面状態を更新
            UpdateBoardState();

            // 配置可能なセルを取得
            var availableCells = GetAvailableCells();
            if (availableCells.Count == 0)
            {
                Debug.Log("AutoPlayer: No available cells");
                return;
            }

            PlacementAction action;

            // ランダム配置の判定
            if (random.NextDouble() < randomPlacementChance)
            {
                action = SelectRandomPlacement(availableCells);
                Debug.Log($"AutoPlayer: Random placement at {action.GridPosition}");
            }
            else
            {
                action = SelectStrategicPlacement(availableCells);
                Debug.Log($"AutoPlayer: Strategic placement at {action.GridPosition} (score: {action.Score:F2})");
            }

            if (action.IsValid)
            {
                ExecutePlacement(action);
            }
        }

        private void UpdateBoardState()
        {
            int width = gridSystem.Width;
            int height = gridSystem.Height;

            currentBoardState = new BoardState(width, height);

            // 全店舗を取得
            if (placementSystem != null)
            {
                var allStores = placementSystem.AllStores;
                foreach (var store in allStores)
                {
                    var pos = store.GridPosition;
                    var worldPos = store.transform.position;

                    currentBoardState.AddStore(new StorePosition(
                        store.GetHashCode(),
                        new int2(pos.x, pos.y),
                        new float3(worldPos.x, worldPos.y, worldPos.z),
                        store.OwnerChain,
                        store.IsPlayerOwned,
                        store.GetDominantRadius()
                    ));
                }
            }
        }

        private List<int2> GetAvailableCells()
        {
            var cells = new List<int2>();

            for (int x = 0; x < gridSystem.Width; x++)
            {
                for (int y = 0; y < gridSystem.Height; y++)
                {
                    var gridPos = new int2(x, y);
                    if (!currentBoardState.OccupiedCells.Contains(gridPos))
                    {
                        // 建物スロットかどうかをチェック（実際のゲームロジックに依存）
                        if (gridSystem.IsValidPlacement(new Vector2Int(x, y)))
                        {
                            cells.Add(gridPos);
                        }
                    }
                }
            }

            return cells;
        }

        private PlacementAction SelectRandomPlacement(List<int2> availableCells)
        {
            if (availableCells.Count == 0)
                return PlacementAction.Invalid;

            var cell = availableCells[random.Next(availableCells.Count)];
            var worldPos = gridSystem.GetWorldPosition(new Vector2Int(cell.x, cell.y));

            return new PlacementAction
            {
                GridPosition = cell,
                WorldPosition = new float3(worldPos.x, worldPos.y, worldPos.z),
                Chain = aiChain,
                Score = 0f
            };
        }

        private PlacementAction SelectStrategicPlacement(List<int2> availableCells)
        {
            // 候補をアクションに変換
            var allActions = new List<PlacementAction>();
            foreach (var cell in availableCells)
            {
                var worldPos = gridSystem.GetWorldPosition(new Vector2Int(cell.x, cell.y));
                allActions.Add(new PlacementAction
                {
                    GridPosition = cell,
                    WorldPosition = new float3(worldPos.x, worldPos.y, worldPos.z),
                    Chain = aiChain,
                    Score = 0f
                });
            }

            // ヒューリスティックで候補を絞り込み
            var filteredActions = strategy.FilterCandidates(
                currentBoardState, aiChain, allActions, maxCandidates
            );

            // 絞り込んだセルリストを作成
            var filteredCells = new List<int2>();
            foreach (var action in filteredActions)
            {
                filteredCells.Add(action.GridPosition);
            }

            // MCTSで最適な配置を探索
            var bestAction = mcts.FindBestPlacement(
                currentBoardState,
                aiChain,
                filteredCells,
                cell => {
                    var worldPos = gridSystem.GetWorldPosition(new Vector2Int(cell.x, cell.y));
                    return new float3(worldPos.x, worldPos.y, worldPos.z);
                }
            );

            return bestAction;
        }

        private void ExecutePlacement(PlacementAction action)
        {
            if (!action.IsValid) return;

            // コストを支払う
            currentMoney -= storeCost;
            OnMoneyChanged?.Invoke(currentMoney);

            // 実際の配置処理（PlacementSystemを使用）
            var gridPos = new Vector2Int(action.GridPosition.x, action.GridPosition.y);

            // ConvenienceStoreDataを取得（aiChainに対応するデータ）
            var storeData = GetStoreDataForChain(aiChain);
            if (storeData != null && placementSystem != null)
            {
                placementSystem.PlaceStore(gridPos, storeData, false);
            }

            OnPlacement?.Invoke(action);
            Debug.Log($"AutoPlayer placed store at {gridPos} for {aiChain}");
        }

        private ConvenienceStoreData GetStoreDataForChain(ChainType chain)
        {
            // ResourcesからScriptableObjectを読み込む
            // 実際の実装ではGameManagerやDataManagerから取得
            string path = $"StoreData/{chain}";
            return Resources.Load<ConvenienceStoreData>(path);
        }

        /// <summary>
        /// デバッグ用：次の配置候補を取得
        /// </summary>
        public List<PlacementAction> GetTopCandidates(int count = 5)
        {
            if (currentBoardState == null)
                UpdateBoardState();

            var availableCells = GetAvailableCells();
            var allActions = new List<PlacementAction>();

            foreach (var cell in availableCells)
            {
                var worldPos = gridSystem.GetWorldPosition(new Vector2Int(cell.x, cell.y));
                allActions.Add(new PlacementAction
                {
                    GridPosition = cell,
                    WorldPosition = new float3(worldPos.x, worldPos.y, worldPos.z),
                    Chain = aiChain,
                    Score = 0f
                });
            }

            return strategy.FilterCandidates(currentBoardState, aiChain, allActions, count);
        }
    }

    /// <summary>
    /// AI難易度
    /// </summary>
    public enum AIDifficulty
    {
        Easy,
        Normal,
        Hard,
        Expert
    }
}
