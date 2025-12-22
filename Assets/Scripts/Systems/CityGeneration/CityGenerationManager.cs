using UnityEngine;
using Unity.Mathematics;

namespace DominantK.Systems.CityGeneration
{
    /// <summary>
    /// 街生成アルゴリズムを統合管理するマネージャー
    /// </summary>
    public class CityGenerationManager : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private CityGeneratorType generatorType = CityGeneratorType.Japanese;
        [SerializeField] private CityGenerationSettings settings = new CityGenerationSettings();

        [Header("Debug Visualization")]
        [SerializeField] private bool showDebugVisualization = true;
        [SerializeField] private float cellVisualizationSize = 0.9f;

        private CityData currentCityData;
        private ICityGenerator currentGenerator;

        /// <summary>
        /// 生成タイプ
        /// </summary>
        public enum CityGeneratorType
        {
            Voronoi,
            BSP,
            Japanese
        }

        /// <summary>
        /// 現在の街データ
        /// </summary>
        public CityData CurrentCityData => currentCityData;

        private void Awake()
        {
            CreateGenerator();
        }

        private void CreateGenerator()
        {
            currentGenerator = generatorType switch
            {
                CityGeneratorType.Voronoi => new VoronoiCityGenerator(),
                CityGeneratorType.BSP => new BSPCityGenerator(),
                CityGeneratorType.Japanese => new JapaneseCityGenerator(),
                _ => new JapaneseCityGenerator()
            };
        }

        /// <summary>
        /// 街を生成
        /// </summary>
        public CityData GenerateCity()
        {
            CreateGenerator();
            currentCityData = currentGenerator.Generate(settings);
            Debug.Log($"City generated: {currentCityData.Districts.Count} districts, {currentCityData.Roads.Count} roads, {currentCityData.BuildingSlots.Count} building slots");
            return currentCityData;
        }

        /// <summary>
        /// 指定したタイプで街を生成
        /// </summary>
        public CityData GenerateCity(CityGeneratorType type)
        {
            generatorType = type;
            return GenerateCity();
        }

        /// <summary>
        /// 新しいシードで再生成
        /// </summary>
        public CityData RegenerateWithNewSeed()
        {
            settings.Seed = UnityEngine.Random.Range(0, int.MaxValue);
            return GenerateCity();
        }

        /// <summary>
        /// 指定位置のセルタイプを取得
        /// </summary>
        public CellType GetCellType(int x, int y)
        {
            if (currentCityData == null) return CellType.Empty;
            if (x < 0 || x >= currentCityData.Width || y < 0 || y >= currentCityData.Height)
                return CellType.Empty;

            return currentCityData.Cells[x, y];
        }

        /// <summary>
        /// ワールド座標からセル座標に変換
        /// </summary>
        public int2 WorldToCell(Vector3 worldPos)
        {
            return new int2(
                Mathf.RoundToInt(worldPos.x / settings.CellSize),
                Mathf.RoundToInt(worldPos.z / settings.CellSize)
            );
        }

        /// <summary>
        /// セル座標からワールド座標に変換
        /// </summary>
        public Vector3 CellToWorld(int2 cell)
        {
            return new Vector3(
                cell.x * settings.CellSize,
                0f,
                cell.y * settings.CellSize
            );
        }

        /// <summary>
        /// 建物スロットを取得
        /// </summary>
        public int2[] GetBuildingSlots()
        {
            return currentCityData?.BuildingSlots.ToArray() ?? new int2[0];
        }

        /// <summary>
        /// 指定タイプの地区を取得
        /// </summary>
        public District[] GetDistrictsByType(CellType type)
        {
            if (currentCityData == null) return new District[0];

            return currentCityData.Districts.FindAll(d => d.Type == type).ToArray();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showDebugVisualization || currentCityData == null) return;

            float size = cellVisualizationSize * settings.CellSize;

            for (int x = 0; x < currentCityData.Width; x++)
            {
                for (int y = 0; y < currentCityData.Height; y++)
                {
                    CellType type = currentCityData.Cells[x, y];
                    Gizmos.color = GetColorForCellType(type);

                    Vector3 pos = CellToWorld(new int2(x, y));
                    Gizmos.DrawCube(pos, new Vector3(size, 0.1f, size));
                }
            }

            // 建物スロットを表示
            Gizmos.color = Color.yellow;
            foreach (var slot in currentCityData.BuildingSlots)
            {
                Vector3 pos = CellToWorld(slot);
                Gizmos.DrawWireCube(pos + Vector3.up * 0.5f, new Vector3(size, 1f, size));
            }
        }

        private Color GetColorForCellType(CellType type)
        {
            return type switch
            {
                CellType.Road => new Color(0.3f, 0.3f, 0.3f, 0.8f),
                CellType.Commercial => new Color(1f, 0.5f, 0f, 0.6f),
                CellType.Residential => new Color(0.2f, 0.6f, 0.2f, 0.6f),
                CellType.Industrial => new Color(0.5f, 0.5f, 0.7f, 0.6f),
                CellType.Park => new Color(0.1f, 0.8f, 0.1f, 0.6f),
                CellType.Water => new Color(0.2f, 0.4f, 0.8f, 0.6f),
                CellType.Building => new Color(0.6f, 0.4f, 0.3f, 0.6f),
                _ => new Color(0.5f, 0.5f, 0.5f, 0.3f)
            };
        }
#endif
    }
}
