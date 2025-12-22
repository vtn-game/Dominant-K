using System.Collections.Generic;
using Unity.Mathematics;

namespace DominantK.Systems.CityGeneration
{
    /// <summary>
    /// 街生成アルゴリズムの共通インターフェース
    /// </summary>
    public interface ICityGenerator
    {
        /// <summary>
        /// 街を生成する
        /// </summary>
        CityData Generate(CityGenerationSettings settings);
    }

    /// <summary>
    /// 街生成の設定
    /// </summary>
    [System.Serializable]
    public class CityGenerationSettings
    {
        public int Width = 100;
        public int Height = 100;
        public int Seed = 12345;
        public float CellSize = 1f;

        // ボロノイ用
        public int VoronoiSiteCount = 20;
        public float VoronoiRelaxationIterations = 3;

        // BSP用
        public int BspMinRoomSize = 10;
        public int BspMaxRoomSize = 30;
        public int BspMinDepth = 3;
        public int BspMaxDepth = 5;

        // 日本風用
        public float MainRoadWidth = 3f;
        public float SubRoadWidth = 1.5f;
        public int BlockCountX = 5;
        public int BlockCountY = 5;
    }

    /// <summary>
    /// 生成された街のデータ
    /// </summary>
    public class CityData
    {
        public int Width;
        public int Height;
        public CellType[,] Cells;
        public List<District> Districts;
        public List<Road> Roads;
        public List<int2> BuildingSlots;

        public CityData(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new CellType[width, height];
            Districts = new List<District>();
            Roads = new List<Road>();
            BuildingSlots = new List<int2>();
        }
    }

    /// <summary>
    /// セルの種類
    /// </summary>
    public enum CellType
    {
        Empty,
        Road,
        Building,
        Park,
        Water,
        Commercial,
        Residential,
        Industrial
    }

    /// <summary>
    /// 地区情報
    /// </summary>
    public class District
    {
        public int Id;
        public CellType Type;
        public List<int2> Cells;
        public float2 Center;
        public float Area;

        public District(int id)
        {
            Id = id;
            Cells = new List<int2>();
        }
    }

    /// <summary>
    /// 道路情報
    /// </summary>
    public class Road
    {
        public List<float2> Points;
        public float Width;
        public bool IsMainRoad;

        public Road()
        {
            Points = new List<float2>();
        }
    }
}
