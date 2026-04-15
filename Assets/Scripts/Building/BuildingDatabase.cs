using System.Collections.Generic;
using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Static database of all building definitions.
    /// In a full game this would load from ScriptableObjects, but for initial development
    /// we define them in code for rapid iteration.
    /// </summary>
    public static class BuildingDatabase
    {
        private static Dictionary<string, BuildingDefinition> _buildings;

        public static void Initialize()
        {
            _buildings = new Dictionary<string, BuildingDefinition>();
            RegisterAllBuildings();
        }

        public static BuildingDefinition GetBuilding(string typeId)
        {
            if (_buildings == null) Initialize();
            return _buildings.TryGetValue(typeId, out var b) ? b : null;
        }

        public static List<BuildingDefinition> GetBuildingsForAge(int avatarAge)
        {
            if (_buildings == null) Initialize();
            var result = new List<BuildingDefinition>();
            foreach (var b in _buildings.Values)
                if (b.RequiredAge <= avatarAge)
                    result.Add(b);
            return result;
        }

        public static List<BuildingDefinition> GetBuildingsByCategory(BuildingCategory category)
        {
            if (_buildings == null) Initialize();
            var result = new List<BuildingDefinition>();
            foreach (var b in _buildings.Values)
                if (b.Category == category)
                    result.Add(b);
            return result;
        }

        // ─────────────────────────────────────────────
        //  Building Definitions
        // ─────────────────────────────────────────────
        private static void RegisterAllBuildings()
        {
            // ══════════════════════════════════════════
            //  AGE 0 — MATSYA (Primordial Waters)
            // ══════════════════════════════════════════
            Register(new BuildingDefinition
            {
                TypeId = "Hut",
                Name = "Kutir (Hut)",
                Description = "A simple dwelling for your people. Provides shelter and increases population capacity.",
                Category = BuildingCategory.Village,
                RequiredAge = 0,
                Size = new Vector2Int(1, 1),
                BuildTimeSeconds = 10f,
                XPReward = 10,
                Cost = new() { { ResourceType.Kashtha, 20f } },
                ProductionPerTick = new(),
                MaxCapacityBonus = new() { { ResourceType.Praja, 5f } },
                PrefabName = "Building_Hut"
            });

            Register(new BuildingDefinition
            {
                TypeId = "FishingDock",
                Name = "Matsya Ghaat (Fishing Dock)",
                Description = "A dock on the water's edge where fishermen gather food from the river.",
                Category = BuildingCategory.Village,
                RequiredAge = 0,
                Size = new Vector2Int(2, 1),
                BuildTimeSeconds = 20f,
                XPReward = 20,
                Cost = new() { { ResourceType.Kashtha, 30f }, { ResourceType.Pashana, 10f } },
                ProductionPerTick = new() { { ResourceType.Anna, 5f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_FishingDock"
            });

            Register(new BuildingDefinition
            {
                TypeId = "Well",
                Name = "Koop (Well)",
                Description = "A sacred well providing clean water. Boosts population happiness and food production nearby.",
                Category = BuildingCategory.Village,
                RequiredAge = 0,
                Size = new Vector2Int(1, 1),
                BuildTimeSeconds = 15f,
                XPReward = 15,
                Cost = new() { { ResourceType.Pashana, 25f } },
                ProductionPerTick = new() { { ResourceType.Anna, 2f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_Well"
            });

            Register(new BuildingDefinition
            {
                TypeId = "SacredAltar",
                Name = "Vedi (Sacred Altar)",
                Description = "An altar for sacred rituals. Generates Shakti (divine energy) through prayers.",
                Category = BuildingCategory.Divine,
                RequiredAge = 0,
                Size = new Vector2Int(1, 1),
                BuildTimeSeconds = 25f,
                XPReward = 30,
                Cost = new() { { ResourceType.Pashana, 20f }, { ResourceType.Kashtha, 15f } },
                ProductionPerTick = new() { { ResourceType.Shakti, 2f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_SacredAltar"
            });

            Register(new BuildingDefinition
            {
                TypeId = "ReedBoat",
                Name = "Nauka (Reed Boat)",
                Description = "A small boat for river exploration. Unlocks water-based resource gathering.",
                Category = BuildingCategory.Village,
                RequiredAge = 0,
                Size = new Vector2Int(1, 1),
                BuildTimeSeconds = 15f,
                XPReward = 15,
                Cost = new() { { ResourceType.Kashtha, 25f } },
                ProductionPerTick = new() { { ResourceType.Anna, 3f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_ReedBoat"
            });

            // ══════════════════════════════════════════
            //  AGE 1 — KURMA (Foundation Era)
            // ══════════════════════════════════════════
            Register(new BuildingDefinition
            {
                TypeId = "StoneQuarry",
                Name = "Pashana Khadan (Stone Quarry)",
                Description = "A quarry for extracting stone — the foundation of civilization.",
                Category = BuildingCategory.Village,
                RequiredAge = 1,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 40f,
                XPReward = 40,
                Cost = new() { { ResourceType.Kashtha, 40f }, { ResourceType.Suvarna, 50f } },
                ProductionPerTick = new() { { ResourceType.Pashana, 8f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_StoneQuarry"
            });

            Register(new BuildingDefinition
            {
                TypeId = "Temple",
                Name = "Mandir (Temple)",
                Description = "A sacred temple dedicated to the gods. Generates Shakti and Vidya.",
                Category = BuildingCategory.Divine,
                RequiredAge = 1,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 60f,
                XPReward = 60,
                Cost = new() { { ResourceType.Pashana, 80f }, { ResourceType.Suvarna, 100f }, { ResourceType.Kashtha, 30f } },
                ProductionPerTick = new() { { ResourceType.Shakti, 5f }, { ResourceType.Vidya, 3f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_Temple"
            });

            Register(new BuildingDefinition
            {
                TypeId = "Granary",
                Name = "Bhandaar (Granary)",
                Description = "A large storehouse for food. Dramatically increases food storage capacity.",
                Category = BuildingCategory.Village,
                RequiredAge = 1,
                Size = new Vector2Int(2, 1),
                BuildTimeSeconds = 30f,
                XPReward = 30,
                Cost = new() { { ResourceType.Kashtha, 50f }, { ResourceType.Pashana, 30f } },
                ProductionPerTick = new(),
                MaxCapacityBonus = new() { { ResourceType.Anna, 500f } },
                PrefabName = "Building_Granary"
            });

            Register(new BuildingDefinition
            {
                TypeId = "StoneHouse",
                Name = "Pashana Griha (Stone House)",
                Description = "A sturdy stone dwelling. Provides more population capacity than huts.",
                Category = BuildingCategory.Village,
                RequiredAge = 1,
                Size = new Vector2Int(1, 1),
                BuildTimeSeconds = 25f,
                XPReward = 25,
                Cost = new() { { ResourceType.Pashana, 40f }, { ResourceType.Kashtha, 15f } },
                ProductionPerTick = new(),
                MaxCapacityBonus = new() { { ResourceType.Praja, 10f } },
                PrefabName = "Building_StoneHouse"
            });

            Register(new BuildingDefinition
            {
                TypeId = "MeditationCircle",
                Name = "Dhyana Mandal (Meditation Circle)",
                Description = "A circle of sacred stones where sages meditate. Boosts Shakti and Vidya generation.",
                Category = BuildingCategory.Divine,
                RequiredAge = 1,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 35f,
                XPReward = 35,
                Cost = new() { { ResourceType.Pashana, 50f }, { ResourceType.Shakti, 20f } },
                ProductionPerTick = new() { { ResourceType.Shakti, 4f }, { ResourceType.Vidya, 2f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_MeditationCircle"
            });

            // ══════════════════════════════════════════
            //  AGE 2 — VARAHA (Land Reclamation)
            // ══════════════════════════════════════════
            Register(new BuildingDefinition
            {
                TypeId = "Farm",
                Name = "Krishi Kshetra (Farm)",
                Description = "Cultivated fields producing abundant food for your growing population.",
                Category = BuildingCategory.Village,
                RequiredAge = 2,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 30f,
                XPReward = 30,
                Cost = new() { { ResourceType.Kashtha, 30f }, { ResourceType.Suvarna, 25f } },
                ProductionPerTick = new() { { ResourceType.Anna, 12f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_Farm"
            });

            Register(new BuildingDefinition
            {
                TypeId = "IrrigationCanal",
                Name = "Sinchai Nala (Irrigation Canal)",
                Description = "A canal system bringing water to farms. Doubles nearby farm production.",
                Category = BuildingCategory.Village,
                RequiredAge = 2,
                Size = new Vector2Int(3, 1),
                BuildTimeSeconds = 45f,
                XPReward = 45,
                Cost = new() { { ResourceType.Pashana, 60f }, { ResourceType.Suvarna, 40f } },
                ProductionPerTick = new() { { ResourceType.Anna, 8f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_IrrigationCanal"
            });

            Register(new BuildingDefinition
            {
                TypeId = "Windmill",
                Name = "Chakki (Windmill)",
                Description = "Grinds grain into flour. Converts raw food into stored food more efficiently.",
                Category = BuildingCategory.Village,
                RequiredAge = 2,
                Size = new Vector2Int(1, 1),
                BuildTimeSeconds = 35f,
                XPReward = 35,
                Cost = new() { { ResourceType.Kashtha, 45f }, { ResourceType.Loha, 15f } },
                ProductionPerTick = new() { { ResourceType.Anna, 6f } },
                MaxCapacityBonus = new() { { ResourceType.Anna, 200f } },
                PrefabName = "Building_Windmill"
            });

            Register(new BuildingDefinition
            {
                TypeId = "LumberMill",
                Name = "Kashtha Shaala (Lumber Mill)",
                Description = "Processes wood from forests. Essential for construction-heavy ages.",
                Category = BuildingCategory.Village,
                RequiredAge = 2,
                Size = new Vector2Int(2, 1),
                BuildTimeSeconds = 30f,
                XPReward = 30,
                Cost = new() { { ResourceType.Pashana, 25f }, { ResourceType.Suvarna, 30f } },
                ProductionPerTick = new() { { ResourceType.Kashtha, 8f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_LumberMill"
            });

            // ══════════════════════════════════════════
            //  AGE 3 — NARASIMHA (Fortress Age)
            // ══════════════════════════════════════════
            Register(new BuildingDefinition
            {
                TypeId = "Fortress",
                Name = "Durg (Fortress)",
                Description = "A mighty stone fortress. Provides massive defense and unlocks military training.",
                Category = BuildingCategory.Kingdom,
                RequiredAge = 3,
                Size = new Vector2Int(3, 3),
                BuildTimeSeconds = 120f,
                XPReward = 100,
                Cost = new() { { ResourceType.Pashana, 200f }, { ResourceType.Kashtha, 80f }, { ResourceType.Loha, 50f }, { ResourceType.Suvarna, 150f } },
                ProductionPerTick = new() { { ResourceType.Shakti, 3f } },
                MaxCapacityBonus = new() { { ResourceType.Praja, 30f } },
                PrefabName = "Building_Fortress"
            });

            Register(new BuildingDefinition
            {
                TypeId = "WatchTower",
                Name = "Prahari Stambh (Watch Tower)",
                Description = "A tall tower for scouts. Reveals fog of war and provides early warning of attacks.",
                Category = BuildingCategory.Kingdom,
                RequiredAge = 3,
                Size = new Vector2Int(1, 1),
                BuildTimeSeconds = 40f,
                XPReward = 35,
                Cost = new() { { ResourceType.Pashana, 60f }, { ResourceType.Kashtha, 30f } },
                ProductionPerTick = new(),
                MaxCapacityBonus = new(),
                PrefabName = "Building_WatchTower"
            });

            Register(new BuildingDefinition
            {
                TypeId = "Barracks",
                Name = "Sainik Shivir (Barracks)",
                Description = "Train Kshatriya warriors here. Required for building your army.",
                Category = BuildingCategory.Kingdom,
                RequiredAge = 3,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 60f,
                XPReward = 50,
                Cost = new() { { ResourceType.Pashana, 80f }, { ResourceType.Kashtha, 50f }, { ResourceType.Loha, 30f } },
                ProductionPerTick = new(),
                MaxCapacityBonus = new(),
                PrefabName = "Building_Barracks",
                TrainsWarriorType = WarriorType.Kshatriya
            });

            Register(new BuildingDefinition
            {
                TypeId = "IronMine",
                Name = "Loha Khadan (Iron Mine)",
                Description = "Deep mine extracting iron ore. Essential for weapons and advanced construction.",
                Category = BuildingCategory.Village,
                RequiredAge = 3,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 50f,
                XPReward = 45,
                Cost = new() { { ResourceType.Pashana, 70f }, { ResourceType.Kashtha, 40f }, { ResourceType.Suvarna, 60f } },
                ProductionPerTick = new() { { ResourceType.Loha, 6f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_IronMine"
            });

            Register(new BuildingDefinition
            {
                TypeId = "StoneWall",
                Name = "Pracheer (Stone Wall)",
                Description = "Defensive wall segment. Connect walls to form a perimeter around your kingdom.",
                Category = BuildingCategory.Kingdom,
                RequiredAge = 3,
                Size = new Vector2Int(1, 1),
                BuildTimeSeconds = 15f,
                XPReward = 10,
                Cost = new() { { ResourceType.Pashana, 30f } },
                ProductionPerTick = new(),
                MaxCapacityBonus = new(),
                PrefabName = "Building_StoneWall"
            });

            // ══════════════════════════════════════════
            //  AGE 4 — VAMANA (Expansion)
            // ══════════════════════════════════════════
            Register(new BuildingDefinition
            {
                TypeId = "Marketplace",
                Name = "Bazaar (Marketplace)",
                Description = "A bustling marketplace. Generates gold from trade and attracts population.",
                Category = BuildingCategory.City,
                RequiredAge = 4,
                Size = new Vector2Int(3, 2),
                BuildTimeSeconds = 70f,
                XPReward = 60,
                Cost = new() { { ResourceType.Pashana, 100f }, { ResourceType.Kashtha, 70f }, { ResourceType.Suvarna, 100f } },
                ProductionPerTick = new() { { ResourceType.Suvarna, 15f }, { ResourceType.Praja, 1f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_Marketplace"
            });

            Register(new BuildingDefinition
            {
                TypeId = "TradePost",
                Name = "Vanijya Kendra (Trade Post)",
                Description = "Establish trade routes with distant lands. Increases gold and rare resource income.",
                Category = BuildingCategory.City,
                RequiredAge = 4,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 60f,
                XPReward = 55,
                Cost = new() { { ResourceType.Kashtha, 60f }, { ResourceType.Suvarna, 150f } },
                ProductionPerTick = new() { { ResourceType.Suvarna, 10f }, { ResourceType.Loha, 3f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_TradePost"
            });

            Register(new BuildingDefinition
            {
                TypeId = "Warehouse",
                Name = "Bhawan (Warehouse)",
                Description = "Large storage facility. Massively increases capacity for all resources.",
                Category = BuildingCategory.City,
                RequiredAge = 4,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 45f,
                XPReward = 40,
                Cost = new() { { ResourceType.Pashana, 80f }, { ResourceType.Kashtha, 60f }, { ResourceType.Loha, 20f } },
                ProductionPerTick = new(),
                MaxCapacityBonus = new()
                {
                    { ResourceType.Suvarna, 1000f }, { ResourceType.Anna, 500f },
                    { ResourceType.Pashana, 500f }, { ResourceType.Kashtha, 500f },
                    { ResourceType.Loha, 300f }
                },
                PrefabName = "Building_Warehouse"
            });

            Register(new BuildingDefinition
            {
                TypeId = "GoldMine",
                Name = "Suvarna Khadan (Gold Mine)",
                Description = "A mine rich in gold deposits. Primary source of Suvarna.",
                Category = BuildingCategory.City,
                RequiredAge = 4,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 55f,
                XPReward = 50,
                Cost = new() { { ResourceType.Pashana, 90f }, { ResourceType.Kashtha, 40f }, { ResourceType.Loha, 30f } },
                ProductionPerTick = new() { { ResourceType.Suvarna, 12f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_GoldMine"
            });

            // ══════════════════════════════════════════
            //  AGE 5 — PARASHURAMA (Warrior Age)
            // ══════════════════════════════════════════
            Register(new BuildingDefinition
            {
                TypeId = "TrainingGround",
                Name = "Vyayam Bhumi (Training Ground)",
                Description = "Advanced military training facility. Trains elite warriors faster.",
                Category = BuildingCategory.Kingdom,
                RequiredAge = 5,
                Size = new Vector2Int(3, 3),
                BuildTimeSeconds = 90f,
                XPReward = 75,
                Cost = new() { { ResourceType.Pashana, 120f }, { ResourceType.Loha, 60f }, { ResourceType.Suvarna, 100f } },
                ProductionPerTick = new(),
                MaxCapacityBonus = new(),
                PrefabName = "Building_TrainingGround"
            });

            Register(new BuildingDefinition
            {
                TypeId = "Armory",
                Name = "Shastra Gaar (Armory)",
                Description = "Forge weapons and armor for your warriors. Increases army combat effectiveness.",
                Category = BuildingCategory.Kingdom,
                RequiredAge = 5,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 70f,
                XPReward = 60,
                Cost = new() { { ResourceType.Loha, 80f }, { ResourceType.Pashana, 50f }, { ResourceType.Suvarna, 80f } },
                ProductionPerTick = new() { { ResourceType.Loha, 4f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_Armory"
            });

            Register(new BuildingDefinition
            {
                TypeId = "ArcheryRange",
                Name = "Dhanush Shaala (Archery Range)",
                Description = "Train Dhanurdhar (archers) here. Archers provide powerful ranged support.",
                Category = BuildingCategory.Kingdom,
                RequiredAge = 5,
                Size = new Vector2Int(3, 2),
                BuildTimeSeconds = 60f,
                XPReward = 55,
                Cost = new() { { ResourceType.Kashtha, 80f }, { ResourceType.Loha, 40f }, { ResourceType.Suvarna, 60f } },
                ProductionPerTick = new(),
                MaxCapacityBonus = new(),
                PrefabName = "Building_ArcheryRange",
                TrainsWarriorType = WarriorType.Dhanurdhar
            });

            // ══════════════════════════════════════════
            //  AGE 6 — RAMA (Kingdom of Dharma)
            // ══════════════════════════════════════════
            Register(new BuildingDefinition
            {
                TypeId = "RoyalPalace",
                Name = "Raj Mahal (Royal Palace)",
                Description = "A magnificent palace befitting a great king. Massively boosts all production and population.",
                Category = BuildingCategory.Kingdom,
                RequiredAge = 6,
                Size = new Vector2Int(4, 4),
                BuildTimeSeconds = 180f,
                XPReward = 150,
                Cost = new() { { ResourceType.Pashana, 300f }, { ResourceType.Kashtha, 150f }, { ResourceType.Loha, 100f }, { ResourceType.Suvarna, 500f }, { ResourceType.Shakti, 50f } },
                ProductionPerTick = new() { { ResourceType.Suvarna, 20f }, { ResourceType.Shakti, 8f }, { ResourceType.Vidya, 5f } },
                MaxCapacityBonus = new() { { ResourceType.Praja, 100f } },
                PrefabName = "Building_RoyalPalace"
            });

            Register(new BuildingDefinition
            {
                TypeId = "CourtOfJustice",
                Name = "Nyaya Sabha (Court of Justice)",
                Description = "Dispense dharmic justice. Increases population happiness and Vidya generation.",
                Category = BuildingCategory.Kingdom,
                RequiredAge = 6,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 80f,
                XPReward = 70,
                Cost = new() { { ResourceType.Pashana, 120f }, { ResourceType.Suvarna, 200f } },
                ProductionPerTick = new() { { ResourceType.Vidya, 8f }, { ResourceType.Praja, 2f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_CourtOfJustice"
            });

            // ══════════════════════════════════════════
            //  AGE 7 — KRISHNA (Age of Strategy)
            // ══════════════════════════════════════════
            Register(new BuildingDefinition
            {
                TypeId = "DiplomaticHall",
                Name = "Rajneetik Sabha (Diplomatic Hall)",
                Description = "Center of diplomacy and alliances. Enables alliance formation with other kingdoms.",
                Category = BuildingCategory.Kingdom,
                RequiredAge = 7,
                Size = new Vector2Int(3, 3),
                BuildTimeSeconds = 100f,
                XPReward = 80,
                Cost = new() { { ResourceType.Pashana, 150f }, { ResourceType.Suvarna, 300f }, { ResourceType.Vidya, 50f } },
                ProductionPerTick = new() { { ResourceType.Suvarna, 25f }, { ResourceType.Vidya, 10f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_DiplomaticHall"
            });

            Register(new BuildingDefinition
            {
                TypeId = "Stables",
                Name = "Ashva Shaala (Royal Stables)",
                Description = "Train Ashvarohi (cavalry) and Rathi (charioteers) for your army.",
                Category = BuildingCategory.Kingdom,
                RequiredAge = 7,
                Size = new Vector2Int(3, 2),
                BuildTimeSeconds = 80f,
                XPReward = 70,
                Cost = new() { { ResourceType.Kashtha, 100f }, { ResourceType.Loha, 60f }, { ResourceType.Suvarna, 150f } },
                ProductionPerTick = new(),
                MaxCapacityBonus = new(),
                PrefabName = "Building_Stables",
                TrainsWarriorType = WarriorType.Ashvarohi
            });

            // ══════════════════════════════════════════
            //  AGE 8 — BUDDHA (Age of Enlightenment)
            // ══════════════════════════════════════════
            Register(new BuildingDefinition
            {
                TypeId = "University",
                Name = "Vishwavidyalaya (University)",
                Description = "A great center of learning like Nalanda. Generates massive Vidya for Veda research.",
                Category = BuildingCategory.City,
                RequiredAge = 8,
                Size = new Vector2Int(3, 3),
                BuildTimeSeconds = 120f,
                XPReward = 100,
                Cost = new() { { ResourceType.Pashana, 200f }, { ResourceType.Kashtha, 100f }, { ResourceType.Suvarna, 400f }, { ResourceType.Vidya, 80f } },
                ProductionPerTick = new() { { ResourceType.Vidya, 20f } },
                MaxCapacityBonus = new() { { ResourceType.Vidya, 500f } },
                PrefabName = "Building_University"
            });

            Register(new BuildingDefinition
            {
                TypeId = "Library",
                Name = "Pustak Bhavan (Library)",
                Description = "House of sacred scrolls and manuscripts. Accelerates Veda research progress.",
                Category = BuildingCategory.City,
                RequiredAge = 8,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 80f,
                XPReward = 70,
                Cost = new() { { ResourceType.Kashtha, 80f }, { ResourceType.Suvarna, 200f }, { ResourceType.Vidya, 40f } },
                ProductionPerTick = new() { { ResourceType.Vidya, 12f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_Library"
            });

            // ══════════════════════════════════════════
            //  AGE 9 — KALKI (Modern Divine City)
            // ══════════════════════════════════════════
            Register(new BuildingDefinition
            {
                TypeId = "DivineTower",
                Name = "Divya Gopuram (Divine Tower)",
                Description = "A celestial tower reaching toward the heavens. Ultimate Shakti generator.",
                Category = BuildingCategory.Divine,
                RequiredAge = 9,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 200f,
                XPReward = 200,
                Cost = new() { { ResourceType.Pashana, 500f }, { ResourceType.Loha, 200f }, { ResourceType.Suvarna, 1000f }, { ResourceType.Shakti, 200f } },
                ProductionPerTick = new() { { ResourceType.Shakti, 25f }, { ResourceType.Vidya, 15f } },
                MaxCapacityBonus = new() { { ResourceType.Shakti, 500f } },
                PrefabName = "Building_DivineTower"
            });

            Register(new BuildingDefinition
            {
                TypeId = "KalkiTemple",
                Name = "Kalki Mandir (Kalki Temple)",
                Description = "Temple of the final avatar. Produces all resources and marks the dawn of Satya Yuga.",
                Category = BuildingCategory.Divine,
                RequiredAge = 9,
                Size = new Vector2Int(4, 4),
                BuildTimeSeconds = 300f,
                XPReward = 500,
                Cost = new() { { ResourceType.Pashana, 500f }, { ResourceType.Kashtha, 300f }, { ResourceType.Loha, 300f }, { ResourceType.Suvarna, 2000f }, { ResourceType.Shakti, 300f }, { ResourceType.Vidya, 200f } },
                ProductionPerTick = new()
                {
                    { ResourceType.Suvarna, 30f }, { ResourceType.Anna, 20f },
                    { ResourceType.Pashana, 15f }, { ResourceType.Kashtha, 15f },
                    { ResourceType.Loha, 10f }, { ResourceType.Shakti, 20f },
                    { ResourceType.Vidya, 20f }, { ResourceType.Praja, 5f }
                },
                MaxCapacityBonus = new(),
                PrefabName = "Building_KalkiTemple"
            });

            // ══════════════════════════════════════════
            //  GURUKUL — Education Buildings
            // ══════════════════════════════════════════
            Register(new BuildingDefinition
            {
                TypeId = "Gurukul",
                Name = "गुरुकुल (Gurukul)",
                Description = "Ancient school where students live with the Guru. Teaches Vedas, martial arts, dharma, and all branches of knowledge. Every citizen can learn here.",
                Category = BuildingCategory.City,
                RequiredAge = 1,
                Size = new Vector2Int(3, 2),
                BuildTimeSeconds = 50f,
                XPReward = 50,
                Cost = new() { { ResourceType.Kashtha, 60f }, { ResourceType.Pashana, 40f }, { ResourceType.Suvarna, 50f } },
                ProductionPerTick = new() { { ResourceType.Vidya, 8f } },
                MaxCapacityBonus = new() { { ResourceType.Praja, 10f } },
                PrefabName = "Building_Gurukul"
            });

            Register(new BuildingDefinition
            {
                TypeId = "VedaPatha",
                Name = "वेद पाठशाला (Veda School)",
                Description = "Advanced Vedic school where Brahmacharya students study all four Vedas under renowned Rishis. Dramatically boosts research speed.",
                Category = BuildingCategory.City,
                RequiredAge = 4,
                Size = new Vector2Int(3, 3),
                BuildTimeSeconds = 90f,
                XPReward = 80,
                Cost = new() { { ResourceType.Pashana, 100f }, { ResourceType.Kashtha, 60f }, { ResourceType.Suvarna, 200f }, { ResourceType.Vidya, 30f } },
                ProductionPerTick = new() { { ResourceType.Vidya, 15f }, { ResourceType.Shakti, 3f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_VedaPatha"
            });

            // ══════════════════════════════════════════
            //  AYURVEDA — Health & Medicine Buildings
            // ══════════════════════════════════════════
            Register(new BuildingDefinition
            {
                TypeId = "VaidyaKutir",
                Name = "वैद्य कुटीर (Healer's Hut)",
                Description = "A village healer's cottage where the Vaidya (physician) treats ailments using herbs, mantras, and Nadi Pariksha (pulse diagnosis).",
                Category = BuildingCategory.Village,
                RequiredAge = 1,
                Size = new Vector2Int(1, 1),
                BuildTimeSeconds = 20f,
                XPReward = 20,
                Cost = new() { { ResourceType.Kashtha, 30f }, { ResourceType.Anna, 15f } },
                ProductionPerTick = new() { { ResourceType.Anna, 2f } },
                MaxCapacityBonus = new() { { ResourceType.Praja, 8f } },
                PrefabName = "Building_VaidyaKutir"
            });

            Register(new BuildingDefinition
            {
                TypeId = "HerbGarden",
                Name = "औषधि वाटिका (Herb Garden)",
                Description = "A garden of medicinal herbs — Tulsi, Ashwagandha, Brahmi, Neem, Amla. Essential for Ayurvedic preparations and population health.",
                Category = BuildingCategory.Village,
                RequiredAge = 2,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 25f,
                XPReward = 25,
                Cost = new() { { ResourceType.Kashtha, 20f }, { ResourceType.Suvarna, 30f }, { ResourceType.Anna, 10f } },
                ProductionPerTick = new() { { ResourceType.Anna, 4f }, { ResourceType.Vidya, 1f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_HerbGarden"
            });

            Register(new BuildingDefinition
            {
                TypeId = "AushadhiKendra",
                Name = "औषधि केन्द्र (Ayurvedic Center)",
                Description = "A full Ayurvedic treatment center offering Panchakarma, Rasayana, and Sushruta's surgical techniques. Major health boost to population.",
                Category = BuildingCategory.City,
                RequiredAge = 4,
                Size = new Vector2Int(3, 2),
                BuildTimeSeconds = 70f,
                XPReward = 65,
                Cost = new() { { ResourceType.Pashana, 80f }, { ResourceType.Kashtha, 50f }, { ResourceType.Suvarna, 120f }, { ResourceType.Vidya, 20f } },
                ProductionPerTick = new() { { ResourceType.Vidya, 3f }, { ResourceType.Praja, 2f } },
                MaxCapacityBonus = new() { { ResourceType.Praja, 20f } },
                PrefabName = "Building_AushadhiKendra"
            });

            Register(new BuildingDefinition
            {
                TypeId = "YogaShala",
                Name = "योग शाला (Yoga Hall)",
                Description = "A hall for practicing Patanjali's Ashtanga Yoga — Asana, Pranayama, Dhyana. Balances Doshas and increases population happiness and productivity.",
                Category = BuildingCategory.City,
                RequiredAge = 3,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 40f,
                XPReward = 40,
                Cost = new() { { ResourceType.Kashtha, 40f }, { ResourceType.Pashana, 30f }, { ResourceType.Suvarna, 50f } },
                ProductionPerTick = new() { { ResourceType.Shakti, 3f }, { ResourceType.Vidya, 2f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_YogaShala"
            });

            Register(new BuildingDefinition
            {
                TypeId = "JalChikitsa",
                Name = "जल चिकित्सा (Water Therapy)",
                Description = "Sacred bathing pools and mineral water therapy using river and hot spring water. Reduces Pitta and Kapha doshas.",
                Category = BuildingCategory.Village,
                RequiredAge = 2,
                Size = new Vector2Int(2, 1),
                BuildTimeSeconds = 30f,
                XPReward = 25,
                Cost = new() { { ResourceType.Pashana, 40f }, { ResourceType.Suvarna, 30f } },
                ProductionPerTick = new() { { ResourceType.Anna, 2f }, { ResourceType.Shakti, 1f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_JalChikitsa"
            });

            // ══════════════════════════════════════════
            //  VASTU & ASTRONOMY Buildings
            // ══════════════════════════════════════════
            Register(new BuildingDefinition
            {
                TypeId = "Observatory",
                Name = "वेधशाला (Vedh Shala / Observatory)",
                Description = "An astronomical observatory for studying Nakshatras, planetary movements, and eclipses per Surya Siddhanta. Reveals celestial knowledge.",
                Category = BuildingCategory.City,
                RequiredAge = 5,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 80f,
                XPReward = 70,
                Cost = new() { { ResourceType.Pashana, 100f }, { ResourceType.Loha, 40f }, { ResourceType.Suvarna, 150f }, { ResourceType.Vidya, 30f } },
                ProductionPerTick = new() { { ResourceType.Vidya, 10f }, { ResourceType.Shakti, 2f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_Observatory"
            });

            Register(new BuildingDefinition
            {
                TypeId = "VastuMandir",
                Name = "वास्तु मन्दिर (Vastu Temple)",
                Description = "A temple to Vastu Purusha — the cosmic being whose body forms the sacred geometry of architecture. Enhances all Vastu placement bonuses in the kingdom.",
                Category = BuildingCategory.Divine,
                RequiredAge = 4,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 60f,
                XPReward = 60,
                Cost = new() { { ResourceType.Pashana, 80f }, { ResourceType.Kashtha, 40f }, { ResourceType.Suvarna, 100f }, { ResourceType.Shakti, 15f } },
                ProductionPerTick = new() { { ResourceType.Shakti, 5f }, { ResourceType.Vidya, 5f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_VastuMandir"
            });

            // ══════════════════════════════════════════
            //  COMMONER & VILLAGE Life Buildings
            // ══════════════════════════════════════════
            Register(new BuildingDefinition
            {
                TypeId = "GoshalaFarm",
                Name = "गोशाला (Cow Sanctuary / Goshala)",
                Description = "Sacred cow sanctuary providing milk, ghee, dung fuel, and Panchagavya medicine. The cow is revered as Kamadhenu, the wish-fulfilling cow.",
                Category = BuildingCategory.Village,
                RequiredAge = 1,
                Size = new Vector2Int(2, 2),
                BuildTimeSeconds = 30f,
                XPReward = 30,
                Cost = new() { { ResourceType.Kashtha, 35f }, { ResourceType.Anna, 20f }, { ResourceType.Suvarna, 25f } },
                ProductionPerTick = new() { { ResourceType.Anna, 6f }, { ResourceType.Shakti, 1f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_GoshalaFarm"
            });

            Register(new BuildingDefinition
            {
                TypeId = "Panchayat",
                Name = "पंचायत भवन (Village Council)",
                Description = "The heart of village democracy since Vedic times. Five elected elders govern the gram. Boosts population happiness and reduces conflict.",
                Category = BuildingCategory.Village,
                RequiredAge = 2,
                Size = new Vector2Int(2, 1),
                BuildTimeSeconds = 25f,
                XPReward = 25,
                Cost = new() { { ResourceType.Kashtha, 30f }, { ResourceType.Pashana, 20f } },
                ProductionPerTick = new() { { ResourceType.Vidya, 2f }, { ResourceType.Praja, 1f } },
                MaxCapacityBonus = new() { { ResourceType.Praja, 10f } },
                PrefabName = "Building_Panchayat"
            });

            Register(new BuildingDefinition
            {
                TypeId = "Dharamshala",
                Name = "धर्मशाला (Rest House)",
                Description = "Free rest house for travelers and pilgrims. Attracts population and increases trade. Every weary soul is welcome in dharma.",
                Category = BuildingCategory.Village,
                RequiredAge = 3,
                Size = new Vector2Int(2, 1),
                BuildTimeSeconds = 30f,
                XPReward = 25,
                Cost = new() { { ResourceType.Kashtha, 40f }, { ResourceType.Pashana, 25f }, { ResourceType.Suvarna, 20f } },
                ProductionPerTick = new() { { ResourceType.Suvarna, 3f }, { ResourceType.Praja, 1f } },
                MaxCapacityBonus = new() { { ResourceType.Praja, 8f } },
                PrefabName = "Building_Dharamshala"
            });

            Register(new BuildingDefinition
            {
                TypeId = "NatyaShala",
                Name = "नाट्यशाला (Theater / Dance Hall)",
                Description = "A hall for Natya (drama), Nritya (dance), and Sangeet (music) per Bharata's Natyashastra. Boosts morale, culture, and population happiness.",
                Category = BuildingCategory.City,
                RequiredAge = 4,
                Size = new Vector2Int(3, 2),
                BuildTimeSeconds = 55f,
                XPReward = 50,
                Cost = new() { { ResourceType.Kashtha, 70f }, { ResourceType.Pashana, 40f }, { ResourceType.Suvarna, 80f } },
                ProductionPerTick = new() { { ResourceType.Suvarna, 5f }, { ResourceType.Praja, 2f }, { ResourceType.Vidya, 2f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_NatyaShala"
            });

            Register(new BuildingDefinition
            {
                TypeId = "KumbharShala",
                Name = "कुम्भार शाला (Potter's Workshop)",
                Description = "Where artisan Kumbhar (potters) craft earthen vessels, Diya lamps, and sacred water pots. Supports village daily life and temple rituals.",
                Category = BuildingCategory.Village,
                RequiredAge = 1,
                Size = new Vector2Int(1, 1),
                BuildTimeSeconds = 18f,
                XPReward = 15,
                Cost = new() { { ResourceType.Pashana, 15f }, { ResourceType.Kashtha, 10f } },
                ProductionPerTick = new() { { ResourceType.Suvarna, 3f } },
                MaxCapacityBonus = new(),
                PrefabName = "Building_KumbharShala"
            });
        }

        private static void Register(BuildingDefinition def)
        {
            _buildings[def.TypeId] = def;
        }
    }

    // ─────────────────────────────────────────────
    //  Building Definition
    // ─────────────────────────────────────────────
    [System.Serializable]
    public class BuildingDefinition
    {
        public string TypeId;
        public string Name;
        public string Description;
        public BuildingCategory Category;
        public int RequiredAge;                                        // minimum avatar age index
        public Vector2Int Size;                                        // grid size (width x height)
        public float BuildTimeSeconds;
        public int XPReward;
        public Dictionary<ResourceType, float> Cost;
        public Dictionary<ResourceType, float> ProductionPerTick;      // resources produced per game tick
        public Dictionary<ResourceType, float> MaxCapacityBonus;       // increases max capacity on construction
        public string PrefabName;

        // Military buildings
        public WarriorType? TrainsWarriorType;
    }
}
