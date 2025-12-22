using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using DominantK.Data;

namespace DominantK.AI
{
    /// <summary>
    /// ドミナント戦略に基づく盤面評価
    /// </summary>
    public class DominantEvaluator
    {
        // 評価重み
        private const float TRIANGLE_WEIGHT = 100f;
        private const float TRIANGLE_POTENTIAL_WEIGHT = 50f;
        private const float ENEMY_CAPTURE_WEIGHT = 200f;
        private const float TERRITORY_WEIGHT = 10f;
        private const float DISTANCE_PENALTY_WEIGHT = 5f;
        private const float CLUSTERING_BONUS = 20f;
        private const float ENEMY_DISRUPTION_WEIGHT = 80f;

        /// <summary>
        /// 盤面を評価
        /// </summary>
        public float Evaluate(BoardState state, ChainType myChain)
        {
            float score = 0f;

            var myStores = state.GetStoresByChain(myChain);
            var enemyStores = state.GetEnemyStores(myChain);

            // 1. 完成した三角形の評価
            score += EvaluateTriangles(myStores, enemyStores) * TRIANGLE_WEIGHT;

            // 2. 三角形形成ポテンシャル
            score += EvaluateTrianglePotential(myStores) * TRIANGLE_POTENTIAL_WEIGHT;

            // 3. 敵店舗の捕獲可能性
            score += EvaluateEnemyCapture(myStores, enemyStores) * ENEMY_CAPTURE_WEIGHT;

            // 4. 領域支配
            score += EvaluateTerritoryControl(myStores, state) * TERRITORY_WEIGHT;

            // 5. 店舗クラスタリング（ドミナント戦略の核心）
            score += EvaluateClustering(myStores) * CLUSTERING_BONUS;

            // 6. 敵の三角形妨害
            score += EvaluateEnemyDisruption(myStores, enemyStores) * ENEMY_DISRUPTION_WEIGHT;

            return score;
        }

        /// <summary>
        /// 配置位置を評価
        /// </summary>
        public float EvaluatePlacement(BoardState state, PlacementAction action, ChainType myChain)
        {
            var testState = state.Clone();
            var newStore = new StorePosition(
                -1, action.GridPosition, action.WorldPosition, myChain, false, 4f
            );
            testState.AddStore(newStore);

            float beforeScore = Evaluate(state, myChain);
            float afterScore = Evaluate(testState, myChain);

            return afterScore - beforeScore;
        }

        private float EvaluateTriangles(List<StorePosition> myStores, List<StorePosition> enemyStores)
        {
            if (myStores.Count < 3) return 0f;

            float score = 0f;
            var triangles = FindTriangles(myStores);

            foreach (var triangle in triangles)
            {
                // 三角形が存在すればボーナス
                score += 1f;

                // 三角形内に敵がいれば追加ボーナス
                foreach (var enemy in enemyStores)
                {
                    if (IsPointInTriangle(enemy.WorldPosition.xz, triangle))
                    {
                        score += 2f;
                    }
                }
            }

            return score;
        }

        private float EvaluateTrianglePotential(List<StorePosition> myStores)
        {
            if (myStores.Count < 2) return 0f;

            float score = 0f;

            // 2店舗があれば三角形のポテンシャル
            for (int i = 0; i < myStores.Count - 1; i++)
            {
                for (int j = i + 1; j < myStores.Count; j++)
                {
                    float dist = math.distance(myStores[i].WorldPosition, myStores[j].WorldPosition);
                    float maxRadius = myStores[i].DominantRadius + myStores[j].DominantRadius;

                    if (dist <= maxRadius * 2f)
                    {
                        // 接続可能な2店舗 = 三角形の辺
                        score += 1f;
                    }
                }
            }

            return score;
        }

        private float EvaluateEnemyCapture(List<StorePosition> myStores, List<StorePosition> enemyStores)
        {
            if (myStores.Count < 3 || enemyStores.Count == 0) return 0f;

            float score = 0f;
            var triangles = FindTriangles(myStores);

            foreach (var enemy in enemyStores)
            {
                foreach (var triangle in triangles)
                {
                    if (IsPointInTriangle(enemy.WorldPosition.xz, triangle))
                    {
                        score += 1f;
                    }
                }
            }

            return score;
        }

        private float EvaluateTerritoryControl(List<StorePosition> myStores, BoardState state)
        {
            if (myStores.Count == 0) return 0f;

            // 中心からの距離と散らばり具合を評価
            float2 center = new float2(state.Width / 2f, state.Height / 2f);
            float score = 0f;

            foreach (var store in myStores)
            {
                float distToCenter = math.distance(new float2(store.GridPosition.x, store.GridPosition.y), center);
                // 中心に近いほど高評価
                score += 1f / (1f + distToCenter * 0.1f);
            }

            return score;
        }

        private float EvaluateClustering(List<StorePosition> myStores)
        {
            if (myStores.Count < 2) return 0f;

            float score = 0f;

            // ドミナント戦略：店舗が適度にクラスタリングされているか
            for (int i = 0; i < myStores.Count; i++)
            {
                int nearbyCount = 0;
                for (int j = 0; j < myStores.Count; j++)
                {
                    if (i == j) continue;

                    float dist = math.distance(myStores[i].WorldPosition, myStores[j].WorldPosition);
                    float optimalDist = myStores[i].DominantRadius * 1.5f;

                    if (dist <= optimalDist * 2f)
                    {
                        nearbyCount++;
                        // 最適距離に近いほど高評価
                        float distScore = 1f - math.abs(dist - optimalDist) / optimalDist;
                        score += math.max(0f, distScore);
                    }
                }

                // 2-3店舗が近くにあるのが理想的（三角形形成可能）
                if (nearbyCount >= 2 && nearbyCount <= 4)
                {
                    score += 1f;
                }
            }

            return score;
        }

        private float EvaluateEnemyDisruption(List<StorePosition> myStores, List<StorePosition> enemyStores)
        {
            if (enemyStores.Count < 3) return 0f;

            float score = 0f;

            // 敵の三角形を検出
            var enemyTriangles = FindTrianglesByChain(enemyStores);

            foreach (var store in myStores)
            {
                foreach (var triangle in enemyTriangles)
                {
                    // 自店舗が敵三角形の辺の近くにあれば妨害効果
                    if (IsNearTriangleEdge(store.WorldPosition.xz, triangle, 3f))
                    {
                        score += 0.5f;
                    }

                    // 敵三角形を分断できる位置にあれば高評価
                    if (IsPointInTriangle(store.WorldPosition.xz, triangle))
                    {
                        // 自分が敵三角形内にいる = 敵三角形は無効化
                        score += 1f;
                    }
                }
            }

            return score;
        }

        private List<float2[]> FindTriangles(List<StorePosition> stores)
        {
            var triangles = new List<float2[]>();

            for (int i = 0; i < stores.Count - 2; i++)
            {
                for (int j = i + 1; j < stores.Count - 1; j++)
                {
                    for (int k = j + 1; k < stores.Count; k++)
                    {
                        var s1 = stores[i];
                        var s2 = stores[j];
                        var s3 = stores[k];

                        if (AreConnected(s1, s2) && AreConnected(s2, s3) && AreConnected(s1, s3))
                        {
                            triangles.Add(new float2[]
                            {
                                s1.WorldPosition.xz,
                                s2.WorldPosition.xz,
                                s3.WorldPosition.xz
                            });
                        }
                    }
                }
            }

            return triangles;
        }

        private List<float2[]> FindTrianglesByChain(List<StorePosition> stores)
        {
            // 同じチェーンの店舗でグループ化
            var byChain = new Dictionary<ChainType, List<StorePosition>>();
            foreach (var store in stores)
            {
                if (!byChain.ContainsKey(store.Chain))
                    byChain[store.Chain] = new List<StorePosition>();
                byChain[store.Chain].Add(store);
            }

            var allTriangles = new List<float2[]>();
            foreach (var chainStores in byChain.Values)
            {
                allTriangles.AddRange(FindTriangles(chainStores));
            }

            return allTriangles;
        }

        private bool AreConnected(StorePosition a, StorePosition b)
        {
            float dist = math.distance(a.WorldPosition, b.WorldPosition);
            float maxRadius = math.max(a.DominantRadius, b.DominantRadius);
            return dist <= maxRadius * 2f;
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

        private bool IsNearTriangleEdge(float2 point, float2[] triangle, float threshold)
        {
            for (int i = 0; i < 3; i++)
            {
                float2 a = triangle[i];
                float2 b = triangle[(i + 1) % 3];
                float dist = DistanceToLineSegment(point, a, b);
                if (dist <= threshold)
                    return true;
            }
            return false;
        }

        private float DistanceToLineSegment(float2 p, float2 a, float2 b)
        {
            float2 ab = b - a;
            float2 ap = p - a;
            float t = math.clamp(math.dot(ap, ab) / math.dot(ab, ab), 0f, 1f);
            float2 closest = a + t * ab;
            return math.distance(p, closest);
        }
    }
}
