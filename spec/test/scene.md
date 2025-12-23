# 遷移チェック

## 概要
UIボタンのInvokeを使った遷移チェックを行う

---

## 1. タイトル画面

**ファイル:** `Assets/Scripts/UI/TitleUIController.cs` (未実装)

### メインメニュー
| ボタン | メソッド | 遷移先 |
|--------|----------|--------|
| ゲームスタート | OnClick_GameStart() | チェーン選択画面 |
| オプション | OnClick_Options() | オプション画面 |
| 終了 | OnClick_Quit() | アプリ終了 |

---

## 2. チェーン選択画面

**ファイル:** `Assets/Scripts/UI/ChainSelectUIController.cs` (未実装)

### チェーン選択
| ボタン | メソッド | 遷移先 |
|--------|----------|--------|
| SevenEleban | OnClick_SelectChain(0) | ステージ選択 |
| Lawson | OnClick_SelectChain(1) | ステージ選択 |
| Famoma | OnClick_SelectChain(4) | ステージ選択 |
| 戻る | OnClick_Back() | タイトル画面 |

---

## 3. インゲーム画面

**ファイル:** `Assets/Scripts/UI/GameUI.cs`

### インゲームUI
| ボタン | メソッド | 動作 |
|--------|----------|------|
| 店舗配置 | OnClick_PlaceStore(data) | 配置モード開始 |
| ポーズ | OnClick_Pause() | ポーズ画面表示 |

---

## 4. ポーズ画面

**ファイル:** `Assets/Scripts/UI/PauseUIController.cs` (未実装)

### ポーズメニュー
| ボタン | メソッド | 動作 |
|--------|----------|------|
| 再開 | OnClick_Resume() | ゲーム再開 |
| リトライ | OnClick_Retry() | ゲームリスタート |
| タイトルへ | OnClick_ToTitle() | タイトル画面 |

---

## 5. リザルト画面

**ファイル:** `Assets/Scripts/UI/ResultUIController.cs` (未実装)

### リザルトメニュー
| ボタン | メソッド | 遷移先 |
|--------|----------|--------|
| 次のステージ | OnClick_NextStage() | 次ステージ |
| リトライ | OnClick_Retry() | 同ステージ再開 |
| タイトルへ | OnClick_ToTitle() | タイトル画面 |
