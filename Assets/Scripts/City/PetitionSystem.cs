using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Handles the Darbar (Royal Court) petitions. Citizens submit problems to the King.
    /// Unresolved problems lower happiness and loyalty. Resolving them costs resources.
    /// </summary>
    public class PetitionSystem : MonoBehaviour
    {
        public static PetitionSystem Instance { get; private set; }

        private GovernanceState State => GameManager.Instance?.CurrentState?.Governance;

        private float _petitionSpawnTimer;
        private const float PETITION_INTERVAL = 120f; // A new petition every 2 minutes
        private const float PETITION_EXPIRY = 300f; // Petitions expire if ignored for 5 minutes

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentPhase != GamePhase.Playing) return;
            if (State == null) return;

            // Handle petition spawning
            _petitionSpawnTimer += Time.deltaTime;
            if (_petitionSpawnTimer >= PETITION_INTERVAL)
            {
                _petitionSpawnTimer = 0f;
                GeneratePetition();
            }

            // Handle petition expiry
            for (int i = State.ActivePetitions.Count - 1; i >= 0; i--)
            {
                var p = State.ActivePetitions[i];
                p.TimeRemainingSeconds -= Time.deltaTime;
                if (p.TimeRemainingSeconds <= 0)
                {
                    FailPetition(p);
                    State.ActivePetitions.RemoveAt(i);
                }
            }
        }

        private void GeneratePetition()
        {
            if (State.ActivePetitions.Count >= 5) return; // Court is full, can't take more problems

            var def = PETITION_TEMPLATES[Random.Range(0, PETITION_TEMPLATES.Length)];
            
            var instance = new PetitionState
            {
                Id = Guid.NewGuid().ToString(),
                Title = def.Title,
                Description = def.Description,
                Type = def.Type,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                TimeRemainingSeconds = PETITION_EXPIRY
            };

            State.ActivePetitions.Add(instance);
            GameEvents.ShowNotification($"📜 New Petition at Darbar: {instance.Title}");
        }

        public void ResolvePetition(string petitionId)
        {
            if (State == null) return;
            int idx = State.ActivePetitions.FindIndex(p => p.Id == petitionId);
            if (idx == -1) return;

            var petition = State.ActivePetitions[idx];
            var def = GetPetitionTemplate(petition.Title);

            var resources = GameManager.Instance.CurrentState.Resources;

            // Check if player can afford it
            bool canAfford = true;
            foreach (var cost in def.ResolutionCost)
            {
                if (!resources.HasEnough(cost.Key, cost.Value))
                {
                    canAfford = false;
                    break;
                }
            }

            if (!canAfford)
            {
                GameEvents.ShowNotification("❌ Not enough resources to resolve this petition!");
                return;
            }

            // Deduct resources
            foreach (var cost in def.ResolutionCost)
            {
                resources.Add(cost.Key, -cost.Value);
            }

            // Apply rewards
            State.Happiness = Mathf.Clamp(State.Happiness + def.HappinessReward, 0, 100);
            State.Loyalty = Mathf.Clamp(State.Loyalty + def.LoyaltyReward, 0, 100);

            GameEvents.ShowNotification($"✅ Petition Resolved: {petition.Title}");
            State.ActivePetitions.RemoveAt(idx);
            
            GameManager.Instance.AddExperience(25);
        }

        private void FailPetition(PetitionState petition)
        {
            var def = GetPetitionTemplate(petition.Title);
            
            // Penalty for ignoring the people
            State.Happiness -= 5f;
            State.Loyalty -= 5f;
            State.Happiness = Mathf.Clamp(State.Happiness, 0, 100);
            State.Loyalty = Mathf.Clamp(State.Loyalty, 0, 100);

            GameEvents.ShowNotification($"📉 Ignored Petition Penalty: {petition.Title}. People are displeased.");
        }

        public PetitionTemplate GetPetitionTemplate(string title)
        {
            foreach (var t in PETITION_TEMPLATES)
            {
                if (t.Title == title) return t;
            }
            return PETITION_TEMPLATES[0];
        }

        // ─────────────────────────────────────────────
        //  Data and Templates
        // ─────────────────────────────────────────────

        public class PetitionTemplate
        {
            public string Title;
            public string Description;
            public string Type;
            public Dictionary<ResourceType, float> ResolutionCost;
            public float HappinessReward;
            public float LoyaltyReward;
        }

        private static readonly PetitionTemplate[] PETITION_TEMPLATES = new PetitionTemplate[]
        {
            new PetitionTemplate
            {
                Title = "Drought in the Villages",
                Description = "Farmers are requesting emergency food and funds to survive the drought period.",
                Type = "Agriculture",
                ResolutionCost = new() { { ResourceType.Anna, 200f }, { ResourceType.Suvarna, 50f } },
                HappinessReward = 10f, LoyaltyReward = 15f
            },
            new PetitionTemplate
            {
                Title = "Bandit Threat",
                Description = "Merchants complain of bandit raids on the trade routes. They need armed escorts.",
                Type = "Security",
                ResolutionCost = new() { { ResourceType.Suvarna, 100f }, { ResourceType.Loha, 40f } },
                HappinessReward = 5f, LoyaltyReward = 10f
            },
            new PetitionTemplate
            {
                Title = "Temple Renovation",
                Description = "Priests request materials and energy to restore the central temple to its former glory.",
                Type = "Religion",
                ResolutionCost = new() { { ResourceType.Pashana, 100f }, { ResourceType.Shakti, 30f } },
                HappinessReward = 15f, LoyaltyReward = 5f
            },
            new PetitionTemplate
            {
                Title = "Dispute Between Guilds",
                Description = "The weavers and the potters are arguing over market space. Requires royal decree (and bribes).",
                Type = "Civil",
                ResolutionCost = new() { { ResourceType.Suvarna, 150f } },
                HappinessReward = 8f, LoyaltyReward = 8f
            },
            new PetitionTemplate
            {
                Title = "Disease Outbreak",
                Description = "A sudden illness has struck the lower district. The Vaidyas need funds for herbs.",
                Type = "Health",
                ResolutionCost = new() { { ResourceType.Anna, 100f }, { ResourceType.Suvarna, 80f } },
                HappinessReward = 12f, LoyaltyReward = 10f
            }
        };
    }
}
