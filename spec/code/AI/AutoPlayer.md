# AutoPlayerクラス設計

# 概要
- AIによる自動店舗配置を行うプレイヤー
- MCTS（モンテカルロ木探索）と戦略評価を組み合わせて配置を決定

# 実装
- MonoBehaviourを継承する
- 難易度設定によりパラメータを調整
- 一定間隔で配置判断を実行

# 外部変数
- aiChain: AIのコンビニチェーン
- decisionInterval: 配置判断間隔（秒）
- startingMoney: 初期資金
- storeCost: 店舗コスト
- randomPlacementChance: ランダム配置確率
- mctsIterations: MCTS反復回数
- mctsDepth: MCTS探索深度
- maxCandidates: 候補絞り込み数
- difficulty: AI難易度

# 難易度設定 (AIDifficulty)
| 難易度 | ランダム率 | MCTS回数 | 判断間隔 |
|--------|-----------|---------|---------|
| Easy   | 40%       | 200     | 5秒     |
| Normal | 15%       | 500     | 3秒     |
| Hard   | 5%        | 1000    | 2秒     |
| Expert | 2%        | 2000    | 1.5秒   |

# 処理フロー

## 初期化 (Initialize)
1. システム参照を設定
2. 初期資金を設定
3. MCTSとStrategyを初期化

## 配置判断 (MakeDecision)
1. 資金チェック
2. 盤面状態を更新
3. 配置可能セルを取得
4. ランダム or 戦略配置を選択
5. 配置を実行

## 戦略配置 (SelectStrategicPlacement)
1. 全候補をアクションに変換
2. ヒューリスティックで絞り込み
3. MCTSで最適解を探索

# イベント
- OnPlacement: 配置実行時
- OnMoneyChanged: 資金変更時

# 外部インタフェース
- Initialize(grid, placement, dominant): 初期化
- Stop(): AI停止
- AddMoney(amount): 資金追加
- GetTopCandidates(count): 上位候補取得（デバッグ用）

# 期待値
- 難易度に応じた強さで配置を行う
- ドミナント戦略を意識した配置

# エッジケース
- 資金不足時は配置をスキップ
- 配置可能セルがない場合はスキップ
