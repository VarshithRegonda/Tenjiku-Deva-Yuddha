using System.Collections.Generic;
using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Mahabharata-inspired battle system.
    /// Unlocks at player level 200 (Senapati rank).
    /// Features Vyuha formations, warrior types, and divine Astras.
    /// </summary>
    public class BattleSystem : MonoBehaviour
    {
        public static BattleSystem Instance { get; private set; }

        public bool IsBattleActive { get; private set; }
        public BattleState CurrentBattle { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // ─────────────────────────────────────────────
        //  Battle Initiation
        // ─────────────────────────────────────────────
        public bool CanStartBattle()
        {
            var state = GameManager.Instance.CurrentState;
            if (state == null) return false;
            if (!state.BattlesUnlocked)
            {
                GameEvents.ShowNotification($"⚔️ Battles unlock at Level 200 (Senapati rank). You are Level {state.PlayerLevel}.");
                return false;
            }
            if (state.Army.GetTotalWarriors() == 0)
            {
                GameEvents.ShowNotification("You have no warriors! Build a Barracks and train troops first.");
                return false;
            }
            if (IsBattleActive)
            {
                GameEvents.ShowNotification("A battle is already in progress!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Start a battle against an NPC enemy.
        /// </summary>
        public bool StartBattle(BattleMode mode, int difficultyLevel)
        {
            if (!CanStartBattle()) return false;

            var state = GameManager.Instance.CurrentState;

            // Generate enemy army based on difficulty
            var enemyArmy = GenerateEnemyArmy(difficultyLevel);
            string enemyName = GenerateEnemyName(difficultyLevel);

            CurrentBattle = new BattleState
            {
                Mode = mode,
                DifficultyLevel = difficultyLevel,
                EnemyName = enemyName,
                PlayerFormation = state.Army.SelectedVyuha,
                PlayerStrength = CalculatePlayerStrength(state),
                EnemyStrength = CalculateEnemyStrength(enemyArmy),
                EnemyArmy = enemyArmy,
                Rounds = new List<BattleRound>(),
                IsComplete = false
            };

            IsBattleActive = true;
            GameEvents.ShowNotification($"⚔️ BATTLE BEGINS!\n{enemyName} challenges {state.KingdomName}!\nYour strength: {CurrentBattle.PlayerStrength:F0} vs Enemy: {CurrentBattle.EnemyStrength:F0}");

            Debug.Log($"[Battle] Started {mode} battle vs {enemyName} (Difficulty {difficultyLevel})");

            // Auto-resolve battle in rounds
            ResolveBattle();

            return true;
        }

        // ─────────────────────────────────────────────
        //  Battle Resolution
        // ─────────────────────────────────────────────
        private void ResolveBattle()
        {
            if (CurrentBattle == null) return;

            var state = GameManager.Instance.CurrentState;
            float playerStr = CurrentBattle.PlayerStrength;
            float enemyStr = CurrentBattle.EnemyStrength;

            // Formation bonus
            float formationBonus = GetFormationBonus(CurrentBattle.PlayerFormation, CurrentBattle.Mode);
            playerStr *= formationBonus;

            // Veda research bonus (Atharvaveda level gives combat bonus)
            int atharvaLevel = state.VedaProgress.AtharvavedaLevel;
            playerStr *= (1f + atharvaLevel * 0.05f);

            // Samaveda morale bonus
            int samaLevel = state.VedaProgress.SamavedaLevel;
            playerStr *= (1f + samaLevel * 0.03f);

            // Simulate rounds
            int maxRounds = 10;
            float playerHP = playerStr;
            float enemyHP = enemyStr;

            for (int round = 1; round <= maxRounds && playerHP > 0 && enemyHP > 0; round++)
            {
                // Player attacks
                float playerDamage = playerHP * Random.Range(0.1f, 0.25f);
                enemyHP -= playerDamage;

                // Enemy attacks
                float enemyDamage = enemyHP * Random.Range(0.08f, 0.2f);
                playerHP -= enemyDamage;

                CurrentBattle.Rounds.Add(new BattleRound
                {
                    RoundNumber = round,
                    PlayerDamageDealt = playerDamage,
                    EnemyDamageDealt = enemyDamage,
                    PlayerHPRemaining = Mathf.Max(0, playerHP),
                    EnemyHPRemaining = Mathf.Max(0, enemyHP)
                });
            }

            // Determine winner
            bool playerWon = playerHP > enemyHP;
            CurrentBattle.IsComplete = true;
            CurrentBattle.PlayerWon = playerWon;

            // Apply results
            if (playerWon)
            {
                OnBattleWon(CurrentBattle);
            }
            else
            {
                OnBattleLost(CurrentBattle);
            }

            IsBattleActive = false;
        }

        private void OnBattleWon(BattleState battle)
        {
            var state = GameManager.Instance.CurrentState;
            state.BattleRecord.BattlesWon++;
            state.BattleRecord.BattlesTotal++;
            if (battle.DifficultyLevel > state.BattleRecord.HighestBattleLevel)
                state.BattleRecord.HighestBattleLevel = battle.DifficultyLevel;

            // Rewards
            float goldReward = 100f + battle.DifficultyLevel * 50f;
            float shaktiReward = 20f + battle.DifficultyLevel * 10f;
            int xpReward = 200 + battle.DifficultyLevel * 100;

            ResourceManager.Instance.AddResource(ResourceType.Suvarna, goldReward);
            ResourceManager.Instance.AddResource(ResourceType.Shakti, shaktiReward);
            GameManager.Instance.AddExperience(xpReward);

            // Casualty calculation (lose some warriors based on damage taken)
            float casualtyRate = 1f - (CurrentBattle.Rounds[^1].PlayerHPRemaining / CurrentBattle.PlayerStrength);
            casualtyRate = Mathf.Clamp(casualtyRate * 0.3f, 0.05f, 0.5f);
            ApplyCasualties(state, casualtyRate);

            state.BattleRecord.EnemiesDefeated += battle.DifficultyLevel;

            GameEvents.ShowNotification(
                $"🏆 VICTORY over {battle.EnemyName}!\n" +
                $"Rewards: {goldReward:F0} Suvarna, {shaktiReward:F0} Shakti, {xpReward} XP\n" +
                $"Casualties: {casualtyRate * 100:F0}% troops lost"
            );
        }

        private void OnBattleLost(BattleState battle)
        {
            var state = GameManager.Instance.CurrentState;
            state.BattleRecord.BattlesLost++;
            state.BattleRecord.BattlesTotal++;

            // Heavier casualties on loss
            float casualtyRate = 0.4f + Random.Range(0f, 0.2f);
            ApplyCasualties(state, casualtyRate);

            // Lose some resources
            float resourceLoss = 0.1f + battle.DifficultyLevel * 0.02f;
            float goldLost = state.Resources.Get(ResourceType.Suvarna) * resourceLoss;
            ResourceManager.Instance.ConsumeResource(ResourceType.Suvarna, goldLost);

            // Small consolation XP
            GameManager.Instance.AddExperience(50);

            GameEvents.ShowNotification(
                $"💀 DEFEAT by {battle.EnemyName}...\n" +
                $"Lost {casualtyRate * 100:F0}% of your troops and {goldLost:F0} Suvarna.\n" +
                $"Rebuild and try again!"
            );
        }

        private void ApplyCasualties(GameState state, float rate)
        {
            state.Army.Kshatriya = Mathf.Max(0, Mathf.FloorToInt(state.Army.Kshatriya * (1 - rate)));
            state.Army.Dhanurdhar = Mathf.Max(0, Mathf.FloorToInt(state.Army.Dhanurdhar * (1 - rate)));
            state.Army.Ashvarohi = Mathf.Max(0, Mathf.FloorToInt(state.Army.Ashvarohi * (1 - rate)));
            state.Army.Rathi = Mathf.Max(0, Mathf.FloorToInt(state.Army.Rathi * (1 - rate)));
            state.Army.Gajasena = Mathf.Max(0, Mathf.FloorToInt(state.Army.Gajasena * (1 - rate)));
        }

        // ─────────────────────────────────────────────
        //  Army & Warrior Training
        // ─────────────────────────────────────────────
        /// <summary>
        /// Train warriors of a specific type. Costs resources and population.
        /// </summary>
        public bool TrainWarriors(WarriorType type, int count)
        {
            var state = GameManager.Instance.CurrentState;
            if (state == null) return false;

            // Check if appropriate building exists
            string requiredBuilding = type switch
            {
                WarriorType.Kshatriya => "Barracks",
                WarriorType.Dhanurdhar => "ArcheryRange",
                WarriorType.Ashvarohi => "Stables",
                WarriorType.Rathi => "Stables",
                WarriorType.Gajasena => "TrainingGround",
                _ => ""
            };

            bool hasBuilding = state.Buildings.Exists(b => b.BuildingTypeId == requiredBuilding && b.IsConstructed);
            if (!hasBuilding)
            {
                var def = BuildingDatabase.GetBuilding(requiredBuilding);
                GameEvents.ShowNotification($"Requires {def?.Name ?? requiredBuilding}!");
                return false;
            }

            // Cost per warrior
            var cost = GetTrainingCost(type);
            var totalCost = new Dictionary<ResourceType, float>();
            foreach (var kvp in cost)
                totalCost[kvp.Key] = kvp.Value * count;

            // Also costs population
            totalCost[ResourceType.Praja] = count;

            if (!ResourceManager.Instance.HasEnough(totalCost))
            {
                GameEvents.ShowNotification("Not enough resources to train warriors!");
                return false;
            }

            ResourceManager.Instance.ConsumeResources(totalCost);

            // Add warriors
            switch (type)
            {
                case WarriorType.Kshatriya: state.Army.Kshatriya += count; break;
                case WarriorType.Dhanurdhar: state.Army.Dhanurdhar += count; break;
                case WarriorType.Ashvarohi: state.Army.Ashvarohi += count; break;
                case WarriorType.Rathi: state.Army.Rathi += count; break;
                case WarriorType.Gajasena: state.Army.Gajasena += count; break;
            }

            GameManager.Instance.AddExperience(count * 10);
            GameEvents.ShowNotification($"⚔️ Trained {count} {type}! Total army: {state.Army.GetTotalWarriors()} warriors.");
            return true;
        }

        public Dictionary<ResourceType, float> GetTrainingCost(WarriorType type)
        {
            return type switch
            {
                WarriorType.Kshatriya => new() { { ResourceType.Suvarna, 20 }, { ResourceType.Loha, 5 }, { ResourceType.Anna, 10 } },
                WarriorType.Dhanurdhar => new() { { ResourceType.Suvarna, 30 }, { ResourceType.Kashtha, 10 }, { ResourceType.Loha, 5 } },
                WarriorType.Ashvarohi => new() { { ResourceType.Suvarna, 60 }, { ResourceType.Anna, 20 }, { ResourceType.Loha, 10 } },
                WarriorType.Rathi => new() { { ResourceType.Suvarna, 100 }, { ResourceType.Kashtha, 30 }, { ResourceType.Loha, 20 } },
                WarriorType.Gajasena => new() { { ResourceType.Suvarna, 200 }, { ResourceType.Anna, 50 }, { ResourceType.Loha, 30 } },
                _ => new() { { ResourceType.Suvarna, 20 } }
            };
        }

        // ─────────────────────────────────────────────
        //  Strength Calculation
        // ─────────────────────────────────────────────
        private float CalculatePlayerStrength(GameState state)
        {
            return state.Army.GetArmyStrength();
        }

        private float CalculateEnemyStrength(EnemyArmy enemy)
        {
            return (enemy.Infantry * 1f) + (enemy.Archers * 1.5f) +
                   (enemy.Cavalry * 2.5f) + (enemy.Chariots * 4f) + (enemy.Elephants * 8f);
        }

        private float GetFormationBonus(string formation, BattleMode mode)
        {
            // Different formations are better for different battle modes
            return (formation, mode) switch
            {
                ("Garudavyuha", BattleMode.Conquest) => 1.3f,       // Eagle is best for attack
                ("Padmavyuha", BattleMode.DefendKingdom) => 1.3f,    // Lotus is best for defense
                ("Chakravyuha", _) => 1.2f,                           // Disc is strong generally
                ("Makaravyuha", BattleMode.Conquest) => 1.25f,       // Crocodile for ambush
                ("Vajravyuha", BattleMode.DefendKingdom) => 1.35f,   // Diamond for elite defense
                ("Sarpavyuha", _) => 1.15f,                           // Serpent for flanking
                _ => 1.1f                                              // Default
            };
        }

        // ─────────────────────────────────────────────
        //  Enemy Generation
        // ─────────────────────────────────────────────
        private EnemyArmy GenerateEnemyArmy(int difficulty)
        {
            int baseTroops = 10 + difficulty * 5;
            return new EnemyArmy
            {
                Infantry = baseTroops + Random.Range(0, difficulty * 3),
                Archers = Mathf.Max(0, baseTroops / 2 + Random.Range(-5, difficulty * 2)),
                Cavalry = Mathf.Max(0, difficulty > 3 ? baseTroops / 3 : 0),
                Chariots = Mathf.Max(0, difficulty > 5 ? baseTroops / 5 : 0),
                Elephants = Mathf.Max(0, difficulty > 8 ? baseTroops / 8 : 0)
            };
        }

        private string GenerateEnemyName(int difficulty)
        {
            string[] names = difficulty switch
            {
                <= 3 => new[] { "Bandit Raiders", "Dasyu Clan", "Forest Thieves", "Tribal Warband" },
                <= 6 => new[] { "Kingdom of Gandhara", "Magadha Forces", "Kashi Warriors", "Panchala Army" },
                <= 9 => new[] { "Kaurava Legion", "Jarasandha's Empire", "Kamsa's Guard", "Ravana's Rakshasa" },
                _ => new[] { "The Asura Horde", "Demon King's Army", "Forces of Adharma", "Armies of Kali Yuga" }
            };
            return names[Random.Range(0, names.Length)];
        }

        /// <summary>
        /// Get available battle difficulties based on player level and army size.
        /// </summary>
        public List<BattleDifficultyOption> GetAvailableBattles()
        {
            var state = GameManager.Instance.CurrentState;
            if (state == null || !state.BattlesUnlocked) return new List<BattleDifficultyOption>();

            var options = new List<BattleDifficultyOption>();
            int maxDifficulty = Mathf.Min(20, (state.PlayerLevel - 200) / 10 + 1);

            for (int i = 1; i <= maxDifficulty; i++)
            {
                var enemy = GenerateEnemyArmy(i);
                options.Add(new BattleDifficultyOption
                {
                    Difficulty = i,
                    EnemyName = GenerateEnemyName(i),
                    EstimatedEnemyStrength = CalculateEnemyStrength(enemy),
                    GoldReward = 100f + i * 50f,
                    XPReward = 200 + i * 100
                });
            }
            return options;
        }
    }

    // ─────────────────────────────────────────────
    //  Battle Data Structures
    // ─────────────────────────────────────────────
    [System.Serializable]
    public class BattleState
    {
        public BattleMode Mode;
        public int DifficultyLevel;
        public string EnemyName;
        public string PlayerFormation;
        public float PlayerStrength;
        public float EnemyStrength;
        public EnemyArmy EnemyArmy;
        public List<BattleRound> Rounds;
        public bool IsComplete;
        public bool PlayerWon;
    }

    [System.Serializable]
    public class BattleRound
    {
        public int RoundNumber;
        public float PlayerDamageDealt;
        public float EnemyDamageDealt;
        public float PlayerHPRemaining;
        public float EnemyHPRemaining;
    }

    [System.Serializable]
    public class EnemyArmy
    {
        public int Infantry;
        public int Archers;
        public int Cavalry;
        public int Chariots;
        public int Elephants;
    }

    [System.Serializable]
    public class BattleDifficultyOption
    {
        public int Difficulty;
        public string EnemyName;
        public float EstimatedEnemyStrength;
        public float GoldReward;
        public int XPReward;
    }
}
