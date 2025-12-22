using UnityEngine;
using DominantK.Core;
using DominantK.Data;
using DominantK.Systems;

namespace DominantK.UI
{
    /// <summary>
    /// Simple IMGUI-based UI for runtime testing.
    /// Replace with proper UI system (Canvas/UI Toolkit) for production.
    /// </summary>
    public class RuntimeUI : MonoBehaviour
    {
        private GameManager gameManager;
        private PlacementSystem placementSystem;
        private ConvenienceStoreData[] storeDataList;

        private GUIStyle headerStyle;
        private GUIStyle buttonStyle;
        private GUIStyle labelStyle;
        private GUIStyle boxStyle;
        private bool stylesInitialized;

        private int selectedStoreIndex = -1;

        public void Setup(GameManager gm, PlacementSystem ps, ConvenienceStoreData[] stores)
        {
            gameManager = gm;
            placementSystem = ps;
            storeDataList = stores;
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            headerStyle.normal.textColor = Color.white;

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                padding = new RectOffset(10, 10, 8, 8)
            };

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14
            };
            labelStyle.normal.textColor = Color.white;

            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();

            DrawTopBar();
            DrawStorePanel();
            DrawInfoPanel();
            DrawHelpPanel();
        }

        private void DrawTopBar()
        {
            GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, 50));
            GUILayout.BeginHorizontal(boxStyle);

            // Funds
            if (gameManager != null)
            {
                GUILayout.Label($"Funds: ${gameManager.PlayerFunds}", headerStyle, GUILayout.Width(150));
                GUILayout.Label($"Phase: {GetPhaseName(gameManager.CurrentPhase)}", labelStyle, GUILayout.Width(200));

                // Timer
                float remaining = gameManager.Phase1Duration - gameManager.PhaseTimer;
                if (remaining > 0)
                {
                    int minutes = Mathf.FloorToInt(remaining / 60);
                    int seconds = Mathf.FloorToInt(remaining % 60);
                    GUILayout.Label($"Time: {minutes:00}:{seconds:00}", labelStyle, GUILayout.Width(100));
                }

                // Store count
                if (placementSystem != null)
                {
                    GUILayout.Label($"Stores: {placementSystem.AllStores.Count}", labelStyle, GUILayout.Width(100));
                }
            }

            GUILayout.FlexibleSpace();

            // Chain indicator
            if (gameManager != null)
            {
                GUILayout.Label($"Chain: {gameManager.PlayerChain}", labelStyle);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawStorePanel()
        {
            float panelWidth = 200;
            float panelHeight = 300;

            GUILayout.BeginArea(new Rect(10, 70, panelWidth, panelHeight));
            GUILayout.BeginVertical(boxStyle);

            GUILayout.Label("Build Store", headerStyle);
            GUILayout.Space(10);

            if (storeDataList != null)
            {
                for (int i = 0; i < storeDataList.Length; i++)
                {
                    var data = storeDataList[i];
                    if (data == null) continue;

                    bool isSelected = selectedStoreIndex == i;
                    bool canAfford = gameManager != null && gameManager.PlayerFunds >= data.buildCost;

                    GUI.enabled = canAfford;

                    // Color button based on chain
                    var originalColor = GUI.backgroundColor;
                    GUI.backgroundColor = isSelected ? Color.yellow : data.chainColor;

                    string buttonText = $"{data.displayName}\n${data.buildCost}";
                    if (GUILayout.Button(buttonText, buttonStyle, GUILayout.Height(50)))
                    {
                        selectedStoreIndex = i;
                        placementSystem?.EnterPlacementMode(data);
                    }

                    GUI.backgroundColor = originalColor;
                    GUI.enabled = true;
                }
            }

            GUILayout.Space(10);

            if (placementSystem != null && placementSystem.IsPlacementMode)
            {
                if (GUILayout.Button("Cancel (Right Click)", buttonStyle))
                {
                    placementSystem.ExitPlacementMode();
                    selectedStoreIndex = -1;
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawInfoPanel()
        {
            if (placementSystem == null || !placementSystem.IsPlacementMode) return;

            var data = placementSystem.SelectedStoreData;
            if (data == null) return;

            float panelWidth = 200;
            float panelHeight = 150;

            GUILayout.BeginArea(new Rect(10, Screen.height - panelHeight - 10, panelWidth, panelHeight));
            GUILayout.BeginVertical(boxStyle);

            GUILayout.Label(data.displayName, headerStyle);
            GUILayout.Label($"ZOC Radius: {data.zocRadius:F1}", labelStyle);
            GUILayout.Label($"Dominant Radius: {data.dominantRadius:F1}", labelStyle);
            GUILayout.Label($"Base Revenue: ${data.baseRevenue}/s", labelStyle);

            if (data.chainType == ChainType.SevenEleban)
            {
                GUILayout.Label("Warning: Corruption effect!", labelStyle);
            }
            else if (data.chainType == ChainType.Famoma)
            {
                GUILayout.Label("Bonus: Sound wave +30%", labelStyle);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawHelpPanel()
        {
            float panelWidth = 250;
            float panelHeight = 120;

            GUILayout.BeginArea(new Rect(Screen.width - panelWidth - 10, Screen.height - panelHeight - 10, panelWidth, panelHeight));
            GUILayout.BeginVertical(boxStyle);

            GUILayout.Label("Controls", headerStyle);
            GUILayout.Label("WASD / Arrows: Move camera", labelStyle);
            GUILayout.Label("Mouse Wheel: Zoom", labelStyle);
            GUILayout.Label("Left Click: Place store", labelStyle);
            GUILayout.Label("Right Click: Cancel", labelStyle);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private string GetPhaseName(GamePhase phase)
        {
            return phase switch
            {
                GamePhase.Phase1_2D => "2D Invasion",
                GamePhase.Phase2_3D => "3D Invasion",
                GamePhase.Phase3_4D => "4D Invasion",
                GamePhase.Phase4_Boss => "BOSS: Aion",
                _ => "Unknown"
            };
        }
    }
}
