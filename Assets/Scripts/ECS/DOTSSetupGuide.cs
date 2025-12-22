/*
================================================================================
DOTS & GPU Instancing セットアップガイド
================================================================================

【環境構築完了項目】
✓ Entities 1.3.10
✓ Entities.Graphics 1.3.10
✓ Assembly Definition に DOTS 参照追加
✓ ECS コンポーネント・システム基盤

================================================================================
【Unity Editor での設定手順】
================================================================================

1. SubScene の作成
   ─────────────────────────────────────────
   - Hierarchy で右クリック → New Sub Scene → Empty Scene
   - コンビニPrefabをSubScene内に配置
   - ConvenienceStoreAuthoring コンポーネントをアタッチ

2. Prefab の Baking 設定
   ─────────────────────────────────────────
   - コンビニPrefabを選択
   - MeshRenderer がアタッチされていることを確認
   - ConvenienceStoreAuthoring を追加
   - ゲームオブジェクトは SubScene 内に配置する

3. マテリアル設定 (GPU Instancing 対応)
   ─────────────────────────────────────────
   Project Settings → Graphics:
   - Instancing Variants: Keep All (推奨)

   マテリアル設定:
   - URP/Lit または URP/Simple Lit を使用
   - "Enable GPU Instancing" にチェック
   - MaterialPropertyBlock 使用可能

4. URP Renderer 設定
   ─────────────────────────────────────────
   URP Asset → Renderer:
   - SRP Batcher: ON (DOTS と併用可能)
   - Dynamic Batching: OFF (GPU Instancing 使用時は不要)

5. Entities Graphics 設定
   ─────────────────────────────────────────
   Edit → Project Settings → Entities:
   - Scene System: Enabled
   - Baking: Enabled

================================================================================
【使用方法】
================================================================================

方法1: SubScene を使用 (推奨)
─────────────────────────────────────────
SubScene内にConvenienceStoreAuthoringをアタッチしたオブジェクトを配置。
Play時に自動的にECSエンティティに変換され、GPU Instancingで描画。

方法2: ランタイムスポーン
─────────────────────────────────────────
StoreSpawnerHelper を使用してランタイムでスポーン:

    var spawner = GetComponent<StoreSpawnerHelper>();
    spawner.Initialize(prefabEntity);
    spawner.SpawnStore(position, gridPos, chainId, isPlayer, ...);

バッチスポーン (大量生成時):

    var stores = new StoreSpawnData[1000];
    // データ設定...
    spawner.SpawnStoresBatch(stores);

================================================================================
【パフォーマンス最適化】
================================================================================

- 同じメッシュ・マテリアルのオブジェクトは自動的にバッチ処理
- URPMaterialPropertyBaseColor で個別の色を適用可能
- Burst Compile で CPU 処理を最適化
- IJobEntity でマルチスレッド並列処理

【期待される描画数】
- 通常 GameObject: ~1,000 オブジェクト
- DOTS + GPU Instancing: ~100,000+ オブジェクト

================================================================================
*/

namespace DominantK.ECS
{
    /// <summary>
    /// このファイルはセットアップガイドです。
    /// 実際の機能は提供しません。
    /// </summary>
    public static class DOTSSetupGuide
    {
        public const string Version = "1.0.0";
    }
}
