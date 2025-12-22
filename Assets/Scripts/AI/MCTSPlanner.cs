using System;
using System.Collections.Generic;
using Unity.Mathematics;
using DominantK.Data;

namespace DominantK.AI
{
    /// <summary>
    /// モンテカルロ木探索（MCTS）による配置計画
    /// UCB1アルゴリズムを使用
    /// </summary>
    public class MCTSPlanner
    {
        private readonly DominantEvaluator evaluator;
        private readonly System.Random random;

        // MCTS設定
        private readonly int maxIterations;
        private readonly int maxDepth;
        private readonly float explorationConstant;

        public MCTSPlanner(int iterations = 1000, int depth = 5, float exploration = 1.414f)
        {
            evaluator = new DominantEvaluator();
            random = new System.Random();
            maxIterations = iterations;
            maxDepth = depth;
            explorationConstant = exploration;
        }

        /// <summary>
        /// 最適な配置を探索
        /// </summary>
        public PlacementAction FindBestPlacement(
            BoardState state,
            ChainType myChain,
            List<int2> availableCells,
            Func<int2, float3> cellToWorld)
        {
            if (availableCells.Count == 0)
                return PlacementAction.Invalid;

            // ルートノードを作成
            var rootNode = new MCTSNode(state, myChain, PlacementAction.Invalid);

            // 利用可能なアクションを展開
            var actions = GenerateActions(availableCells, myChain, cellToWorld);
            if (actions.Count == 0)
                return PlacementAction.Invalid;

            // MCTSメインループ
            for (int i = 0; i < maxIterations; i++)
            {
                // 1. 選択（Selection）
                var selectedNode = Select(rootNode);

                // 2. 展開（Expansion）
                if (!selectedNode.IsFullyExpanded && selectedNode.Depth < maxDepth)
                {
                    selectedNode = Expand(selectedNode, actions, cellToWorld);
                }

                // 3. シミュレーション（Simulation）
                float result = Simulate(selectedNode, actions, cellToWorld);

                // 4. 逆伝播（Backpropagation）
                Backpropagate(selectedNode, result);
            }

            // 最も訪問された子ノードを選択
            return SelectBestAction(rootNode);
        }

        private List<PlacementAction> GenerateActions(
            List<int2> availableCells,
            ChainType chain,
            Func<int2, float3> cellToWorld)
        {
            var actions = new List<PlacementAction>();

            foreach (var cell in availableCells)
            {
                actions.Add(new PlacementAction
                {
                    GridPosition = cell,
                    WorldPosition = cellToWorld(cell),
                    Chain = chain,
                    Score = 0f
                });
            }

            return actions;
        }

        private MCTSNode Select(MCTSNode node)
        {
            while (node.Children.Count > 0)
            {
                node = SelectChild(node);
            }
            return node;
        }

        private MCTSNode SelectChild(MCTSNode node)
        {
            MCTSNode bestChild = null;
            float bestValue = float.MinValue;

            foreach (var child in node.Children)
            {
                float ucb1 = CalculateUCB1(child, node.VisitCount);
                if (ucb1 > bestValue)
                {
                    bestValue = ucb1;
                    bestChild = child;
                }
            }

            return bestChild ?? node;
        }

        private float CalculateUCB1(MCTSNode node, int parentVisits)
        {
            if (node.VisitCount == 0)
                return float.MaxValue;

            float exploitation = node.TotalScore / node.VisitCount;
            float exploration = explorationConstant * MathF.Sqrt(MathF.Log(parentVisits) / node.VisitCount);

            return exploitation + exploration;
        }

        private MCTSNode Expand(MCTSNode node, List<PlacementAction> allActions, Func<int2, float3> cellToWorld)
        {
            // まだ試していないアクションを選択
            var triedActions = new HashSet<int2>();
            foreach (var child in node.Children)
            {
                triedActions.Add(child.Action.GridPosition);
            }

            var untriedActions = new List<PlacementAction>();
            foreach (var action in allActions)
            {
                if (!triedActions.Contains(action.GridPosition) &&
                    !node.State.OccupiedCells.Contains(action.GridPosition))
                {
                    untriedActions.Add(action);
                }
            }

            if (untriedActions.Count == 0)
            {
                node.IsFullyExpanded = true;
                return node;
            }

            // ランダムに1つ選択
            var selectedAction = untriedActions[random.Next(untriedActions.Count)];

            // 新しい状態を作成
            var newState = node.State.Clone();
            var newStore = new StorePosition(
                -1,
                selectedAction.GridPosition,
                selectedAction.WorldPosition,
                node.MyChain,
                false,
                4f
            );
            newState.AddStore(newStore);

            // 子ノードを作成
            var childNode = new MCTSNode(newState, node.MyChain, selectedAction);
            childNode.Parent = node;
            childNode.Depth = node.Depth + 1;
            node.Children.Add(childNode);

            if (untriedActions.Count == 1)
            {
                node.IsFullyExpanded = true;
            }

            return childNode;
        }

        private float Simulate(MCTSNode node, List<PlacementAction> allActions, Func<int2, float3> cellToWorld)
        {
            var simState = node.State.Clone();
            int simDepth = 0;

            // ランダムプレイアウト
            while (simDepth < maxDepth - node.Depth)
            {
                var availableActions = new List<PlacementAction>();
                foreach (var action in allActions)
                {
                    if (!simState.OccupiedCells.Contains(action.GridPosition))
                    {
                        availableActions.Add(action);
                    }
                }

                if (availableActions.Count == 0) break;

                // ランダムに配置
                var action = availableActions[random.Next(availableActions.Count)];
                var store = new StorePosition(
                    -1, action.GridPosition, action.WorldPosition, node.MyChain, false, 4f
                );
                simState.AddStore(store);
                simDepth++;
            }

            // 最終状態を評価
            return evaluator.Evaluate(simState, node.MyChain);
        }

        private void Backpropagate(MCTSNode node, float result)
        {
            while (node != null)
            {
                node.VisitCount++;
                node.TotalScore += result;
                node = node.Parent;
            }
        }

        private PlacementAction SelectBestAction(MCTSNode rootNode)
        {
            if (rootNode.Children.Count == 0)
                return PlacementAction.Invalid;

            MCTSNode bestChild = null;
            int maxVisits = 0;

            foreach (var child in rootNode.Children)
            {
                if (child.VisitCount > maxVisits)
                {
                    maxVisits = child.VisitCount;
                    bestChild = child;
                }
            }

            if (bestChild == null)
                return PlacementAction.Invalid;

            var action = bestChild.Action;
            action.Score = bestChild.TotalScore / bestChild.VisitCount;
            return action;
        }
    }

    /// <summary>
    /// MCTSノード
    /// </summary>
    public class MCTSNode
    {
        public BoardState State;
        public ChainType MyChain;
        public PlacementAction Action;
        public MCTSNode Parent;
        public List<MCTSNode> Children;
        public int VisitCount;
        public float TotalScore;
        public int Depth;
        public bool IsFullyExpanded;

        public MCTSNode(BoardState state, ChainType myChain, PlacementAction action)
        {
            State = state;
            MyChain = myChain;
            Action = action;
            Children = new List<MCTSNode>();
            VisitCount = 0;
            TotalScore = 0f;
            Depth = 0;
            IsFullyExpanded = false;
        }
    }
}
