export interface ProgressionTier {
  stage: string;
  title: string;
  unlock: string;
}

export interface LoreEntry {
  title: string;
  focus: string;
  gameplayValue: string;
}

export const SOCIAL_PROGRESSION_PATH: ProgressionTier[] = [
  { stage: "I", title: "Commoner", unlock: "City basics, farming, and survival economy." },
  { stage: "II", title: "Warrior", unlock: "Combat drills, battalion command, and defense bonuses." },
  { stage: "III", title: "Minister", unlock: "Policy choices, resource efficiency, and petitions." },
  { stage: "IV", title: "King", unlock: "Kingdom governance, alliances, and regional war planning." },
  { stage: "V", title: "Deva", unlock: "Divine strategy passives and advanced dharma missions." },
  { stage: "VI", title: "Cosmic Architect", unlock: "Endgame creation tools for developers and lore stewards." },
];

export const FOUR_VEDAS_KNOWLEDGE: LoreEntry[] = [
  { title: "Rig Veda", focus: "Hymns and cosmic order", gameplayValue: "Unlocks battle chants and morale boosts." },
  { title: "Yajur Veda", focus: "Ritual action and discipline", gameplayValue: "Unlocks tactical formation and timing buffs." },
  { title: "Sama Veda", focus: "Melody, rhythm, and resonance", gameplayValue: "Unlocks focus recovery and team aura effects." },
  { title: "Atharva Veda", focus: "Applied wisdom and healing", gameplayValue: "Unlocks resilience, warding, and recovery tech." },
];

export const EPIC_STORY_ARCS: LoreEntry[] = [
  { title: "Mahabharata: Dharma Yuddha", focus: "Duty, choice, and consequence", gameplayValue: "Narrative campaigns with branching outcomes." },
  { title: "Bhagavad Gita Dialogues", focus: "Inner clarity before conflict", gameplayValue: "Meditation decisions grant situational combat traits." },
];

export const RUDRA_FORMS: LoreEntry[] = [
  { title: "Rudra the Storm", focus: "Fierce transformation", gameplayValue: "High-risk offense stance and fear suppression." },
  { title: "Shiva the Yogi", focus: "Stillness and mastery", gameplayValue: "Focus regen, cooldown reduction, and mental resistance." },
  { title: "Nataraja", focus: "Cosmic dance of creation", gameplayValue: "Tempo-based buffs for coordinated team windows." },
  { title: "Ardhanarishvara", focus: "Balance of dual energies", gameplayValue: "Hybrid support-offense specialization." },
];

export const RUDRA_FAMILY_STORY: LoreEntry[] = [
  { title: "Parvati", focus: "Power, devotion, and compassion", gameplayValue: "Protective blessings and resilience traits." },
  { title: "Ganesha", focus: "Wisdom and obstacle removal", gameplayValue: "Unlock pathing and strategic utility options." },
  { title: "Kartikeya", focus: "War command and courage", gameplayValue: "Advanced commander perks and strike formations." },
  { title: "Nandi", focus: "Loyalty and guardianship", gameplayValue: "Defensive fortification and sentinel systems." },
];

export const WORLD_UNITY_ARCS: LoreEntry[] = [
  {
    title: "Ancient Bharat Foundation",
    focus: "Dharma, knowledge, craft, and civic order",
    gameplayValue: "Unlocks civilization technologies, gurukul schools, and minister councils.",
  },
  {
    title: "Sindhu to Samudra Network",
    focus: "Trade routes and cultural exchange across regions",
    gameplayValue: "Unlocks world map diplomacy, commerce lanes, and alliance treaties.",
  },
  {
    title: "Vasudhaiva Kutumbakam Campaign",
    focus: "One world family and shared prosperity",
    gameplayValue: "Unlocks global peace missions and co-op kingdom objectives.",
  },
  {
    title: "Final Era: Harmony of All Beings",
    focus: "Peace, balance, and spiritual unity",
    gameplayValue: "Endgame victory condition based on harmony, not conquest.",
  },
];
