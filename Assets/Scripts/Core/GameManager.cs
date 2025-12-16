using System;
using UnityEngine;
using DominantK.Data;
using DominantK.Systems;

namespace DominantK.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Phase")]
        [SerializeField] private GamePhase currentPhase = GamePhase.Phase1_2D;

        [Header("Player")]
        [SerializeField] private ChainType playerChain;
        [SerializeField] private int playerFunds = 500;

        [Header("Phase Transition")]
        [SerializeField] private float phase1Duration = 120f;
        [SerializeField] private float phase2Duration = 180f;

        [Header("References")]
        [SerializeField] private GridSystem gridSystem;
        [SerializeField] private PlacementSystem placementSystem;
        [SerializeField] private DominantSystem dominantSystem;

        private float phaseTimer;

        public GamePhase CurrentPhase => currentPhase;
        public ChainType PlayerChain => playerChain;
        public int PlayerFunds => playerFunds;

        public event Action<GamePhase> OnPhaseChanged;
        public event Action<int> OnFundsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeGame();
        }

        private void Update()
        {
            UpdatePhaseTimer();
        }

        private void InitializeGame()
        {
            currentPhase = GamePhase.Phase1_2D;
            phaseTimer = 0f;

            gridSystem?.Initialize();
            placementSystem?.Initialize();
            dominantSystem?.Initialize();
        }

        private void UpdatePhaseTimer()
        {
            phaseTimer += Time.deltaTime;

            switch (currentPhase)
            {
                case GamePhase.Phase1_2D:
                    if (phaseTimer >= phase1Duration)
                    {
                        TransitionToPhase(GamePhase.Phase2_3D);
                    }
                    break;
                case GamePhase.Phase2_3D:
                    if (phaseTimer >= phase2Duration)
                    {
                        TransitionToPhase(GamePhase.Phase3_4D);
                    }
                    break;
            }
        }

        public void TransitionToPhase(GamePhase newPhase)
        {
            if (currentPhase == newPhase) return;

            currentPhase = newPhase;
            phaseTimer = 0f;

            OnPhaseChanged?.Invoke(currentPhase);
            Debug.Log($"Phase transitioned to: {currentPhase}");
        }

        public bool TrySpendFunds(int amount)
        {
            if (playerFunds >= amount)
            {
                playerFunds -= amount;
                OnFundsChanged?.Invoke(playerFunds);
                return true;
            }
            return false;
        }

        public void AddFunds(int amount)
        {
            playerFunds += amount;
            OnFundsChanged?.Invoke(playerFunds);
        }

        public void SetPlayerChain(ChainType chain)
        {
            playerChain = chain;
        }

        /// <summary>
        /// Check if all enemy stores are destroyed (Phase 1 victory condition)
        /// </summary>
        public void CheckPhase1Victory()
        {
            if (currentPhase != GamePhase.Phase1_2D) return;

            if (dominantSystem != null && dominantSystem.AreAllEnemiesDefeated())
            {
                TransitionToPhase(GamePhase.Phase2_3D);
            }
        }
    }
}
