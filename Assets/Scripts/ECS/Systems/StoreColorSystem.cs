using Unity.Entities;
using Unity.Burst;
using Unity.Rendering;
using Unity.Mathematics;
using DominantK.ECS.Components;

namespace DominantK.ECS.Systems
{
    /// <summary>
    /// マテリアルプロパティをオーバーライドして各コンビニの色を設定
    /// GPU Instancingでも個別の色を適用可能にする
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct StoreColorSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new UpdateColorJob().ScheduleParallel();
        }
    }

    /// <summary>
    /// 色更新ジョブ（Burst最適化）
    /// </summary>
    [BurstCompile]
    public partial struct UpdateColorJob : IJobEntity
    {
        public void Execute(
            in StoreVisualData visualData,
            ref URPMaterialPropertyBaseColor baseColor)
        {
            baseColor.Value = visualData.ChainColor;
        }
    }
}
