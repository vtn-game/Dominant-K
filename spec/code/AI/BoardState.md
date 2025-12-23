# BoardStateクラス設計

# 概要
- ゲーム盤面の状態を表現するクラス
- AI探索用に複製可能な設計

# 実装
- 純粋なC#クラス
- 不変性を意識した設計
- Clone()で盤面を複製可能

# 外部変数
- Width: 盤面幅
- Height: 盤面高さ

# データ構造

## StorePosition
- Id: 店舗ID
- GridPosition: グリッド座標 (int2)
- WorldPosition: ワールド座標 (float3)
- Chain: チェーンタイプ
- IsPlayerOwned: プレイヤー所有フラグ
- DominantRadius: ドミナント半径

# 状態
- Stores: 全店舗リスト
- OccupiedCells: 占有セル集合 (HashSet)
- StoresByChain: チェーン別店舗辞書

# 外部インタフェース
- AddStore(store): 店舗を追加
- RemoveStore(id): 店舗を削除
- GetStoresByChain(chain): チェーン別店舗取得
- Clone(): 盤面を複製

# 期待値
- 探索中に盤面状態が破壊されない
- 高速な複製が可能

# エッジケース
- 同一位置への重複追加は無視
