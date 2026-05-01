import { Platform } from "react-native";
import type { PlatformTarget } from "./config";

export interface CrossProgressionProfile {
  playerId: string;
  displayName: string;
  linkedPlatforms: PlatformTarget[];
  lastLoginPlatform: PlatformTarget;
  seasonLevel: number;
  seasonXp: number;
  updatedAt: number;
}

const inMemoryProfiles = new Map<string, CrossProgressionProfile>();

function platformToTarget(): PlatformTarget {
  if (Platform.OS === "android") return "android";
  if (Platform.OS === "ios") return "ios";
  return "desktop";
}

export function createOrGetProfile(playerId: string, displayName: string): CrossProgressionProfile {
  const existing = inMemoryProfiles.get(playerId);
  if (existing) return existing;

  const currentPlatform = platformToTarget();
  const profile: CrossProgressionProfile = {
    playerId,
    displayName,
    linkedPlatforms: [currentPlatform],
    lastLoginPlatform: currentPlatform,
    seasonLevel: 1,
    seasonXp: 0,
    updatedAt: Date.now(),
  };
  inMemoryProfiles.set(playerId, profile);
  return profile;
}

export function linkPlatform(playerId: string, platform: PlatformTarget): CrossProgressionProfile | null {
  const profile = inMemoryProfiles.get(playerId);
  if (!profile) return null;
  if (!profile.linkedPlatforms.includes(platform)) profile.linkedPlatforms.push(platform);
  profile.lastLoginPlatform = platform;
  profile.updatedAt = Date.now();
  return profile;
}

export function grantSeasonXp(playerId: string, amount: number): CrossProgressionProfile | null {
  const profile = inMemoryProfiles.get(playerId);
  if (!profile) return null;

  profile.seasonXp += Math.max(0, amount);
  while (profile.seasonXp >= 1000) {
    profile.seasonXp -= 1000;
    profile.seasonLevel += 1;
  }
  profile.updatedAt = Date.now();
  return profile;
}
