import { Platform } from "react-native";

export type PlatformTarget = "desktop" | "android" | "ios";
export type DeviceTier = "low" | "mid" | "high";
export type QueueType = "cross_input" | "controller_only" | "touch_only";

export interface PerformanceProfile {
  targetFps: number;
  maxEffectsQuality: "low" | "medium" | "high";
  shadows: boolean;
  postProcessing: boolean;
  maxParticles: number;
}

export interface BuildProfile {
  platform: PlatformTarget;
  tickRateHz: 64 | 128;
  packetBudgetBytes: number;
  performance: Record<DeviceTier, PerformanceProfile>;
}

export const BUILD_PROFILES: Record<PlatformTarget, BuildProfile> = {
  desktop: {
    platform: "desktop",
    tickRateHz: 128,
    packetBudgetBytes: 32768,
    performance: {
      low: { targetFps: 60, maxEffectsQuality: "medium", shadows: false, postProcessing: false, maxParticles: 500 },
      mid: { targetFps: 120, maxEffectsQuality: "high", shadows: true, postProcessing: true, maxParticles: 1200 },
      high: { targetFps: 165, maxEffectsQuality: "high", shadows: true, postProcessing: true, maxParticles: 2400 },
    },
  },
  android: {
    platform: "android",
    tickRateHz: 64,
    packetBudgetBytes: 16384,
    performance: {
      low: { targetFps: 30, maxEffectsQuality: "low", shadows: false, postProcessing: false, maxParticles: 250 },
      mid: { targetFps: 60, maxEffectsQuality: "medium", shadows: false, postProcessing: false, maxParticles: 700 },
      high: { targetFps: 90, maxEffectsQuality: "high", shadows: true, postProcessing: false, maxParticles: 1200 },
    },
  },
  ios: {
    platform: "ios",
    tickRateHz: 64,
    packetBudgetBytes: 16384,
    performance: {
      low: { targetFps: 30, maxEffectsQuality: "low", shadows: false, postProcessing: false, maxParticles: 250 },
      mid: { targetFps: 60, maxEffectsQuality: "medium", shadows: false, postProcessing: false, maxParticles: 700 },
      high: { targetFps: 120, maxEffectsQuality: "high", shadows: true, postProcessing: true, maxParticles: 1400 },
    },
  },
};

export const RANKED_POLICY = {
  defaultQueue: "cross_input" as QueueType,
  placementMatches: 10,
  mmrKFactor: 30,
  smurfSuspicionThreshold: 250,
  maxPartySkillDelta: 400,
};

export const LIVE_OPS_POLICY = {
  balancePatchCadenceDays: 14,
  majorSeasonCadenceDays: 84,
  rankedDecayGraceDays: 28,
  antiCheatReviewWindowHours: 24,
};

export function resolvePlatformTarget(): PlatformTarget {
  if (Platform.OS === "ios") return "ios";
  if (Platform.OS === "android") return "android";
  return "desktop";
}
