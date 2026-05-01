import type { QueueType } from "./config";

export interface RankedTicket {
  playerId: string;
  mmr: number;
  queueType: QueueType;
  enqueuedAt: number;
}

export interface MatchPair {
  queueType: QueueType;
  players: [RankedTicket, RankedTicket];
  matchedAt: number;
}

export function buildRankedMatches(
  tickets: RankedTicket[],
  maxSkillGap = 150
): { matches: MatchPair[]; remaining: RankedTicket[] } {
  const sorted = [...tickets].sort((a, b) => a.mmr - b.mmr);
  const used = new Set<string>();
  const matches: MatchPair[] = [];

  for (let i = 0; i < sorted.length; i += 1) {
    const a = sorted[i];
    if (used.has(a.playerId)) continue;

    for (let j = i + 1; j < sorted.length; j += 1) {
      const b = sorted[j];
      if (used.has(b.playerId)) continue;
      if (a.queueType !== b.queueType) continue;
      if (Math.abs(a.mmr - b.mmr) > maxSkillGap) continue;

      matches.push({
        queueType: a.queueType,
        players: [a, b],
        matchedAt: Date.now(),
      });
      used.add(a.playerId);
      used.add(b.playerId);
      break;
    }
  }

  const remaining = sorted.filter((ticket) => !used.has(ticket.playerId));
  return { matches, remaining };
}
