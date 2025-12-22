using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using DominantK.ECS.Components;

namespace DominantK.ECS.Authoring
{
    /// <summary>
    /// コンビニエンスストアのオーサリングコンポーネント
    /// MonoBehaviourからECSエンティティへ変換するためのBaker
    /// </summary>
    public class ConvenienceStoreAuthoring : MonoBehaviour
    {
        [Header("Store Settings")]
        public Vector2Int gridPosition;
        public int ownerChainId;
        public bool isPlayerOwned;
        public float zocRadius = 5f;
        public float dominantRadius = 3f;
        public int baseRevenue = 100;

        [Header("Visual")]
        public Color chainColor = Color.white;

        private class Baker : Baker<ConvenienceStoreAuthoring>
        {
            public override void Bake(ConvenienceStoreAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);

                AddComponent(entity, new ConvenienceStoreComponent
                {
                    GridPosition = new int2(authoring.gridPosition.x, authoring.gridPosition.y),
                    OwnerChainId = authoring.ownerChainId,
                    IsPlayerOwned = authoring.isPlayerOwned,
                    CurrentFaith = 0f,
                    DominantCount = 0,
                    ZOCRadius = authoring.zocRadius,
                    DominantRadius = authoring.dominantRadius,
                    BaseRevenue = authoring.baseRevenue
                });

                AddComponent(entity, new StoreVisualData
                {
                    ChainColor = new float4(
                        authoring.chainColor.r,
                        authoring.chainColor.g,
                        authoring.chainColor.b,
                        authoring.chainColor.a
                    )
                });

                if (authoring.isPlayerOwned)
                {
                    AddComponent<PlayerOwnedTag>(entity);
                }
                else
                {
                    AddComponent<EnemyOwnedTag>(entity);
                }
            }
        }
    }
}
