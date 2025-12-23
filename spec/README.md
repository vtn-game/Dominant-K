# spec
このフォルダ以下には、コードを記載するうえでの詳細な仕様書が格納されます

**[TODO.md](TODO.md)** - 未定義項目の一覧

# フォルダ構成
## code
コードに関する仕様書が入ります
- Core/ : ゲーム全体の制御クラス
- Systems/ : ゲームシステム（グリッド、配置、ドミナント計算）
- AI/ : AI関連（MCTS、戦略評価）
- Entities/ : ゲーム内エンティティ
- ECS/ : Unity DOTS関連

## gamedesign
ゲーム内の仕様、レベルデザインにまつわる仕様書が入ります
- chains.md : コンビニチェーンの定義
- maps.md : マップ・ステージ設計

## image
説明に必要な画像をここに入れていきます
(obsidianのデフォルト)

## rule
AIの行動コード生成やゲーム全体にかかわるルールが記載されます
- general.md : プロジェクト全般ルール
- gamerule.md : ゲームルール
- coding.md : コーディング規約
- classes.md : クラス設計方針
- Input.md : 入力設定

## usecase
ゲーム内のふるまいを定義します

## ux
ゲームが目指す目標、ゴールをここに示します

## test
テスト仕様書が入ります
- feature.md : 機能テスト
- performance.md : パフォーマンステスト
- scene.md : シーン遷移テスト
