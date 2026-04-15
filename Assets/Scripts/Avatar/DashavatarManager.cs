using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Manages progression through the 10 Dashavatar ages.
    /// Tracks requirements and triggers age advancement ceremonies.
    /// </summary>
    public class DashavatarManager : MonoBehaviour
    {
        public static DashavatarManager Instance { get; private set; }

        public int CurrentAge => GameManager.Instance?.CurrentState?.CurrentAvatarAge ?? 0;
        public AvatarInfo CurrentAvatar => GameConstants.AVATARS[Mathf.Clamp(CurrentAge, 0, 9)];
        public bool IsMaxAge => CurrentAge >= 9;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AvatarMgr = this;
        }

        // ─────────────────────────────────────────────
        //  Check and Advance Age
        // ─────────────────────────────────────────────
        /// <summary>
        /// Checks if the player meets requirements to advance to the next avatar age.
        /// Called periodically or after significant player actions.
        /// </summary>
        public bool CanAdvanceAge()
        {
            if (IsMaxAge) return false;
            var state = GameManager.Instance.CurrentState;
            if (state == null) return false;

            int nextAge = state.CurrentAvatarAge + 1;
            if (nextAge >= GameConstants.AVATARS.Length) return false;

            var nextAvatar = GameConstants.AVATARS[nextAge];
            int population = Mathf.FloorToInt(state.Resources.Get(ResourceType.Praja));
            int buildings = BuildingSystem.Instance?.GetConstructedBuildingCount() ?? 0;

            return population >= nextAvatar.RequiredPopulation &&
                   buildings >= nextAvatar.RequiredBuildings;
        }

        /// <summary>
        /// Advance to the next avatar age. Triggers ceremony and unlocks.
        /// </summary>
        public bool AdvanceAge()
        {
            if (!CanAdvanceAge()) return false;

            var state = GameManager.Instance.CurrentState;
            int oldAge = state.CurrentAvatarAge;
            int newAge = oldAge + 1;

            state.CurrentAvatarAge = newAge;
            state.AgeProgress = 0f;

            var avatar = GameConstants.AVATARS[newAge];

            // Grant XP
            GameManager.Instance.AddExperience(1000);

            // Fire events
            GameEvents.AgeChanged(oldAge, newAge);
            GameEvents.AvatarUnlocked(newAge);

            // Show notification
            GameEvents.ShowNotification(
                $"🕉️ NEW AGE: {avatar.Title}!\n{avatar.Description}"
            );

            Debug.Log($"[Dashavatar] Advanced to Age {newAge}: {avatar.Name} — {avatar.Title}");

            // Enter ceremony phase briefly
            GameManager.Instance.SetPhase(GamePhase.AvatarCeremony);

            return true;
        }

        /// <summary>
        /// Calculate progress toward the next age (0 to 1).
        /// </summary>
        public float GetAgeProgress()
        {
            if (IsMaxAge) return 1f;
            var state = GameManager.Instance.CurrentState;
            if (state == null) return 0f;

            int nextAge = state.CurrentAvatarAge + 1;
            var nextAvatar = GameConstants.AVATARS[nextAge];

            float popProgress = 0f;
            float buildProgress = 0f;

            if (nextAvatar.RequiredPopulation > 0)
            {
                float pop = state.Resources.Get(ResourceType.Praja);
                popProgress = Mathf.Clamp01(pop / nextAvatar.RequiredPopulation);
            }
            else
            {
                popProgress = 1f;
            }

            if (nextAvatar.RequiredBuildings > 0)
            {
                int buildings = BuildingSystem.Instance?.GetConstructedBuildingCount() ?? 0;
                buildProgress = Mathf.Clamp01((float)buildings / nextAvatar.RequiredBuildings);
            }
            else
            {
                buildProgress = 1f;
            }

            return (popProgress + buildProgress) * 0.5f;
        }

        /// <summary>
        /// Get info about what's needed for the next age.
        /// </summary>
        public string GetNextAgeRequirements()
        {
            if (IsMaxAge) return "You have reached the final age — Kalki!";

            var state = GameManager.Instance.CurrentState;
            int nextAge = state.CurrentAvatarAge + 1;
            var next = GameConstants.AVATARS[nextAge];

            int currentPop = Mathf.FloorToInt(state.Resources.Get(ResourceType.Praja));
            int currentBuildings = BuildingSystem.Instance?.GetConstructedBuildingCount() ?? 0;

            return $"Next Age: {next.Name} — {next.Title}\n" +
                   $"Population: {currentPop}/{next.RequiredPopulation}\n" +
                   $"Buildings: {currentBuildings}/{next.RequiredBuildings}";
        }

        /// <summary>
        /// End the avatar ceremony and return to playing.
        /// Called after the ceremony animation/UI completes.
        /// </summary>
        public void EndCeremony()
        {
            GameManager.Instance.SetPhase(GamePhase.Playing);
        }

        private void Update()
        {
            // Periodically update age progress
            if (GameManager.Instance?.CurrentState != null &&
                GameManager.Instance.CurrentPhase == GamePhase.Playing)
            {
                float progress = GetAgeProgress();
                GameManager.Instance.CurrentState.AgeProgress = progress;
                GameEvents.AgeProgressUpdated(CurrentAge, progress);
            }
        }
    }
}
