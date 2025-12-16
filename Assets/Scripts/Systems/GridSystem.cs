using System.Collections.Generic;
using UnityEngine;
using DominantK.Entities;

namespace DominantK.Systems
{
    public enum CellType
    {
        Empty,
        Road,
        Building,
        ConvenienceStore,
        Station
    }

    [System.Serializable]
    public class GridCell
    {
        public Vector2Int position;
        public CellType cellType;
        public GameObject occupant;
        public Building building;
        public ConvenienceStore store;

        public bool IsEmpty => cellType == CellType.Empty || cellType == CellType.Road;
        public bool HasBuilding => cellType == CellType.Building && building != null;
        public bool HasStore => cellType == CellType.ConvenienceStore && store != null;
    }

    public class GridSystem : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 50;
        [SerializeField] private int gridHeight = 50;
        [SerializeField] private float cellSize = 1f;

        [Header("Generation")]
        [SerializeField] private Vector2Int stationPosition = new Vector2Int(25, 25);
        [SerializeField] private float buildingDensity = 0.6f;
        [SerializeField] private int roadWidth = 1;
        [SerializeField] private int blockSize = 5;

        [Header("Prefabs")]
        [SerializeField] private GameObject groundPrefab;
        [SerializeField] private GameObject roadPrefab;
        [SerializeField] private GameObject[] buildingPrefabs;
        [SerializeField] private GameObject stationPrefab;

        private GridCell[,] grid;
        private Transform gridParent;

        public int Width => gridWidth;
        public int Height => gridHeight;
        public float CellSize => cellSize;

        public void Initialize()
        {
            CreateGrid();
            GenerateCity();
        }

        private void CreateGrid()
        {
            grid = new GridCell[gridWidth, gridHeight];
            gridParent = new GameObject("Grid").transform;
            gridParent.SetParent(transform);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    grid[x, z] = new GridCell
                    {
                        position = new Vector2Int(x, z),
                        cellType = CellType.Empty
                    };
                }
            }
        }

        private void GenerateCity()
        {
            // Place station at center
            PlaceStation();

            // Generate road grid
            GenerateRoads();

            // Fill blocks with buildings
            GenerateBuildings();
        }

        private void PlaceStation()
        {
            var cell = GetCell(stationPosition);
            if (cell != null)
            {
                cell.cellType = CellType.Station;
                if (stationPrefab != null)
                {
                    cell.occupant = Instantiate(stationPrefab, GetWorldPosition(stationPosition), Quaternion.identity, gridParent);
                }
            }
        }

        private void GenerateRoads()
        {
            // Generate grid-pattern roads
            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    bool isRoadX = (x % (blockSize + roadWidth)) < roadWidth;
                    bool isRoadZ = (z % (blockSize + roadWidth)) < roadWidth;

                    if (isRoadX || isRoadZ)
                    {
                        var cell = GetCell(x, z);
                        if (cell != null && cell.cellType != CellType.Station)
                        {
                            cell.cellType = CellType.Road;
                            if (roadPrefab != null)
                            {
                                cell.occupant = Instantiate(roadPrefab, GetWorldPosition(x, z), Quaternion.identity, gridParent);
                            }
                        }
                    }
                }
            }
        }

        private void GenerateBuildings()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    var cell = GetCell(x, z);
                    if (cell != null && cell.cellType == CellType.Empty)
                    {
                        if (Random.value < buildingDensity && buildingPrefabs != null && buildingPrefabs.Length > 0)
                        {
                            cell.cellType = CellType.Building;
                            var prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
                            cell.occupant = Instantiate(prefab, GetWorldPosition(x, z), Quaternion.identity, gridParent);
                            cell.building = cell.occupant.GetComponent<Building>();
                            if (cell.building != null)
                            {
                                cell.building.Initialize(cell.position);
                            }
                        }
                    }
                }
            }
        }

        public GridCell GetCell(int x, int z)
        {
            if (x < 0 || x >= gridWidth || z < 0 || z >= gridHeight)
                return null;
            return grid[x, z];
        }

        public GridCell GetCell(Vector2Int pos)
        {
            return GetCell(pos.x, pos.y);
        }

        public GridCell GetCellFromWorldPosition(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x / cellSize);
            int z = Mathf.FloorToInt(worldPos.z / cellSize);
            return GetCell(x, z);
        }

        public Vector3 GetWorldPosition(int x, int z)
        {
            return new Vector3(x * cellSize + cellSize / 2f, 0, z * cellSize + cellSize / 2f);
        }

        public Vector3 GetWorldPosition(Vector2Int pos)
        {
            return GetWorldPosition(pos.x, pos.y);
        }

        public List<GridCell> GetCellsInRadius(Vector2Int center, float radius)
        {
            var result = new List<GridCell>();
            int radiusCells = Mathf.CeilToInt(radius / cellSize);

            for (int x = center.x - radiusCells; x <= center.x + radiusCells; x++)
            {
                for (int z = center.y - radiusCells; z <= center.y + radiusCells; z++)
                {
                    var cell = GetCell(x, z);
                    if (cell != null)
                    {
                        float distance = Vector2Int.Distance(center, new Vector2Int(x, z)) * cellSize;
                        if (distance <= radius)
                        {
                            result.Add(cell);
                        }
                    }
                }
            }

            return result;
        }

        public bool DestroyBuilding(Vector2Int position)
        {
            var cell = GetCell(position);
            if (cell == null || !cell.HasBuilding) return false;

            if (cell.occupant != null)
            {
                Destroy(cell.occupant);
            }

            cell.cellType = CellType.Empty;
            cell.building = null;
            cell.occupant = null;

            return true;
        }

        public bool PlaceStore(Vector2Int position, ConvenienceStore store)
        {
            var cell = GetCell(position);
            if (cell == null) return false;

            // Can only place on empty or building cells
            if (cell.cellType != CellType.Empty && cell.cellType != CellType.Building)
                return false;

            // Destroy existing building if present
            if (cell.HasBuilding)
            {
                DestroyBuilding(position);
            }

            cell.cellType = CellType.ConvenienceStore;
            cell.store = store;
            cell.occupant = store.gameObject;

            return true;
        }
    }
}
