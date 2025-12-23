# Dominant-K

**コンビニチェーンが異常発達した西葛西で生き残れ！**

インクリメンタルコンビニ経営ゲーム

## ゲーム概要

### コンセプト
「ゲームが現実のルールを破壊して侵略してくる」

だんだん壊れていく現実空間を見て楽しむストラテジーゲーム。

### ゲームフロー
1. **Phase 1: 2次元的侵略** - 街を破壊しコンビニを建設、ドミナント三角形で敵を潰す
2. **Phase 2: 3次元的侵略** - 縦にスタック、信仰度システム解禁
3. **Phase 3: 4次元的侵略** - 多元世界への展開、反コンビニと対消滅
4. **Boss: アイオーン** - 高次元存在からの逃走

### プレイアブルチェーン
| チェーン | 特徴 |
|---------|------|
| ローサン | バリエーション展開可能、バランス型 |
| ファモマ | 怪音波で洗脳、広いZOC、高コスト |
| セバンイレバン | 低コスト、ドミナント増加で腐敗 |

## 環境構築

### 必要環境
- Unity 6 (6000.0.x)
- .NET Standard 2.1

### 使用パッケージ
| パッケージ | バージョン | 用途 |
|-----------|-----------|------|
| Unity Entities | 1.4.2 | ECS / DOTS |
| Unity Entities Graphics | 1.4.15 | GPU Instancing |
| UniTask | - | 非同期処理 |
| DOTween | - | アニメーション |
| Universal RP | 17.3.0 | レンダリング |

### セットアップ
```bash
# リポジトリをクローン
git clone <repository-url>
cd Dominant-K

# Unityでプロジェクトを開く
# Unity Hub → Add → フォルダを選択
```

## プロジェクト構成

```
Dominant-K/
├── Assets/
│   └── Scripts/
│       ├── Core/           # ゲーム制御 (GameManager, CameraController)
│       ├── Systems/        # ゲームシステム (Grid, Placement, Dominant)
│       ├── AI/             # AI (MCTS, AutoPlayer, Strategy)
│       ├── Data/           # ScriptableObject定義
│       ├── Entities/       # ゲームエンティティ (ConvenienceStore, Building)
│       ├── ECS/            # Unity DOTS
│       │   ├── Authoring/  # Baker
│       │   ├── Components/ # ECSコンポーネント
│       │   └── Systems/    # ECSシステム
│       └── UI/             # UI関連
├── Packages/               # パッケージ設定
├── ProjectSettings/        # Unity設定
├── Setup/                  # セットアップ関連
└── spec/                   # 仕様書
    ├── README.md           # 仕様書概要
    ├── TODO.md             # 未定義項目一覧
    ├── code/               # コード仕様
    ├── gamedesign/         # ゲームデザイン仕様
    ├── rule/               # ルール定義
    ├── usecase/            # ユースケース
    ├── ux/                 # UX設計
    └── test/               # テスト仕様
```

## アーキテクチャ

### ハイブリッド構成
- **MonoBehaviour**: ゲームロジック、UI
- **Unity DOTS (ECS)**: 大量オブジェクト描画 (GPU Instancing)

### 主要クラス
| クラス | 役割 |
|--------|------|
| GameManager | ゲーム全体制御、フェーズ管理 |
| GridSystem | グリッド管理、都市生成 |
| PlacementSystem | 店舗配置制御 |
| DominantSystem | ドミナント三角形計算、収益処理 |
| AutoPlayer | AIプレイヤー (MCTS) |

## 開発ガイド

### コーディング規約
- 名前空間: `DominantK.*`
- 非同期処理: UniTask使用
- 疎結合設計を心がける

### ブランチ戦略
- `main`: 安定版
- `develop`: 開発版
- `feature/*`: 機能開発

## ドキュメント

詳細な仕様は [spec/](spec/) を参照

- [ゲームルール](spec/rule/gamerule.md)
- [チェーン定義](spec/gamedesign/chains.md)
- [UX設計](spec/ux/ux_design.md)
- [ボス: アイオーン](spec/gamedesign/boss_aion.md)

## ライセンス

<!-- TODO: ライセンス記載 -->
