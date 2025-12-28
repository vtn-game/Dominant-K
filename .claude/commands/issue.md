# GitHub Issue 処理コマンド

GitHub CLIを使用して特定のIssueを取得し、実装してPRを発行するコマンド。

## ロール
あなたはゲームプログラマです。
GitHubのIssueを確認し、その内容に基づいて実装を行い、PRを発行します。

## 引数
コマンド引数には `{Issue番号}` が渡される。
- 例: `/issue 123`
- Issue番号がない場合は、オープン中のIssue一覧を表示して選択を促す。

## 処理フロー

### 1. Issue情報の取得
```bash
gh issue view {Issue番号} --json title,body,labels,assignees,milestone
```
- Issueのタイトル、本文、ラベル、担当者、マイルストーンを取得する

### 2. 作業ブランチの作成
- ブランチ名: `feature/issue-{Issue番号}-{簡潔な説明}`
- 例: `feature/issue-123-add-player-jump`
```bash
git checkout -b feature/issue-{Issue番号}-{簡潔な説明}
```

### 3. Issue内容の分析
- Issueの本文を読み、実装すべき内容を理解する
- 関連する仕様書があれば `spec/` フォルダ内を確認する
- 不明点があればユーザーに確認する

### 4. 実装
- Issueの要件に従って実装を行う
- CLAUDE.mdのルールに従うこと
- NOTEコメントを適切に追加すること
- 未実装部分はTODOを記載すること

### 5. コミット
- 変更内容をコミットする
- コミットメッセージにIssue番号を含める
- 例: `fix: プレイヤージャンプ機能を追加 (#123)`

### 6. PRの作成
```bash
gh pr create --title "{PRタイトル}" --body "{PR本文}" --assignee "@me"
```

PRの本文には以下を含める:
```markdown
## 概要
{Issueの要約と実装内容}

## 関連Issue
Closes #{Issue番号}

## 変更点
- {変更点1}
- {変更点2}
- ...

## テスト確認項目
- [ ] {確認項目1}
- [ ] {確認項目2}

---
Generated with [Claude Code](https://claude.ai/code)
```

### 7. ログ出力
`log/` フォルダに実装ログを出力する。

## 便利なGitHub CLIコマンド

### Issue一覧表示
```bash
gh issue list --state open
```

### Issue詳細表示
```bash
gh issue view {番号}
```

### PR作成
```bash
gh pr create --title "タイトル" --body "本文"
```

### PRにレビュアー追加
```bash
gh pr edit --add-reviewer {ユーザー名}
```

## 注意事項
- mainブランチに直接コミットしないこと
- PRを作成する前に、変更内容を確認すること
- Issueをクローズするには `Closes #番号` をPR本文に含めること
