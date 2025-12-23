# プロジェクト全体の詳細設計
- 個々の詳細な設計は、code以下にクラス名と同じmdを記載し仕様化する
  - 個々の仕様が無いものは、このページの情報をもとに生成する事

## 主要クラス構成

### Core層
- GameManager : ゲーム全体の制御、フェーズ管理
- CameraController : カメラ制御（クォータービュー）
- GamePhase : ゲームフェーズ定義

### Systems層
- GridSystem : グリッド管理、都市生成
- PlacementSystem : 店舗配置制御
- DominantSystem : ドミナント（支配圏）計算

### AI層
- AutoPlayer : AIプレイヤー制御
- MCTSPlanner : モンテカルロ木探索
- BoardState : 盤面状態管理
- DominantStrategy : 戦略評価
- DominantEvaluator : 盤面評価

### Entities層
- ConvenienceStore : コンビニ店舗エンティティ
- Building : 建物エンティティ

### ECS層 (DOTS)
- ConvenienceStoreComponent : 店舗コンポーネント
- ConvenienceStoreSpawnerSystem : 店舗スポーンシステム
- StoreColorSystem : 店舗色更新システム
