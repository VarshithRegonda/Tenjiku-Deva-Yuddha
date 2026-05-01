using UnityEngine;
using UnityEngine.SceneManagement;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Central singleton managing overall game lifecycle, state, and system initialization.
    /// Persists across scenes via DontDestroyOnLoad.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        public GameState CurrentState { get; private set; }
        public GamePhase CurrentPhase { get; private set; } = GamePhase.MainMenu;

        [Header("Runtime Flags")]
        public bool IsPaused { get; private set; }
        public bool IsDamaruActive { get; private set; }
        public float DamaruTimeRemaining { get; private set; }
        public bool IsMeditationActive { get; private set; }
        public float MeditationTimeRemaining { get; private set; }

        // System references (set up in GameScene)
        public ResourceManager ResourceMgr { get; set; }
        public DashavatarManager AvatarMgr { get; set; }

        private float _resourceTickTimer;

        // ─────────────────────────────────────────────
        //  Lifecycle
        // ─────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
        }

        private void Update()
        {
            if (CurrentPhase != GamePhase.Playing || IsPaused || CurrentState == null)
                return;

            float dt = Time.deltaTime;

            // Resource production tick
            _resourceTickTimer += dt;
            if (_resourceTickTimer >= GameConstants.RESOURCE_TICK_INTERVAL)
            {
                _resourceTickTimer = 0f;
                ResourceMgr?.ProduceResources();
            }

            // Damaru Beat speed boost timer
            if (IsDamaruActive)
            {
                DamaruTimeRemaining -= dt;
                if (DamaruTimeRemaining <= 0f)
                {
                    IsDamaruActive = false;
                    DamaruTimeRemaining = 0f;
                    GameEvents.ShowNotification("Damaru Beat effect has ended.");
                }
            }

            // Meditation Shakti generation timer
            if (IsMeditationActive)
            {
                MeditationTimeRemaining -= dt;
                float shaktiGain = GameConstants.MEDITATION_SHAKTI_PER_SECOND * dt;
                ResourceMgr?.AddResource(ResourceType.Shakti, shaktiGain);
                if (MeditationTimeRemaining <= 0f)
                {
                    IsMeditationActive = false;
                    MeditationTimeRemaining = 0f;
                    GameEvents.ShowNotification("Meditation has concluded. Shakti restored.");
                }
            }
        }

        // ─────────────────────────────────────────────
        //  Game Lifecycle
        // ─────────────────────────────────────────────
        public void StartNewGame(string playerName, string kingdomName)
        {
            CurrentState = new GameState();
            CurrentState.InitializeNewGame(playerName, kingdomName);
            Debug.Log($"[GameManager] New game started: {playerName} of {kingdomName}");
            SetPhase(GamePhase.Loading);
            SceneManager.LoadScene("GameScene");
        }

        public void LoadGame(int slot)
        {
            var state = SaveLoadManager.Instance.LoadGame(slot);
            if (state != null)
            {
                CurrentState = state;
                state.RecalculateLevel();
                Debug.Log($"[GameManager] Game loaded: {state.PlayerName} Lv.{state.PlayerLevel}");
                SetPhase(GamePhase.Loading);
                SceneManager.LoadScene("GameScene");
            }
            else
            {
                GameEvents.ShowNotification("Failed to load save file.");
            }
        }

        public void LoadAutoSave()
        {
            var state = SaveLoadManager.Instance.LoadAutoSave();
            if (state != null)
            {
                CurrentState = state;
                state.RecalculateLevel();
                SetPhase(GamePhase.Loading);
                SceneManager.LoadScene("GameScene");
            }
        }

        /// <summary>
        /// Called by GameSceneInitializer when the game scene is fully loaded.
        /// </summary>
        public void OnGameSceneReady()
        {
            SetPhase(GamePhase.Playing);
            _resourceTickTimer = 0f;
            Debug.Log("[GameManager] Game scene ready — entering play mode.");
        }

        public void SaveGame(int slot)
        {
            if (CurrentState == null) return;
            SaveLoadManager.Instance.SaveGame(CurrentState, slot);
        }

        public void ReturnToMainMenu()
        {
            SaveLoadManager.Instance.AutoSave();
            SetPhase(GamePhase.MainMenu);
            SceneManager.LoadScene("MainMenu");
        }

        // ─────────────────────────────────────────────
        //  Experience & Leveling
        // ─────────────────────────────────────────────
        public void AddExperience(long amount)
        {
            if (CurrentState == null) return;
            int oldLevel = CurrentState.PlayerLevel;
            CurrentState.TotalExperience += amount;
            CurrentState.RecalculateLevel();
            int newLevel = CurrentState.PlayerLevel;

            if (newLevel > oldLevel)
            {
                Debug.Log($"[GameManager] LEVEL UP! {oldLevel} → {newLevel} ({CurrentState.PlayerTitle})");
                GameEvents.ShowNotification($"🎉 Level Up! You are now Level {newLevel} — {CurrentState.PlayerTitle}");

                if (newLevel >= 200 && oldLevel < 200)
                {
                    GameEvents.ShowNotification("⚔️ BATTLES UNLOCKED! You have reached Senapati rank. Prepare for war!");
                }
            }
        }

        // ─────────────────────────────────────────────
        //  Rudra Powers Runtime
        // ─────────────────────────────────────────────
        public void ActivateDamaruBeat(float duration)
        {
            IsDamaruActive = true;
            DamaruTimeRemaining = duration;
        }

        public void ActivateMeditation(float duration)
        {
            IsMeditationActive = true;
            MeditationTimeRemaining = duration;
        }

        public float GetConstructionSpeedMultiplier()
        {
            float multiplier = GameConstants.CONSTRUCTION_SPEED_BASE;
            if (IsDamaruActive)
                multiplier *= GameConstants.DAMARU_SPEED_MULTIPLIER;
            return multiplier;
        }

        // ─────────────────────────────────────────────
        //  Pause / Phase
        // ─────────────────────────────────────────────
        public void PauseGame()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            GameEvents.GamePaused();
        }

        public void ResumeGame()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            GameEvents.GameResumed();
        }

        public void SetPhase(GamePhase phase)
        {
            var old = CurrentPhase;
            CurrentPhase = phase;
            Debug.Log($"[GameManager] Phase: {old} → {phase}");
            GameEvents.GamePhaseChanged(phase);
        }
    }
}
