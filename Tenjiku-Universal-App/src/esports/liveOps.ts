export interface BalanceSignal {
  entityId: string;
  winRate: number;
  pickRate: number;
  banRate: number;
}

export interface BalanceRecommendation {
  entityId: string;
  action: "buff" | "nerf" | "hold";
  reason: string;
}

export function generateBalanceRecommendations(
  signals: BalanceSignal[]
): BalanceRecommendation[] {
  return signals.map((signal) => {
    if (signal.winRate > 54 || signal.banRate > 35) {
      return {
        entityId: signal.entityId,
        action: "nerf",
        reason: "High win/ban pressure indicates dominant meta impact.",
      };
    }
    if (signal.winRate < 47 && signal.pickRate < 12) {
      return {
        entityId: signal.entityId,
        action: "buff",
        reason: "Underperforming and underpicked in current meta.",
      };
    }
    return {
      entityId: signal.entityId,
      action: "hold",
      reason: "Within healthy competitive thresholds.",
    };
  });
}
