using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DominantK.Core;
using DominantK.Data;
using DominantK.Systems;

namespace DominantK.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlacementSystem placementSystem;

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI fundsText;
        [SerializeField] private TextMeshProUGUI phaseText;
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Store Selection")]
        [SerializeField] private Transform storeButtonContainer;
        [SerializeField] private Button storeButtonPrefab;
        [SerializeField] private ConvenienceStoreData[] availableStores;

        [Header("Info Panel")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private TextMeshProUGUI storeNameText;
        [SerializeField] private TextMeshProUGUI storeCostText;
        [SerializeField] private TextMeshProUGUI storeStatsText;

        private ConvenienceStoreData selectedStore;

        private void Start()
        {
            SetupStoreButtons();
            UpdateUI();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnFundsChanged += OnFundsChanged;
                GameManager.Instance.OnPhaseChanged += OnPhaseChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnFundsChanged -= OnFundsChanged;
                GameManager.Instance.OnPhaseChanged -= OnPhaseChanged;
            }
        }

        private void Update()
        {
            UpdateTimer();
        }

        private void SetupStoreButtons()
        {
            if (storeButtonContainer == null || storeButtonPrefab == null) return;

            foreach (var storeData in availableStores)
            {
                var button = Instantiate(storeButtonPrefab, storeButtonContainer);
                var text = button.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{storeData.displayName}\n${storeData.buildCost}";
                }

                // Set button color
                var colors = button.colors;
                colors.normalColor = storeData.chainColor;
                button.colors = colors;

                // Add click handler
                var data = storeData; // Capture for closure
                button.onClick.AddListener(() => OnStoreButtonClicked(data));
            }
        }

        private void OnStoreButtonClicked(ConvenienceStoreData storeData)
        {
            selectedStore = storeData;
            ShowStoreInfo(storeData);
            placementSystem?.EnterPlacementMode(storeData);
        }

        private void ShowStoreInfo(ConvenienceStoreData data)
        {
            if (infoPanel == null) return;

            infoPanel.SetActive(true);

            if (storeNameText != null)
                storeNameText.text = data.displayName;

            if (storeCostText != null)
                storeCostText.text = $"Cost: ${data.buildCost}";

            if (storeStatsText != null)
            {
                storeStatsText.text = $"ZOC: {data.zocRadius:F1}\n" +
                                      $"Dominant: {data.dominantRadius:F1}\n" +
                                      $"Revenue: ${data.baseRevenue}/s";
            }
        }

        private void UpdateUI()
        {
            if (GameManager.Instance == null) return;

            OnFundsChanged(GameManager.Instance.PlayerFunds);
            OnPhaseChanged(GameManager.Instance.CurrentPhase);
        }

        private void OnFundsChanged(int funds)
        {
            if (fundsText != null)
            {
                fundsText.text = $"${funds}";
            }
        }

        private void OnPhaseChanged(GamePhase phase)
        {
            if (phaseText != null)
            {
                phaseText.text = phase switch
                {
                    GamePhase.Phase1_2D => "Phase 1: 2D Invasion",
                    GamePhase.Phase2_3D => "Phase 2: 3D Invasion",
                    GamePhase.Phase3_4D => "Phase 3: 4D Invasion",
                    GamePhase.Phase4_Boss => "Final: Aion",
                    _ => "Unknown Phase"
                };
            }
        }

        private void UpdateTimer()
        {
            // Timer display would show remaining time in phase
            // Implementation depends on how timer is exposed from GameManager
        }

        public void OnCancelPlacement()
        {
            placementSystem?.ExitPlacementMode();
            selectedStore = null;

            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
        }
    }
}
