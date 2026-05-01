using System;
using System.Collections.Generic;
using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Handles building placement, construction progress, upgrading, and demolishing.
    /// Works with the GridManager to validate placement and the ResourceManager for costs.
    /// </summary>
    public class BuildingSystem : MonoBehaviour
    {
        public static BuildingSystem Instance { get; private set; }

        [Header("Build Mode")]
        public bool IsInBuildMode { get; private set; }
        public string SelectedBuildingType { get; private set; }

        // Runtime building GameObjects
        private Dictionary<string, GameObject> _buildingObjects = new();

        private void Awake()
        {
            Instance = this;
            BuildingDatabase.Initialize();
        }

        // ─────────────────────────────────────────────
        //  Build Mode
        // ─────────────────────────────────────────────
        public void EnterBuildMode(string buildingTypeId)
        {
            var def = BuildingDatabase.GetBuilding(buildingTypeId);
            if (def == null)
            {
                Debug.LogError($"[BuildingSystem] Unknown building type: {buildingTypeId}");
                return;
            }

            // Check age requirement
            var state = GameManager.Instance.CurrentState;
            if (state.CurrentAvatarAge < def.RequiredAge)
            {
                string requiredAvatar = GameConstants.AVATARS[def.RequiredAge].Name;
                GameEvents.ShowNotification($"Requires Age of {requiredAvatar} (Age {def.RequiredAge + 1})");
                return;
            }

            // Check resources
            if (!ResourceManager.Instance.HasEnough(def.Cost))
            {
                GameEvents.ShowNotification("Not enough resources!");
                GameEvents.ResourcesInsufficient();
                return;
            }

            IsInBuildMode = true;
            SelectedBuildingType = buildingTypeId;
            GameManager.Instance.SetPhase(GamePhase.BuildMode);
            Debug.Log($"[BuildingSystem] Entered build mode: {def.Name}");
        }

        public void ExitBuildMode()
        {
            IsInBuildMode = false;
            SelectedBuildingType = null;
            GameManager.Instance.SetPhase(GamePhase.Playing);
        }

        // ─────────────────────────────────────────────
        //  Place Building
        // ─────────────────────────────────────────────
        public bool PlaceBuilding(Vector2Int gridPos)
        {
            if (!IsInBuildMode || string.IsNullOrEmpty(SelectedBuildingType))
                return false;

            var def = BuildingDatabase.GetBuilding(SelectedBuildingType);
            if (def == null) return false;

            // Validate placement on grid
            if (!GridManager.Instance.CanPlaceBuilding(gridPos, def.Size))
            {
                GameEvents.ShowNotification("Cannot place here!");
                return false;
            }

            // Consume resources
            if (!ResourceManager.Instance.ConsumeResources(def.Cost))
                return false;

            // Create building state
            var buildingState = new BuildingState
            {
                UniqueId = Guid.NewGuid().ToString(),
                BuildingTypeId = SelectedBuildingType,
                GridX = gridPos.x,
                GridY = gridPos.y,
                Level = 1,
                ConstructionProgress = 0f,
                IsConstructed = false,
                PlacedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            // Add to game state
            var state = GameManager.Instance.CurrentState;
            state.Buildings.Add(buildingState);
            state.TotalBuildingsPlaced++;

            // Mark grid tiles as occupied
            GridManager.Instance.OccupyTiles(gridPos, def.Size, buildingState.UniqueId);

            // Create visual representation
            CreateBuildingVisual(buildingState, def, gridPos);

            // Apply max capacity bonuses immediately
            foreach (var bonus in def.MaxCapacityBonus)
                ResourceManager.Instance.IncreaseMaxResource(bonus.Key, bonus.Value);

            // Fire events
            GameEvents.BuildingPlaced(new BuildingPlacedEventData
            {
                BuildingId = buildingState.UniqueId,
                BuildingType = SelectedBuildingType,
                GridPosition = gridPos,
                Level = 1
            });

            // XP reward
            GameManager.Instance.AddExperience(def.XPReward);

            Debug.Log($"[BuildingSystem] Placed {def.Name} at ({gridPos.x}, {gridPos.y})");

            // Stay in build mode for multiple placements, or exit
            // ExitBuildMode();
            return true;
        }

        // ─────────────────────────────────────────────
        //  Construction Progress
        // ─────────────────────────────────────────────
        private void Update()
        {
            if (GameManager.Instance.CurrentState == null) return;
            if (GameManager.Instance.CurrentPhase != GamePhase.Playing &&
                GameManager.Instance.CurrentPhase != GamePhase.BuildMode) return;

            float dt = Time.deltaTime;
            float speedMult = GameManager.Instance.GetConstructionSpeedMultiplier();

            foreach (var building in GameManager.Instance.CurrentState.Buildings)
            {
                if (building.IsConstructed) continue;

                var def = BuildingDatabase.GetBuilding(building.BuildingTypeId);
                if (def == null) continue;

                float buildRate = (1f / def.BuildTimeSeconds) * speedMult;
                building.ConstructionProgress += buildRate * dt;

                if (building.ConstructionProgress >= 1f)
                {
                    building.ConstructionProgress = 1f;
                    building.IsConstructed = true;

                    // Recalculate production now that this building is completed
                    ResourceManager.Instance.RecalculateProductionRates();

                    GameEvents.BuildingCompleted(new BuildingPlacedEventData
                    {
                        BuildingId = building.UniqueId,
                        BuildingType = building.BuildingTypeId,
                        GridPosition = new Vector2Int(building.GridX, building.GridY),
                        Level = building.Level
                    });

                    GameEvents.ShowNotification($"✅ {def.Name} construction complete!");
                    Debug.Log($"[BuildingSystem] {def.Name} completed at ({building.GridX}, {building.GridY})");

                    // Update building visual
                    UpdateBuildingVisual(building);
                }
            }
        }

        // ─────────────────────────────────────────────
        //  Upgrade Building
        // ─────────────────────────────────────────────
        public bool UpgradeBuilding(string uniqueId)
        {
            var state = GameManager.Instance.CurrentState;
            var building = state.Buildings.Find(b => b.UniqueId == uniqueId);
            if (building == null || !building.IsConstructed) return false;

            var def = BuildingDatabase.GetBuilding(building.BuildingTypeId);
            if (def == null) return false;

            // Upgrade cost = base cost * level * 0.8
            var upgradeCost = new Dictionary<ResourceType, float>();
            foreach (var kvp in def.Cost)
                upgradeCost[kvp.Key] = kvp.Value * building.Level * 0.8f;

            if (!ResourceManager.Instance.ConsumeResources(upgradeCost))
                return false;

            building.Level++;
            building.ConstructionProgress = 0f;
            building.IsConstructed = false;   // needs to re-construct for upgrade

            GameEvents.BuildingUpgraded(new BuildingPlacedEventData
            {
                BuildingId = building.UniqueId,
                BuildingType = building.BuildingTypeId,
                GridPosition = new Vector2Int(building.GridX, building.GridY),
                Level = building.Level
            });

            GameManager.Instance.AddExperience(def.XPReward * building.Level);
            GameEvents.ShowNotification($"⬆️ Upgrading {def.Name} to Level {building.Level}!");
            return true;
        }

        // ─────────────────────────────────────────────
        //  Demolish Building
        // ─────────────────────────────────────────────
        public void DemolishBuilding(string uniqueId)
        {
            var state = GameManager.Instance.CurrentState;
            var building = state.Buildings.Find(b => b.UniqueId == uniqueId);
            if (building == null) return;

            var def = BuildingDatabase.GetBuilding(building.BuildingTypeId);
            var gridPos = new Vector2Int(building.GridX, building.GridY);

            // Free grid tiles
            if (def != null)
                GridManager.Instance.FreeTiles(gridPos, def.Size);

            // Refund 50% of base cost
            if (def != null)
            {
                foreach (var kvp in def.Cost)
                    ResourceManager.Instance.AddResource(kvp.Key, kvp.Value * 0.5f);
            }

            // Remove visual
            if (_buildingObjects.TryGetValue(uniqueId, out var obj))
            {
                Destroy(obj);
                _buildingObjects.Remove(uniqueId);
            }

            // Remove from state
            state.Buildings.Remove(building);

            // Recalculate production
            ResourceManager.Instance.RecalculateProductionRates();

            GameEvents.BuildingDemolished(new BuildingPlacedEventData
            {
                BuildingId = uniqueId,
                BuildingType = building.BuildingTypeId,
                GridPosition = gridPos,
                Level = building.Level
            });

            GameEvents.ShowNotification($"🏚️ {def?.Name ?? "Building"} demolished. 50% resources refunded.");
        }

        // ─────────────────────────────────────────────
        //  Visuals
        // ─────────────────────────────────────────────
        private void CreateBuildingVisual(BuildingState buildingState, BuildingDefinition def, Vector2Int gridPos)
        {
            Vector3 worldPos = GridManager.Instance.GridToWorld(gridPos);

            // Create a placeholder cube — will be replaced with proper prefabs later
            GameObject buildingObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            buildingObj.name = $"Building_{def.TypeId}_{buildingState.UniqueId[..8]}";
            buildingObj.transform.position = worldPos + new Vector3(
                (def.Size.x - 1) * 0.5f * GameConstants.TILE_SIZE,
                0.5f,
                (def.Size.y - 1) * 0.5f * GameConstants.TILE_SIZE
            );
            buildingObj.transform.localScale = new Vector3(
                def.Size.x * GameConstants.TILE_SIZE * 0.9f,
                1f,
                def.Size.y * GameConstants.TILE_SIZE * 0.9f
            );

            // Color by category
            var renderer = buildingObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = def.Category switch
                {
                    BuildingCategory.Village => new Color(0.6f, 0.4f, 0.2f),   // Brown
                    BuildingCategory.City => new Color(0.3f, 0.5f, 0.7f),      // Blue
                    BuildingCategory.Kingdom => new Color(0.8f, 0.6f, 0.2f),   // Gold
                    BuildingCategory.Divine => new Color(0.9f, 0.3f, 0.9f),    // Purple
                    _ => Color.gray
                };
                // Dim color during construction
                if (!buildingState.IsConstructed)
                    color *= 0.5f;
                renderer.material.color = color;
            }

            _buildingObjects[buildingState.UniqueId] = buildingObj;
        }

        private void UpdateBuildingVisual(BuildingState buildingState)
        {
            if (!_buildingObjects.TryGetValue(buildingState.UniqueId, out var obj)) return;

            var def = BuildingDatabase.GetBuilding(buildingState.BuildingTypeId);
            if (def == null) return;

            // Brighten color when construction is complete
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = def.Category switch
                {
                    BuildingCategory.Village => new Color(0.6f, 0.4f, 0.2f),
                    BuildingCategory.City => new Color(0.3f, 0.5f, 0.7f),
                    BuildingCategory.Kingdom => new Color(0.8f, 0.6f, 0.2f),
                    BuildingCategory.Divine => new Color(0.9f, 0.3f, 0.9f),
                    _ => Color.gray
                };
                renderer.material.color = color;
            }

            // Scale up slightly per level
            float levelScale = 1f + (buildingState.Level - 1) * 0.1f;
            obj.transform.localScale = new Vector3(
                def.Size.x * GameConstants.TILE_SIZE * 0.9f * levelScale,
                1f * levelScale,
                def.Size.y * GameConstants.TILE_SIZE * 0.9f * levelScale
            );
        }

        /// <summary>
        /// Rebuild all building visuals from saved state (called after loading a game).
        /// </summary>
        public void RebuildAllVisuals()
        {
            // Clear existing
            foreach (var obj in _buildingObjects.Values)
                if (obj != null) Destroy(obj);
            _buildingObjects.Clear();

            var state = GameManager.Instance.CurrentState;
            if (state?.Buildings == null) return;

            foreach (var building in state.Buildings)
            {
                var def = BuildingDatabase.GetBuilding(building.BuildingTypeId);
                if (def == null) continue;
                CreateBuildingVisual(building, def, new Vector2Int(building.GridX, building.GridY));
                if (building.IsConstructed)
                    UpdateBuildingVisual(building);
            }
        }

        public int GetConstructedBuildingCount()
        {
            return GameManager.Instance.CurrentState?.Buildings
                .FindAll(b => b.IsConstructed)?.Count ?? 0;
        }
    }
}
