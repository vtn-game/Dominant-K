using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Rendering;
using DominantK.ECS.Components;

namespace DominantK.ECS.Systems
{
    /// <summary>
    /// コンビニエンスストアをGPU Instancingで大量スポーンするシステム
    /// </summary>
    [BurstCompile]
    public partial struct ConvenienceStoreSpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpawnStoreRequest>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (request, entity) in SystemAPI.Query<RefRO<SpawnStoreRequest>>().WithEntityAccess())
            {
                var storeEntity = ecb.Instantiate(request.ValueRO.Prefab);

                ecb.SetComponent(storeEntity, new LocalTransform
                {
                    Position = request.ValueRO.Position,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });

                ecb.SetComponent(storeEntity, new ConvenienceStoreComponent
                {
                    GridPosition = request.ValueRO.GridPosition,
                    OwnerChainId = request.ValueRO.OwnerChainId,
                    IsPlayerOwned = request.ValueRO.IsPlayerOwned,
                    CurrentFaith = 0f,
                    DominantCount = 0,
                    ZOCRadius = request.ValueRO.ZOCRadius,
                    DominantRadius = request.ValueRO.DominantRadius,
                    BaseRevenue = request.ValueRO.BaseRevenue
                });

                ecb.SetComponent(storeEntity, new StoreVisualData
                {
                    ChainColor = request.ValueRO.ChainColor
                });

                if (request.ValueRO.IsPlayerOwned)
                {
                    ecb.AddComponent<PlayerOwnedTag>(storeEntity);
                }
                else
                {
                    ecb.AddComponent<EnemyOwnedTag>(storeEntity);
                }

                ecb.DestroyEntity(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// ストアスポーンリクエストコンポーネント
    /// </summary>
    public struct SpawnStoreRequest : IComponentData
    {
        public Entity Prefab;
        public float3 Position;
        public int2 GridPosition;
        public int OwnerChainId;
        public bool IsPlayerOwned;
        public float ZOCRadius;
        public float DominantRadius;
        public int BaseRevenue;
        public float4 ChainColor;
    }
}
