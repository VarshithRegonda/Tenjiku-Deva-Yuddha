using System.Collections.Generic;
using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Manages the Navaratna (9 Ministers), Praja (Population) Roles, and Dharmaniti (Laws).
    /// </summary>
    public class GovernanceSystem : MonoBehaviour
    {
        public static GovernanceSystem Instance { get; private set; }

        private GovernanceState State => GameManager.Instance?.CurrentState?.Governance;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentPhase != GamePhase.Playing) return;
            if (State == null) return;

            // Optional: Periodically calculate happiness/loyalty drifts
        }

        // ─────────────────────────────────────────────
        //  Navaratna — The 9 Ministers
        // ─────────────────────────────────────────────
        
        public void AppointMinister(MinisterRole role, string ministerName)
        {
            if (State == null) return;

            switch (role)
            {
                case MinisterRole.Purohita: State.Purohita = ministerName; break;
                case MinisterRole.Senapati: State.Senapati = ministerName; break;
                case MinisterRole.Amatya: State.Amatya = ministerName; break;
                case MinisterRole.Nyayadhish: State.Nyayadhish = ministerName; break;
                case MinisterRole.Mantri: State.Mantri = ministerName; break;
                case MinisterRole.Sthapati: State.Sthapati = ministerName; break;
                case MinisterRole.Vaidyaraj: State.Vaidyaraj = ministerName; break;
                case MinisterRole.Senani: State.Senani = ministerName; break;
                case MinisterRole.Pradhan: State.Pradhan = ministerName; break;
            }

            GameEvents.ShowNotification($"📜 {ministerName} appointed as {role}!");
        }

        public void DismissMinister(MinisterRole role)
        {
            AppointMinister(role, "");
        }

        public bool IsMinisterAppointed(MinisterRole role)
        {
            if (State == null) return false;
            return role switch
            {
                MinisterRole.Purohita => !string.IsNullOrEmpty(State.Purohita),
                MinisterRole.Senapati => !string.IsNullOrEmpty(State.Senapati),
                MinisterRole.Amatya => !string.IsNullOrEmpty(State.Amatya),
                MinisterRole.Nyayadhish => !string.IsNullOrEmpty(State.Nyayadhish),
                MinisterRole.Mantri => !string.IsNullOrEmpty(State.Mantri),
                MinisterRole.Sthapati => !string.IsNullOrEmpty(State.Sthapati),
                MinisterRole.Vaidyaraj => !string.IsNullOrEmpty(State.Vaidyaraj),
                MinisterRole.Senani => !string.IsNullOrEmpty(State.Senani),
                MinisterRole.Pradhan => !string.IsNullOrEmpty(State.Pradhan),
                _ => false
            };
        }

        // ─────────────────────────────────────────────
        //  Praja — Population Roles
        // ─────────────────────────────────────────────

        public void SetPrajaAllocation(float krishak, float shilpi, float sainik, float vidvan)
        {
            if (State == null) return;
            
            // Normalize to ensure sum is 1.0 (100%)
            float total = krishak + shilpi + sainik + vidvan;
            if (total > 0)
            {
                State.KrishakAllocation = krishak / total;
                State.ShilpiAllocation = shilpi / total;
                State.SainikAllocation = sainik / total;
                State.VidvanAllocation = vidvan / total;
            }
        }

        // Production Multipliers based on Praja Allocation
        public float GetKrishakMultiplier() => 0.5f + (State != null ? State.KrishakAllocation * 2f : 0f); // Affects Food
        public float GetShilpiMultiplier() => 0.5f + (State != null ? State.ShilpiAllocation * 2f : 0f);   // Affects Stone/Wood/Iron
        public float GetSainikMultiplier() => 0.5f + (State != null ? State.SainikAllocation * 2f : 0f);   // Affects Defense/Training Speed
        public float GetVidvanMultiplier() => 0.5f + (State != null ? State.VidvanAllocation * 2f : 0f);   // Affects Vidya/Shakti

        // ─────────────────────────────────────────────
        //  Dharmaniti — Rules & Laws
        // ─────────────────────────────────────────────

        public void EnactLaw(string lawId)
        {
            if (State == null) return;
            var law = GetLawInfo(lawId);
            
            if (State.ActiveLaws.Contains(lawId))
            {
                GameEvents.ShowNotification($"⚠️ {law.Name} is already enacted.");
                return;
            }

            // Check prerequisites
            if (!IsLawUnlocked(lawId))
            {
                GameEvents.ShowNotification($"🔒 Cannot enact {law.Name}. Requires {law.Prerequisite}.");
                return;
            }

            State.ActiveLaws.Add(lawId);
            GameEvents.ShowNotification($"⚖️ Law Enacted: {law.Name}");
        }

        public void RevokeLaw(string lawId)
        {
            if (State == null) return;
            if (State.ActiveLaws.Remove(lawId))
            {
                var law = GetLawInfo(lawId);
                GameEvents.ShowNotification($"⚖️ Law Revoked: {law.Name}");
            }
        }

        public bool IsLawActive(string lawId)
        {
            if (State == null) return false;
            return State.ActiveLaws.Contains(lawId);
        }

        public bool IsLawUnlocked(string lawId)
        {
            var law = GetLawInfo(lawId);
            if (law == null) return false;

            if (law.Source == LawSource.Basic) return true;

            var vedaState = GameManager.Instance?.CurrentState?.VedaProgress;
            if (vedaState == null) return false;

            // Simplified prerequisite checks based on total Veda research level for now
            if (law.Source == LawSource.Vedas && vedaState.GetTotalResearchLevel() >= 10) return true;
            if (law.Source == LawSource.Upanishads && vedaState.GetTotalResearchLevel() >= 20) return true;
            if (law.Source == LawSource.BhagavadGita && vedaState.GetTotalResearchLevel() >= 30) return true;
            if (law.Source == LawSource.SkandaPurana && vedaState.GetTotalResearchLevel() >= 15) return true;

            return false;
        }

        public LawInfo GetLawInfo(string lawId)
        {
            foreach (var law in ALL_LAWS)
            {
                if (law.Id == lawId) return law;
            }
            return null;
        }

        // ─────────────────────────────────────────────
        //  Law Definitions
        // ─────────────────────────────────────────────

        public static readonly List<LawInfo> ALL_LAWS = new List<LawInfo>
        {
            // --- BASIC LAWS ---
            new LawInfo { Id = "Law_FairTax", Name = "Nyayakar (Fair Taxation)", Source = LawSource.Basic, Description = "Increases Gold income securely. Small drop to Happiness.", Prerequisite = "None" },
            new LawInfo { Id = "Law_Conscription", Name = "Aanivarya Sena (Conscription)", Source = LawSource.Basic, Description = "Doubles soldier training speed. Major drop to Happiness.", Prerequisite = "None" },
            
            // --- VEDAS ---
            new LawInfo { Id = "Law_Yajna", Name = "Maha Yajna (Great Sacrifice)", Source = LawSource.Vedas, Description = "Rituals invoke cosmic balance. Constant drain on Food and Gold, massive Shakti boost.", Prerequisite = "Vedas Lv.10" },
            new LawInfo { Id = "Law_VedicEcology", Name = "Bhumi Pujan (Earth Veneration)", Source = LawSource.Vedas, Description = "Honoring the five elements. Significantly boosts Agriculture.", Prerequisite = "Vedas Lv.10" },

            // --- UPANISHADS ---
            new LawInfo { Id = "Law_Atman", Name = "Atman Bodh (Self-Realization)", Source = LawSource.Upanishads, Description = "Focus on inner knowledge. Massive boost to Vidya generation, slow population growth.", Prerequisite = "Upanishads (Vedas Lv.20)" },
            new LawInfo { Id = "Law_Ahimsa", Name = "Ahimsa (Non-Violence)", Source = LawSource.Upanishads, Description = "Extreme peace. Battles cannot be initiated. Max Happiness and Loyalty.", Prerequisite = "Upanishads (Vedas Lv.20)" },

            // --- BHAGAVAD GITA ---
            new LawInfo { Id = "Law_NishkamaKarma", Name = "Nishkama Karma (Detached Action)", Source = LawSource.BhagavadGita, Description = "Action without attachment to results. Warriors never rout, massive morale boost.", Prerequisite = "Gita (Vedas Lv.30)" },
            new LawInfo { Id = "Law_DharmaSansthapana", Name = "Dharma Sansthapana (Establishing Righteousness)", Source = LawSource.BhagavadGita, Description = "Supreme societal balance. Reduces corruption and boosts all production 25%.", Prerequisite = "Gita (Vedas Lv.30)" },

            // --- SKANDA PURANA ---
            new LawInfo { Id = "Law_TirthaYatra", Name = "Tirtha Yatra (Pilgrimage)", Source = LawSource.SkandaPurana, Description = "Encourages sacred travel to Tirthas. Boosts Suvarna (from tourism) and Shakti.", Prerequisite = "Puranas (Vedas Lv.15)" },
            new LawInfo { Id = "Law_TempleFestivals", Name = "Utsava (Temple Festivals)", Source = LawSource.SkandaPurana, Description = "Grand temple festivals based on Puranic lore. High Gold cost, Max Happiness.", Prerequisite = "Puranas (Vedas Lv.15)" }
        };
    }

    public enum MinisterRole
    {
        Purohita,   // Chief Priest
        Senapati,   // Commander
        Amatya,     // Finance
        Nyayadhish, // Justice
        Mantri,     // Diplomat
        Sthapati,   // Architect
        Vaidyaraj,  // Physician
        Senani,     // Spymaster
        Pradhan     // Prime Minister
    }

    public enum LawSource
    {
        Basic,
        Vedas,
        Upanishads,
        BhagavadGita,
        SkandaPurana
    }

    public class LawInfo
    {
        public string Id;
        public string Name;
        public LawSource Source;
        public string Description;
        public string Prerequisite;
    }
}
