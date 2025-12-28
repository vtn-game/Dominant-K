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
        [SerializeField] private int gridWidth = 30;
        [SerializeField] private int gridHeight = 30;
        [SerializeField] private float cellSize = 1f;

        [Header("Generation")]
        [SerializeField] private float buildingDensity = 0.5f;
        [SerializeField] private int roadWidth = 1;
        [SerializeField] private int blockSize = 5;

        [Header("Prefabs (Optional)")]
        [SerializeField] private GameObject groundPrefab;
        [SerializeField] private GameObject roadPrefab;
        [SerializeField] private GameObject[] buildingPrefabs;
        [SerializeField] private GameObject stationPrefab;

        [Header("Runtime Materials")]
        [SerializeField] private Material roadMaterial;
        [SerializeField] private Material buildingMaterial;
        [SerializeField] private Material stationMaterial;

        private GridCell[,] grid;
        private Transform gridParent;
        private Vector2Int stationPosition;

        public int Width => gridWidth;
        public int Height => gridHeight;
        public float CellSize => cellSize;

        public void Setup(int width, int height, float cell)
        {
            gridWidth = width;
            gridHeight = height;
            cellSize = cell;
            stationPosition = new Vector2Int(width / 2, height / 2);
        }

        public void Initialize()
        {
            CreateMaterials();
            CreateGrid();
            GenerateCity();
        }

        private void CreateMaterials()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (roadMaterial == null)
            {
                roadMaterial = new Material(shader);
                roadMaterial.color = new Color(0.25f, 0.25f, 0.25f);
            }

            if (buildingMaterial == null)
            {
                buildingMaterial = new Material(shader);
                buildingMaterial.color = new Color(0.6f, 0.55f, 0.5f);
            }

            if (stationMaterial == null)
            {
                stationMaterial = new Material(shader);
                stationMaterial.color = new Color(0.8f, 0.7f, 0.3f);
            }
        }

        private void CreateGrid()
        {
            grid = new GridCell[gridWidth, gridHeight];
            gridParent = new GameObject("GridObjects").transform;
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
            if (cell == null) return;

            cell.cellType = CellType.Station;

            if (stationPrefab != null)
            {
                cell.occupant = Instantiate(stationPrefab, GetWorldPosition(stationPosition), Quaternion.identity, gridParent);
            }
            else
            {
                // Create station from primitives
                var station = CreateStationPrimitive(stationPosition);
                cell.occupant = station;
            }
        }

        private GameObject CreateStationPrimitive(Vector2Int pos)
        {
            var station = new GameObject("Station");
            station.transform.SetParent(gridParent);
            station.transform.position = GetWorldPosition(pos);

            // Main building
            var mainBuilding = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainBuilding.transform.SetParent(station.transform);
            mainBuilding.transform.localPosition = new Vector3(0, 0.75f, 0);
            mainBuilding.transform.localScale = new Vector3(cellSize * 2f, 1.5f, cellSize * 2f);
            mainBuilding.GetComponent<MeshRenderer>().material = stationMaterial;

            // Roof
            var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.transform.SetParent(station.transform);
            roof.transform.localPosition = new Vector3(0, 1.6f, 0);
            roof.transform.localScale = new Vector3(cellSize * 2.2f, 0.2f, cellSize * 2.2f);
            roof.GetComponent<MeshRenderer>().material = stationMaterial;

            return station;
        }

        private void GenerateRoads()
        {
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
                            else
                            {
                                cell.occupant = CreateRoadPrimitive(x, z);
                            }
                        }
                    }
                }
            }
        }

        private GameObject CreateRoadPrimitive(int x, int z)
        {
            var road = GameObject.CreatePrimitive(PrimitiveType.Quad);
            road.name = $"Road_{x}_{z}";
            road.transform.SetParent(gridParent);
            road.transform.position = GetWorldPosition(x, z) + Vector3.up * 0.01f;
            road.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            road.transform.localScale = new Vector3(cellSize, cellSize, 1f);
            road.GetComponent<MeshRenderer>().material = roadMaterial;

            // Remove collider from road
            var collider = road.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            return road;
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
                        // Skip area around station
                        float distToStation = Vector2Int.Distance(new Vector2Int(x, z), stationPosition);
                        if (distToStation < 3) continue;

                        if (Random.value < buildingDensity)
                        {
                            cell.cellType = CellType.Building;

                            if (buildingPrefabs != null && buildingPrefabs.Length > 0)
                            {
                                var prefab = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];
                                cell.occupant = Instantiate(prefab, GetWorldPosition(x, z), Quaternion.identity, gridParent);
                            }
                            else
                            {
                                cell.occupant = CreateBuildingPrimitive(x, z);
                            }

                            cell.building = cell.occupant.GetComponent<Building>();
                            if (cell.building == null)
                            {
                                cell.building = cell.occupant.AddComponent<Building>();
                            }
                            cell.building.Initialize(cell.position);
                        }
                    }
                }
            }
        }

        private GameObject CreateBuildingPrimitive(int x, int z)
        {
            var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = $"Building_{x}_{z}";
            building.transform.SetParent(gridParent);

            float height = Random.Range(0.5f, 2f);
            building.transform.position = GetWorldPosition(x, z) + Vector3.up * (height / 2f);
            building.transform.localScale = new Vector3(cellSize * 0.8f, height, cellSize * 0.8f);

            var renderer = building.GetComponent<MeshRenderer>();
            var mat = new Material(buildingMaterial);
            // Vary color slightly
            float variation = Random.Range(-0.1f, 0.1f);
            mat.color = new Color(
                buildingMaterial.color.r + variation,
                buildingMaterial.color.g + variation,
                buildingMaterial.color.b + variation
            );
            renderer.material = mat;

            return building;
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
            // NOTE: GetWorldPositionはセルの中心座標を返すため、
            // ワールド座標からセル座標への変換時もセル中心を基準にする
            int x = Mathf.RoundToInt(worldPos.x / cellSize - 0.5f);
            int z = Mathf.RoundToInt(worldPos.z / cellSize - 0.5f);
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

            if (cell.building != null)
            {
                cell.building.OnDestroyed();
            }

            if (cell.occupant != null)
            {
                Destroy(cell.occupant);
            }

            cell.cellType = CellType.Empty;
            cell.building = null;
            cell.occupant = null;

            return true;
        }

        public bool IsValidPlacement(Vector2Int position)
        {
            var cell = GetCell(position);
            if (cell == null) return false;

            // Can only place on empty or building cells
            return cell.cellType == CellType.Empty || cell.cellType == CellType.Building;
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
