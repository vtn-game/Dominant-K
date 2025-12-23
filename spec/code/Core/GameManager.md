# GameManagerクラス設計

# 概要
- ゲーム全体の制御をするシングルトンクラス
- フェーズ管理、資金管理、勝敗判定を担当

# 実装
- MonoBehaviourを継承する
- シングルトンパターンを使用 (Instance)
- GridSystem, PlacementSystem, DominantSystemへの参照を保持

# 外部変数
- currentPhase: 現在のゲームフェーズ
- playerChain: プレイヤーのコンビニチェーン
- playerFunds: プレイヤーの所持金
- phase1Duration: Phase 1 の制限時間 (120秒)
- phase2Duration: Phase 2 の制限時間 (180秒)

# 処理フロー

## 初期化 (Setup)
1. プレイヤーチェーンと初期資金を設定
2. 各システムへの参照を保持
3. Phase 1 を開始

## フェーズ更新 (Update)
1. フェーズタイマーを更新
2. 制限時間に達したら次のフェーズへ遷移

## 資金管理
- TrySpendFunds: 資金消費を試行（不足時はfalse）
- AddFunds: 資金を追加

## 勝利判定 (CheckPhase1Victory)
- DominantSystemに敵全滅を問い合わせ
- 全滅していればPhase 2へ遷移

# イベント
- OnPhaseChanged: フェーズ変更時
- OnFundsChanged: 資金変更時

# 期待値
- シングルトンとして全システムからアクセス可能
- フェーズ遷移が正しく行われる

# エッジケース
- 資金が0の場合でも店舗配置UIは表示（配置不可表示）
