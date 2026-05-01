using System.Collections.Generic;
using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Vastu Shastra (वास्तु शास्त्र) — Ancient Indian science of architecture and spatial arrangement.
    /// Buildings placed according to Vastu principles receive bonuses.
    /// 
    /// Jyotish Shastra (ज्योतिष शास्त्र) — Vedic astrology system.
    /// Nakshatras (lunar mansions) cycle through time, affecting gameplay.
    ///
    /// Khagol Vidya (खगोल विद्या) — Ancient Indian astronomy.
    /// Observatory buildings reveal celestial knowledge for bonuses.
    /// </summary>
    public class VastuAstrologySystem : MonoBehaviour
    {
        public static VastuAstrologySystem Instance { get; private set; }

        // Current Nakshatra (changes every in-game day)
        public int CurrentNakshatraIndex { get; private set; } = 0;
        public NakshatraInfo CurrentNakshatra => NAKSHATRAS[CurrentNakshatraIndex];

        // Vastu compliance score for the overall kingdom (0-100)
        public float KingdomVastuScore { get; private set; } = 50f;

        private float _nakshatraTimer;
        private const float NAKSHATRA_CYCLE_SECONDS = 120f; // 2 minutes per nakshatra

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentPhase != GamePhase.Playing) return;

            // Cycle through Nakshatras
            _nakshatraTimer += Time.deltaTime;
            if (_nakshatraTimer >= NAKSHATRA_CYCLE_SECONDS)
            {
                _nakshatraTimer = 0f;
                CurrentNakshatraIndex = (CurrentNakshatraIndex + 1) % 27;
                GameEvents.ShowNotification($"🌙 Nakshatra changed to {CurrentNakshatra.Name} ({CurrentNakshatra.SanskritName}) — {CurrentNakshatra.Effect}");
            }
        }

        // ─────────────────────────────────────────────
        //  Vastu Shastra — Building Placement Bonuses
        // ─────────────────────────────────────────────

        /// <summary>
        /// Calculate Vastu compliance for a building at a given position.
        /// Returns a multiplier (0.8 to 1.5) based on Vastu principles.
        /// 
        /// Key Vastu rules:
        /// - North-East (Ishanya): Best for temples, water, wells → Shakti bonus
        /// - South-East (Agneya): Best for kitchens, forges → production bonus
        /// - South-West (Nairutya): Best for heavy buildings, forts → defense bonus
        /// - North-West (Vayavya): Best for storage, granaries → storage bonus
        /// - Center (Brahmasthan): Should be open — penalty for building here
        /// </summary>
        public float GetVastuMultiplier(Vector2Int gridPos, string buildingTypeId)
        {
            var state = GameManager.Instance?.CurrentState;
            if (state == null) return 1f;

            int mapW = state.MapWidth;
            int mapH = state.MapHeight;
            float relX = (float)gridPos.x / mapW;  // 0 = West, 1 = East
            float relY = (float)gridPos.y / mapH;  // 0 = South, 1 = North

            VastuDirection direction = GetVastuDirection(relX, relY);
            var def = BuildingDatabase.GetBuilding(buildingTypeId);
            if (def == null) return 1f;

            float multiplier = 1f;

            // Check Vastu alignment
            switch (direction)
            {
                case VastuDirection.NorthEast_Ishanya:
                    // Best for: divine buildings, water, wells, temples
                    if (def.Category == BuildingCategory.Divine) multiplier = 1.5f;
                    else if (def.TypeId == "Well" || def.TypeId == "Temple" || def.TypeId == "MeditationCircle")
                        multiplier = 1.4f;
                    else if (def.TypeId == "SacredAltar") multiplier = 1.5f;
                    else multiplier = 1.1f;
                    break;

                case VastuDirection.SouthEast_Agneya:
                    // Best for: production, forges, kitchens, armories
                    if (def.TypeId == "Armory" || def.TypeId == "IronMine" || def.TypeId == "LumberMill")
                        multiplier = 1.4f;
                    else if (def.Category == BuildingCategory.Village) multiplier = 1.2f;
                    else multiplier = 1.05f;
                    break;

                case VastuDirection.SouthWest_Nairutya:
                    // Best for: heavy buildings, forts, palaces, storage
                    if (def.TypeId == "Fortress" || def.TypeId == "RoyalPalace" || def.TypeId == "StoneWall")
                        multiplier = 1.4f;
                    else if (def.Category == BuildingCategory.Kingdom) multiplier = 1.3f;
                    else multiplier = 1.05f;
                    break;

                case VastuDirection.NorthWest_Vayavya:
                    // Best for: storage, granaries, warehouses, wind
                    if (def.TypeId == "Granary" || def.TypeId == "Warehouse" || def.TypeId == "Windmill")
                        multiplier = 1.4f;
                    else if (def.TypeId == "TradePost" || def.TypeId == "Marketplace") multiplier = 1.3f;
                    else multiplier = 1.1f;
                    break;

                case VastuDirection.Center_Brahmasthan:
                    // Center should ideally be open (Brahmasthan)
                    // Building here gives a penalty unless it's a sacred building
                    if (def.Category == BuildingCategory.Divine) multiplier = 1.3f;
                    else multiplier = 0.85f; // Vastu penalty
                    break;

                case VastuDirection.North_Uttara:
                    // Good for: knowledge, education
                    if (def.TypeId == "University" || def.TypeId == "Library" || def.TypeId == "Gurukul")
                        multiplier = 1.35f;
                    else multiplier = 1.1f;
                    break;

                case VastuDirection.East_Purva:
                    // Good for: entrance, trade, markets (sunrise direction)
                    if (def.TypeId == "Marketplace" || def.TypeId == "TradePost") multiplier = 1.35f;
                    else if (def.TypeId == "GoldenGate") multiplier = 1.5f;
                    else multiplier = 1.15f;
                    break;

                case VastuDirection.South_Dakshina:
                    // Neutral to slightly negative for most
                    if (def.Category == BuildingCategory.Kingdom) multiplier = 1.1f;
                    else multiplier = 0.95f;
                    break;

                case VastuDirection.West_Pashchima:
                    // Good for: dining, food production (sunset direction)
                    if (def.TypeId == "Farm" || def.TypeId == "FishingDock") multiplier = 1.2f;
                    else multiplier = 1.05f;
                    break;
            }

            return multiplier;
        }

        /// <summary>
        /// Get Vastu direction hint for the UI when placing buildings.
        /// </summary>
        public string GetVastuAdvice(Vector2Int gridPos, string buildingTypeId)
        {
            float mult = GetVastuMultiplier(gridPos, buildingTypeId);
            var state = GameManager.Instance?.CurrentState;
            if (state == null) return "";

            float relX = (float)gridPos.x / state.MapWidth;
            float relY = (float)gridPos.y / state.MapHeight;
            VastuDirection dir = GetVastuDirection(relX, relY);

            string dirName = dir switch
            {
                VastuDirection.NorthEast_Ishanya => "Ishanya (NE) — Zone of Water & Divinity",
                VastuDirection.SouthEast_Agneya => "Agneya (SE) — Zone of Fire & Production",
                VastuDirection.SouthWest_Nairutya => "Nairutya (SW) — Zone of Earth & Strength",
                VastuDirection.NorthWest_Vayavya => "Vayavya (NW) — Zone of Wind & Storage",
                VastuDirection.Center_Brahmasthan => "Brahmasthan (Center) — Sacred Open Space",
                VastuDirection.North_Uttara => "Uttara (N) — Zone of Knowledge",
                VastuDirection.East_Purva => "Purva (E) — Zone of Sunrise & Commerce",
                VastuDirection.South_Dakshina => "Dakshina (S) — Zone of Yama",
                VastuDirection.West_Pashchima => "Pashchima (W) — Zone of Sunset & Nourishment",
                _ => ""
            };

            string rating;
            if (mult >= 1.4f) rating = "🟢 Excellent Vastu!";
            else if (mult >= 1.2f) rating = "🟡 Good Vastu";
            else if (mult >= 1.0f) rating = "⚪ Neutral";
            else rating = "🔴 Poor Vastu — consider relocating";

            return $"Vastu: {dirName}\n{rating} (×{mult:F2} bonus)";
        }

        private VastuDirection GetVastuDirection(float relX, float relY)
        {
            // Center zone (Brahmasthan)
            if (relX > 0.4f && relX < 0.6f && relY > 0.4f && relY < 0.6f)
                return VastuDirection.Center_Brahmasthan;

            // Cardinal and intercardinal directions
            bool isNorth = relY > 0.6f;
            bool isSouth = relY < 0.4f;
            bool isEast = relX > 0.6f;
            bool isWest = relX < 0.4f;

            if (isNorth && isEast) return VastuDirection.NorthEast_Ishanya;
            if (isSouth && isEast) return VastuDirection.SouthEast_Agneya;
            if (isSouth && isWest) return VastuDirection.SouthWest_Nairutya;
            if (isNorth && isWest) return VastuDirection.NorthWest_Vayavya;

            if (isNorth) return VastuDirection.North_Uttara;
            if (isSouth) return VastuDirection.South_Dakshina;
            if (isEast) return VastuDirection.East_Purva;
            if (isWest) return VastuDirection.West_Pashchima;

            return VastuDirection.Center_Brahmasthan;
        }

        /// <summary>
        /// Recalculate the overall kingdom Vastu score (0-100).
        /// Called when buildings are placed or demolished.
        /// </summary>
        public void RecalculateKingdomVastu()
        {
            var state = GameManager.Instance?.CurrentState;
            if (state?.Buildings == null || state.Buildings.Count == 0)
            {
                KingdomVastuScore = 50f;
                return;
            }

            float totalScore = 0f;
            int count = 0;
            foreach (var building in state.Buildings)
            {
                float mult = GetVastuMultiplier(
                    new Vector2Int(building.GridX, building.GridY),
                    building.BuildingTypeId);
                totalScore += (mult - 0.8f) / (1.5f - 0.8f) * 100f; // normalize to 0-100
                count++;
            }

            KingdomVastuScore = count > 0 ? Mathf.Clamp(totalScore / count, 0f, 100f) : 50f;
        }

        // ─────────────────────────────────────────────
        //  Jyotish Shastra — Nakshatra System
        // ─────────────────────────────────────────────

        /// <summary>
        /// Get the current Nakshatra's gameplay effect multiplier for a given resource.
        /// </summary>
        public float GetNakshatraBonus(ResourceType type)
        {
            var nakshatra = CurrentNakshatra;
            if (nakshatra.BonusResource == type) return nakshatra.BonusMultiplier;
            return 1f;
        }

        /// <summary>
        /// Check if current Nakshatra is auspicious for battle.
        /// </summary>
        public bool IsAuspiciousForBattle()
        {
            return CurrentNakshatra.IsAuspiciousForWar;
        }

        // ─── 27 Nakshatras ───
        public static readonly NakshatraInfo[] NAKSHATRAS = new NakshatraInfo[]
        {
            new() { Index = 0,  Name = "Ashwini",       SanskritName = "अश्विनी",      Deity = "Ashwini Kumaras", BonusResource = ResourceType.Anna,    BonusMultiplier = 1.2f, Effect = "Healing boost — Ayurveda buildings +20%",    IsAuspiciousForWar = false },
            new() { Index = 1,  Name = "Bharani",       SanskritName = "भरणी",          Deity = "Yama",            BonusResource = ResourceType.Loha,    BonusMultiplier = 1.3f, Effect = "Metal forging enhanced",                      IsAuspiciousForWar = true },
            new() { Index = 2,  Name = "Krittika",      SanskritName = "कृत्तिका",      Deity = "Agni",            BonusResource = ResourceType.Shakti,  BonusMultiplier = 1.3f, Effect = "Fire of purification — Shakti +30%",          IsAuspiciousForWar = true },
            new() { Index = 3,  Name = "Rohini",        SanskritName = "रोहिणी",         Deity = "Brahma",          BonusResource = ResourceType.Anna,    BonusMultiplier = 1.5f, Effect = "Abundance — Food production +50%",            IsAuspiciousForWar = false },
            new() { Index = 4,  Name = "Mrigashira",    SanskritName = "मृगशीर्ष",      Deity = "Soma",            BonusResource = ResourceType.Vidya,   BonusMultiplier = 1.3f, Effect = "Quest for knowledge — Research +30%",         IsAuspiciousForWar = false },
            new() { Index = 5,  Name = "Ardra",         SanskritName = "आर्द्रा",        Deity = "Rudra",           BonusResource = ResourceType.Shakti,  BonusMultiplier = 1.5f, Effect = "Rudra's storm — Divine powers enhanced",      IsAuspiciousForWar = true },
            new() { Index = 6,  Name = "Punarvasu",     SanskritName = "पुनर्वसु",       Deity = "Aditi",           BonusResource = ResourceType.Praja,   BonusMultiplier = 1.4f, Effect = "Renewal — Population growth +40%",            IsAuspiciousForWar = false },
            new() { Index = 7,  Name = "Pushya",        SanskritName = "पुष्य",          Deity = "Brihaspati",      BonusResource = ResourceType.Vidya,   BonusMultiplier = 1.5f, Effect = "Most auspicious! All learning enhanced",      IsAuspiciousForWar = false },
            new() { Index = 8,  Name = "Ashlesha",      SanskritName = "आश्लेषा",        Deity = "Nagas",           BonusResource = ResourceType.Suvarna, BonusMultiplier = 1.2f, Effect = "Serpent's treasure — Hidden gold revealed",   IsAuspiciousForWar = false },
            new() { Index = 9,  Name = "Magha",         SanskritName = "मघा",            Deity = "Pitris",          BonusResource = ResourceType.Shakti,  BonusMultiplier = 1.3f, Effect = "Ancestral power — Royal buildings enhanced",  IsAuspiciousForWar = true },
            new() { Index = 10, Name = "Purva Phalguni",SanskritName = "पूर्व फाल्गुनी",  Deity = "Bhaga",           BonusResource = ResourceType.Suvarna, BonusMultiplier = 1.4f, Effect = "Prosperity — Gold income +40%",               IsAuspiciousForWar = false },
            new() { Index = 11, Name = "Uttara Phalguni",SanskritName = "उत्तर फाल्गुनी", Deity = "Aryaman",         BonusResource = ResourceType.Praja,   BonusMultiplier = 1.3f, Effect = "Friendship — Alliances strengthened",         IsAuspiciousForWar = false },
            new() { Index = 12, Name = "Hasta",         SanskritName = "हस्त",           Deity = "Savitar",         BonusResource = ResourceType.Kashtha, BonusMultiplier = 1.4f, Effect = "Skilled hands — Construction speed +40%",     IsAuspiciousForWar = false },
            new() { Index = 13, Name = "Chitra",        SanskritName = "चित्रा",          Deity = "Vishwakarma",     BonusResource = ResourceType.Pashana, BonusMultiplier = 1.5f, Effect = "Divine architect — Building quality +50%",    IsAuspiciousForWar = false },
            new() { Index = 14, Name = "Swati",         SanskritName = "स्वाति",          Deity = "Vayu",            BonusResource = ResourceType.Kashtha, BonusMultiplier = 1.3f, Effect = "Wind god's grace — Trade routes enhanced",    IsAuspiciousForWar = false },
            new() { Index = 15, Name = "Vishakha",      SanskritName = "विशाखा",          Deity = "Indra-Agni",      BonusResource = ResourceType.Loha,    BonusMultiplier = 1.4f, Effect = "Warrior's determination — Army strength +40%",IsAuspiciousForWar = true },
            new() { Index = 16, Name = "Anuradha",      SanskritName = "अनुराधा",         Deity = "Mitra",           BonusResource = ResourceType.Vidya,   BonusMultiplier = 1.3f, Effect = "Devotion — Veda research +30%",               IsAuspiciousForWar = false },
            new() { Index = 17, Name = "Jyeshtha",      SanskritName = "ज्येष्ठा",        Deity = "Indra",           BonusResource = ResourceType.Loha,    BonusMultiplier = 1.5f, Effect = "King of gods — Military power +50%",          IsAuspiciousForWar = true },
            new() { Index = 18, Name = "Moola",         SanskritName = "मूल",             Deity = "Nirriti",         BonusResource = ResourceType.Pashana, BonusMultiplier = 1.3f, Effect = "Root power — Foundation buildings enhanced",   IsAuspiciousForWar = true },
            new() { Index = 19, Name = "Purva Ashadha",  SanskritName = "पूर्वाषाढ़ा",     Deity = "Apas",            BonusResource = ResourceType.Anna,    BonusMultiplier = 1.3f, Effect = "Invincible water — Irrigation +30%",          IsAuspiciousForWar = true },
            new() { Index = 20, Name = "Uttara Ashadha", SanskritName = "उत्तराषाढ़ा",     Deity = "Vishvadevas",     BonusResource = ResourceType.Shakti,  BonusMultiplier = 1.4f, Effect = "Universal gods — All divine buildings +40%",  IsAuspiciousForWar = true },
            new() { Index = 21, Name = "Shravana",      SanskritName = "श्रवण",           Deity = "Vishnu",          BonusResource = ResourceType.Vidya,   BonusMultiplier = 1.5f, Effect = "Lord Vishnu's grace — All knowledge +50%",    IsAuspiciousForWar = false },
            new() { Index = 22, Name = "Dhanishtha",    SanskritName = "धनिष्ठा",         Deity = "Vasus",           BonusResource = ResourceType.Suvarna, BonusMultiplier = 1.5f, Effect = "Wealth of the Vasus — Gold +50%",             IsAuspiciousForWar = true },
            new() { Index = 23, Name = "Shatabhisha",   SanskritName = "शतभिषा",          Deity = "Varuna",          BonusResource = ResourceType.Anna,    BonusMultiplier = 1.3f, Effect = "Hundred healers — Ayurveda potency +30%",     IsAuspiciousForWar = false },
            new() { Index = 24, Name = "Purva Bhadrapada",SanskritName = "पूर्वभाद्रपदा",  Deity = "Aja Ekapada",     BonusResource = ResourceType.Shakti,  BonusMultiplier = 1.3f, Effect = "Cosmic fire — Shakti generation +30%",        IsAuspiciousForWar = true },
            new() { Index = 25, Name = "Uttara Bhadrapada",SanskritName = "उत्तरभाद्रपदा", Deity = "Ahir Budhnya",    BonusResource = ResourceType.Pashana, BonusMultiplier = 1.3f, Effect = "Cosmic serpent — Deep mining +30%",            IsAuspiciousForWar = false },
            new() { Index = 26, Name = "Revati",        SanskritName = "रेवती",           Deity = "Pushan",          BonusResource = ResourceType.Praja,   BonusMultiplier = 1.4f, Effect = "Nourisher — Population & livestock +40%",     IsAuspiciousForWar = false }
        };
    }

    // ─────────────────────────────────────────────
    //  Data Structures
    // ─────────────────────────────────────────────
    public enum VastuDirection
    {
        NorthEast_Ishanya,      // Water & divinity
        SouthEast_Agneya,       // Fire & production
        SouthWest_Nairutya,     // Earth & strength
        NorthWest_Vayavya,      // Wind & storage
        North_Uttara,           // Knowledge
        East_Purva,             // Commerce & sunrise
        South_Dakshina,         // Yama's realm
        West_Pashchima,         // Sunset & nourishment
        Center_Brahmasthan      // Sacred center
    }

    [System.Serializable]
    public struct NakshatraInfo
    {
        public int Index;
        public string Name;
        public string SanskritName;
        public string Deity;
        public ResourceType BonusResource;
        public float BonusMultiplier;
        public string Effect;
        public bool IsAuspiciousForWar;
    }
}
