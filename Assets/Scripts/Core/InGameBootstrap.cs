using UnityEngine;
using DominantK.Data;
using DominantK.Systems;
using DominantK.UI;

namespace DominantK.Core
{
    /// <summary>
    /// Bootstrap script that sets up the entire InGame scene.
    /// Add this to an empty GameObject in a new scene to create the game.
    /// </summary>
    public class InGameBootstrap : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 30;
        [SerializeField] private int gridHeight = 30;
        [SerializeField] private float cellSize = 1f;

        [Header("Player Settings")]
        [SerializeField] private ChainType playerChain = ChainType.SevenEleban;
        [SerializeField] private int startingFunds = 500;

        [Header("Colors")]
        [SerializeField] private Color sevenElebanColor = new Color(0.2f, 0.6f, 0.3f);
        [SerializeField] private Color lawsonColor = new Color(0.2f, 0.4f, 0.8f);
        [SerializeField] private Color famomaColor = new Color(0.3f, 0.7f, 0.6f);

        // Runtime references
        private GameManager gameManager;
        private GridSystem gridSystem;
        private PlacementSystem placementSystem;
        private DominantSystem dominantSystem;
        private CameraController cameraController;
        private RuntimeUI runtimeUI;

        private void Awake()
        {
            SetupScene();
        }

        private void SetupScene()
        {
            // Create main camera
            CreateCamera();

            // Create lighting
            CreateLighting();

            // Create ground
            CreateGround();

            // Create game systems
            CreateGameManager();
            CreateGridSystem();
            CreateDominantSystem();
            CreatePlacementSystem();

            // Create UI
            CreateUI();

            // Initialize systems
            InitializeSystems();
        }

        private void CreateCamera()
        {
            var cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";

            var camera = cameraObj.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 15f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.15f, 0.15f, 0.2f);

            cameraController = cameraObj.AddComponent<CameraController>();

            // Add audio listener
            cameraObj.AddComponent<AudioListener>();
        }

        private void CreateLighting()
        {
            // Directional light for quarter view
            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1f;
            light.shadows = LightShadows.Soft;

            // Angle to match quarter view
            lightObj.transform.rotation = Quaternion.Euler(50f, 45f, 0f);
        }

        private void CreateGround()
        {
            var groundObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundObj.name = "Ground";
            groundObj.layer = LayerMask.NameToLayer("Default"); // Will use default if Ground layer doesn't exist

            float scaleX = (gridWidth * cellSize) / 10f;
            float scaleZ = (gridHeight * cellSize) / 10f;
            groundObj.transform.localScale = new Vector3(scaleX, 1f, scaleZ);
            groundObj.transform.position = new Vector3(gridWidth * cellSize / 2f, -0.01f, gridHeight * cellSize / 2f);

            var renderer = groundObj.GetComponent<MeshRenderer>();
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.3f, 0.35f, 0.3f);
            renderer.material = material;
        }

        private void CreateGameManager()
        {
            var gmObj = new GameObject("GameManager");
            gameManager = gmObj.AddComponent<GameManager>();
        }

        private void CreateGridSystem()
        {
            var gridObj = new GameObject("GridSystem");
            gridSystem = gridObj.AddComponent<GridSystem>();
        }

        private void CreateDominantSystem()
        {
            var domObj = new GameObject("DominantSystem");
            dominantSystem = domObj.AddComponent<DominantSystem>();
        }

        private void CreatePlacementSystem()
        {
            var placeObj = new GameObject("PlacementSystem");
            placementSystem = placeObj.AddComponent<PlacementSystem>();
        }

        private void CreateUI()
        {
            var uiObj = new GameObject("RuntimeUI");
            runtimeUI = uiObj.AddComponent<RuntimeUI>();
        }

        private void InitializeSystems()
        {
            // Setup GridSystem
            gridSystem.Setup(gridWidth, gridHeight, cellSize);

            // Setup PlacementSystem with references
            placementSystem.Setup(gridSystem, dominantSystem, Camera.main);

            // Setup DominantSystem
            dominantSystem.Setup(placementSystem, gridSystem);

            // Setup GameManager
            gameManager.Setup(playerChain, startingFunds, gridSystem, placementSystem, dominantSystem);

            // Setup UI
            runtimeUI.Setup(gameManager, placementSystem, CreateDefaultStoreData());

            // Initialize
            gridSystem.Initialize();
            dominantSystem.Initialize();
            placementSystem.Initialize();
        }

        private ConvenienceStoreData[] CreateDefaultStoreData()
        {
            var stores = new ConvenienceStoreData[3];

            // SevenEleban
            stores[0] = ScriptableObject.CreateInstance<ConvenienceStoreData>();
            stores[0].id = "seven_eleban";
            stores[0].displayName = "SevenEleban";
            stores[0].chainType = ChainType.SevenEleban;
            stores[0].buildCost = 100;
            stores[0].zocRadius = 3f;
            stores[0].dominantRadius = 4f;
            stores[0].baseRevenue = 10;
            stores[0].baseFaithGain = 1f;
            stores[0].customerSpendMultiplier = 1f;
            stores[0].corruptionRate = 0.05f;
            stores[0].chainColor = sevenElebanColor;

            // Lawson
            stores[1] = ScriptableObject.CreateInstance<ConvenienceStoreData>();
            stores[1].id = "lawson";
            stores[1].displayName = "Lawson";
            stores[1].chainType = ChainType.Lawson;
            stores[1].buildCost = 120;
            stores[1].zocRadius = 2.5f;
            stores[1].dominantRadius = 3.5f;
            stores[1].baseRevenue = 9;
            stores[1].baseFaithGain = 0.8f;
            stores[1].customerSpendMultiplier = 1f;
            stores[1].chainColor = lawsonColor;

            // Famoma
            stores[2] = ScriptableObject.CreateInstance<ConvenienceStoreData>();
            stores[2].id = "famoma";
            stores[2].displayName = "Famoma";
            stores[2].chainType = ChainType.Famoma;
            stores[2].buildCost = 200;
            stores[2].zocRadius = 5f;
            stores[2].dominantRadius = 6f;
            stores[2].baseRevenue = 12;
            stores[2].baseFaithGain = 1.2f;
            stores[2].customerSpendMultiplier = 1.1f;
            stores[2].brainwashSpeedBonus = 0.3f;
            stores[2].chainColor = famomaColor;

            return stores;
        }
    }
}
