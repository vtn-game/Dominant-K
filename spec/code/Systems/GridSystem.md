# GridSystemクラス設計

# 概要
- グリッドベースの都市管理システム
- 道路、建物、駅、コンビニの配置を管理

# 実装
- MonoBehaviourを継承する
- 2次元配列でGridCellを管理
- BSP/Voronoi等のアルゴリズムで都市を自動生成

# 外部変数
- gridWidth: グリッド幅 (デフォルト 30)
- gridHeight: グリッド高さ (デフォルト 30)
- cellSize: セルサイズ (デフォルト 1.0)
- buildingDensity: 建物密度 (0.0-1.0)
- roadWidth: 道路幅
- blockSize: ブロックサイズ

# セルタイプ (CellType)
- Empty: 空き地
- Road: 道路（配置不可）
- Building: 建物（配置可能・破壊される）
- ConvenienceStore: コンビニ（配置済み）
- Station: 駅（配置不可）

# 処理フロー

## 初期化 (Initialize)
1. マテリアルを生成
2. グリッド配列を初期化
3. 都市を生成 (GenerateCity)

## 都市生成 (GenerateCity)
1. 中央に駅を配置
2. 道路グリッドを生成
3. ブロック内に建物を生成

# 外部インタフェース
- GetCell(x, z): セル取得
- GetWorldPosition(pos): グリッド座標→ワールド座標
- GetCellFromWorldPosition(pos): ワールド座標→セル
- IsValidPlacement(pos): 配置可能判定
- PlaceStore(pos, store): 店舗配置
- DestroyBuilding(pos): 建物破壊
- GetCellsInRadius(center, radius): 半径内セル取得

# 期待値
- グリッド座標とワールド座標の相互変換が正確
- 配置可能判定が正しく機能

# エッジケース
- グリッド範囲外へのアクセスはnullを返す
