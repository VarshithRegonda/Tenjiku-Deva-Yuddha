using System;
using System.Collections.Generic;
using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Complete serializable game state. This is saved/loaded as JSON.
    /// Contains all player progress: resources, buildings, avatar age, level, research, army.
    /// </summary>
    [Serializable]
    public class GameState
    {
        // ─────────────────────────────────────────────
        //  Player Identity
        // ─────────────────────────────────────────────
        public string PlayerName = "Arjuna";
        public string KingdomName = "Hastinapura";
        public string PlayerId = "";
        public long CreatedTimestamp;
        public long LastSavedTimestamp;

        // ─────────────────────────────────────────────
        //  Player Level & Experience
        // ─────────────────────────────────────────────
        public int PlayerLevel = 1;
        public long TotalExperience = 0;
        public string PlayerTitle = "Gramin";
        public bool BattlesUnlocked = false;   // unlocked at level 200

        // ─────────────────────────────────────────────
        //  Resources
        // ─────────────────────────────────────────────
        public SerializableResourceDict Resources = new();
        public SerializableResourceDict MaxResources = new();

        // ─────────────────────────────────────────────
        //  Dashavatar Progression
        // ─────────────────────────────────────────────
        public int CurrentAvatarAge = 0;            // 0 = Matsya, 9 = Kalki
        public float AgeProgress = 0f;              // 0-1 progress to next age

        // ─────────────────────────────────────────────
        //  Buildings
        // ─────────────────────────────────────────────
        public List<BuildingState> Buildings = new();
        public int TotalBuildingsPlaced = 0;

        // ─────────────────────────────────────────────
        //  Veda Research
        // ─────────────────────────────────────────────
        public VedaResearchState VedaProgress = new();

        // ─────────────────────────────────────────────
        //  Battle / Army
        // ─────────────────────────────────────────────
        public ArmyState Army = new();
        public BattleStats BattleRecord = new();

        // ─────────────────────────────────────────────
        //  Rudra Powers
        // ─────────────────────────────────────────────
        public Dictionary<string, float> RudraCooldowns = new();

        // ─────────────────────────────────────────────
        //  Map
        // ─────────────────────────────────────────────
        public int MapWidth = GameConstants.DEFAULT_MAP_WIDTH;
        public int MapHeight = GameConstants.DEFAULT_MAP_HEIGHT;
        public int MapSeed = 0;

        // ─────────────────────────────────────────────
        //  Governance & Laws (Darbar)
        // ─────────────────────────────────────────────
        public GovernanceState Governance = new();

        // ─────────────────────────────────────────────
        //  Settings
        // ─────────────────────────────────────────────
        public float MusicVolume = 0.7f;
        public float SfxVolume = 1f;
        public int GraphicsQuality = 2;  // 0=Low, 1=Med, 2=High

        /// <summary>
        /// Initialize a fresh game state with starting resources.
        /// </summary>
        public void InitializeNewGame(string playerName, string kingdomName)
        {
            PlayerName = playerName;
            KingdomName = kingdomName;
            PlayerId = Guid.NewGuid().ToString();
            CreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            LastSavedTimestamp = CreatedTimestamp;
            PlayerLevel = 1;
            TotalExperience = 0;
            PlayerTitle = "Gramin";
            BattlesUnlocked = false;
            CurrentAvatarAge = 0;
            AgeProgress = 0f;
            TotalBuildingsPlaced = 0;
            MapSeed = UnityEngine.Random.Range(0, 999999);

            // Starting resources
            Resources = new SerializableResourceDict();
            MaxResources = new SerializableResourceDict();
            foreach (var kvp in GameConstants.STARTING_RESOURCES)
                Resources.Set(kvp.Key, kvp.Value);
            foreach (var kvp in GameConstants.MAX_RESOURCES_BASE)
                MaxResources.Set(kvp.Key, kvp.Value);

            // Initialize Veda progress
            VedaProgress = new VedaResearchState();
            Army = new ArmyState();
            BattleRecord = new BattleStats();
            Buildings = new List<BuildingState>();
            RudraCooldowns = new Dictionary<string, float>();
        }

        /// <summary>
        /// Calculate what level corresponds to total XP and update the title.
        /// XP formula: level N requires N*100 + (N-1)*50 total XP.
        /// </summary>
        public void RecalculateLevel()
        {
            int level = 1;
            long xpNeeded = 0;
            while (true)
            {
                long nextLevelXP = (level * 100L) + ((level - 1) * 50L);
                if (TotalExperience < xpNeeded + nextLevelXP) break;
                xpNeeded += nextLevelXP;
                level++;
                if (level > 500) break;
            }
            PlayerLevel = level;
            BattlesUnlocked = PlayerLevel >= 200;
            PlayerTitle = GetTitleForLevel(PlayerLevel);
        }

        public static string GetTitleForLevel(int level)
        {
            if (level >= 401) return "Chakravarti (Emperor)";
            if (level >= 301) return "Atimaharathi (Supreme Warrior)";
            if (level >= 201) return "Maharathi (Great Warrior)";
            if (level >= 200) return "Senapati (Commander)";
            if (level >= 151) return "Mantri (Minister)";
            if (level >= 101) return "Rajya Sevak (Kingdom Servant)";
            if (level >= 51)  return "Nagarvasi (Citizen)";
            return "Gramin (Villager)";
        }

        public long GetXPForNextLevel()
        {
            return (PlayerLevel * 100L) + ((PlayerLevel - 1) * 50L);
        }

        public float GetLevelProgress()
        {
            long xpAccumulated = 0;
            for (int l = 1; l < PlayerLevel; l++)
                xpAccumulated += (l * 100L) + ((l - 1) * 50L);
            long currentLevelXP = TotalExperience - xpAccumulated;
            long needed = GetXPForNextLevel();
            return needed > 0 ? Mathf.Clamp01((float)currentLevelXP / needed) : 1f;
        }
    }

    // ─────────────────────────────────────────────
    //  Serializable Resource Dictionary
    // ─────────────────────────────────────────────
    [Serializable]
    public class SerializableResourceDict
    {
        public float Suvarna;
        public float Anna;
        public float Pashana;
        public float Kashtha;
        public float Loha;
        public float Shakti;
        public float Vidya;
        public float Praja;

        public float Get(ResourceType type) => type switch
        {
            ResourceType.Suvarna => Suvarna,
            ResourceType.Anna => Anna,
            ResourceType.Pashana => Pashana,
            ResourceType.Kashtha => Kashtha,
            ResourceType.Loha => Loha,
            ResourceType.Shakti => Shakti,
            ResourceType.Vidya => Vidya,
            ResourceType.Praja => Praja,
            _ => 0f
        };

        public void Set(ResourceType type, float value)
        {
            switch (type)
            {
                case ResourceType.Suvarna: Suvarna = value; break;
                case ResourceType.Anna: Anna = value; break;
                case ResourceType.Pashana: Pashana = value; break;
                case ResourceType.Kashtha: Kashtha = value; break;
                case ResourceType.Loha: Loha = value; break;
                case ResourceType.Shakti: Shakti = value; break;
                case ResourceType.Vidya: Vidya = value; break;
                case ResourceType.Praja: Praja = value; break;
            }
        }

        public void Add(ResourceType type, float amount) => Set(type, Get(type) + amount);
        public bool HasEnough(ResourceType type, float amount) => Get(type) >= amount;
    }

    // ─────────────────────────────────────────────
    //  Building State
    // ─────────────────────────────────────────────
    [Serializable]
    public class BuildingState
    {
        public string UniqueId;
        public string BuildingTypeId;
        public int GridX;
        public int GridY;
        public int Level = 1;
        public float ConstructionProgress = 0f;     // 0 = not started, 1 = complete
        public bool IsConstructed = false;
        public long PlacedTimestamp;
    }

    // ─────────────────────────────────────────────
    //  Veda Research State
    // ─────────────────────────────────────────────
    [Serializable]
    public class VedaResearchState
    {
        public int RigvedaLevel = 0;     // 0-10
        public int YajurvedaLevel = 0;
        public int SamavedaLevel = 0;
        public int AtharvavedaLevel = 0;

        public float CurrentResearchProgress = 0f;  // 0-1 for active research
        public string ActiveResearchVeda = "";       // "Rigveda", "Yajurveda", etc. or ""

        public int GetLevel(VedaType veda) => veda switch
        {
            VedaType.Rigveda => RigvedaLevel,
            VedaType.Yajurveda => YajurvedaLevel,
            VedaType.Samaveda => SamavedaLevel,
            VedaType.Atharvaveda => AtharvavedaLevel,
            _ => 0
        };

        public void SetLevel(VedaType veda, int level)
        {
            switch (veda)
            {
                case VedaType.Rigveda: RigvedaLevel = level; break;
                case VedaType.Yajurveda: YajurvedaLevel = level; break;
                case VedaType.Samaveda: SamavedaLevel = level; break;
                case VedaType.Atharvaveda: AtharvavedaLevel = level; break;
            }
        }

        public int GetTotalResearchLevel() =>
            RigvedaLevel + YajurvedaLevel + SamavedaLevel + AtharvavedaLevel;
    }

    public enum VedaType
    {
        Rigveda,
        Yajurveda,
        Samaveda,
        Atharvaveda
    }

    // ─────────────────────────────────────────────
    //  Army & Battle State
    // ─────────────────────────────────────────────
    [Serializable]
    public class ArmyState
    {
        // Warrior counts by type
        public int Kshatriya = 0;       // Infantry
        public int Dhanurdhar = 0;      // Archers
        public int Ashvarohi = 0;       // Cavalry
        public int Rathi = 0;           // Chariot warriors
        public int Gajasena = 0;        // War elephants

        // Unlocked formations
        public List<string> UnlockedVyuhas = new() { "Krauncha" };

        // Unlocked Astras (divine weapons)
        public List<string> UnlockedAstras = new();

        public string SelectedVyuha = "Krauncha";

        public int GetTotalWarriors() =>
            Kshatriya + Dhanurdhar + Ashvarohi + Rathi + Gajasena;

        public float GetArmyStrength()
        {
            return (Kshatriya * 1f) +
                   (Dhanurdhar * 1.5f) +
                   (Ashvarohi * 2.5f) +
                   (Rathi * 4f) +
                   (Gajasena * 8f);
        }
    }

    [Serializable]
    public class BattleStats
    {
        public int BattlesWon = 0;
        public int BattlesLost = 0;
        public int BattlesTotal = 0;
        public int EnemiesDefeated = 0;
        public int HighestBattleLevel = 0;
    }

    // ─────────────────────────────────────────────
    //  Battle Enums
    // ─────────────────────────────────────────────
    public enum WarriorType
    {
        Kshatriya,      // Infantry — basic foot soldiers
        Dhanurdhar,     // Archer — ranged
        Ashvarohi,      // Cavalry — fast mounted
        Rathi,          // Chariot — powerful mobile
        Gajasena        // War Elephant — heavy tank
    }

    public enum VyuhaFormation
    {
        Krauncha,       // Crane — basic balanced formation
        Chakravyuha,    // Disc — powerful but hard to escape (Abhimanyu's formation)
        Padmavyuha,     // Lotus — defensive formation
        Garudavyuha,    // Eagle — aggressive attack formation
        Makaravyuha,    // Crocodile — ambush formation
        Vajravyuha,     // Diamond — elite defensive
        Shakatavyuha,   // Cart — supply protection
        Sarpavyuha      // Serpent — flanking formation
    }

    public enum AstraType
    {
        Agneyastra,         // Fire weapon — area fire damage
        Varunastra,         // Water weapon — flood/slow enemies
        Vayavyastra,        // Wind weapon — push enemies back
        Nagastra,           // Serpent weapon — poison damage
        Brahmastra,         // Brahma's weapon — massive single target (rare)
        Pashupatastra,      // Shiva's weapon — ultimate destruction (legendary)
        Narayanastra,       // Vishnu's weapon — unstoppable barrage (legendary)
        Vajra,              // Indra's thunderbolt — lightning damage
        Sudarshana,         // Vishnu's disc — homing attack
        Gandiva             // Arjuna's bow — rapid multi-target
    }

    public enum BattleMode
    {
        DefendKingdom,      // NPC enemies attack your city
        Conquest,           // Attack NPC kingdoms
        DharmaYuddha,      // Honorable battles with rules
        KurukshetraArena    // PvP (future multiplayer)
    }

    // ─────────────────────────────────────────────
    //  Governance & Law State
    // ─────────────────────────────────────────────
    [Serializable]
    public class GovernanceState
    {
        // 1. Praja (Population) Allocation (0.0 to 1.0 percentages)
        public float KrishakAllocation = 0.40f; // Farmers/Gatherers
        public float ShilpiAllocation = 0.30f;  // Artisans/Builders
        public float SainikAllocation = 0.10f;  // Soldiers/Guards
        public float VidvanAllocation = 0.20f;  // Scholars/Priests

        // 2. Navaratna (9 Ministers) — stores the names of appointed ministers. Empty means vacant.
        public string Purohita = "";
        public string Senapati = "";
        public string Amatya = "";
        public string Nyayadhish = "";
        public string Mantri = "";
        public string Sthapati = "";
        public string Vaidyaraj = "";
        public string Senani = "";
        public string Pradhan = "";

        // 3. Dharmaniti (Active Laws)
        public List<string> ActiveLaws = new();

        // 4. Petitions (Active problems from citizens)
        public List<PetitionState> ActivePetitions = new();

        // 5. Kingdom Metrics
        public float Happiness = 80f;     // 0-100
        public float Loyalty = 75f;       // 0-100
    }

    [Serializable]
    public class PetitionState
    {
        public string Id;
        public string Title;
        public string Description;
        public string Type; // "Agriculture", "Crime", "Religion", etc.
        public long CreatedAt;
        public float TimeRemainingSeconds;
    }
}
