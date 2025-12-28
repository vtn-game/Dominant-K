# Issue実装からPRマージまでの一貫ワークフロー

GitHub Issueの取得から実装、PR作成、レビュー対応までを一貫して行うコマンド。

## ロール
あなたはゲームプログラマです。
Issueを確認し、実装を行い、PRを作成し、レビュー対応まで完了させます。

## 引数
コマンド引数には `{Issue番号}` が渡される。
- 例: `/ipr 6`
- Issue番号がない場合は、オープン中のIssue一覧を表示して選択を促す。

## 事前に確認しておくルール
これはメモリ上に展開済みの場合は省略してよい
- `CLAUDE.md` を確認し、アーキテクチャルールを把握しておくこと
- `spec/rule` フォルダの中身を確認し、コーディングルールを把握しておくこと

## ワークフロー概要

```
┌─────────────────┐
│  Phase 1: Issue │
│  実装とPR作成    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Phase 2: Review │
│  待機と確認      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Phase 3: Fix   │
│  レビュー対応    │
└────────┬────────┘
         │
    ┌────┴────┐
    │ レビュー  │──→ Phase 2に戻る
    │ あり？   │
    └────┬────┘
         │なし
         ▼
┌─────────────────┐
│    完了         │
└─────────────────┘
```

## Phase 1: Issue実装とPR作成

### 1.1 Issue情報の取得
```bash
gh issue view {Issue番号} --json title,body,labels,assignees,milestone
```

### 1.2 作業ブランチの作成
```bash
git checkout develop
git checkout -b feature/issue-{Issue番号}-{簡潔な説明}
```
- developブランチから新しいブランチを作成

### 1.3 Issue内容の分析
- Issueの本文を読み、実装すべき内容を理解する
- 関連する仕様書があれば `spec/` フォルダ内を確認する
- 不明点があればユーザーに確認する

### 1.4 実装
- Issueの要件に従って実装を行う
- `CLAUDE.md` のルールに従うこと
- NOTEコメントを適切に追加すること
- 未実装部分はTODOを記載すること

### 1.5 コミット
```bash
git add {変更ファイル}
git commit -m "feat: {変更内容} (#{Issue番号})"
```

### 1.6 プッシュとPR作成
```bash
git push -u origin {ブランチ名}
gh pr create --title "{PRタイトル}" --body "{PR本文}" --assignee "@me"
```

PR本文テンプレート:
```markdown
## 概要
{Issueの要約と実装内容}

## 関連Issue
Closes #{Issue番号}

## 変更点
- {変更点1}
- {変更点2}

## テスト確認項目
- [ ] {確認項目1}
- [ ] {確認項目2}

---
Generated with [Claude Code](https://claude.ai/code)
```

## Phase 2: レビュー待機と確認

### 2.1 ユーザーへの確認
PRが作成されたら、ユーザーに以下を確認:
- レビューが完了したか
- 続行するか（レビュー対応に進むか）

### 2.2 レビューコメントの取得
```bash
gh api repos/{owner}/{repo}/pulls/{PR番号}/comments
```

### 2.3 レビュー状態の判定
- コメントがない場合 → 完了
- コメントがある場合 → Phase 3に進む

## Phase 3: レビュー対応

### 3.1 レビューコメントの分析
各コメントから以下を抽出:
- `id`: コメントID
- `body`: 修正内容
- `path`: 対象ファイル
- `line`: 対象行

### 3.2 修正の実施
1. 対象ファイルを読み込む
2. レビュー指摘に従って修正
3. コミットを作成
```bash
git add {修正ファイル}
git commit -m "fix: レビュー指摘対応 (#{Issue番号})"
```

### 3.3 プッシュ
```bash
git push
```

### 3.4 コメントへの返信
```bash
gh api -X POST "repos/{owner}/{repo}/pulls/{PR番号}/comments/{コメントID}/replies" -f body="修正しました。{修正内容}"
```

### 3.5 再度Phase 2へ
新たなレビューコメントがないか確認

## Phase 4: 完了

### 4.1 ログ出力
`log/` フォルダに実装ログを出力する。
ファイル名: `issue-{Issue番号}-workflow.md`

ログ内容:
```markdown
# Issue #{Issue番号} ワークフローログ

## Issue情報
- タイトル: {タイトル}
- PR: {PR URL}

## 実装内容
- {実装内容}

## レビュー対応
- {対応内容}

## 最終コミット
- {コミットハッシュ}
```

## 便利なコマンド

### PR状態確認
```bash
gh pr view {PR番号} --json state,reviews,comments
```

### PRマージ（ユーザーが手動で行う）
```bash
gh pr merge {PR番号} --squash
```

## 注意事項
- mainブランチに直接コミットしないこと
- developブランチから作業ブランチを作成すること
- レビュー対応後は必ずコメントに返信すること
- 各フェーズでユーザーに進捗を報告すること
