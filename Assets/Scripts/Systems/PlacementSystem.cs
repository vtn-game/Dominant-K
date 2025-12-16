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

        [Header("Store Data")]
        [SerializeField] private List<ConvenienceStoreData> availableStores;
        [SerializeField] private ConvenienceStoreData selectedStoreData;

        [Header("Preview")]
        [SerializeField] private GameObject previewObject;
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;

        [Header("Settings")]
        [SerializeField] private LayerMask groundLayer;

        private bool isPlacementMode;
        private Vector2Int currentPreviewPosition;
        private List<ConvenienceStore> allStores = new List<ConvenienceStore>();

        public List<ConvenienceStore> AllStores => allStores;
        public bool IsPlacementMode => isPlacementMode;

        public void Initialize()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;

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

            // Left click to place or select
            if (Input.GetMouseButtonDown(0))
            {
                if (isPlacementMode)
                {
                    TryPlaceStore();
                }
                else
                {
                    TrySelectCell();
                }
            }
        }

        private void UpdatePreview()
        {
            if (!isPlacementMode || previewObject == null) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
            {
                var cell = gridSystem.GetCellFromWorldPosition(hit.point);
                if (cell != null)
                {
                    currentPreviewPosition = cell.position;
                    Vector3 worldPos = gridSystem.GetWorldPosition(cell.position);
                    previewObject.transform.position = worldPos;

                    bool canPlace = CanPlaceAt(cell);
                    UpdatePreviewMaterial(canPlace);
                }
            }
        }

        private void UpdatePreviewMaterial(bool valid)
        {
            if (previewObject == null) return;

            var renderer = previewObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = valid ? validPlacementMaterial : invalidPlacementMaterial;
            }
        }

        public void EnterPlacementMode(ConvenienceStoreData storeData)
        {
            selectedStoreData = storeData;
            isPlacementMode = true;

            if (previewObject != null)
            {
                previewObject.SetActive(true);
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
            if (selectedStoreData != null &&
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
            if (!GameManager.Instance.TrySpendFunds(selectedStoreData.buildCost))
            {
                return;
            }

            // Destroy building if present (Phase 1 mechanic)
            if (cell.HasBuilding)
            {
                cell.building.OnDestroyed();
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
                GameManager.Instance.CheckPhase1Victory();
            }
        }

        private void TrySelectCell()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
            {
                var cell = gridSystem.GetCellFromWorldPosition(hit.point);
                if (cell != null && cell.HasStore)
                {
                    // Select store for info display, etc.
                    Debug.Log($"Selected store: {cell.store.Data.displayName}");
                }
            }
        }

        public ConvenienceStore CreateStore(Vector2Int position, ConvenienceStoreData data, bool playerOwned)
        {
            if (data.prefab == null)
            {
                Debug.LogError($"Store prefab is null for {data.displayName}");
                return null;
            }

            Vector3 worldPos = gridSystem.GetWorldPosition(position);
            var storeObj = Instantiate(data.prefab, worldPos, Quaternion.identity, transform);
            var store = storeObj.GetComponent<ConvenienceStore>();

            if (store == null)
            {
                store = storeObj.AddComponent<ConvenienceStore>();
            }

            ChainType owner = playerOwned ? GameManager.Instance.PlayerChain : data.chainType;
            store.Initialize(data, position, owner, playerOwned);

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
