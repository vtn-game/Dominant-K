using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DominantK.Core;
using DominantK.Data;
using DominantK.Entities;

namespace DominantK.Systems
{
    [System.Serializable]
    public class DominantTriangle
    {
        public ConvenienceStore store1;
        public ConvenienceStore store2;
        public ConvenienceStore store3;
        public ChainType ownerChain;
        public Vector3[] worldVertices;

        public DominantTriangle(ConvenienceStore s1, ConvenienceStore s2, ConvenienceStore s3)
        {
            store1 = s1;
            store2 = s2;
            store3 = s3;
            ownerChain = s1.OwnerChain;

            worldVertices = new Vector3[3];
            UpdateVertices();
        }

        public void UpdateVertices()
        {
            worldVertices[0] = store1.transform.position;
            worldVertices[1] = store2.transform.position;
            worldVertices[2] = store3.transform.position;
        }

        public bool ContainsPoint(Vector3 point)
        {
            return IsPointInTriangle(
                new Vector2(point.x, point.z),
                new Vector2(worldVertices[0].x, worldVertices[0].z),
                new Vector2(worldVertices[1].x, worldVertices[1].z),
                new Vector2(worldVertices[2].x, worldVertices[2].z)
            );
        }

        private bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);

            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(hasNeg && hasPos);
        }

        private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        public bool ContainsStore(ConvenienceStore store)
        {
            if (store == store1 || store == store2 || store == store3)
                return false;

            return ContainsPoint(store.transform.position);
        }
    }

    public class DominantSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlacementSystem placementSystem;
        [SerializeField] private GridSystem gridSystem;

        [Header("Settings")]
        [SerializeField] private float triangleUpdateInterval = 0.5f;
        [SerializeField] private float revenueInterval = 1f;
        [SerializeField] private float dominantRevenueMultiplier = 1.5f;

        [Header("Visualization")]
        [SerializeField] private bool showTriangles = true;

        private List<DominantTriangle> activeTriangles = new List<DominantTriangle>();
        private Dictionary<ChainType, List<ConvenienceStore>> storesByChain = new Dictionary<ChainType, List<ConvenienceStore>>();
        private Dictionary<ChainType, int> dominantCounts = new Dictionary<ChainType, int>();

        private float triangleTimer;
        private float revenueTimer;

        private List<GameObject> triangleVisuals = new List<GameObject>();
        private Material triangleMaterial;

        public List<DominantTriangle> ActiveTriangles => activeTriangles;

        public void Setup(PlacementSystem placement, GridSystem grid)
        {
            placementSystem = placement;
            gridSystem = grid;
        }

        public void Initialize()
        {
            storesByChain.Clear();
            dominantCounts.Clear();
            activeTriangles.Clear();

            foreach (ChainType chain in System.Enum.GetValues(typeof(ChainType)))
            {
                storesByChain[chain] = new List<ConvenienceStore>();
                dominantCounts[chain] = 0;
            }

            CreateTriangleMaterial();
        }

        private void CreateTriangleMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            triangleMaterial = new Material(shader);
            triangleMaterial.SetFloat("_Surface", 1);
            triangleMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            triangleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            triangleMaterial.SetInt("_ZWrite", 0);
            triangleMaterial.renderQueue = 3000;
        }

        private void Update()
        {
            triangleTimer += Time.deltaTime;
            revenueTimer += Time.deltaTime;

            if (triangleTimer >= triangleUpdateInterval)
            {
                triangleTimer = 0f;
                UpdateTriangles();
                CheckTriangleCaptures();
            }

            if (revenueTimer >= revenueInterval)
            {
                revenueTimer = 0f;
                ProcessRevenue();
            }
        }

        public void OnStoreAdded(ConvenienceStore store)
        {
            ChainType chain = store.OwnerChain;

            if (!storesByChain.ContainsKey(chain))
            {
                storesByChain[chain] = new List<ConvenienceStore>();
            }

            storesByChain[chain].Add(store);
            UpdateDominantCounts();
            UpdateTriangles();
        }

        public void OnStoreRemoved(ConvenienceStore store)
        {
            ChainType chain = store.OwnerChain;

            if (storesByChain.ContainsKey(chain))
            {
                storesByChain[chain].Remove(store);
            }

            UpdateDominantCounts();
            UpdateTriangles();
        }

        private void UpdateDominantCounts()
        {
            foreach (var chain in storesByChain.Keys)
            {
                dominantCounts[chain] = storesByChain[chain].Count;

                // Update each store's dominant count
                foreach (var store in storesByChain[chain])
                {
                    store.SetDominantCount(dominantCounts[chain]);
                }
            }
        }

        private void UpdateTriangles()
        {
            activeTriangles.Clear();
            ClearTriangleVisuals();

            foreach (var kvp in storesByChain)
            {
                var stores = kvp.Value;
                if (stores.Count < 3) continue;

                // Find all valid triangles for this chain
                var triangles = FindValidTriangles(stores);
                activeTriangles.AddRange(triangles);
            }

            if (showTriangles)
            {
                VisualizeTriangles();
            }
        }

        private List<DominantTriangle> FindValidTriangles(List<ConvenienceStore> stores)
        {
            var triangles = new List<DominantTriangle>();

            for (int i = 0; i < stores.Count - 2; i++)
            {
                for (int j = i + 1; j < stores.Count - 1; j++)
                {
                    for (int k = j + 1; k < stores.Count; k++)
                    {
                        var s1 = stores[i];
                        var s2 = stores[j];
                        var s3 = stores[k];

                        // Check if stores are within dominant radius of each other
                        if (AreStoresConnected(s1, s2) &&
                            AreStoresConnected(s2, s3) &&
                            AreStoresConnected(s1, s3))
                        {
                            triangles.Add(new DominantTriangle(s1, s2, s3));
                        }
                    }
                }
            }

            return triangles;
        }

        private bool AreStoresConnected(ConvenienceStore a, ConvenienceStore b)
        {
            float distance = Vector3.Distance(a.transform.position, b.transform.position);
            float maxRadius = Mathf.Max(a.GetDominantRadius(), b.GetDominantRadius());
            return distance <= maxRadius * 2f;
        }

        private void CheckTriangleCaptures()
        {
            if (placementSystem == null) return;

            var storesToDestroy = new List<ConvenienceStore>();

            foreach (var triangle in activeTriangles)
            {
                // Check if any enemy stores are inside this triangle
                foreach (var kvp in storesByChain)
                {
                    if (kvp.Key == triangle.ownerChain) continue;

                    foreach (var store in kvp.Value)
                    {
                        if (triangle.ContainsStore(store))
                        {
                            storesToDestroy.Add(store);
                        }
                    }
                }
            }

            // Destroy captured stores
            foreach (var store in storesToDestroy.Distinct().ToList())
            {
                Debug.Log($"Store {store.Data.displayName} captured by Dominant Triangle!");
                placementSystem.RemoveStore(store);
            }
        }

        private void ProcessRevenue()
        {
            if (placementSystem == null || GameManager.Instance == null) return;

            var playerStores = placementSystem.GetStoresByOwner(true);

            foreach (var store in playerStores)
            {
                int baseRevenue = store.CalculateRevenue();

                // Check if store is in any friendly triangle
                bool inTriangle = activeTriangles.Any(t =>
                    t.ownerChain == store.OwnerChain &&
                    (t.store1 == store || t.store2 == store || t.store3 == store ||
                     t.ContainsPoint(store.transform.position)));

                float multiplier = inTriangle ? dominantRevenueMultiplier : 1f;
                int finalRevenue = Mathf.RoundToInt(baseRevenue * multiplier);

                GameManager.Instance.AddFunds(finalRevenue);
            }
        }

        private void VisualizeTriangles()
        {
            foreach (var triangle in activeTriangles)
            {
                CreateTriangleVisual(triangle);
            }
        }

        private void CreateTriangleVisual(DominantTriangle triangle)
        {
            var go = new GameObject("TriangleVisual");
            go.transform.SetParent(transform);

            var meshFilter = go.AddComponent<MeshFilter>();
            var meshRenderer = go.AddComponent<MeshRenderer>();

            var mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                triangle.worldVertices[0] + Vector3.up * 0.1f,
                triangle.worldVertices[1] + Vector3.up * 0.1f,
                triangle.worldVertices[2] + Vector3.up * 0.1f
            };
            mesh.triangles = new int[] { 0, 1, 2, 2, 1, 0 }; // Double-sided
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;

            var mat = new Material(triangleMaterial);
            var store = triangle.store1;
            if (store.Data != null)
            {
                var color = store.Data.chainColor;
                color.a = 0.3f;
                mat.color = color;
            }
            meshRenderer.material = mat;

            triangleVisuals.Add(go);
        }

        private void ClearTriangleVisuals()
        {
            foreach (var visual in triangleVisuals)
            {
                if (visual != null)
                {
                    Destroy(visual);
                }
            }
            triangleVisuals.Clear();
        }

        public bool IsPointInAnyTriangle(Vector3 point, ChainType chain)
        {
            return activeTriangles.Any(t => t.ownerChain == chain && t.ContainsPoint(point));
        }

        public int GetDominantCount(ChainType chain)
        {
            return dominantCounts.ContainsKey(chain) ? dominantCounts[chain] : 0;
        }

        public bool AreAllEnemiesDefeated()
        {
            if (GameManager.Instance == null) return false;

            ChainType playerChain = GameManager.Instance.PlayerChain;

            foreach (var kvp in storesByChain)
            {
                if (kvp.Key != playerChain && kvp.Value.Count > 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
