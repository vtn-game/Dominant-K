# MCTSPlannerクラス設計

# 概要
- モンテカルロ木探索（MCTS）による最適配置探索
- UCB1アルゴリズムでバランスの取れた探索を実現

# 実装
- 純粋なC#クラス（MonoBehaviourなし）
- ノードベースの木構造を構築
- Selection → Expansion → Simulation → Backpropagation

# 外部変数
- maxIterations: 探索反復回数
- maxDepth: 最大探索深度
- explorationConstant: UCB1の探索定数 (√2)

# データ構造

## MCTSNode
- State: 盤面状態 (BoardState)
- Parent: 親ノード
- Children: 子ノードリスト
- Action: このノードに至ったアクション
- Visits: 訪問回数
- TotalValue: 累計評価値
- UntriedActions: 未試行アクション

# 処理フロー

## 探索 (FindBestPlacement)
```
for i in maxIterations:
    node = Select(root)
    if node has untried actions:
        node = Expand(node)
    value = Simulate(node)
    Backpropagate(node, value)
return best child action
```

## 選択 (Select)
- UCB1値が最大の子ノードを選択
- UCB1 = value/visits + C * sqrt(ln(parent.visits) / visits)

## 拡張 (Expand)
- 未試行アクションからランダムに選択
- 新しい子ノードを生成

## シミュレーション (Simulate)
- ランダムプレイアウトを実行
- 最終盤面をDominantEvaluatorで評価

## 逆伝播 (Backpropagate)
- ルートまで評価値を伝播
- 訪問回数をインクリメント

# 外部インタフェース
- FindBestPlacement(state, chain, cells, cellToWorld): 最適配置を探索

# 期待値
- 反復回数に応じて配置品質が向上
- 探索と活用のバランスが取れている

# エッジケース
- 候補が0の場合はInvalidアクションを返す
