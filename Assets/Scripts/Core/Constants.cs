using System.Collections.Generic;
using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Central game constants and static data definitions.
    /// </summary>
    public static class GameConstants
    {
        // ─────────────────────────────────────────────
        //  Grid
        // ─────────────────────────────────────────────
        public const int DEFAULT_MAP_WIDTH = 64;
        public const int DEFAULT_MAP_HEIGHT = 64;
        public const float TILE_SIZE = 1f;
        public const float ISO_TILE_WIDTH = 2f;
        public const float ISO_TILE_HEIGHT = 1f;

        // ─────────────────────────────────────────────
        //  Resources — Starting Values
        // ─────────────────────────────────────────────
        public static readonly Dictionary<ResourceType, float> STARTING_RESOURCES = new()
        {
            { ResourceType.Suvarna, 500f },
            { ResourceType.Anna,    300f },
            { ResourceType.Pashana, 200f },
            { ResourceType.Kashtha, 200f },
            { ResourceType.Loha,    50f },
            { ResourceType.Shakti,  100f },
            { ResourceType.Vidya,   0f },
            { ResourceType.Praja,   10f }
        };

        public static readonly Dictionary<ResourceType, float> MAX_RESOURCES_BASE = new()
        {
            { ResourceType.Suvarna, 5000f },
            { ResourceType.Anna,    3000f },
            { ResourceType.Pashana, 2000f },
            { ResourceType.Kashtha, 2000f },
            { ResourceType.Loha,    1000f },
            { ResourceType.Shakti,  500f },
            { ResourceType.Vidya,   500f },
            { ResourceType.Praja,   100f }
        };

        // ─────────────────────────────────────────────
        //  Resource Display Names
        // ─────────────────────────────────────────────
        public static readonly Dictionary<ResourceType, string> RESOURCE_NAMES = new()
        {
            { ResourceType.Suvarna, "Suvarna (Gold)" },
            { ResourceType.Anna,    "Anna (Food)" },
            { ResourceType.Pashana, "Pashana (Stone)" },
            { ResourceType.Kashtha, "Kashtha (Wood)" },
            { ResourceType.Loha,    "Loha (Iron)" },
            { ResourceType.Shakti,  "Shakti (Divine Energy)" },
            { ResourceType.Vidya,   "Vidya (Knowledge)" },
            { ResourceType.Praja,   "Praja (Population)" }
        };

        public static readonly Dictionary<ResourceType, string> RESOURCE_ICONS = new()
        {
            { ResourceType.Suvarna, "🪙" },
            { ResourceType.Anna,    "🌾" },
            { ResourceType.Pashana, "🪨" },
            { ResourceType.Kashtha, "🪵" },
            { ResourceType.Loha,    "⚒️" },
            { ResourceType.Shakti,  "🔱" },
            { ResourceType.Vidya,   "📜" },
            { ResourceType.Praja,   "👥" }
        };

        // ─────────────────────────────────────────────
        //  Dashavatar Data
        // ─────────────────────────────────────────────
        public static readonly AvatarInfo[] AVATARS = new AvatarInfo[]
        {
            new AvatarInfo
            {
                Index = 0,
                Name = "Matsya",
                Title = "The Fish — Primordial Waters",
                Description = "Lord Vishnu descends as a great fish to save the Vedas from the cosmic deluge.",
                RequiredPopulation = 0,
                RequiredBuildings = 0,
                UnlockedBuildingTypes = new[] { "FishingDock", "Hut", "Well", "SacredAltar", "ReedBoat" },
                BonusResourceType = ResourceType.Anna,
                BonusMultiplier = 1.2f,
                PrimaryColor = new Color(0.15f, 0.45f, 0.75f),    // Ocean blue
                SecondaryColor = new Color(0.85f, 0.75f, 0.30f)    // Gold
            },
            new AvatarInfo
            {
                Index = 1,
                Name = "Kurma",
                Title = "The Tortoise — Foundation Era",
                Description = "Lord Vishnu takes the form of a cosmic tortoise to support Mount Mandara during the churning of the ocean.",
                RequiredPopulation = 20,
                RequiredBuildings = 5,
                UnlockedBuildingTypes = new[] { "StoneQuarry", "Temple", "Granary", "StoneHouse", "MeditationCircle" },
                BonusResourceType = ResourceType.Pashana,
                BonusMultiplier = 1.3f,
                PrimaryColor = new Color(0.45f, 0.35f, 0.20f),    // Earth brown
                SecondaryColor = new Color(0.60f, 0.80f, 0.40f)    // Moss green
            },
            new AvatarInfo
            {
                Index = 2,
                Name = "Varaha",
                Title = "The Boar — Land Reclamation",
                Description = "Lord Vishnu manifests as a mighty boar to rescue the earth from the demon Hiranyaksha.",
                RequiredPopulation = 40,
                RequiredBuildings = 12,
                UnlockedBuildingTypes = new[] { "Farm", "IrrigationCanal", "Windmill", "Barn", "HerbGarden" },
                BonusResourceType = ResourceType.Anna,
                BonusMultiplier = 1.5f,
                PrimaryColor = new Color(0.30f, 0.55f, 0.20f),    // Forest green
                SecondaryColor = new Color(0.75f, 0.55f, 0.25f)    // Earth gold
            },
            new AvatarInfo
            {
                Index = 3,
                Name = "Narasimha",
                Title = "The Lion-Man — Fortress Age",
                Description = "Lord Vishnu appears as a fierce half-lion to destroy the demon king Hiranyakashipu.",
                RequiredPopulation = 70,
                RequiredBuildings = 20,
                UnlockedBuildingTypes = new[] { "Fortress", "WatchTower", "Barracks", "StoneWall", "WarDrumTower" },
                BonusResourceType = ResourceType.Loha,
                BonusMultiplier = 1.4f,
                PrimaryColor = new Color(0.75f, 0.35f, 0.15f),    // Lion orange
                SecondaryColor = new Color(0.85f, 0.20f, 0.10f)    // Blood red
            },
            new AvatarInfo
            {
                Index = 4,
                Name = "Vamana",
                Title = "The Dwarf — Age of Expansion",
                Description = "Lord Vishnu comes as a diminutive Brahmin who conquers the three worlds with three steps.",
                RequiredPopulation = 100,
                RequiredBuildings = 30,
                UnlockedBuildingTypes = new[] { "Marketplace", "TradePost", "CaravanRoute", "Warehouse", "Tavern" },
                BonusResourceType = ResourceType.Suvarna,
                BonusMultiplier = 1.5f,
                PrimaryColor = new Color(0.90f, 0.75f, 0.20f),    // Saffron gold
                SecondaryColor = new Color(0.95f, 0.95f, 0.85f)    // Sacred white
            },
            new AvatarInfo
            {
                Index = 5,
                Name = "Parashurama",
                Title = "The Warrior Sage — Warrior Age",
                Description = "Lord Vishnu incarnates as a fierce Brahmin warrior wielding a divine axe.",
                RequiredPopulation = 150,
                RequiredBuildings = 42,
                UnlockedBuildingTypes = new[] { "TrainingGround", "Armory", "ArcheryRange", "SiegeWorkshop", "WarTemple" },
                BonusResourceType = ResourceType.Loha,
                BonusMultiplier = 1.6f,
                PrimaryColor = new Color(0.65f, 0.15f, 0.15f),    // Warrior red
                SecondaryColor = new Color(0.50f, 0.50f, 0.55f)    // Steel
            },
            new AvatarInfo
            {
                Index = 6,
                Name = "Rama",
                Title = "Prince of Ayodhya — Kingdom of Dharma",
                Description = "Lord Vishnu incarnates as the ideal king Rama, embodiment of dharma and righteousness.",
                RequiredPopulation = 200,
                RequiredBuildings = 55,
                UnlockedBuildingTypes = new[] { "RoyalPalace", "CourtOfJustice", "GoldenGate", "RoyalGarden", "DharmaHall" },
                BonusResourceType = ResourceType.Vidya,
                BonusMultiplier = 1.5f,
                PrimaryColor = new Color(0.20f, 0.40f, 0.70f),    // Royal blue
                SecondaryColor = new Color(0.95f, 0.85f, 0.30f)    // Royal gold
            },
            new AvatarInfo
            {
                Index = 7,
                Name = "Krishna",
                Title = "The Divine Strategist — Age of Strategy",
                Description = "Lord Vishnu incarnates as Krishna, the supreme personality of Godhead, master of diplomacy and cosmic strategy.",
                RequiredPopulation = 300,
                RequiredBuildings = 70,
                UnlockedBuildingTypes = new[] { "DiplomaticHall", "AllianceCenter", "SpyNetwork", "GrandBazaar", "MusicHall" },
                BonusResourceType = ResourceType.Suvarna,
                BonusMultiplier = 2.0f,
                PrimaryColor = new Color(0.10f, 0.10f, 0.45f),    // Krishna blue-dark
                SecondaryColor = new Color(1f, 0.85f, 0.0f)         // Peacock gold
            },
            new AvatarInfo
            {
                Index = 8,
                Name = "Buddha",
                Title = "The Enlightened One — Age of Enlightenment",
                Description = "Lord Vishnu incarnates to spread compassion and wisdom, guiding beings toward liberation.",
                RequiredPopulation = 400,
                RequiredBuildings = 85,
                UnlockedBuildingTypes = new[] { "University", "MeditationCenter", "Library", "Observatory", "HealingCenter" },
                BonusResourceType = ResourceType.Vidya,
                BonusMultiplier = 2.5f,
                PrimaryColor = new Color(0.95f, 0.65f, 0.10f),    // Saffron
                SecondaryColor = new Color(0.95f, 0.95f, 0.90f)    // Lotus white
            },
            new AvatarInfo
            {
                Index = 9,
                Name = "Kalki",
                Title = "The Future Warrior — Modern Divine City",
                Description = "The prophesied final avatar of Lord Vishnu who will end the Kali Yuga and usher in a new golden age.",
                RequiredPopulation = 500,
                RequiredBuildings = 100,
                UnlockedBuildingTypes = new[] { "DivineTower", "KalkiTemple", "CelestialGate", "AstralObservatory", "SatyaYugaMonument" },
                BonusResourceType = ResourceType.Shakti,
                BonusMultiplier = 3.0f,
                PrimaryColor = new Color(0.95f, 0.95f, 0.95f),    // Divine white
                SecondaryColor = new Color(0.85f, 0.70f, 0.20f)    // Celestial gold
            }
        };

        // ─────────────────────────────────────────────
        //  Rudra Powers
        // ─────────────────────────────────────────────
        public static readonly Dictionary<RudraPowerType, RudraPowerInfo> RUDRA_POWERS = new()
        {
            {
                RudraPowerType.Tandava, new RudraPowerInfo
                {
                    Name = "Tandava",
                    Description = "The cosmic dance of destruction — demolish structures and clear terrain in a wide area.",
                    ShaktiCost = 80f,
                    CooldownSeconds = 120f,
                    AreaRadius = 5f
                }
            },
            {
                RudraPowerType.TrishulStrike, new RudraPowerInfo
                {
                    Name = "Trishul Strike",
                    Description = "Hurl Lord Shiva's divine trident to devastate a single target with immense force.",
                    ShaktiCost = 50f,
                    CooldownSeconds = 60f,
                    AreaRadius = 1f
                }
            },
            {
                RudraPowerType.ThirdEye, new RudraPowerInfo
                {
                    Name = "Third Eye",
                    Description = "Open Shiva's third eye to reveal hidden resources and enemy plans across the map.",
                    ShaktiCost = 40f,
                    CooldownSeconds = 90f,
                    AreaRadius = 15f
                }
            },
            {
                RudraPowerType.DamaruBeat, new RudraPowerInfo
                {
                    Name = "Damaru Beat",
                    Description = "The rhythm of Shiva's drum accelerates all construction and production for a time.",
                    ShaktiCost = 60f,
                    CooldownSeconds = 180f,
                    AreaRadius = 0f // global effect
                }
            },
            {
                RudraPowerType.Meditation, new RudraPowerInfo
                {
                    Name = "Dhyana (Meditation)",
                    Description = "Channel Lord Shiva's meditative energy to rapidly generate Shakti over time.",
                    ShaktiCost = 0f,
                    CooldownSeconds = 300f,
                    AreaRadius = 0f // global effect
                }
            }
        };

        // ─────────────────────────────────────────────
        //  Game Timing
        // ─────────────────────────────────────────────
        public const float RESOURCE_TICK_INTERVAL = 5f;           // seconds between resource production ticks
        public const float CONSTRUCTION_SPEED_BASE = 1f;
        public const float DAMARU_SPEED_MULTIPLIER = 3f;
        public const float MEDITATION_SHAKTI_PER_SECOND = 5f;
        public const float MEDITATION_DURATION = 30f;
        public const float AUTOSAVE_INTERVAL = 60f;
    }

    // ─────────────────────────────────────────────
    //  Data Structures
    // ─────────────────────────────────────────────
    [System.Serializable]
    public struct AvatarInfo
    {
        public int Index;
        public string Name;
        public string Title;
        public string Description;
        public int RequiredPopulation;
        public int RequiredBuildings;
        public string[] UnlockedBuildingTypes;
        public ResourceType BonusResourceType;
        public float BonusMultiplier;
        public Color PrimaryColor;
        public Color SecondaryColor;
    }

    [System.Serializable]
    public struct RudraPowerInfo
    {
        public string Name;
        public string Description;
        public float ShaktiCost;
        public float CooldownSeconds;
        public float AreaRadius;
    }
}
