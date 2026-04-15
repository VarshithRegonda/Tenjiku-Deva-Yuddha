using System;
using System.Collections.Generic;
using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Central event bus for decoupled communication between game systems.
    /// All game events flow through this system to keep components independent.
    /// </summary>
    public static class GameEvents
    {
        // ─────────────────────────────────────────────
        //  Resource Events
        // ─────────────────────────────────────────────
        public static event Action<ResourceType, float, float> OnResourceChanged;         // type, oldVal, newVal
        public static event Action<ResourceType, float> OnResourceProduced;                // type, amount
        public static event Action<ResourceType, float> OnResourceConsumed;                // type, amount
        public static event Action OnResourcesInsufficient;                                // not enough to build

        public static void ResourceChanged(ResourceType type, float oldVal, float newVal)
            => OnResourceChanged?.Invoke(type, oldVal, newVal);
        public static void ResourceProduced(ResourceType type, float amount)
            => OnResourceProduced?.Invoke(type, amount);
        public static void ResourceConsumed(ResourceType type, float amount)
            => OnResourceConsumed?.Invoke(type, amount);
        public static void ResourcesInsufficient()
            => OnResourcesInsufficient?.Invoke();

        // ─────────────────────────────────────────────
        //  Building Events
        // ─────────────────────────────────────────────
        public static event Action<BuildingPlacedEventData> OnBuildingPlaced;
        public static event Action<BuildingPlacedEventData> OnBuildingCompleted;
        public static event Action<BuildingPlacedEventData> OnBuildingUpgraded;
        public static event Action<BuildingPlacedEventData> OnBuildingDemolished;
        public static event Action<string> OnBuildingSelected;                             // buildingId

        public static void BuildingPlaced(BuildingPlacedEventData data)
            => OnBuildingPlaced?.Invoke(data);
        public static void BuildingCompleted(BuildingPlacedEventData data)
            => OnBuildingCompleted?.Invoke(data);
        public static void BuildingUpgraded(BuildingPlacedEventData data)
            => OnBuildingUpgraded?.Invoke(data);
        public static void BuildingDemolished(BuildingPlacedEventData data)
            => OnBuildingDemolished?.Invoke(data);
        public static void BuildingSelected(string buildingId)
            => OnBuildingSelected?.Invoke(buildingId);

        // ─────────────────────────────────────────────
        //  Dashavatar / Age Events
        // ─────────────────────────────────────────────
        public static event Action<int, int> OnAgeChanged;                                 // oldAge, newAge (0-9)
        public static event Action<int> OnAvatarUnlocked;                                  // avatarIndex
        public static event Action<int, float> OnAgeProgressUpdated;                       // ageIndex, progress (0-1)

        public static void AgeChanged(int oldAge, int newAge)
            => OnAgeChanged?.Invoke(oldAge, newAge);
        public static void AvatarUnlocked(int avatarIndex)
            => OnAvatarUnlocked?.Invoke(avatarIndex);
        public static void AgeProgressUpdated(int ageIndex, float progress)
            => OnAgeProgressUpdated?.Invoke(ageIndex, progress);

        // ─────────────────────────────────────────────
        //  Rudra Power Events
        // ─────────────────────────────────────────────
        public static event Action<RudraPowerType> OnRudraPowerActivated;
        public static event Action<RudraPowerType> OnRudraPowerReady;
        public static event Action<RudraPowerType, float> OnRudraPowerCooldownUpdate;      // type, remaining seconds

        public static void RudraPowerActivated(RudraPowerType type)
            => OnRudraPowerActivated?.Invoke(type);
        public static void RudraPowerReady(RudraPowerType type)
            => OnRudraPowerReady?.Invoke(type);
        public static void RudraPowerCooldownUpdate(RudraPowerType type, float remaining)
            => OnRudraPowerCooldownUpdate?.Invoke(remaining > 0 ? type : type, remaining);

        // ─────────────────────────────────────────────
        //  Game State Events
        // ─────────────────────────────────────────────
        public static event Action OnGameSaved;
        public static event Action OnGameLoaded;
        public static event Action OnGamePaused;
        public static event Action OnGameResumed;
        public static event Action<GamePhase> OnGamePhaseChanged;

        public static void GameSaved() => OnGameSaved?.Invoke();
        public static void GameLoaded() => OnGameLoaded?.Invoke();
        public static void GamePaused() => OnGamePaused?.Invoke();
        public static void GameResumed() => OnGameResumed?.Invoke();
        public static void GamePhaseChanged(GamePhase phase) => OnGamePhaseChanged?.Invoke(phase);

        // ─────────────────────────────────────────────
        //  UI Events
        // ─────────────────────────────────────────────
        public static event Action<string> OnNotification;                                 // message
        public static event Action<string, string> OnDialogRequested;                      // title, message

        public static void ShowNotification(string message) => OnNotification?.Invoke(message);
        public static void ShowDialog(string title, string message) => OnDialogRequested?.Invoke(title, message);

        // ─────────────────────────────────────────────
        //  Grid / Map Events
        // ─────────────────────────────────────────────
        public static event Action<Vector2Int> OnTileSelected;
        public static event Action<Vector2Int> OnTileHovered;

        public static void TileSelected(Vector2Int pos) => OnTileSelected?.Invoke(pos);
        public static void TileHovered(Vector2Int pos) => OnTileHovered?.Invoke(pos);
    }

    // ─────────────────────────────────────────────
    //  Event Data Structures
    // ─────────────────────────────────────────────
    [Serializable]
    public struct BuildingPlacedEventData
    {
        public string BuildingId;
        public string BuildingType;
        public Vector2Int GridPosition;
        public int Level;
    }

    // ─────────────────────────────────────────────
    //  Enums
    // ─────────────────────────────────────────────
    public enum ResourceType
    {
        Suvarna,    // Gold
        Anna,       // Food
        Pashana,    // Stone
        Kashtha,    // Wood
        Loha,       // Iron
        Shakti,     // Divine Energy
        Vidya,      // Knowledge
        Praja       // Population
    }

    public enum RudraPowerType
    {
        Tandava,        // Destroy / clear land
        TrishulStrike,  // Targeted divine attack
        ThirdEye,       // Reveal hidden resources
        DamaruBeat,     // Speed up construction
        Meditation      // Generate divine energy
    }

    public enum GamePhase
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        BuildMode,
        AvatarCeremony
    }

    public enum BuildingCategory
    {
        Village,
        City,
        Kingdom,
        Divine
    }

    public enum TerrainType
    {
        Plains,
        Water,
        Forest,
        Mountain,
        SacredGround,
        Desert,
        Riverbank
    }
}
