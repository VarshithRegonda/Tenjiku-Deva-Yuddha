using System.Collections.Generic;
using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Legendary city templates from the Mahabharata and ancient Indian texts.
    /// Players can choose a city archetype which gives unique bonuses, 
    /// starting conditions, and visual themes.
    /// </summary>
    public static class LegendaryCityDatabase
    {
        private static Dictionary<string, LegendaryCity> _cities;

        public static void Initialize()
        {
            _cities = new Dictionary<string, LegendaryCity>();

            Register(new LegendaryCity
            {
                CityId = "Indraprastha",
                Name = "इन्द्रप्रस्थ (Indraprastha)",
                Description = "The magnificent capital built by the Pandavas with divine architect Maya Danava. " +
                    "A city of illusions where floors mirrored water and walls shone like crystal.",
                Era = "Mahabharata",
                Founded = "Built by Maya Danava for the Pandavas on the banks of Yamuna",
                BonusResource = ResourceType.Suvarna,
                BonusMultiplier = 1.5f,
                SecondaryBonus = ResourceType.Vidya,
                SecondaryMultiplier = 1.3f,
                StartingAge = 0,
                SpecialBuilding = "MayaSabha",
                SpecialBuildingName = "Maya Sabha (Hall of Illusions)",
                SpecialBuildingDescription = "The legendary assembly hall of the Pandavas where Duryodhana was humiliated. Generates massive gold and knowledge.",
                TerrainPreference = TerrainType.Plains,
                PrimaryColor = new Color(0.85f, 0.75f, 0.25f),   // Royal gold
                SecondaryColor = new Color(0.20f, 0.35f, 0.70f),  // Yamuna blue
                UniqueTraits = new[] { "Trade Hub", "Crystal Architecture", "Maya's Illusions" }
            });

            Register(new LegendaryCity
            {
                CityId = "Dwarka",
                Name = "द्वारका (Dwarka)",
                Description = "Lord Krishna's golden city built on the ocean. A maritime paradise with " +
                    "golden spires that touched the sky, submerged beneath the sea after Krishna's departure.",
                Era = "Mahabharata / Puranic",
                Founded = "Built by Vishwakarma for Lord Krishna in the western ocean",
                BonusResource = ResourceType.Shakti,
                BonusMultiplier = 2.0f,
                SecondaryBonus = ResourceType.Suvarna,
                SecondaryMultiplier = 1.5f,
                StartingAge = 0,
                SpecialBuilding = "SudharmaHall",
                SpecialBuildingName = "Sudharma Sabha (Divine Assembly)",
                SpecialBuildingDescription = "The heavenly assembly hall brought from Indraloka. All who enter feel no hunger, fatigue, or aging.",
                TerrainPreference = TerrainType.Water,
                PrimaryColor = new Color(1f, 0.85f, 0.20f),       // Divine gold
                SecondaryColor = new Color(0.15f, 0.45f, 0.80f),   // Ocean blue
                UniqueTraits = new[] { "Maritime Power", "Golden Spires", "Divine Protection" }
            });

            Register(new LegendaryCity
            {
                CityId = "Hastinapura",
                Name = "हस्तिनापुर (Hastinapura)",
                Description = "The elephant city — ancient capital of the Kuru dynasty, seat of the great " +
                    "Bharata war. Where Bhishma, Drona, and the Pandavas walked the halls of power.",
                Era = "Mahabharata",
                Founded = "Founded by King Hastin on the banks of the Ganges",
                BonusResource = ResourceType.Praja,
                BonusMultiplier = 1.5f,
                SecondaryBonus = ResourceType.Loha,
                SecondaryMultiplier = 1.3f,
                StartingAge = 0,
                SpecialBuilding = "KuruThrone",
                SpecialBuildingName = "Kuru Simhasana (Throne of Kurus)",
                SpecialBuildingDescription = "The legendary throne of the Kuru dynasty. Grants massive population growth and military strength.",
                TerrainPreference = TerrainType.Plains,
                PrimaryColor = new Color(0.60f, 0.45f, 0.25f),    // Elephant brown
                SecondaryColor = new Color(0.75f, 0.70f, 0.60f),   // Ivory
                UniqueTraits = new[] { "Military Capital", "War Elephants", "Kuru Legacy" }
            });

            Register(new LegendaryCity
            {
                CityId = "Ayodhya",
                Name = "अयोध्या (Ayodhya)",
                Description = "The unconquerable city — birthplace of Lord Rama, capital of the Ikshvaku " +
                    "dynasty. A city of dharma where righteousness prevailed above all.",
                Era = "Ramayana / Treta Yuga",
                Founded = "Founded by Manu, the first man, on the banks of Sarayu river",
                BonusResource = ResourceType.Vidya,
                BonusMultiplier = 1.5f,
                SecondaryBonus = ResourceType.Shakti,
                SecondaryMultiplier = 1.4f,
                StartingAge = 0,
                SpecialBuilding = "RaghuVanshPalace",
                SpecialBuildingName = "Raghuvanshi Raj Bhavan (Palace of Solar Dynasty)",
                SpecialBuildingDescription = "The ancestral palace of the Solar Dynasty (Suryavanshi). Generates divine energy and knowledge.",
                TerrainPreference = TerrainType.Riverbank,
                PrimaryColor = new Color(0.95f, 0.60f, 0.15f),    // Saffron
                SecondaryColor = new Color(0.95f, 0.90f, 0.70f),   // Temple white
                UniqueTraits = new[] { "Dharma Capital", "Unconquerable", "Solar Dynasty" }
            });

            Register(new LegendaryCity
            {
                CityId = "Lanka",
                Name = "लंका (Lanka)",
                Description = "The golden fortress of Ravana — a city of immense wealth and dark power. " +
                    "Though ruled by a demon king, its architecture and riches were unmatched in all three worlds.",
                Era = "Ramayana",
                Founded = "Built by Vishwakarma for Kubera, seized by Ravana",
                BonusResource = ResourceType.Suvarna,
                BonusMultiplier = 2.0f,
                SecondaryBonus = ResourceType.Loha,
                SecondaryMultiplier = 1.5f,
                StartingAge = 0,
                SpecialBuilding = "PushpakaVimana",
                SpecialBuildingName = "Pushpaka Vimana Hangar",
                SpecialBuildingDescription = "Housing for the legendary flying chariot. Dramatically increases trade and exploration range.",
                TerrainPreference = TerrainType.Mountain,
                PrimaryColor = new Color(0.85f, 0.70f, 0.15f),    // Lanka gold
                SecondaryColor = new Color(0.40f, 0.15f, 0.15f),   // Dark red
                UniqueTraits = new[] { "Golden Fortress", "Aerial Power", "Immense Wealth" }
            });

            Register(new LegendaryCity
            {
                CityId = "Mathura",
                Name = "मथुरा (Mathura)",
                Description = "The sacred birthplace of Lord Krishna. A city of pastoral beauty on the " +
                    "banks of the Yamuna, where divine plays (leelas) filled every corner.",
                Era = "Puranic / Mahabharata",
                Founded = "Ancient city of the Yadava clan on the Yamuna river",
                BonusResource = ResourceType.Anna,
                BonusMultiplier = 1.5f,
                SecondaryBonus = ResourceType.Shakti,
                SecondaryMultiplier = 1.5f,
                StartingAge = 0,
                SpecialBuilding = "KrishnaJanmasthan",
                SpecialBuildingName = "Krishna Janmasthan (Birthplace Temple)",
                SpecialBuildingDescription = "The sacred birthplace of Lord Krishna. Generates immense Shakti and blesses all food production.",
                TerrainPreference = TerrainType.Riverbank,
                PrimaryColor = new Color(0.10f, 0.20f, 0.55f),    // Krishna blue
                SecondaryColor = new Color(0.90f, 0.85f, 0.30f),   // Butter yellow
                UniqueTraits = new[] { "Sacred Birthplace", "Pastoral Paradise", "Divine Leelas" }
            });

            Register(new LegendaryCity
            {
                CityId = "Kishkindha",
                Name = "किष्किन्धा (Kishkindha)",
                Description = "The Vanara kingdom of Sugriva and Hanuman. A mighty mountain fortress " +
                    "hidden within the Rishyamukha hills, home to the greatest warriors of the monkey race.",
                Era = "Ramayana",
                Founded = "Kingdom of Vanara king Vali in the southern mountains",
                BonusResource = ResourceType.Kashtha,
                BonusMultiplier = 1.8f,
                SecondaryBonus = ResourceType.Anna,
                SecondaryMultiplier = 1.4f,
                StartingAge = 0,
                SpecialBuilding = "VanaraFort",
                SpecialBuildingName = "Rishyamukha Parvat (Mountain Fortress)",
                SpecialBuildingDescription = "The mountain stronghold of the Vanaras. Grants powerful defensive bonuses and fast warriors.",
                TerrainPreference = TerrainType.Mountain,
                PrimaryColor = new Color(0.35f, 0.55f, 0.20f),    // Forest green
                SecondaryColor = new Color(0.55f, 0.40f, 0.25f),   // Mountain brown
                UniqueTraits = new[] { "Mountain Fortress", "Vanara Warriors", "Forest Kingdom" }
            });

            Register(new LegendaryCity
            {
                CityId = "Takshashila",
                Name = "तक्षशिला (Takshashila)",
                Description = "The world's first university city — ancient center of learning where " +
                    "Chanakya taught, Charaka practiced medicine, and Panini authored grammar.",
                Era = "Historical / Mahabharata",
                Founded = "Founded by Taksha, son of Bharata, in the Gandhara kingdom",
                BonusResource = ResourceType.Vidya,
                BonusMultiplier = 2.5f,
                SecondaryBonus = ResourceType.Praja,
                SecondaryMultiplier = 1.3f,
                StartingAge = 0,
                SpecialBuilding = "AncientUniversity",
                SpecialBuildingName = "Takshashila Vishwavidyalaya (Ancient University)",
                SpecialBuildingDescription = "The world's first university. Triples Veda research speed and generates massive knowledge.",
                TerrainPreference = TerrainType.Plains,
                PrimaryColor = new Color(0.45f, 0.35f, 0.55f),    // Scholarly purple
                SecondaryColor = new Color(0.80f, 0.75f, 0.60f),   // Scroll parchment
                UniqueTraits = new[] { "First University", "Center of Learning", "Research Hub" }
            });
        }

        private static void Register(LegendaryCity city) => _cities[city.CityId] = city;

        public static LegendaryCity GetCity(string cityId)
        {
            if (_cities == null) Initialize();
            return _cities.TryGetValue(cityId, out var c) ? c : null;
        }

        public static List<LegendaryCity> GetAllCities()
        {
            if (_cities == null) Initialize();
            return new List<LegendaryCity>(_cities.Values);
        }

        public static string[] GetCityNames()
        {
            if (_cities == null) Initialize();
            var names = new List<string>();
            foreach (var c in _cities.Values) names.Add(c.CityId);
            return names.ToArray();
        }
    }

    [System.Serializable]
    public class LegendaryCity
    {
        public string CityId;
        public string Name;                // Sanskrit + English
        public string Description;
        public string Era;
        public string Founded;
        public ResourceType BonusResource;
        public float BonusMultiplier;
        public ResourceType SecondaryBonus;
        public float SecondaryMultiplier;
        public int StartingAge;
        public string SpecialBuilding;      // unique building type ID
        public string SpecialBuildingName;
        public string SpecialBuildingDescription;
        public TerrainType TerrainPreference;
        public Color PrimaryColor;
        public Color SecondaryColor;
        public string[] UniqueTraits;
    }
}
