using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using DominantK.ECS.Systems;

namespace DominantK.ECS
{
    /// <summary>
    /// MonoBehaviourからECSエンティティをスポーンするためのヘルパー
    /// GPU Instancingで大量のコンビニを描画可能にする
    /// </summary>
    public class StoreSpawnerHelper : MonoBehaviour
    {
        [Header("Prefab (ECS Baked)")]
        [SerializeField] private GameObject storePrefab;

        private Entity prefabEntity;
        private EntityManager entityManager;
        private bool isInitialized;

        private void Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;

            entityManager = world.EntityManager;

            // SubSceneでBakeされたPrefabを取得する場合は
            // EntityPrefabReferenceを使用
        }

        /// <summary>
        /// EntityPrefabReferenceから初期化
        /// </summary>
        public void Initialize(Entity prefab)
        {
            prefabEntity = prefab;
            isInitialized = true;
        }

        /// <summary>
        /// コンビニをスポーン（GPU Instancing対応）
        /// </summary>
        public void SpawnStore(
            Vector3 position,
            Vector2Int gridPosition,
            int ownerChainId,
            bool isPlayerOwned,
            float zocRadius,
            float dominantRadius,
            int baseRevenue,
            Color chainColor)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("StoreSpawnerHelper not initialized with prefab entity");
                return;
            }

            var requestEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(requestEntity, new SpawnStoreRequest
            {
                Prefab = prefabEntity,
                Position = new float3(position.x, position.y, position.z),
                GridPosition = new int2(gridPosition.x, gridPosition.y),
                OwnerChainId = ownerChainId,
                IsPlayerOwned = isPlayerOwned,
                ZOCRadius = zocRadius,
                DominantRadius = dominantRadius,
                BaseRevenue = baseRevenue,
                ChainColor = new float4(chainColor.r, chainColor.g, chainColor.b, chainColor.a)
            });
        }

        /// <summary>
        /// 複数のコンビニを一括スポーン（最適化版）
        /// </summary>
        public void SpawnStoresBatch(StoreSpawnData[] stores)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("StoreSpawnerHelper not initialized with prefab entity");
                return;
            }

            foreach (var store in stores)
            {
                var requestEntity = entityManager.CreateEntity();
                entityManager.AddComponentData(requestEntity, new SpawnStoreRequest
                {
                    Prefab = prefabEntity,
                    Position = store.Position,
                    GridPosition = store.GridPosition,
                    OwnerChainId = store.OwnerChainId,
                    IsPlayerOwned = store.IsPlayerOwned,
                    ZOCRadius = store.ZOCRadius,
                    DominantRadius = store.DominantRadius,
                    BaseRevenue = store.BaseRevenue,
                    ChainColor = store.ChainColor
                });
            }
        }
    }

    /// <summary>
    /// バッチスポーン用のデータ構造体
    /// </summary>
    public struct StoreSpawnData
    {
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
