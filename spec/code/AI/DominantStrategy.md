# DominantStrategyクラス設計

# 概要
- ドミナント戦略に基づく候補フィルタリング
- ヒューリスティック評価で有望な配置候補を絞り込む

# 実装
- 純粋なC#クラス
- DominantEvaluatorを使用して評価

# 評価基準
1. 三角形形成可能性
   - 既存の2店舗と三角形を形成できるか
2. 敵店舗への脅威
   - 形成される三角形内に敵店舗があるか
3. 防御価値
   - 敵の三角形を阻止できるか
4. 拡張性
   - 将来の三角形形成に有利な位置か

# 外部インタフェース
- FilterCandidates(state, chain, actions, maxCount): 候補を絞り込み
- EvaluateAction(state, chain, action): 単一アクションを評価

# 期待値
- 有望な候補が上位に来る
- 計算コストがMCTSより低い

# エッジケース
- 候補数がmaxCount以下ならそのまま返す
