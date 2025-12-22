using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using DominantK.Data;

namespace DominantK.AI
{
    /// <summary>
    /// AIが評価するための盤面状態
    /// </summary>
    public class BoardState
    {
        public int Width;
        public int Height;
        public List<StorePosition> Stores;
        public HashSet<int2> OccupiedCells;
        public HashSet<int2> AvailableCells;

        public BoardState(int width, int height)
        {
            Width = width;
            Height = height;
            Stores = new List<StorePosition>();
            OccupiedCells = new HashSet<int2>();
            AvailableCells = new HashSet<int2>();
        }

        public BoardState Clone()
        {
            var clone = new BoardState(Width, Height);
            clone.Stores = new List<StorePosition>(Stores);
            clone.OccupiedCells = new HashSet<int2>(OccupiedCells);
            clone.AvailableCells = new HashSet<int2>(AvailableCells);
            return clone;
        }

        public void AddStore(StorePosition store)
        {
            Stores.Add(store);
            OccupiedCells.Add(store.GridPosition);
            AvailableCells.Remove(store.GridPosition);
        }

        public void RemoveStore(StorePosition store)
        {
            Stores.Remove(store);
            OccupiedCells.Remove(store.GridPosition);
            AvailableCells.Add(store.GridPosition);
        }

        public List<StorePosition> GetStoresByChain(ChainType chain)
        {
            return Stores.FindAll(s => s.Chain == chain);
        }

        public List<StorePosition> GetEnemyStores(ChainType myChain)
        {
            return Stores.FindAll(s => s.Chain != myChain);
        }
    }

    /// <summary>
    /// 店舗の位置情報
    /// </summary>
    public struct StorePosition
    {
        public int Id;
        public int2 GridPosition;
        public float3 WorldPosition;
        public ChainType Chain;
        public bool IsPlayerOwned;
        public float DominantRadius;

        public StorePosition(int id, int2 gridPos, float3 worldPos, ChainType chain, bool isPlayer, float dominantRadius)
        {
            Id = id;
            GridPosition = gridPos;
            WorldPosition = worldPos;
            Chain = chain;
            IsPlayerOwned = isPlayer;
            DominantRadius = dominantRadius;
        }
    }

    /// <summary>
    /// AIの行動（配置）
    /// </summary>
    public struct PlacementAction
    {
        public int2 GridPosition;
        public float3 WorldPosition;
        public ChainType Chain;
        public float Score;

        public static PlacementAction Invalid => new PlacementAction { GridPosition = new int2(-1, -1), Score = float.MinValue };

        public bool IsValid => GridPosition.x >= 0 && GridPosition.y >= 0;
    }
}
