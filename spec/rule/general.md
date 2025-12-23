# プロジェクト全般ルール

## プロジェクト概要
- ゲーム名: Dominant-K
- ジャンル: コンビニ経営ストラテジー
- エンジン: Unity 6 (URP)

## アーキテクチャ
- MonoBehaviour + Unity DOTS ハイブリッド構成
- 大量のコンビニ表示にはECS + GPU Instancingを使用
- ゲームロジックはMonoBehaviourベースで実装

## フォルダ構成
```
Assets/Scripts/
├── Core/          # ゲーム制御
├── Systems/       # ゲームシステム
├── AI/            # AI関連
├── Data/          # ScriptableObject等
├── Entities/      # ゲームエンティティ
├── ECS/           # Unity DOTS
│   ├── Authoring/ # Baker
│   ├── Components/
│   └── Systems/
└── UI/            # UI関連
```

## バージョン管理
- main: 安定版
- develop: 開発版
- feature/*: 機能開発
