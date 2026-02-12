# 未実装構成一覧 (Unimplemented Configurations)

仕様書 (`spec/`) と実装コード (`Assets/Scripts/`) を比較し、未実装の構成を分類・整理した。

---

## 1. 完全未実装のシステム (Systems Not Yet Created)

### 1.1 住民システム (Resident System)
- **仕様**: `spec/entities/residents.md`
- **状態**: `ResidentData` (ScriptableObject定義) のみ存在。住民の生成・行動・移動ロジックは未実装
- **不足しているクラス**:
  - `ResidentSpawner` — 住民の生成（時間帯別スポーン重み付き）
  - `ResidentBehavior` / `ResidentController` — 住民の移動・店舗訪問AI
  - `ResidentManager` — 住民全体の管理
- **不足している機能**:
  - 6種類の住民タイプ別行動パターン（サラリーマン、学生、主婦、高齢者、ITエンジニア、インド系住民）
  - 時間帯別スポーン率（朝ラッシュ、昼、夕方、深夜）
  - 住民の店舗間移動と購買行動
  - チェーンロイヤリティ（学生の洗脳固定）
  - コミュニティボーナス（インド系住民の近接ボーナス +50%）
  - 高齢者の信仰永続化（最大時ロック）
  - ITエンジニアの音波耐性（20%）

### 1.2 UI画面 (UI Screens)
- **仕様**: `spec/ux/ux_design.md`
- **状態**: 完全未実装（クラスすら存在しない）
- **不足しているクラス**:
  - `TitleUIController` — タイトル画面
  - `ChainSelectUIController` — チェーン選択画面
  - `PauseUIController` — ポーズメニュー
  - `ResultUIController` — リザルト画面

### 1.3 Phase 2: 3D侵略メカニクス (3D Invasion Mechanics)
- **仕様**: `spec/phase1-2d-invasion.md`, `spec/phase2-3d-invasion.md`
- **状態**: `GamePhase.Phase2_3D` のenum値とタイマー遷移のみ存在。固有メカニクスは未実装
- **不足している機能**:
  - 縦積みシステム（同一セルへの多層店舗配置）
  - フロアコスト計算（階数に応じたコスト増加）
  - 信仰度の動的変動ロジック（Phase 2固有の近隣ストアとの相対的支配度）
  - イメージ戦略アクション（手動での信仰度増加）
  - Phase 2勝利条件: 256階建てビル達成

### 1.4 Phase 3: 4D侵略メカニクス (4D Invasion Mechanics)
- **仕様**: `spec/phase3-4d-invasion.md`
- **状態**: `GamePhase.Phase3_4D` のenum値のみ存在。完全未実装
- **不足しているクラス/機能**:
  - `DimensionManager` — 次元管理システム
  - `InversionDrive` — 反転ドライブ（敵次元ストアをアンチストアに変換）
  - `AntiStore` — アンチストアエンティティ
  - `AnnihilationSystem` — 対消滅システム（アンチストア + 自ストアの衝突）
  - コンビニエンスエネルギー（自次元での収入・信仰バフ）
  - インコンビニエンスエネルギー（敵次元の破壊）
  - Phase 3勝利条件: 他の2次元を破壊

### 1.5 Phase 4: ボス戦 — アイオーン (Boss Battle: Aion)
- **仕様**: `spec/boss-aion.md`, `spec/gamedesign/boss_aion.md`
- **状態**: `GamePhase.Phase4_Boss` のenum値のみ存在。完全未実装
- **不足しているクラス/機能**:
  - `AionBoss` — ボスエンティティ（時間操作能力、30秒顕現制限）
  - `AionRetinue` — 眷属システム（四大臣: 死角悦、霞、重成、大栄）
  - `AionAbility` — 神技「MaxValue」（Int64.MaxValue ダメージ）
  - `DimensionEscape` — 無限次元逃避メカニクス
  - 勝利条件: 無限次元に逃避し自ストアを1つ以上維持
  - 敗北条件: 全ストアがアイオーンに上書き

### 1.6 ステージ構成 (Stage Configuration)
- **仕様**: `spec/gamedesign/maps.md`
- **状態**: 完全未実装
- **不足している構成**:
  - ステージ1「駅前広場」: 20×20, Easy, 駅周辺密集
  - ステージ2「住宅街」: 30×30, Normal, 散在建物
  - ステージ3「商業地区」: 40×40, Hard, 複数駅, 複雑道路
  - ステージ選択・読み込みシステム

### 1.7 タッチ入力 (Touch Input)
- **仕様**: `spec/rule/Input.md`
- **状態**: キーボード/マウス入力のみ実装。タッチ操作は未実装
- **不足している機能**:
  - シングルタップ: 店舗配置
  - ピンチイン/アウト: カメラズーム
  - 2本指ドラッグ: カメラ移動
  - 2本指回転: カメラ回転

---

## 2. 部分実装のシステム (Partially Implemented Systems)

### 2.1 GameManager — フェーズ遷移
- **ファイル**: `Assets/Scripts/Core/GameManager.cs`
- **実装済み**: Phase 1 → Phase 2 → Phase 3 のタイマー遷移
- **未実装**:
  - Phase 3 → Phase 4 (Boss) への遷移条件
  - Phase 2固有のゲームロジック起動
  - Phase 3固有のゲームロジック起動
  - Phase 4固有のゲームロジック起動

### 2.2 EconomyManager — イベントハンドラ
- **ファイル**: `Assets/Scripts/Systems/Economy/EconomyManager.cs:265-273`
- **未実装メソッド**:
  - `HandleSimulationTick(SimulationTickResult result)` — 空実装（シミュレーション結果の処理なし）
  - `HandleRevenueGenerated(int storeId, int revenue)` — 空実装（収入発生時の処理なし）

### 2.3 GameUI — タイマー表示
- **ファイル**: `Assets/Scripts/UI/GameUI.cs:141-145`
- **未実装メソッド**:
  - `UpdateTimer()` — 空実装（コメントのみ: "Implementation depends on how timer is exposed from GameManager"）

### 2.4 PurchaseSimulationSystem — イベントハンドラ
- **ファイル**: `Assets/Scripts/Systems/Economy/PurchaseSimulationSystem.cs:240-254`
- **部分実装メソッド**:
  - `HandleFaithChanged()` — 空実装（デバッグログなし）
  - `HandlePurchase()` — 空実装

### 2.5 チェーン固有特殊能力
- **SevenEleban「エレバン」能力**: 仕様では7つ以上のドミナント店舗で発動する特殊能力（効果未定義）。コード上も未実装
- **Lawson 洗脳耐性**: 仕様では20%の顧客移動耐性。`ResidentData`にフィールドは存在するが、処理ロジックなし
- **Famoma 怪音波**: `ConvenienceStoreData`に`brainwashSpeedBonus`と`movementSuppression`フィールドは存在するが、`FaithSystem`で使用されていない

---

## 3. ScriptableObjectデータアセット未作成 (Missing Data Assets)

### 3.1 コンビニチェーンデータ
- **定義**: `ConvenienceStoreData` (ScriptableObject)
- **状態**: `.asset` ファイルが1つも作成されていない
- **必要なアセット**:
  - `SevenEleban.asset` (コスト100, ZOC 3.0, Dominant 4.0, 収入10)
  - `Lawson.asset` (ZOC 2.5, Dominant 3.5)
  - `Lawson100.asset` (コスト80, 収入6, 乗数0.7)
  - `NaturalLawson.asset` (コスト180, 収入14, 乗数1.4)
  - `Famoma.asset` (コスト200, ZOC 5.0, Dominant 6.0, 収入12)

### 3.2 住民データ
- **定義**: `ResidentData` (ScriptableObject)
- **状態**: `.asset` ファイルが1つも作成されていない
- **必要なアセット**:
  - `Salaryman.asset` (速度1.2, 消費力1.5, 信仰耐性0.8, 重み30)
  - `Student.asset` (速度1.0, 消費力0.6, 信仰耐性0.5, 重み25)
  - `Homemaker.asset` (速度0.8, 消費力1.2, 信仰耐性1.2, 重み20)
  - `Elder.asset` (速度0.5, 消費力0.8, 信仰耐性1.5, 重み10)
  - `Engineer.asset` (速度0.9, 消費力1.3, 信仰耐性1.0, 重み10)
  - `IndianResident.asset` (速度1.0, 消費力1.0, 信仰耐性0.7, 重み5)

---

## 4. CityGenerationManagerとGridSystemの未統合 (Integration Gap)

- `CityGenerationManager` は独立した街生成システム（Voronoi/BSP/Japanese の3アルゴリズム）を実装済み
- `GridSystem` も独自の簡易都市生成を内蔵
- **問題**: 2つのシステムが統合されていない。`CityGenerationManager` の生成結果を `GridSystem` が利用する連携コードが不在

---

## 5. 仕様上未定義の項目 (Undefined in Specs)

以下は仕様書内で「TBD」「未定義」とされている項目:

- SevenEleban「エレバン」能力の具体的効果
- 四大臣（死角悦、霞、重成、大栄）の詳細能力
- 眷属（めにーすとっぷ、まいびすけっと）の詳細仕様
- Phase 2のフロアコスト計算式
- Phase 2のイメージ戦略アクションのコスト
- チェーンカラーの具体的RGB値

---

## サマリー

| カテゴリ | 状態 | 優先度 |
|---------|------|-------|
| 住民システム全体 | 未実装 | 高 |
| UI画面 (Title/Select/Pause/Result) | 未実装 | 高 |
| Phase 2 固有メカニクス | 未実装 | 中 |
| Phase 3 固有メカニクス | 未実装 | 中 |
| Phase 4 ボス戦 | 未実装 | 低 |
| ステージ構成 | 未実装 | 中 |
| タッチ入力 | 未実装 | 低 |
| ScriptableObjectデータアセット | 未作成 | 高 |
| CityGeneration ↔ GridSystem 統合 | 未統合 | 中 |
| EconomyManager イベントハンドラ | スタブ | 低 |
| GameUI タイマー表示 | スタブ | 低 |
| チェーン固有能力の処理ロジック | 部分実装 | 中 |
