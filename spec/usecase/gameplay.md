# インゲーム中に期待される振る舞い
インゲームはInGameシーンを使用する

## Feature: 店舗配置

#### Scenario: 配置モード開始
  Given プレイヤーがチェーンを選択済み
  When UIから店舗アイコンをクリック
  Then PlacementSystemが配置モードに入る
  And プレビューオブジェクトが表示される

#### Scenario: 配置位置の選択
  Given 配置モード中
  When マウスをグリッド上で移動
  Then プレビューがグリッドにスナップして追従
  And 配置可能なら緑、不可なら赤で表示

#### Scenario: 店舗の配置
  Given 配置モード中かつ配置可能位置
  When 左クリック
  Then 資金が消費される
  And 店舗が生成される
  And DominantSystemに通知される

#### Scenario: 配置のキャンセル
  Given 配置モード中
  When 右クリック
  Then 配置モードを終了
  And プレビューが非表示になる

## Feature: ドミナント三角形

#### Scenario: 三角形の形成
  Given 同一チェーンの店舗が3つ以上存在
  When 3店舗がドミナント半径内で接続
  Then 三角形が形成される
  And 三角形エリアが可視化される

#### Scenario: 敵店舗の排除
  Given 三角形が形成済み
  When 敵店舗が三角形内に存在
  Then 敵店舗が破壊される
  And プレイヤーに通知される

## Feature: 収益システム

#### Scenario: 定期収益
  Given プレイヤー店舗が存在
  When 1秒経過
  Then 各店舗の収益が計算される
  And プレイヤー資金に加算される

#### Scenario: ドミナントボーナス
  Given 店舗が三角形の頂点または内部
  When 収益計算時
  Then 収益が1.5倍になる

## Feature: ゲーム進行

#### Scenario: Phase 1 勝利
  Given Phase 1 プレイ中
  When 敵の全店舗が排除された
  Then Phase 2 に移行する

#### Scenario: Phase 1 時間切れ
  Given Phase 1 プレイ中
  When 120秒経過
  Then Phase 2 に移行する

## Feature: AI行動

#### Scenario: AI店舗配置
  Given AI AutoPlayerがアクティブ
  When 判断間隔（難易度による）が経過
  Then AIが最適な位置に店舗を配置
