using System.Collections.Generic;
using UnityEngine;
using DominantK.Core;
using DominantK.Data;
using DominantK.Entities;

namespace DominantK.Systems
{
    public class PlacementSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridSystem gridSystem;
        [SerializeField] private DominantSystem dominantSystem;
        [SerializeField] private Camera mainCamera;

        [Header("Settings")]
        [SerializeField] private LayerMask groundLayer = ~0;

        [Header("Preview")]
        [SerializeField] private Color validPlacementColor = new Color(0, 1, 0, 0.5f);
        [SerializeField] private Color invalidPlacementColor = new Color(1, 0, 0, 0.5f);

        private ConvenienceStoreData selectedStoreData;
        private bool isPlacementMode;
        private Vector2Int currentPreviewPosition;
        private List<ConvenienceStore> allStores = new List<ConvenienceStore>();

        private GameObject previewObject;
        private MeshRenderer previewRenderer;
        private Material previewMaterial;

        public List<ConvenienceStore> AllStores => allStores;
        public bool IsPlacementMode => isPlacementMode;
        public ConvenienceStoreData SelectedStoreData => selectedStoreData;

        public void Setup(GridSystem grid, DominantSystem dominant, Camera camera)
        {
            gridSystem = grid;
            dominantSystem = dominant;
            mainCamera = camera;
        }

        public void Initialize()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            CreatePreviewObject();
        }

        private void CreatePreviewObject()
        {
            previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            previewObject.name = "PlacementPreview";
            previewObject.transform.SetParent(transform);

            // Remove collider
            var collider = previewObject.GetComponent<Collider>();
            if (collider != null) Destroy(collider);

            // Setup transparent material
            previewRenderer = previewObject.GetComponent<MeshRenderer>();
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            previewMaterial = new Material(shader);
            previewMaterial.SetFloat("_Surface", 1); // Transparent
            previewMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            previewMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            previewMaterial.SetInt("_ZWrite", 0);
            previewMaterial.renderQueue = 3000;
            previewMaterial.color = validPlacementColor;
            previewRenderer.material = previewMaterial;

            previewObject.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            previewObject.SetActive(false);
        }

        private void Update()
        {
            if (gridSystem == null) return;

            HandleInput();
            UpdatePreview();
        }

        private void HandleInput()
        {
            // Right click to cancel placement mode
            if (Input.GetMouseButtonDown(1))
            {
                ExitPlacementMode();
                return;
            }

            // Left click to place
            if (Input.GetMouseButtonDown(0) && isPlacementMode)
            {
                TryPlaceStore();
            }
        }

        private void UpdatePreview()
        {
            if (!isPlacementMode || previewObject == null || mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            // Try raycast first
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
            {
                UpdatePreviewPosition(hit.point);
            }
            // Fallback to plane intersection
            else if (ray.direction.y != 0)
            {
                float t = -ray.origin.y / ray.direction.y;
                if (t > 0)
                {
                    Vector3 point = ray.origin + ray.direction * t;
                    UpdatePreviewPosition(point);
                }
            }
        }

        private void UpdatePreviewPosition(Vector3 worldPoint)
        {
            var cell = gridSystem.GetCellFromWorldPosition(worldPoint);
            if (cell != null)
            {
                currentPreviewPosition = cell.position;
                Vector3 worldPos = gridSystem.GetWorldPosition(cell.position);
                worldPos.y = 0.5f;
                previewObject.transform.position = worldPos;

                bool canPlace = CanPlaceAt(cell);
                previewMaterial.color = canPlace ? validPlacementColor : invalidPlacementColor;
            }
        }

        public void EnterPlacementMode(ConvenienceStoreData storeData)
        {
            selectedStoreData = storeData;
            isPlacementMode = true;

            if (previewObject != null)
            {
                previewObject.SetActive(true);
                // Update preview color to match chain
                if (storeData != null)
                {
                    var color = storeData.chainColor;
                    color.a = 0.5f;
                    validPlacementColor = color;
                }
            }
        }

        public void ExitPlacementMode()
        {
            isPlacementMode = false;
            selectedStoreData = null;

            if (previewObject != null)
            {
                previewObject.SetActive(false);
            }
        }

        private bool CanPlaceAt(GridCell cell)
        {
            if (cell == null) return false;

            // Cannot place on roads, stations, or existing stores
            if (cell.cellType == CellType.Road ||
                cell.cellType == CellType.Station ||
                cell.cellType == CellType.ConvenienceStore)
            {
                return false;
            }

            // Check if player has enough funds
            if (selectedStoreData != null && GameManager.Instance != null &&
                GameManager.Instance.PlayerFunds < selectedStoreData.buildCost)
            {
                return false;
            }

            return true;
        }

        private void TryPlaceStore()
        {
            var cell = gridSystem.GetCell(currentPreviewPosition);
            if (!CanPlaceAt(cell)) return;

            if (selectedStoreData == null) return;

            // Spend funds
            if (GameManager.Instance != null && !GameManager.Instance.TrySpendFunds(selectedStoreData.buildCost))
            {
                return;
            }

            // Destroy building if present (Phase 1 mechanic)
            if (cell.HasBuilding)
            {
                gridSystem.DestroyBuilding(currentPreviewPosition);
            }

            // Create store
            var store = CreateStore(currentPreviewPosition, selectedStoreData, true);
            if (store != null)
            {
                gridSystem.PlaceStore(currentPreviewPosition, store);
                allStores.Add(store);

                // Update dominant system
                dominantSystem?.OnStoreAdded(store);

                // Check victory condition
                GameManager.Instance?.CheckPhase1Victory();
            }
        }

        public ConvenienceStore CreateStore(Vector2Int position, ConvenienceStoreData data, bool playerOwned)
        {
            Vector3 worldPos = gridSystem.GetWorldPosition(position);

            // Create store from primitive if no prefab
            GameObject storeObj;
            if (data.prefab != null)
            {
                storeObj = Instantiate(data.prefab, worldPos, Quaternion.identity, transform);
            }
            else
            {
                storeObj = CreateStorePrimitive(worldPos, data);
            }

            var store = storeObj.GetComponent<ConvenienceStore>();
            if (store == null)
            {
                store = storeObj.AddComponent<ConvenienceStore>();
            }

            ChainType owner = playerOwned && GameManager.Instance != null
                ? GameManager.Instance.PlayerChain
                : data.chainType;
            store.Initialize(data, position, owner, playerOwned);

            return store;
        }

        private GameObject CreateStorePrimitive(Vector3 position, ConvenienceStoreData data)
        {
            var store = new GameObject($"Store_{data.displayName}");
            store.transform.SetParent(transform);
            store.transform.position = position;

            // Main building
            var building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.transform.SetParent(store.transform);
            building.transform.localPosition = new Vector3(0, 0.5f, 0);
            building.transform.localScale = new Vector3(0.9f, 1f, 0.9f);

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var mat = new Material(shader);
            mat.color = data.chainColor;
            building.GetComponent<MeshRenderer>().material = mat;

            // Sign/roof
            var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.transform.SetParent(store.transform);
            sign.transform.localPosition = new Vector3(0, 1.1f, 0);
            sign.transform.localScale = new Vector3(1f, 0.2f, 1f);

            var signMat = new Material(shader);
            signMat.color = data.chainColor * 1.2f;
            sign.GetComponent<MeshRenderer>().material = signMat;

            return store;
        }

        public ConvenienceStore PlaceStore(Vector2Int position, ConvenienceStoreData data, bool playerOwned)
        {
            var cell = gridSystem.GetCell(position);
            if (cell == null) return null;

            // Cannot place on roads, stations, or existing stores
            if (cell.cellType == CellType.Road ||
                cell.cellType == CellType.Station ||
                cell.cellType == CellType.ConvenienceStore)
            {
                return null;
            }

            // Destroy building if present
            if (cell.HasBuilding)
            {
                gridSystem.DestroyBuilding(position);
            }

            // Create store
            var store = CreateStore(position, data, playerOwned);
            if (store != null)
            {
                gridSystem.PlaceStore(position, store);
                allStores.Add(store);
                dominantSystem?.OnStoreAdded(store);
            }

            return store;
        }

        public void RemoveStore(ConvenienceStore store)
        {
            if (store == null) return;

            var cell = gridSystem.GetCell(store.GridPosition);
            if (cell != null)
            {
                cell.cellType = CellType.Empty;
                cell.store = null;
                cell.occupant = null;
            }

            allStores.Remove(store);
            dominantSystem?.OnStoreRemoved(store);

            store.OnStoreDestroyed();
            Destroy(store.gameObject);
        }

        public List<ConvenienceStore> GetStoresByOwner(bool playerOwned)
        {
            return allStores.FindAll(s => s.IsPlayerOwned == playerOwned);
        }

        public List<ConvenienceStore> GetStoresByChain(ChainType chain)
        {
            return allStores.FindAll(s => s.OwnerChain == chain);
        }
    }
}
