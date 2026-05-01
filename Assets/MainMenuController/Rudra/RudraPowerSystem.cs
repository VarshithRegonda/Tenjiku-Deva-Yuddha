using System.Collections.Generic;
using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Lord Rudra's (Shiva's) divine intervention powers.
    /// Five powers: Tandava, Trishul Strike, Third Eye, Damaru Beat, Meditation.
    /// Each has Shakti cost, cooldown, and unique gameplay effect.
    /// </summary>
    public class RudraPowerSystem : MonoBehaviour
    {
        public static RudraPowerSystem Instance { get; private set; }

        private Dictionary<RudraPowerType, float> _cooldowns = new();

        private void Awake()
        {
            Instance = this;
            foreach (RudraPowerType type in System.Enum.GetValues(typeof(RudraPowerType)))
                _cooldowns[type] = 0f;
        }

        private void Update()
        {
            if (GameManager.Instance.CurrentPhase != GamePhase.Playing) return;

            // Update cooldowns
            float dt = Time.deltaTime;
            var keys = new List<RudraPowerType>(_cooldowns.Keys);
            foreach (var type in keys)
            {
                if (_cooldowns[type] > 0)
                {
                    _cooldowns[type] -= dt;
                    GameEvents.RudraPowerCooldownUpdate(type, _cooldowns[type]);

                    if (_cooldowns[type] <= 0)
                    {
                        _cooldowns[type] = 0;
                        GameEvents.RudraPowerReady(type);
                    }
                }
            }
        }

        // ─────────────────────────────────────────────
        //  Activate Powers
        // ─────────────────────────────────────────────
        public bool ActivatePower(RudraPowerType type, Vector2Int? targetGridPos = null)
        {
            // Check cooldown
            if (_cooldowns[type] > 0)
            {
                GameEvents.ShowNotification($"🔱 {type} is on cooldown! {_cooldowns[type]:F0}s remaining.");
                return false;
            }

            var powerInfo = GameConstants.RUDRA_POWERS[type];

            // Check Shakti cost
            if (powerInfo.ShaktiCost > 0 &&
                !ResourceManager.Instance.HasEnough(ResourceType.Shakti, powerInfo.ShaktiCost))
            {
                GameEvents.ShowNotification($"Not enough Shakti! Need {powerInfo.ShaktiCost}.");
                return false;
            }

            // Consume Shakti
            if (powerInfo.ShaktiCost > 0)
                ResourceManager.Instance.ConsumeResource(ResourceType.Shakti, powerInfo.ShaktiCost);

            // Start cooldown
            _cooldowns[type] = powerInfo.CooldownSeconds;

            // Execute power effect
            ExecutePower(type, targetGridPos);

            // Fire event
            GameEvents.RudraPowerActivated(type);
            GameEvents.ShowNotification($"🔱 {powerInfo.Name} activated!");

            // XP for using divine powers
            GameManager.Instance.AddExperience(25);

            Debug.Log($"[Rudra] {powerInfo.Name} activated! Cooldown: {powerInfo.CooldownSeconds}s");
            return true;
        }

        private void ExecutePower(RudraPowerType type, Vector2Int? targetPos)
        {
            switch (type)
            {
                case RudraPowerType.Tandava:
                    ExecuteTandava(targetPos);
                    break;
                case RudraPowerType.TrishulStrike:
                    ExecuteTrishulStrike(targetPos);
                    break;
                case RudraPowerType.ThirdEye:
                    ExecuteThirdEye(targetPos);
                    break;
                case RudraPowerType.DamaruBeat:
                    ExecuteDamaruBeat();
                    break;
                case RudraPowerType.Meditation:
                    ExecuteMeditation();
                    break;
            }
        }

        // ─────────────────────────────────────────────
        //  Individual Power Implementations
        // ─────────────────────────────────────────────

        /// <summary>
        /// TANDAVA — The cosmic dance of destruction.
        /// Clears terrain (forests/mountains become plains) in a wide area.
        /// Useful for land clearing before building.
        /// </summary>
        private void ExecuteTandava(Vector2Int? center)
        {
            if (!center.HasValue) return;
            Vector2Int c = center.Value;
            int radius = 5;

            int cleared = 0;
            for (int x = c.x - radius; x <= c.x + radius; x++)
            {
                for (int y = c.y - radius; y <= c.y + radius; y++)
                {
                    if (!GridManager.Instance.IsValidPosition(x, y)) continue;
                    float dist = Vector2Int.Distance(c, new Vector2Int(x, y));
                    if (dist > radius) continue;

                    var tile = GridManager.Instance.GetTile(x, y);
                    if (tile != null && (tile.Terrain == TerrainType.Forest ||
                                          tile.Terrain == TerrainType.Mountain ||
                                          tile.Terrain == TerrainType.Desert))
                    {
                        tile.Terrain = TerrainType.Plains;
                        cleared++;
                    }
                }
            }

            // TODO: Spawn VFX — cosmic dance shockwave
            GameEvents.ShowNotification($"🕺 Tandava! {cleared} tiles transformed by Lord Shiva's dance!");
            Debug.Log($"[Rudra] Tandava cleared {cleared} tiles around ({c.x}, {c.y})");
        }

        /// <summary>
        /// TRISHUL STRIKE — Targeted divine attack.
        /// Demolishes a single building instantly (useful for rebuilding or removing misplaced structures).
        /// In battle mode, deals massive damage to an enemy target.
        /// </summary>
        private void ExecuteTrishulStrike(Vector2Int? target)
        {
            if (!target.HasValue) return;
            var tile = GridManager.Instance.GetTile(target.Value);

            if (tile != null && tile.IsOccupied && !string.IsNullOrEmpty(tile.OccupantBuildingId))
            {
                string buildingId = tile.OccupantBuildingId;
                BuildingSystem.Instance.DemolishBuilding(buildingId);
                GameEvents.ShowNotification("🔱 Trishul Strike! Building struck by the divine trident!");
            }
            else
            {
                // Clear terrain at target
                if (tile != null && tile.Terrain != TerrainType.Plains)
                {
                    tile.Terrain = TerrainType.Plains;
                    GameEvents.ShowNotification("🔱 Trishul Strike! Terrain cleared by divine force!");
                }
                else
                {
                    GameEvents.ShowNotification("🔱 Trishul Strike hits the ground... nothing to target here.");
                }
            }

            // TODO: Spawn VFX — trident impact
        }

        /// <summary>
        /// THIRD EYE — Reveal hidden resources.
        /// Reveals SacredGround tiles in a large area (sacred ground gives building bonuses).
        /// Also reveals resource-rich terrain.
        /// </summary>
        private void ExecuteThirdEye(Vector2Int? center)
        {
            Vector2Int c = center ?? GridManager.Instance.GetMapCenter();
            int radius = 15;
            int revealed = 0;

            for (int x = c.x - radius; x <= c.x + radius; x++)
            {
                for (int y = c.y - radius; y <= c.y + radius; y++)
                {
                    if (!GridManager.Instance.IsValidPosition(x, y)) continue;
                    float dist = Vector2Int.Distance(c, new Vector2Int(x, y));
                    if (dist > radius) continue;

                    // Small chance to convert a tile to Sacred Ground
                    var tile = GridManager.Instance.GetTile(x, y);
                    if (tile != null && tile.Terrain == TerrainType.Plains && Random.value < 0.05f)
                    {
                        tile.Terrain = TerrainType.SacredGround;
                        revealed++;
                    }
                }
            }

            // Bonus: reveal some extra resources
            ResourceManager.Instance.AddResource(ResourceType.Shakti, 20f);
            ResourceManager.Instance.AddResource(ResourceType.Vidya, 15f);

            // TODO: Spawn VFX — third eye glow effect
            GameEvents.ShowNotification($"👁️ Third Eye opened! {revealed} sacred sites revealed. Bonus Shakti and Vidya granted.");
        }

        /// <summary>
        /// DAMARU BEAT — Speed up everything.
        /// Triples construction speed and research speed for 30 seconds.
        /// </summary>
        private void ExecuteDamaruBeat()
        {
            float duration = 30f;
            GameManager.Instance.ActivateDamaruBeat(duration);

            // TODO: Spawn VFX — rhythmic pulse effect across all buildings
            GameEvents.ShowNotification($"🥁 Damaru Beat! All construction and research {GameConstants.DAMARU_SPEED_MULTIPLIER}x faster for {duration}s!");
        }

        /// <summary>
        /// MEDITATION — Generate Shakti over time.
        /// Passively generates Shakti for 30 seconds. Free to use (no Shakti cost) but long cooldown.
        /// </summary>
        private void ExecuteMeditation()
        {
            float duration = GameConstants.MEDITATION_DURATION;
            GameManager.Instance.ActivateMeditation(duration);

            // TODO: Spawn VFX — peaceful glow over the kingdom
            GameEvents.ShowNotification($"🧘 Meditation begun. Generating {GameConstants.MEDITATION_SHAKTI_PER_SECOND}/s Shakti for {duration}s.");
        }

        // ─────────────────────────────────────────────
        //  Info
        // ─────────────────────────────────────────────
        public float GetCooldownRemaining(RudraPowerType type) =>
            _cooldowns.TryGetValue(type, out var cd) ? cd : 0f;

        public bool IsReady(RudraPowerType type) => GetCooldownRemaining(type) <= 0f;

        public bool RequiresTarget(RudraPowerType type) =>
            type == RudraPowerType.Tandava ||
            type == RudraPowerType.TrishulStrike ||
            type == RudraPowerType.ThirdEye;
    }
}
