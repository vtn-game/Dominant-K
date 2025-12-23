# ConvenienceStoreクラス設計

# 概要
- コンビニ店舗を表すエンティティクラス
- 店舗データ、所有者情報、収益計算を管理

# 実装
- MonoBehaviourを継承する
- ConvenienceStoreDataから初期化

# 外部変数
- Data: 店舗データ (ScriptableObject)
- GridPosition: グリッド座標
- OwnerChain: 所有チェーン
- IsPlayerOwned: プレイヤー所有フラグ
- DominantCount: ドミナント数（同チェーン店舗数）

# 処理フロー

## 初期化 (Initialize)
1. 店舗データを設定
2. グリッド座標を設定
3. 所有者情報を設定
4. 視覚的表現を更新

## 収益計算 (CalculateRevenue)
- baseRevenue * (1 + dominantBonus * dominantCount)

# 外部インタフェース
- Initialize(data, gridPos, chain, playerOwned): 初期化
- GetDominantRadius(): ドミナント半径取得
- GetZOCRadius(): ZOC半径取得
- SetDominantCount(count): ドミナント数設定
- CalculateRevenue(): 収益計算
- OnStoreDestroyed(): 破壊時処理

# 期待値
- チェーンカラーで視覚的に識別可能
- 収益がドミナント数で増加

# エッジケース
- Dataがnullの場合はデフォルト値を使用
