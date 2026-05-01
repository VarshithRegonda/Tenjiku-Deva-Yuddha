using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Initializes all game systems when the GameScene is loaded.
    /// Attach this to a root GameObject in the GameScene.
    /// </summary>
    public class GameSceneInitializer : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("[GameScene] Initializing game systems...");

            var state = GameManager.Instance.CurrentState;
            if (state == null)
            {
                Debug.LogWarning("[GameScene] No game state found. Starting new game with defaults.");
                state = new GameState();
                state.InitializeNewGame("Arjuna", "Hastinapura");
                GameManager.Instance.StartNewGame("Arjuna", "Hastinapura");
                return; // StartNewGame will reload the scene
            }

            // Initialize building database
            BuildingDatabase.Initialize();

            // Initialize Grid
            var gridManager = FindAnyObjectByType<GridManager>();
            if (gridManager != null)
                gridManager.Initialize(state.MapWidth, state.MapHeight, state.MapSeed);

            // Initialize Resource Manager
            var resourceMgr = FindAnyObjectByType<ResourceManager>();
            if (resourceMgr != null)
                resourceMgr.Initialize(state);

            // Initialize Building System — rebuild visuals from saved state
            var buildingSys = FindAnyObjectByType<BuildingSystem>();
            if (buildingSys != null)
                buildingSys.RebuildAllVisuals();

            // Initialize UI
            var gameHUD = FindAnyObjectByType<GameHUD>();
            if (gameHUD != null)
                gameHUD.Initialize();

            // Notify GameManager that the scene is ready
            GameManager.Instance.OnGameSceneReady();

            Debug.Log($"[GameScene] Initialized! Player: {state.PlayerName} Lv.{state.PlayerLevel} | Age: {GameConstants.AVATARS[state.CurrentAvatarAge].Name}");
        }
    }
}
