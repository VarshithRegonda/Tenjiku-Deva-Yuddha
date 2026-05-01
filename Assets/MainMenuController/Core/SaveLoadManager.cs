using System;
using System.IO;
using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Handles saving and loading the GameState to/from local JSON files.
    /// Supports multiple save slots and autosave.
    /// </summary>
    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }

        private const string SAVE_FOLDER = "Saves";
        private const string AUTOSAVE_FILE = "autosave.json";
        private const int MAX_SAVE_SLOTS = 5;

        private float _autosaveTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSaveDirectory();
        }

        private void Update()
        {
            // Autosave timer
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != null &&
                GameManager.Instance.CurrentPhase == GamePhase.Playing)
            {
                _autosaveTimer += Time.deltaTime;
                if (_autosaveTimer >= GameConstants.AUTOSAVE_INTERVAL)
                {
                    _autosaveTimer = 0f;
                    AutoSave();
                }
            }
        }

        // ─────────────────────────────────────────────
        //  Save
        // ─────────────────────────────────────────────
        public bool SaveGame(GameState state, int slot)
        {
            if (slot < 0 || slot >= MAX_SAVE_SLOTS)
            {
                Debug.LogError($"[SaveLoad] Invalid save slot: {slot}");
                return false;
            }

            try
            {
                state.LastSavedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string json = JsonUtility.ToJson(state, true);
                string path = GetSavePath(slot);
                File.WriteAllText(path, json);
                Debug.Log($"[SaveLoad] Game saved to slot {slot}: {path}");
                GameEvents.GameSaved();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoad] Failed to save: {e.Message}");
                return false;
            }
        }

        public bool AutoSave()
        {
            if (GameManager.Instance?.CurrentState == null) return false;

            try
            {
                var state = GameManager.Instance.CurrentState;
                state.LastSavedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string json = JsonUtility.ToJson(state, true);
                string path = GetAutoSavePath();
                File.WriteAllText(path, json);
                Debug.Log("[SaveLoad] Autosaved.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoad] Auto-save failed: {e.Message}");
                return false;
            }
        }

        // ─────────────────────────────────────────────
        //  Load
        // ─────────────────────────────────────────────
        public GameState LoadGame(int slot)
        {
            string path = GetSavePath(slot);
            return LoadFromPath(path);
        }

        public GameState LoadAutoSave()
        {
            return LoadFromPath(GetAutoSavePath());
        }

        private GameState LoadFromPath(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[SaveLoad] No save found at: {path}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                GameState state = JsonUtility.FromJson<GameState>(json);
                Debug.Log($"[SaveLoad] Game loaded from: {path}");
                GameEvents.GameLoaded();
                return state;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveLoad] Failed to load: {e.Message}");
                return null;
            }
        }

        // ─────────────────────────────────────────────
        //  Utility
        // ─────────────────────────────────────────────
        public bool SaveExists(int slot)
        {
            return File.Exists(GetSavePath(slot));
        }

        public bool AutoSaveExists()
        {
            return File.Exists(GetAutoSavePath());
        }

        public void DeleteSave(int slot)
        {
            string path = GetSavePath(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[SaveLoad] Deleted save slot {slot}");
            }
        }

        /// <summary>
        /// Returns summary info for save slots (for the load game UI).
        /// </summary>
        public SaveSlotInfo GetSaveSlotInfo(int slot)
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path))
                return new SaveSlotInfo { Slot = slot, Exists = false };

            try
            {
                string json = File.ReadAllText(path);
                GameState state = JsonUtility.FromJson<GameState>(json);
                return new SaveSlotInfo
                {
                    Slot = slot,
                    Exists = true,
                    PlayerName = state.PlayerName,
                    KingdomName = state.KingdomName,
                    PlayerLevel = state.PlayerLevel,
                    AvatarAge = state.CurrentAvatarAge,
                    LastSaved = DateTimeOffset.FromUnixTimeSeconds(state.LastSavedTimestamp).LocalDateTime
                };
            }
            catch
            {
                return new SaveSlotInfo { Slot = slot, Exists = false };
            }
        }

        // ─────────────────────────────────────────────
        //  Paths
        // ─────────────────────────────────────────────
        private string GetSavePath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, SAVE_FOLDER, $"save_slot_{slot}.json");
        }

        private string GetAutoSavePath()
        {
            return Path.Combine(Application.persistentDataPath, SAVE_FOLDER, AUTOSAVE_FILE);
        }

        private void EnsureSaveDirectory()
        {
            string dir = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }

    [Serializable]
    public struct SaveSlotInfo
    {
        public int Slot;
        public bool Exists;
        public string PlayerName;
        public string KingdomName;
        public int PlayerLevel;
        public int AvatarAge;
        public DateTime LastSaved;
    }
}
