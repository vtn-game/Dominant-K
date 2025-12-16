using UnityEngine;
using DominantK.Systems;

namespace DominantK.Core
{
    /// <summary>
    /// Bootstrap script for InGame scene. Attach to a GameObject in the scene.
    /// Sets up all required systems and references.
    /// </summary>
    public class InGameSceneSetup : MonoBehaviour
    {
        [Header("System Prefabs")]
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject gridSystemPrefab;
        [SerializeField] private GameObject placementSystemPrefab;
        [SerializeField] private GameObject dominantSystemPrefab;

        [Header("Ground")]
        [SerializeField] private Vector2 groundSize = new Vector2(50, 50);
        [SerializeField] private Material groundMaterial;

        private void Awake()
        {
            SetupSystems();
            CreateGround();
        }

        private void SetupSystems()
        {
            // Create systems if prefabs are assigned
            // Otherwise, ensure they exist in scene

            if (GameManager.Instance == null)
            {
                if (gameManagerPrefab != null)
                {
                    Instantiate(gameManagerPrefab);
                }
                else
                {
                    var go = new GameObject("GameManager");
                    go.AddComponent<GameManager>();
                }
            }

            var gridSystem = FindFirstObjectByType<GridSystem>();
            if (gridSystem == null)
            {
                if (gridSystemPrefab != null)
                {
                    Instantiate(gridSystemPrefab);
                }
                else
                {
                    var go = new GameObject("GridSystem");
                    go.AddComponent<GridSystem>();
                }
            }

            var placementSystem = FindFirstObjectByType<PlacementSystem>();
            if (placementSystem == null)
            {
                if (placementSystemPrefab != null)
                {
                    Instantiate(placementSystemPrefab);
                }
                else
                {
                    var go = new GameObject("PlacementSystem");
                    go.AddComponent<PlacementSystem>();
                }
            }

            var dominantSystem = FindFirstObjectByType<DominantSystem>();
            if (dominantSystem == null)
            {
                if (dominantSystemPrefab != null)
                {
                    Instantiate(dominantSystemPrefab);
                }
                else
                {
                    var go = new GameObject("DominantSystem");
                    go.AddComponent<DominantSystem>();
                }
            }
        }

        private void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = new Vector3(groundSize.x / 2f, 0, groundSize.y / 2f);
            ground.transform.localScale = new Vector3(groundSize.x / 10f, 1, groundSize.y / 10f);
            ground.layer = LayerMask.NameToLayer("Ground");

            if (groundMaterial != null)
            {
                ground.GetComponent<MeshRenderer>().material = groundMaterial;
            }
        }
    }
}
