using System.Collections.Generic;
using Unity.Mathematics;
using DominantK.Data;

namespace DominantK.AI
{
    /// <summary>
    /// ドミナント戦略に基づく配置ヒューリスティック
    /// MCTSの補助として、有望な候補を絞り込む
    /// </summary>
    public class DominantStrategy
    {
        private readonly DominantEvaluator evaluator;

        public DominantStrategy()
        {
            evaluator = new DominantEvaluator();
        }

        /// <summary>
        /// 戦略に基づいて候補を絞り込み
        /// </summary>
        public List<PlacementAction> FilterCandidates(
            BoardState state,
            ChainType myChain,
            List<PlacementAction> allCandidates,
            int maxCandidates = 20)
        {
            if (allCandidates.Count <= maxCandidates)
                return allCandidates;

            var myStores = state.GetStoresByChain(myChain);
            var enemyStores = state.GetEnemyStores(myChain);

            // 各候補にスコアを付ける
            var scoredCandidates = new List<(PlacementAction action, float score)>();

            foreach (var candidate in allCandidates)
            {
                float score = ScoreCandidate(candidate, myStores, enemyStores, state);
                scoredCandidates.Add((candidate, score));
            }

            // スコア順にソート
            scoredCandidates.Sort((a, b) => b.score.CompareTo(a.score));

            // 上位を返す
            var result = new List<PlacementAction>();
            for (int i = 0; i < math.min(maxCandidates, scoredCandidates.Count); i++)
            {
                result.Add(scoredCandidates[i].action);
            }

            return result;
        }

        private float ScoreCandidate(
            PlacementAction candidate,
            List<StorePosition> myStores,
            List<StorePosition> enemyStores,
            BoardState state)
        {
            float score = 0f;
            float2 pos = new float2(candidate.GridPosition.x, candidate.GridPosition.y);

            // 1. 三角形完成ボーナス
            score += EvaluateTriangleCompletion(candidate, myStores) * 100f;

            // 2. 既存店舗との距離（ドミナント戦略：近すぎず遠すぎず）
            score += EvaluateDistanceToFriendly(candidate, myStores) * 30f;

            // 3. 敵店舗を囲う位置
            score += EvaluateEncirclement(candidate, myStores, enemyStores) * 80f;

            // 4. 敵三角形の妨害
            score += EvaluateEnemyTriangleDisruption(candidate, enemyStores) * 60f;

            // 5. 領域拡大
            score += EvaluateTerritoryExpansion(candidate, myStores, state) * 20f;

            return score;
        }

        private float EvaluateTriangleCompletion(PlacementAction candidate, List<StorePosition> myStores)
        {
            if (myStores.Count < 2) return 0f;

            float3 candidatePos = candidate.WorldPosition;
            float dominantRadius = 4f;

            int connectionCount = 0;

            // 接続可能な店舗ペアを数える
            for (int i = 0; i < myStores.Count; i++)
            {
                float distI = math.distance(candidatePos, myStores[i].WorldPosition);
                if (distI > dominantRadius * 2f) continue;

                for (int j = i + 1; j < myStores.Count; j++)
                {
                    float distJ = math.distance(candidatePos, myStores[j].WorldPosition);
                    float distIJ = math.distance(myStores[i].WorldPosition, myStores[j].WorldPosition);

                    if (distJ <= dominantRadius * 2f && distIJ <= dominantRadius * 2f)
                    {
                        // 三角形が完成する
                        return 1f;
                    }
                }

                connectionCount++;
            }

            // 2店舗と接続可能 = 三角形の一辺を形成
            if (connectionCount >= 2) return 0.5f;
            if (connectionCount >= 1) return 0.2f;

            return 0f;
        }

        private float EvaluateDistanceToFriendly(PlacementAction candidate, List<StorePosition> myStores)
        {
            if (myStores.Count == 0) return 0.5f;

            float3 candidatePos = candidate.WorldPosition;
            float optimalDistance = 6f; // 最適距離
            float minDistance = float.MaxValue;

            foreach (var store in myStores)
            {
                float dist = math.distance(candidatePos, store.WorldPosition);
                minDistance = math.min(minDistance, dist);
            }

            // 最適距離との差を評価
            float deviation = math.abs(minDistance - optimalDistance);
            return 1f / (1f + deviation * 0.2f);
        }

        private float EvaluateEncirclement(
            PlacementAction candidate,
            List<StorePosition> myStores,
            List<StorePosition> enemyStores)
        {
            if (myStores.Count < 2 || enemyStores.Count == 0) return 0f;

            float3 candidatePos = candidate.WorldPosition;
            float dominantRadius = 4f;
            float score = 0f;

            // この候補と既存2店舗で形成できる三角形を検索
            for (int i = 0; i < myStores.Count; i++)
            {
                for (int j = i + 1; j < myStores.Count; j++)
                {
                    var s1 = myStores[i];
                    var s2 = myStores[j];

                    float d1 = math.distance(candidatePos, s1.WorldPosition);
                    float d2 = math.distance(candidatePos, s2.WorldPosition);
                    float d12 = math.distance(s1.WorldPosition, s2.WorldPosition);

                    if (d1 > dominantRadius * 2f || d2 > dominantRadius * 2f || d12 > dominantRadius * 2f)
                        continue;

                    // 三角形内に敵がいるかチェック
                    float2[] triangle = new float2[]
                    {
                        candidatePos.xz,
                        s1.WorldPosition.xz,
                        s2.WorldPosition.xz
                    };

                    foreach (var enemy in enemyStores)
                    {
                        if (IsPointInTriangle(enemy.WorldPosition.xz, triangle))
                        {
                            score += 1f;
                        }
                    }
                }
            }

            return score;
        }

        private float EvaluateEnemyTriangleDisruption(PlacementAction candidate, List<StorePosition> enemyStores)
        {
            if (enemyStores.Count < 3) return 0f;

            float3 candidatePos = candidate.WorldPosition;
            float dominantRadius = 4f;
            float score = 0f;

            // 敵の三角形を検出
            for (int i = 0; i < enemyStores.Count - 2; i++)
            {
                for (int j = i + 1; j < enemyStores.Count - 1; j++)
                {
                    for (int k = j + 1; k < enemyStores.Count; k++)
                    {
                        if (enemyStores[i].Chain != enemyStores[j].Chain ||
                            enemyStores[j].Chain != enemyStores[k].Chain)
                            continue;

                        var e1 = enemyStores[i];
                        var e2 = enemyStores[j];
                        var e3 = enemyStores[k];

                        float d12 = math.distance(e1.WorldPosition, e2.WorldPosition);
                        float d23 = math.distance(e2.WorldPosition, e3.WorldPosition);
                        float d13 = math.distance(e1.WorldPosition, e3.WorldPosition);

                        if (d12 > dominantRadius * 2f || d23 > dominantRadius * 2f || d13 > dominantRadius * 2f)
                            continue;

                        // 候補がこの三角形内にあれば妨害
                        float2[] triangle = new float2[]
                        {
                            e1.WorldPosition.xz,
                            e2.WorldPosition.xz,
                            e3.WorldPosition.xz
                        };

                        if (IsPointInTriangle(candidatePos.xz, triangle))
                        {
                            score += 1f;
                        }
                    }
                }
            }

            return score;
        }

        private float EvaluateTerritoryExpansion(PlacementAction candidate, List<StorePosition> myStores, BoardState state)
        {
            if (myStores.Count == 0)
            {
                // 最初の店舗は中心近くがよい
                float2 center = new float2(state.Width / 2f, state.Height / 2f);
                float2 pos = new float2(candidate.GridPosition.x, candidate.GridPosition.y);
                float distToCenter = math.distance(pos, center);
                return 1f / (1f + distToCenter * 0.05f);
            }

            // 既存の領域を拡大する方向を評価
            float2 myCenter = float2.zero;
            foreach (var store in myStores)
            {
                myCenter += new float2(store.GridPosition.x, store.GridPosition.y);
            }
            myCenter /= myStores.Count;

            float2 candidatePos = new float2(candidate.GridPosition.x, candidate.GridPosition.y);
            float2 mapCenter = new float2(state.Width / 2f, state.Height / 2f);

            // 自陣から未制覇の方向へ拡大
            float2 expansionDir = math.normalize(mapCenter - myCenter);
            float2 candidateDir = math.normalize(candidatePos - myCenter);

            float alignment = math.dot(expansionDir, candidateDir);
            return (alignment + 1f) * 0.5f; // 0-1に正規化
        }

        private bool IsPointInTriangle(float2 p, float2[] triangle)
        {
            float2 a = triangle[0];
            float2 b = triangle[1];
            float2 c = triangle[2];

            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(hasNeg && hasPos);
        }

        private float Sign(float2 p1, float2 p2, float2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }
    }
}
