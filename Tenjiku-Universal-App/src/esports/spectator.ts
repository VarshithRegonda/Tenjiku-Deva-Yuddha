export interface SpectatorSettings {
  freeCam: boolean;
  showOutlines: boolean;
  instantReplaySeconds: number;
  tacticalMinimap: boolean;
}

export interface TournamentLobby {
  lobbyId: string;
  tournamentCode: string;
  matchTitle: string;
  observers: string[];
  playerSlots: number;
  mapName: string;
  startedAt: number | null;
}

const replayBookmarks: { matchId: string; timestampMs: number; label: string }[] = [];

export function defaultSpectatorSettings(): SpectatorSettings {
  return {
    freeCam: true,
    showOutlines: true,
    instantReplaySeconds: 12,
    tacticalMinimap: true,
  };
}

export function addReplayBookmark(matchId: string, label: string): void {
  replayBookmarks.unshift({ matchId, timestampMs: Date.now(), label });
  replayBookmarks.splice(30);
}

export function getReplayBookmarks(matchId: string) {
  return replayBookmarks.filter((bookmark) => bookmark.matchId === matchId);
}
