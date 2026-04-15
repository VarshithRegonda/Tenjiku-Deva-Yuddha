using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Manages all 8 resource types, production rates, consumption, and capacity.
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        private GameState _state;

        // Per-resource production rates (per tick), built from active buildings
        private Dictionary<ResourceType, float> _productionRates = new();

        // Per-resource consumption rates (per tick)
        private Dictionary<ResourceType, float> _consumptionRates = new();

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize(GameState state)
        {
            _state = state;
            RecalculateProductionRates();
            GameManager.Instance.ResourceMgr = this;
        }

        // ─────────────────────────────────────────────
        //  Resource Access
        // ─────────────────────────────────────────────
        public float GetResource(ResourceType type) => _state.Resources.Get(type);
        public float GetMaxResource(ResourceType type) => _state.MaxResources.Get(type);
        public float GetProductionRate(ResourceType type) =>
            _productionRates.TryGetValue(type, out var r) ? r : 0f;

        public bool HasEnough(ResourceType type, float amount) =>
            _state.Resources.HasEnough(type, amount);

        public bool HasEnough(Dictionary<ResourceType, float> costs)
        {
            foreach (var kvp in costs)
                if (!HasEnough(kvp.Key, kvp.Value)) return false;
            return true;
        }

        // ─────────────────────────────────────────────
        //  Modify Resources
        // ─────────────────────────────────────────────
        public void AddResource(ResourceType type, float amount)
        {
            if (amount <= 0) return;
            float old = _state.Resources.Get(type);
            float max = _state.MaxResources.Get(type);

            // Apply avatar bonus
            float bonus = GetAvatarBonus(type);
            amount *= bonus;

            float newVal = Mathf.Min(old + amount, max);
            _state.Resources.Set(type, newVal);
            GameEvents.ResourceChanged(type, old, newVal);
            GameEvents.ResourceProduced(type, amount);
        }

        public bool ConsumeResource(ResourceType type, float amount)
        {
            if (amount <= 0) return true;
            float current = _state.Resources.Get(type);
            if (current < amount)
            {
                GameEvents.ResourcesInsufficient();
                return false;
            }
            float old = current;
            _state.Resources.Set(type, current - amount);
            GameEvents.ResourceChanged(type, old, current - amount);
            GameEvents.ResourceConsumed(type, amount);
            return true;
        }

        public bool ConsumeResources(Dictionary<ResourceType, float> costs)
        {
            if (!HasEnough(costs))
            {
                GameEvents.ResourcesInsufficient();
                return false;
            }
            foreach (var kvp in costs)
                ConsumeResource(kvp.Key, kvp.Value);
            return true;
        }

        public void IncreaseMaxResource(ResourceType type, float additionalCapacity)
        {
            float current = _state.MaxResources.Get(type);
            _state.MaxResources.Set(type, current + additionalCapacity);
        }

        // ─────────────────────────────────────────────
        //  Production Tick
        // ─────────────────────────────────────────────
        /// <summary>
        /// Called by GameManager every RESOURCE_TICK_INTERVAL seconds.
        /// Produces resources from all completed buildings.
        /// </summary>
        public void ProduceResources()
        {
            foreach (var kvp in _productionRates)
            {
                if (kvp.Value > 0)
                    AddResource(kvp.Key, kvp.Value);
            }

            // Consume food for population
            float population = _state.Resources.Get(ResourceType.Praja);
            float foodConsumption = population * 0.5f;  // each person eats 0.5 food per tick
            if (foodConsumption > 0)
            {
                float food = _state.Resources.Get(ResourceType.Anna);
                if (food < foodConsumption)
                {
                    // Starvation — population decreases
                    float lost = Mathf.Min(1f, foodConsumption - food);
                    _state.Resources.Set(ResourceType.Anna, 0);
                    float pop = _state.Resources.Get(ResourceType.Praja);
                    _state.Resources.Set(ResourceType.Praja, Mathf.Max(0, pop - lost));
                    GameEvents.ShowNotification("⚠️ Your people are starving!");
                }
                else
                {
                    ConsumeResource(ResourceType.Anna, foodConsumption);
                }
            }
        }

        // ─────────────────────────────────────────────
        //  Production Rate Calculation
        // ─────────────────────────────────────────────
        /// <summary>
        /// Recalculates production rates from all constructed buildings.
        /// Should be called when buildings are placed, completed, upgraded, or demolished.
        /// </summary>
        public void RecalculateProductionRates()
        {
            _productionRates.Clear();
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
                _productionRates[type] = 0f;

            if (_state?.Buildings == null) return;

            foreach (var building in _state.Buildings)
            {
                if (!building.IsConstructed) continue;

                var buildingDef = BuildingDatabase.GetBuilding(building.BuildingTypeId);
                if (buildingDef == null) continue;

                foreach (var production in buildingDef.ProductionPerTick)
                {
                    float rate = production.Value * building.Level;
                    _productionRates[production.Key] += rate;
                }
            }
        }

        // ─────────────────────────────────────────────
        //  Avatar Bonus
        // ─────────────────────────────────────────────
        private float GetAvatarBonus(ResourceType type)
        {
            if (_state == null) return 1f;
            int age = _state.CurrentAvatarAge;
            if (age < 0 || age >= GameConstants.AVATARS.Length) return 1f;

            var avatar = GameConstants.AVATARS[age];
            if (avatar.BonusResourceType == type)
                return avatar.BonusMultiplier;
            return 1f;
        }
    }
}
