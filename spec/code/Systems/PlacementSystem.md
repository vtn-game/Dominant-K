# PlacementSystemクラス設計

# 概要
- コンビニ店舗の配置を管理するシステム
- プレビュー表示、配置可能判定、店舗生成を担当

# 実装
- MonoBehaviourを継承する
- マウス/タッチ入力を処理
- GridSystemと連携して配置を実行

# 外部変数
- groundLayer: レイキャスト用レイヤーマスク
- validPlacementColor: 配置可能時のプレビュー色
- invalidPlacementColor: 配置不可時のプレビュー色

# 状態
- isPlacementMode: 配置モード中フラグ
- selectedStoreData: 選択中の店舗データ
- currentPreviewPosition: プレビュー位置
- allStores: 配置済み全店舗リスト

# 処理フロー

## プレビュー更新 (Update)
1. 右クリックで配置モードキャンセル
2. 左クリックで配置実行
3. マウス位置からレイキャスト
4. グリッド座標に変換してプレビュー表示

## 配置可能判定 (CanPlaceAt)
- Road, Station, ConvenienceStoreには配置不可
- 資金不足時は配置不可

## 店舗配置 (TryPlaceStore)
1. 配置可能か確認
2. 資金を消費
3. 既存建物があれば破壊
4. 店舗を生成
5. GridSystemに登録
6. DominantSystemに通知

# 外部インタフェース
- EnterPlacementMode(data): 配置モード開始
- ExitPlacementMode(): 配置モード終了
- PlaceStore(pos, data, playerOwned): 店舗配置（AI用）
- CreateStore(pos, data, playerOwned): 店舗生成
- RemoveStore(store): 店舗削除
- GetStoresByOwner(playerOwned): 所有者別店舗取得
- GetStoresByChain(chain): チェーン別店舗取得

# 期待値
- プレビューがグリッドにスナップする
- 配置不可位置では赤色プレビュー

# エッジケース
- カメラがnullの場合はCamera.mainを使用
