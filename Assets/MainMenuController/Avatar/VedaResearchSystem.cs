using System.Collections.Generic;
using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Four Vedas research/technology system.
    /// Players research Mantras (verses) using Vidya (Knowledge) to unlock
    /// bonuses, buildings, battle formations, and Astras.
    /// </summary>
    public class VedaResearchSystem : MonoBehaviour
    {
        public static VedaResearchSystem Instance { get; private set; }

        public const int MAX_VEDA_LEVEL = 10;

        // Cost multipliers per level (cumulative)
        private static readonly float[] RESEARCH_COSTS = { 50, 100, 200, 350, 550, 800, 1100, 1500, 2000, 3000 };

        private void Awake()
        {
            Instance = this;
        }

        // ─────────────────────────────────────────────
        //  Research
        // ─────────────────────────────────────────────
        /// <summary>
        /// Start researching the next level of a Veda.
        /// </summary>
        public bool StartResearch(VedaType veda)
        {
            var state = GameManager.Instance.CurrentState;
            if (state == null) return false;

            int currentLevel = state.VedaProgress.GetLevel(veda);
            if (currentLevel >= MAX_VEDA_LEVEL)
            {
                GameEvents.ShowNotification($"{veda} research is already at maximum level!");
                return false;
            }

            // Can only research one thing at a time
            if (!string.IsNullOrEmpty(state.VedaProgress.ActiveResearchVeda))
            {
                GameEvents.ShowNotification("Already researching! Complete current research first.");
                return false;
            }

            float cost = GetResearchCost(currentLevel);
            if (!ResourceManager.Instance.ConsumeResource(ResourceType.Vidya, cost))
            {
                GameEvents.ShowNotification($"Not enough Vidya! Need {cost} Vidya.");
                return false;
            }

            state.VedaProgress.ActiveResearchVeda = veda.ToString();
            state.VedaProgress.CurrentResearchProgress = 0f;

            GameEvents.ShowNotification($"📜 Started {veda} Level {currentLevel + 1} research!");
            Debug.Log($"[Veda] Started researching {veda} level {currentLevel + 1}");
            return true;
        }

        /// <summary>
        /// Called every frame to progress active research.
        /// Research speed is boosted by libraries and universities.
        /// </summary>
        private void Update()
        {
            var state = GameManager.Instance?.CurrentState;
            if (state == null) return;
            if (GameManager.Instance.CurrentPhase != GamePhase.Playing) return;
            if (string.IsNullOrEmpty(state.VedaProgress.ActiveResearchVeda)) return;

            // Research speed = base + (number of libraries * 0.5 + universities * 1.0)
            float researchSpeed = 0.01f;  // base speed (will complete in ~100 seconds)
            // Boost from Vidya production rate
            float vidyaRate = ResourceManager.Instance.GetProductionRate(ResourceType.Vidya);
            researchSpeed += vidyaRate * 0.002f;

            // Damaru Beat boost
            if (GameManager.Instance.IsDamaruActive)
                researchSpeed *= GameConstants.DAMARU_SPEED_MULTIPLIER;

            state.VedaProgress.CurrentResearchProgress += researchSpeed * Time.deltaTime;

            if (state.VedaProgress.CurrentResearchProgress >= 1f)
            {
                CompleteResearch();
            }
        }

        private void CompleteResearch()
        {
            var state = GameManager.Instance.CurrentState;
            if (!System.Enum.TryParse<VedaType>(state.VedaProgress.ActiveResearchVeda, out var veda))
                return;

            int oldLevel = state.VedaProgress.GetLevel(veda);
            int newLevel = oldLevel + 1;
            state.VedaProgress.SetLevel(veda, newLevel);
            state.VedaProgress.ActiveResearchVeda = "";
            state.VedaProgress.CurrentResearchProgress = 0f;

            // XP reward
            GameManager.Instance.AddExperience(50 + newLevel * 50);

            // Apply bonuses
            ApplyVedaBonus(veda, newLevel);

            GameEvents.ShowNotification($"✨ {veda} Level {newLevel} research complete!\n{GetLevelDescription(veda, newLevel)}");
            Debug.Log($"[Veda] {veda} advanced to level {newLevel}");
        }

        // ─────────────────────────────────────────────
        //  Veda Bonuses
        // ─────────────────────────────────────────────
        private void ApplyVedaBonus(VedaType veda, int level)
        {
            var state = GameManager.Instance.CurrentState;
            switch (veda)
            {
                case VedaType.Rigveda:
                    // Divine Power: increases Shakti max capacity
                    ResourceManager.Instance.IncreaseMaxResource(ResourceType.Shakti, 50f * level);
                    break;

                case VedaType.Yajurveda:
                    // Construction: increases all max storage
                    foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
                        ResourceManager.Instance.IncreaseMaxResource(type, 100f);
                    break;

                case VedaType.Samaveda:
                    // Morale: increases population capacity
                    ResourceManager.Instance.IncreaseMaxResource(ResourceType.Praja, 20f);
                    break;

                case VedaType.Atharvaveda:
                    // Warfare: unlock formations and Astras
                    UnlockBattleRewards(level, state);
                    break;
            }
        }

        private void UnlockBattleRewards(int level, GameState state)
        {
            // Unlock Vyuha formations at certain Atharvaveda levels
            switch (level)
            {
                case 2:
                    if (!state.Army.UnlockedVyuhas.Contains("Chakravyuha"))
                    {
                        state.Army.UnlockedVyuhas.Add("Chakravyuha");
                        GameEvents.ShowNotification("⚔️ Chakravyuha formation unlocked!");
                    }
                    break;
                case 3:
                    if (!state.Army.UnlockedVyuhas.Contains("Garudavyuha"))
                    {
                        state.Army.UnlockedVyuhas.Add("Garudavyuha");
                        GameEvents.ShowNotification("⚔️ Garudavyuha formation unlocked!");
                    }
                    break;
                case 4:
                    if (!state.Army.UnlockedAstras.Contains("Agneyastra"))
                    {
                        state.Army.UnlockedAstras.Add("Agneyastra");
                        GameEvents.ShowNotification("🔥 Agneyastra (Fire Weapon) unlocked!");
                    }
                    break;
                case 5:
                    if (!state.Army.UnlockedVyuhas.Contains("Padmavyuha"))
                    {
                        state.Army.UnlockedVyuhas.Add("Padmavyuha");
                        state.Army.UnlockedAstras.Add("Varunastra");
                        GameEvents.ShowNotification("🌊 Padmavyuha & Varunastra unlocked!");
                    }
                    break;
                case 6:
                    if (!state.Army.UnlockedVyuhas.Contains("Makaravyuha"))
                    {
                        state.Army.UnlockedVyuhas.Add("Makaravyuha");
                        state.Army.UnlockedAstras.Add("Nagastra");
                        GameEvents.ShowNotification("🐍 Makaravyuha & Nagastra unlocked!");
                    }
                    break;
                case 7:
                    if (!state.Army.UnlockedAstras.Contains("Vajra"))
                    {
                        state.Army.UnlockedAstras.Add("Vajra");
                        state.Army.UnlockedAstras.Add("Vayavyastra");
                        GameEvents.ShowNotification("⚡ Vajra & Vayavyastra unlocked!");
                    }
                    break;
                case 8:
                    if (!state.Army.UnlockedVyuhas.Contains("Vajravyuha"))
                    {
                        state.Army.UnlockedVyuhas.Add("Vajravyuha");
                        state.Army.UnlockedAstras.Add("Gandiva");
                        GameEvents.ShowNotification("🏹 Vajravyuha & Gandiva (Arjuna's Bow) unlocked!");
                    }
                    break;
                case 9:
                    if (!state.Army.UnlockedAstras.Contains("Brahmastra"))
                    {
                        state.Army.UnlockedAstras.Add("Brahmastra");
                        state.Army.UnlockedAstras.Add("Sudarshana");
                        GameEvents.ShowNotification("☀️ LEGENDARY: Brahmastra & Sudarshana Chakra unlocked!");
                    }
                    break;
                case 10:
                    if (!state.Army.UnlockedVyuhas.Contains("Sarpavyuha"))
                    {
                        state.Army.UnlockedVyuhas.Add("Sarpavyuha");
                        state.Army.UnlockedAstras.Add("Pashupatastra");
                        state.Army.UnlockedAstras.Add("Narayanastra");
                        GameEvents.ShowNotification("🔱 LEGENDARY: Sarpavyuha, Pashupatastra & Narayanastra unlocked! All battle knowledge mastered!");
                    }
                    break;
            }
        }

        // ─────────────────────────────────────────────
        //  Info
        // ─────────────────────────────────────────────
        public float GetResearchCost(int currentLevel)
        {
            if (currentLevel < 0 || currentLevel >= RESEARCH_COSTS.Length) return float.MaxValue;
            return RESEARCH_COSTS[currentLevel];
        }

        public string GetLevelDescription(VedaType veda, int level)
        {
            return veda switch
            {
                VedaType.Rigveda => $"Shakti capacity increased! Divine power grows stronger.",
                VedaType.Yajurveda => $"All resource storage expanded! Construction knowledge deepens.",
                VedaType.Samaveda => $"Population capacity increased! Harmony through sacred music.",
                VedaType.Atharvaveda => $"Warfare knowledge advanced! New formations and weapons available.",
                _ => "Knowledge expanded."
            };
        }

        public static readonly Dictionary<VedaType, VedaInfo> VEDA_INFO = new()
        {
            {
                VedaType.Rigveda, new VedaInfo
                {
                    Name = "ऋग्वेद (Rigveda)",
                    Subtitle = "The Veda of Hymns & Divine Power",
                    Description = "The oldest and most sacred text. Contains 1,028 hymns (suktas) dedicated to the gods.\nResearch boosts Shakti generation and divine building power.",
                    Color = new Color(0.95f, 0.65f, 0.10f)  // Saffron
                }
            },
            {
                VedaType.Yajurveda, new VedaInfo
                {
                    Name = "यजुर्वेद (Yajurveda)",
                    Subtitle = "The Veda of Rituals & Construction",
                    Description = "Contains prose mantras for yajna (fire rituals) and construction of altars.\nResearch boosts building efficiency and resource storage.",
                    Color = new Color(0.75f, 0.20f, 0.15f)  // Deep red
                }
            },
            {
                VedaType.Samaveda, new VedaInfo
                {
                    Name = "सामवेद (Samaveda)",
                    Subtitle = "The Veda of Music & Morale",
                    Description = "The Veda of melodies and chants. Root of Indian classical music.\nResearch boosts population morale and cultural growth.",
                    Color = new Color(0.20f, 0.50f, 0.75f)  // Sky blue
                }
            },
            {
                VedaType.Atharvaveda, new VedaInfo
                {
                    Name = "अथर्ववेद (Atharvaveda)",
                    Subtitle = "The Veda of Science & Warfare",
                    Description = "Contains knowledge of medicine, science, and martial arts.\nResearch unlocks battle formations (Vyuhas) and divine weapons (Astras).",
                    Color = new Color(0.30f, 0.65f, 0.30f)  // Forest green
                }
            }
        };
    }

    [System.Serializable]
    public struct VedaInfo
    {
        public string Name;
        public string Subtitle;
        public string Description;
        public Color Color;
    }
}
