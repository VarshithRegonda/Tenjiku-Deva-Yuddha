using System.Collections.Generic;
using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Gurukul & Ayurveda system — Ancient Indian education and health.
    /// 
    /// Gurukul (गुरुकुल): Traditional education where students live with the Guru.
    /// Education branches: Vedas, Astras (weapons), Ayurveda, Astronomy, Arts, Dharma.
    /// 
    /// Ayurveda (आयुर्वेद): Ancient Indian system of medicine and health.
    /// Maintains population health which affects productivity and happiness.
    /// Three Doshas: Vata, Pitta, Kapha — balance them for optimal health.
    ///
    /// Village/Town progression: Gram → Nagara → Mahanagara
    /// </summary>
    public class GurukulAyurvedaSystem : MonoBehaviour
    {
        public static GurukulAyurvedaSystem Instance { get; private set; }

        // Population health (0-100) — affects productivity
        public float PopulationHealth { get; private set; } = 70f;

        // Education level (0-100) — affects research speed and army quality
        public float EducationLevel { get; private set; } = 10f;

        // Dosha balance (each 0-100, ideal is balanced ~33 each)
        public float VataBalance { get; private set; } = 33f;
        public float PittaBalance { get; private set; } = 33f;
        public float KaphaBalance { get; private set; } = 34f;

        // Settlement tier
        public SettlementTier CurrentTier { get; private set; } = SettlementTier.Gram;

        private float _healthTickTimer;
        private const float HEALTH_TICK = 10f;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentPhase != GamePhase.Playing) return;

            _healthTickTimer += Time.deltaTime;
            if (_healthTickTimer >= HEALTH_TICK)
            {
                _healthTickTimer = 0f;
                UpdateHealth();
                UpdateEducation();
                UpdateSettlementTier();
            }
        }

        // ─────────────────────────────────────────────
        //  Ayurveda — Health System
        // ─────────────────────────────────────────────

        /// <summary>
        /// Update population health based on Ayurvedic facilities, food, and dosha balance.
        /// </summary>
        private void UpdateHealth()
        {
            var state = GameManager.Instance?.CurrentState;
            if (state == null) return;

            float baseHealth = 50f;

            // Food availability bonus
            float food = state.Resources.Get(ResourceType.Anna);
            float population = state.Resources.Get(ResourceType.Praja);
            float foodRatio = population > 0 ? food / (population * 10f) : 1f;
            baseHealth += Mathf.Clamp(foodRatio * 15f, 0f, 15f);

            // Ayurvedic building bonuses
            int vaidyaCount = CountBuilding(state, "VaidyaKutir");
            int herbGardenCount = CountBuilding(state, "HerbGarden");
            int aushadhiCount = CountBuilding(state, "AushadhiKendra");
            int yogaCount = CountBuilding(state, "YogaShala");
            int jalChikitsaCount = CountBuilding(state, "JalChikitsa");

            baseHealth += vaidyaCount * 5f;          // Each Vaidya hut +5 health
            baseHealth += herbGardenCount * 3f;       // Each herb garden +3
            baseHealth += aushadhiCount * 8f;         // Each Ayurvedic center +8
            baseHealth += yogaCount * 4f;             // Each Yoga hall +4
            baseHealth += jalChikitsaCount * 3f;      // Each water therapy +3

            // Dosha balance bonus (optimal when balanced)
            float doshaImbalance = Mathf.Abs(VataBalance - 33.3f) +
                                   Mathf.Abs(PittaBalance - 33.3f) +
                                   Mathf.Abs(KaphaBalance - 33.3f);
            float doshaBonus = Mathf.Max(0, 15f - doshaImbalance * 0.3f);
            baseHealth += doshaBonus;

            PopulationHealth = Mathf.Clamp(baseHealth, 0f, 100f);

            // Adjust Doshas based on environment
            AdjustDoshas(state);
        }

        /// <summary>
        /// Adjust the three Doshas based on buildings, season, and terrain.
        /// </summary>
        private void AdjustDoshas(GameState state)
        {
            // Vata (air/space) — increased by dry terrain, reduced by warm buildings
            float vata = 33f;
            vata += CountBuilding(state, "Windmill") * 2f;      // Wind increases Vata
            vata -= CountBuilding(state, "YogaShala") * 3f;     // Yoga balances Vata
            vata -= CountBuilding(state, "AushadhiKendra") * 2f;

            // Pitta (fire/water) — increased by forges, reduced by water
            float pitta = 33f;
            pitta += CountBuilding(state, "Armory") * 2f;       // Fire increases Pitta
            pitta += CountBuilding(state, "IronMine") * 1f;
            pitta -= CountBuilding(state, "Well") * 2f;         // Water cools Pitta
            pitta -= CountBuilding(state, "JalChikitsa") * 3f;

            // Kapha (water/earth) — increased by water, reduced by activity
            float kapha = 34f;
            kapha += CountBuilding(state, "FishingDock") * 1f;
            kapha += CountBuilding(state, "IrrigationCanal") * 1f;
            kapha -= CountBuilding(state, "TrainingGround") * 2f; // Activity reduces Kapha
            kapha -= CountBuilding(state, "ArcheryRange") * 1f;

            // Normalize
            float total = Mathf.Max(1f, vata + pitta + kapha);
            VataBalance = (vata / total) * 100f;
            PittaBalance = (pitta / total) * 100f;
            KaphaBalance = (kapha / total) * 100f;
        }

        /// <summary>
        /// Get health-based productivity multiplier (applied to resource production).
        /// </summary>
        public float GetHealthProductivityMultiplier()
        {
            // Health 0-100 maps to 0.5-1.5 multiplier
            return 0.5f + (PopulationHealth / 100f);
        }

        // ─────────────────────────────────────────────
        //  Gurukul — Education System
        // ─────────────────────────────────────────────

        /// <summary>
        /// Update education level based on Gurukul and learning buildings.
        /// </summary>
        private void UpdateEducation()
        {
            var state = GameManager.Instance?.CurrentState;
            if (state == null) return;

            float education = 10f; // base literacy

            // Gurukul buildings
            int gurukulCount = CountBuilding(state, "Gurukul");
            int universityCount = CountBuilding(state, "University");
            int libraryCount = CountBuilding(state, "Library");
            int observatoryCount = CountBuilding(state, "Observat");
            int vedaPathaCount = CountBuilding(state, "VedaPatha");

            education += gurukulCount * 8f;
            education += universityCount * 15f;
            education += libraryCount * 10f;
            education += observatoryCount * 6f;
            education += vedaPathaCount * 12f;

            // Veda research contribution
            int totalVedaLevel = state.VedaProgress.GetTotalResearchLevel();
            education += totalVedaLevel * 2f;

            EducationLevel = Mathf.Clamp(education, 0f, 100f);
        }

        /// <summary>
        /// Get education-based research speed multiplier.
        /// </summary>
        public float GetResearchSpeedMultiplier()
        {
            return 0.5f + (EducationLevel / 100f);
        }

        /// <summary>
        /// Get education-based army quality multiplier.
        /// Educated soldiers fight better.
        /// </summary>
        public float GetArmyQualityMultiplier()
        {
            return 0.8f + (EducationLevel / 200f); // 0.8 to 1.3
        }

        // ─────────────────────────────────────────────
        //  Settlement Tier — Village → Town → City
        // ─────────────────────────────────────────────

        private void UpdateSettlementTier()
        {
            var state = GameManager.Instance?.CurrentState;
            if (state == null) return;

            float pop = state.Resources.Get(ResourceType.Praja);
            int buildings = state.TotalBuildingsPlaced;

            SettlementTier newTier;
            if (pop >= 300 && buildings >= 80)
                newTier = SettlementTier.Mahanagara;
            else if (pop >= 150 && buildings >= 40)
                newTier = SettlementTier.Nagara;
            else if (pop >= 50 && buildings >= 15)
                newTier = SettlementTier.Kasba;
            else
                newTier = SettlementTier.Gram;

            if (newTier != CurrentTier)
            {
                CurrentTier = newTier;
                string tierName = newTier switch
                {
                    SettlementTier.Gram => "ग्राम (Village)",
                    SettlementTier.Kasba => "कस्बा (Town)",
                    SettlementTier.Nagara => "नगर (City)",
                    SettlementTier.Mahanagara => "महानगर (Metropolis)",
                    _ => ""
                };
                GameEvents.ShowNotification($"🏘️ Settlement upgraded to {tierName}!");
                GameManager.Instance.AddExperience(200);
            }
        }

        // ─────────────────────────────────────────────
        //  Info for UI
        // ─────────────────────────────────────────────
        public string GetHealthReport()
        {
            string doshaStr = $"Vata: {VataBalance:F0}% | Pitta: {PittaBalance:F0}% | Kapha: {KaphaBalance:F0}%";
            string balanceStr;
            float imbalance = Mathf.Abs(VataBalance - 33.3f) + Mathf.Abs(PittaBalance - 33.3f) + Mathf.Abs(KaphaBalance - 33.3f);
            if (imbalance < 10) balanceStr = "🟢 Balanced (Excellent)";
            else if (imbalance < 25) balanceStr = "🟡 Slightly Imbalanced";
            else balanceStr = "🔴 Imbalanced — Build Ayurvedic facilities!";

            return $"Health: {PopulationHealth:F0}/100 | {doshaStr}\n{balanceStr}";
        }

        public string GetEducationReport()
        {
            return $"Education: {EducationLevel:F0}/100 | Settlement: {CurrentTier}";
        }

        // ─────────────────────────────────────────────
        //  Gurukul Curriculum
        // ─────────────────────────────────────────────
        public static readonly GurukulSubject[] GURUKUL_SUBJECTS = new[]
        {
            new GurukulSubject
            {
                Name = "वेद पाठ (Veda Recitation)",
                Description = "Students learn to chant and understand the sacred Vedas — Rigveda, Yajurveda, Samaveda, Atharvaveda.",
                Benefit = "Boosts Veda research speed by 20% per Gurukul",
                BonusType = ResourceType.Vidya
            },
            new GurukulSubject
            {
                Name = "धनुर्विद्या (Archery / Martial Arts)",
                Description = "Training in Dhanurvidya — the art of weapons including bow, sword, mace, and divine Astras.",
                Benefit = "Increases army training quality by 15%",
                BonusType = ResourceType.Loha
            },
            new GurukulSubject
            {
                Name = "आयुर्वेद (Ayurveda)",
                Description = "The science of life — herbal medicine, surgery (Sushruta), diet (Pathya), and Panchakarma detox.",
                Benefit = "Population health +10 per Ayurveda Gurukul",
                BonusType = ResourceType.Anna
            },
            new GurukulSubject
            {
                Name = "ज्योतिष (Astronomy & Astrology)",
                Description = "Study of Nakshatras, planetary movements, eclipses, and time-keeping per Surya Siddhanta.",
                Benefit = "Reveals auspicious times for building and battle",
                BonusType = ResourceType.Vidya
            },
            new GurukulSubject
            {
                Name = "वास्तु शास्त्र (Architecture)",
                Description = "Ancient science of sacred geometry, town planning, and directional alignment of buildings.",
                Benefit = "Vastu bonuses increased by 10% per Architecture Gurukul",
                BonusType = ResourceType.Pashana
            },
            new GurukulSubject
            {
                Name = "संगीत एवं नृत्य (Music & Dance)",
                Description = "Training in Sangeet (music), Nritya (dance), and Natya (drama) per Bharata's Natyashastra.",
                Benefit = "Population happiness +15, troop morale +10%",
                BonusType = ResourceType.Praja
            },
            new GurukulSubject
            {
                Name = "गणित (Mathematics)",
                Description = "Study of Ganit — algebra (Bijaganita), geometry (Rekhaganita), zero (Shunya), and decimal system.",
                Benefit = "Resource calculations more efficient — less waste",
                BonusType = ResourceType.Suvarna
            },
            new GurukulSubject
            {
                Name = "धर्म शास्त्र (Dharma & Ethics)",
                Description = "Study of Dharma — righteous conduct, justice, governance, and the Arthashastra of Chanakya.",
                Benefit = "Reduces corruption — all resource production +5%",
                BonusType = ResourceType.Shakti
            }
        };

        // ─────────────────────────────────────────────
        //  Ayurveda Treatment Types
        // ─────────────────────────────────────────────
        public static readonly AyurvedaTreatment[] AYURVEDA_TREATMENTS = new[]
        {
            new AyurvedaTreatment
            {
                Name = "पंचकर्म (Panchakarma)",
                Description = "Five-fold detoxification: Vamana (emesis), Virechana (purgation), Basti (enema), Nasya (nasal), Raktamokshana (bloodletting).",
                HealthBoost = 15f,
                DoshaBalance = "Balances all three Doshas",
                ShaktiCost = 10f
            },
            new AyurvedaTreatment
            {
                Name = "रसायन (Rasayana)",
                Description = "Rejuvenation therapy using herbs like Ashwagandha, Brahmi, Amla, and Shatavari.",
                HealthBoost = 10f,
                DoshaBalance = "Strengthens Ojas (immunity)",
                ShaktiCost = 5f
            },
            new AyurvedaTreatment
            {
                Name = "योग चिकित्सा (Yoga Chikitsa)",
                Description = "Therapeutic yoga: Asanas for body, Pranayama for breath, Dhyana for mind.",
                HealthBoost = 8f,
                DoshaBalance = "Balances Vata and calms Pitta",
                ShaktiCost = 3f
            },
            new AyurvedaTreatment
            {
                Name = "जल चिकित्सा (Jal Chikitsa)",
                Description = "Water therapy — bathing in sacred rivers, mineral water treatments.",
                HealthBoost = 7f,
                DoshaBalance = "Reduces excess Pitta and Kapha",
                ShaktiCost = 2f
            },
            new AyurvedaTreatment
            {
                Name = "औषधि (Aushadhi / Herbal Medicine)",
                Description = "Medicinal formulations from herbs — Churna, Kashaya, Ghrita, Taila, Asava.",
                HealthBoost = 12f,
                DoshaBalance = "Targeted Dosha correction",
                ShaktiCost = 8f
            }
        };

        private int CountBuilding(GameState state, string typeId)
        {
            return state.Buildings.FindAll(b => b.BuildingTypeId == typeId && b.IsConstructed).Count;
        }
    }

    // ─────────────────────────────────────────────
    //  Data Structures
    // ─────────────────────────────────────────────
    public enum SettlementTier
    {
        Gram,           // Village (< 50 pop)
        Kasba,          // Town (50-150 pop)
        Nagara,         // City (150-300 pop)
        Mahanagara      // Metropolis (300+ pop)
    }

    [System.Serializable]
    public struct GurukulSubject
    {
        public string Name;
        public string Description;
        public string Benefit;
        public ResourceType BonusType;
    }

    [System.Serializable]
    public struct AyurvedaTreatment
    {
        public string Name;
        public string Description;
        public float HealthBoost;
        public string DoshaBalance;
        public float ShaktiCost;
    }
}
