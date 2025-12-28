# コンパイルエラー自動修正コマンド

## 概要
CIと同じコンパイルチェックを実行し、エラーがあれば自動修正を繰り返すコマンド。
コンパイルエラーがなくなるまでループ処理を行う。

## 設定
- **Unity Path**: `.env` ファイルの `UNITY_EDITOR_PATH` から取得
- **Project Path**: プロジェクトルート（`E:\Document\WhatTheStrike`）
- **Log File**: `compile.log`
- **最大リトライ回数**: 10回

## 事前準備

### .env ファイルの作成
1. `.env.example` をコピーして `.env` を作成
2. `UNITY_EDITOR_PATH` を自分の環境に合わせて設定

```bash
# .env の例
UNITY_EDITOR_PATH=C:\Program Files\Unity\Hub\Editor\6000.3.2f1\Editor\Unity.exe
```

## 処理フロー

### 1. 初期化
1. `.env` ファイルを読み込み、`UNITY_EDITOR_PATH` を取得
2. `.env` が存在しない場合はエラーメッセージを表示して終了
3. リトライカウンタを0に初期化

### 2. コンパイルチェック実行ループ
以下の処理を最大10回まで繰り返す：

#### 2.1 コンパイルチェック実行
Unityのbatchmodeでコンパイルチェックを実行：

**Windows:**
```bash
"$UNITY_EDITOR_PATH" -batchmode -quit -projectPath . -logFile compile.log
```

**Mac/Linux:**
```bash
"$UNITY_EDITOR_PATH" -batchmode -quit -projectPath . -logFile compile.log
```

終了コードを確認（0=成功、それ以外=失敗）

#### 2.2 成功時の処理
終了コードが0の場合：
1. 「コンパイル成功」を出力
2. ループを終了して完了報告

#### 2.3 失敗時のエラー解析
終了コードが0以外の場合：
1. `compile.log` を読み込む
2. エラーメッセージを解析し、以下の情報を抽出：
   - エラーが発生したファイルパス（例：`Assets/Scripts/xxx.cs`）
   - 行番号
   - エラーコード（例：CS0103, CS1061など）
   - エラーメッセージ

#### 2.4 エラー修正
1. 抽出したエラー情報に基づき、対象ファイルを読み込む
2. エラーの種類に応じて修正を行う：
   - **CS0103**: 名前が存在しない → 参照追加、using追加、型名修正
   - **CS1061**: メソッドが存在しない → メソッド追加、型変更
   - **CS0246**: 型が見つからない → using追加、参照確認
   - **CS0029**: 暗黙的な型変換エラー → キャスト追加、型変更
   - **CS0019**: 演算子エラー → 型変換追加
   - **その他**: エラーメッセージを解析して適切な修正を行う
3. 修正内容をファイルに保存
4. リトライカウンタをインクリメント

#### 2.5 ループ継続判定
- リトライ回数が10回未満ならステップ2.1に戻る
- リトライ回数が10回に達した場合：
  1. 「最大リトライ回数に到達」を警告出力
  2. 未解決のエラー一覧を出力
  3. ループを終了

### 3. 完了報告
1. 修正したファイル一覧を出力
2. 総リトライ回数を出力
3. 最終結果（成功/失敗）を出力

## エラーメッセージ解析パターン

### コンパイルエラーの正規表現
```
Assets[\\/].*\.cs\(\d+,\d+\): error CS\d+:.*
```

### エラー情報の抽出例
入力: `Assets/Scripts/InGame/Player/PlayerMove.cs(456,17): error CS0029: Cannot implicitly convert type 'double' to 'float'`

抽出結果:
- ファイル: `Assets/Scripts/InGame/Player/PlayerMove.cs`
- 行: 456
- 列: 17
- エラーコード: CS0029
- メッセージ: `Cannot implicitly convert type 'double' to 'float'`

## .env ファイルの読み込み方法

```bash
# bashでの読み込み
source .env
echo $UNITY_EDITOR_PATH
```

## 注意事項
- `.env` ファイルは `.gitignore` に追加すること
- Unityエディタが起動中の場合はbatchmodeが失敗する可能性がある
- 大規模な構造変更が必要なエラーは手動対応が必要な場合がある
- 循環参照やアセンブリ定義の問題は自動修正が困難
