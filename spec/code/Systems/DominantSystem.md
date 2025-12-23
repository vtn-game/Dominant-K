# DominantSystemクラス設計

# 概要
- ドミナント戦略（三角形支配）の計算システム
- 敵店舗の捕獲判定、収益計算を担当

# 実装
- MonoBehaviourを継承する
- チェーンごとの店舗リストを管理
- 有効な三角形を動的に計算

# 外部変数
- triangleUpdateInterval: 三角形更新間隔 (0.5秒)
- revenueInterval: 収益計算間隔 (1.0秒)
- dominantRevenueMultiplier: ドミナント収益倍率 (1.5)
- showTriangles: 三角形可視化フラグ

# データ構造

## DominantTriangle
- store1, store2, store3: 三角形を構成する3店舗
- ownerChain: 所有チェーン
- worldVertices: ワールド座標の頂点配列

# 処理フロー

## 三角形更新 (UpdateTriangles)
1. 全三角形をクリア
2. チェーンごとに3店舗以上あれば三角形を探索
3. 接続条件を満たす組み合わせを登録
4. 可視化オブジェクトを生成

## 接続判定 (AreStoresConnected)
- 2店舗間の距離がドミナント半径×2以内なら接続

## 捕獲判定 (CheckTriangleCaptures)
1. 各三角形について敵店舗を走査
2. 三角形内にある敵店舗をリストアップ
3. PlacementSystemで店舗を削除

## 収益処理 (ProcessRevenue)
1. プレイヤー店舗を走査
2. 三角形内/頂点なら収益1.5倍
3. GameManagerに資金追加

# 外部インタフェース
- OnStoreAdded(store): 店舗追加時のコールバック
- OnStoreRemoved(store): 店舗削除時のコールバック
- IsPointInAnyTriangle(point, chain): 点が三角形内か判定
- GetDominantCount(chain): チェーンの店舗数取得
- AreAllEnemiesDefeated(): 敵全滅判定

# 期待値
- 三角形内の敵店舗が正しく排除される
- 収益ボーナスが正しく計算される

# エッジケース
- 店舗数が3未満のチェーンは三角形を形成しない
