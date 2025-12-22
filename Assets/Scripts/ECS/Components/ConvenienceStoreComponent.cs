using Unity.Entities;
using Unity.Mathematics;

namespace DominantK.ECS.Components
{
    /// <summary>
    /// コンビニエンスストアのECSコンポーネント
    /// GPU Instancingで大量描画するためのデータコンポーネント
    /// </summary>
    public struct ConvenienceStoreComponent : IComponentData
    {
        public int2 GridPosition;
        public int OwnerChainId;
        public bool IsPlayerOwned;
        public float CurrentFaith;
        public int DominantCount;
        public float ZOCRadius;
        public float DominantRadius;
        public int BaseRevenue;
    }

    /// <summary>
    /// ストアのビジュアル情報（マテリアルプロパティオーバーライド用）
    /// </summary>
    public struct StoreVisualData : IComponentData
    {
        public float4 ChainColor;
    }

    /// <summary>
    /// タグ：プレイヤー所有のストア
    /// </summary>
    public struct PlayerOwnedTag : IComponentData { }

    /// <summary>
    /// タグ：敵所有のストア
    /// </summary>
    public struct EnemyOwnedTag : IComponentData { }
}
