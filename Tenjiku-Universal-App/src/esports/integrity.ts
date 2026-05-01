import type { QueueType } from "./config";

export interface RankedProfile {
  playerId: string;
  mmr: number;
  matchesPlayed: number;
  winRate: number;
  avgCombatScore: number;
  preferredInput: "touch" | "controller" | "keyboard_mouse";
}

export interface MatchIntegrityResult {
  allowed: boolean;
  queueType: QueueType;
  reason?: string;
}

export function evaluateQueueIntegrity(
  player: RankedProfile,
  requestedQueue: QueueType
): MatchIntegrityResult {
  if (requestedQueue === "touch_only" && player.preferredInput !== "touch") {
    return {
      allowed: false,
      queueType: requestedQueue,
      reason: "Touch-only queue requires touch primary input.",
    };
  }

  if (requestedQueue === "controller_only" && player.preferredInput === "touch") {
    return {
      allowed: false,
      queueType: requestedQueue,
      reason: "Controller-only queue rejects touch-only profile.",
    };
  }

  return { allowed: true, queueType: requestedQueue };
}

export function estimateSmurfScore(player: RankedProfile): number {
  const baselineSkill = Math.max(1, player.mmr / 25);
  const performanceMultiplier = player.avgCombatScore / 100;
  const velocity = player.matchesPlayed < 30 ? 1.35 : 1;
  return Math.round(baselineSkill * performanceMultiplier * velocity);
}

export function nextMmr(currentMmr: number, didWin: boolean, kFactor: number): number {
  const delta = didWin ? kFactor : -kFactor;
  return Math.max(0, currentMmr + delta);
}
