const express = require('express');
const http = require('http');
const { Server } = require('socket.io');
const fs = require('fs');
const path = require('path');

const app = express();
const server = http.createServer(app);
const io = new Server(server, {
  cors: {
    origin: "*",
    methods: ["GET", "POST"]
  }
});

const PORT = process.env.PORT || 8080;
const TICK_RATE_HZ = Number(process.env.TICK_RATE_HZ || 64);
const MAX_ACTIONS_PER_SECOND = 25;
const ROOM_CAPACITY = 10;
const ALLOWED_QUEUE_TYPES = new Set(['cross_input', 'controller_only', 'touch_only']);
const PROFILE_DEFAULT_MMR = 1000;
const MATCHMAKING_MAX_GAP = 180;
const STATE_DIR = path.join(__dirname, 'state');
const RANKED_PROFILE_STATE_FILE = path.join(STATE_DIR, 'ranked-profiles.json');
const STATE_FLUSH_INTERVAL_MS = 15000;

const kingdoms = {};
const matches = new Map();
const actionWindow = new Map();
const rankedProfiles = new Map();
const socketProfileIds = new Map();
const matchmakingQueues = {
  cross_input: [],
  controller_only: [],
  touch_only: [],
};
const telemetry = {
  actionRateLimited: 0,
  invalidPayloads: 0,
  integrityViolations: 0,
  totalMatchesCreated: 0,
  queueJoins: 0,
};

function ensureStateDirectory() {
  if (!fs.existsSync(STATE_DIR)) {
    fs.mkdirSync(STATE_DIR, { recursive: true });
  }
}

function profileKeyFromName(displayName) {
  return `profile_${displayName.trim().toLowerCase().replace(/[^a-z0-9_]+/g, '_')}`;
}

function loadRankedProfilesFromDisk() {
  try {
    ensureStateDirectory();
    if (!fs.existsSync(RANKED_PROFILE_STATE_FILE)) return;
    const raw = fs.readFileSync(RANKED_PROFILE_STATE_FILE, 'utf8');
    if (!raw.trim()) return;
    const parsed = JSON.parse(raw);
    if (!Array.isArray(parsed)) return;
    for (const profile of parsed) {
      if (!profile || typeof profile.playerId !== 'string') continue;
      rankedProfiles.set(profile.playerId, profile);
    }
    console.log(`📦 Loaded ${rankedProfiles.size} ranked profiles from disk.`);
  } catch (error) {
    console.error('Failed to load ranked profiles state:', error.message);
  }
}

function persistRankedProfilesToDisk() {
  try {
    ensureStateDirectory();
    const serialized = JSON.stringify(Array.from(rankedProfiles.values()), null, 2);
    const tempPath = `${RANKED_PROFILE_STATE_FILE}.tmp`;
    fs.writeFileSync(tempPath, serialized, 'utf8');
    fs.renameSync(tempPath, RANKED_PROFILE_STATE_FILE);
  } catch (error) {
    console.error('Failed to persist ranked profiles state:', error.message);
  }
}

function createMatchState(matchId) {
  return {
    matchId,
    phase: 'warmup',
    startedAt: Date.now(),
    players: new Map(),
    spectators: new Set(),
    scoreboard: {},
    replayBuffer: [],
  };
}

function validateActionPayload(payload) {
  if (!payload || typeof payload !== 'object') return false;
  if (typeof payload.actionType !== 'string' || payload.actionType.length > 64) return false;
  if (typeof payload.clientTick !== 'number' || payload.clientTick < 0) return false;
  return true;
}

function actionAllowed(socketId) {
  const now = Date.now();
  const windowStart = now - 1000;
  const actions = (actionWindow.get(socketId) || []).filter((ts) => ts > windowStart);
  if (actions.length >= MAX_ACTIONS_PER_SECOND) {
    actionWindow.set(socketId, actions);
    return false;
  }
  actions.push(now);
  actionWindow.set(socketId, actions);
  return true;
}

function appendReplayFrame(matchState, frame) {
  matchState.replayBuffer.push(frame);
  if (matchState.replayBuffer.length > TICK_RATE_HZ * 20) {
    matchState.replayBuffer.shift();
  }
}

function sanitizeString(value, fallback, maxLength = 48) {
  if (typeof value !== 'string') return fallback;
  const trimmed = value.trim();
  if (!trimmed) return fallback;
  return trimmed.slice(0, maxLength);
}

function ensureRankedProfile(playerId, displayName = 'Unknown') {
  if (!rankedProfiles.has(playerId)) {
    rankedProfiles.set(playerId, {
      playerId,
      displayName,
      mmr: PROFILE_DEFAULT_MMR,
      wins: 0,
      losses: 0,
      matchesPlayed: 0,
      smurfScore: 0,
      antiCheatFlags: 0,
      lastUpdatedAt: Date.now(),
    });
    persistRankedProfilesToDisk();
  }
  return rankedProfiles.get(playerId);
}

function calculateSmurfScore(profile, combatScore) {
  const winRate = profile.matchesPlayed > 0 ? profile.wins / profile.matchesPlayed : 0;
  return Math.round((profile.mmr / 25) * (1 + winRate) * Math.max(1, combatScore / 100));
}

function updateMmr(profile, didWin) {
  const kFactor = 30;
  profile.mmr = Math.max(0, profile.mmr + (didWin ? kFactor : -kFactor));
  if (didWin) profile.wins += 1;
  else profile.losses += 1;
  profile.matchesPlayed += 1;
  profile.lastUpdatedAt = Date.now();
  persistRankedProfilesToDisk();
}

function tryMatchmaking(queueType) {
  const queue = matchmakingQueues[queueType];
  queue.sort((a, b) => a.mmr - b.mmr);
  if (queue.length < 2) return null;

  for (let i = 0; i < queue.length - 1; i += 1) {
    const a = queue[i];
    const b = queue[i + 1];
    if (Math.abs(a.mmr - b.mmr) <= MATCHMAKING_MAX_GAP) {
      queue.splice(i, 2);
      const lobbyId = `ranked_${Math.random().toString(36).slice(2, 8)}`;
      const match = createMatchState(lobbyId);
      match.queueType = queueType;
      match.isRanked = true;
      matches.set(lobbyId, match);
      telemetry.totalMatchesCreated += 1;
      return { lobbyId, players: [a, b] };
    }
  }
  return null;
}

app.get('/health', (_req, res) => {
  res.status(200).json({
    ok: true,
    tickRateHz: TICK_RATE_HZ,
    activeMatches: matches.size,
    connectedKingdoms: Object.keys(kingdoms).length,
    queueDepth: {
      cross_input: matchmakingQueues.cross_input.length,
      controller_only: matchmakingQueues.controller_only.length,
      touch_only: matchmakingQueues.touch_only.length,
    },
    telemetry,
    timestamp: Date.now(),
  });
});

loadRankedProfilesFromDisk();
setInterval(persistRankedProfilesToDisk, STATE_FLUSH_INTERVAL_MS).unref();

io.on('connection', (socket) => {
  console.log('👑 A High King has connected:', socket.id);

  // Join the Dharma Grid
  socket.on('join_kingdom', (data) => {
    kingdoms[socket.id] = data;
    const safeKingdomName = sanitizeString(data?.kingdomName, 'UnknownKingdom');
    const safePlayerName = sanitizeString(data?.playerName, 'UnknownPlayer');
    const profileId = profileKeyFromName(safePlayerName);
    socketProfileIds.set(socket.id, profileId);
    ensureRankedProfile(profileId, safePlayerName);
    console.log(`🏰 ${safeKingdomName} led by ${safePlayerName} has joined the server.`);
    io.emit('kingdom_list_update', Object.values(kingdoms));
  });

  socket.on('join_matchmaking_queue', ({ queueType = 'cross_input', mmr = 1000 } = {}) => {
    const normalizedQueueType = ALLOWED_QUEUE_TYPES.has(queueType) ? queueType : 'cross_input';
    const normalizedMmr = Number.isFinite(mmr) ? Math.max(0, Math.min(5000, Number(mmr))) : 1000;
    telemetry.queueJoins += 1;
    socket.data.queueType = normalizedQueueType;
    socket.data.mmr = normalizedMmr;
    const profileId = socketProfileIds.get(socket.id) || profileKeyFromName(`guest_${socket.id}`);
    socketProfileIds.set(socket.id, profileId);
    const profile = ensureRankedProfile(profileId);
    profile.mmr = normalizedMmr;
    profile.lastUpdatedAt = Date.now();
    persistRankedProfilesToDisk();
    matchmakingQueues[normalizedQueueType].push({
      socketId: socket.id,
      mmr: normalizedMmr,
      joinedAt: Date.now(),
    });

    const matched = tryMatchmaking(normalizedQueueType);
    if (matched) {
      matched.players.forEach((playerTicket) => {
        const playerSocket = io.sockets.sockets.get(playerTicket.socketId);
        if (!playerSocket) return;
        playerSocket.join(matched.lobbyId);
        const match = matches.get(matched.lobbyId);
        if (match) {
          match.players.set(playerTicket.socketId, { kills: 0, deaths: 0, assists: 0, connectedAt: Date.now() });
        }
        playerSocket.emit('ranked_match_found', {
          lobbyId: matched.lobbyId,
          queueType: normalizedQueueType,
          tickRateHz: TICK_RATE_HZ,
        });
      });
    }

    socket.emit('queue_joined', {
      queueType: normalizedQueueType,
      estimatedWaitSeconds: 15,
    });
  });

  socket.on('create_tournament_lobby', ({ tournamentCode, mapName, matchTitle }) => {
    const lobbyId = `lobby_${Math.random().toString(36).slice(2, 8)}`;
    const match = createMatchState(lobbyId);
    match.mapName = sanitizeString(mapName, 'AryavartaPrime');
    match.matchTitle = sanitizeString(matchTitle, 'Tournament Match', 64);
    match.tournamentCode = sanitizeString(tournamentCode, 'OPEN', 24);
    matches.set(lobbyId, match);
    socket.join(lobbyId);
    match.players.set(socket.id, { kills: 0, deaths: 0, assists: 0, connectedAt: Date.now() });
    socket.emit('tournament_lobby_created', { lobbyId, tournamentCode: match.tournamentCode });
  });

  socket.on('join_tournament_lobby', ({ lobbyId, role = 'player' }) => {
    if (typeof lobbyId !== 'string' || !lobbyId.trim()) {
      socket.emit('error_notice', { code: 'INVALID_LOBBY_ID' });
      return;
    }
    const match = matches.get(lobbyId);
    if (!match) {
      socket.emit('error_notice', { code: 'LOBBY_NOT_FOUND' });
      return;
    }

    if (role === 'spectator') {
      socket.join(lobbyId);
      match.spectators.add(socket.id);
      socket.emit('spectator_ready', { lobbyId, replayFrames: match.replayBuffer.slice(-TICK_RATE_HZ * 10) });
      return;
    }

    if (match.players.size >= ROOM_CAPACITY) {
      socket.emit('error_notice', { code: 'LOBBY_FULL' });
      return;
    }

    socket.join(lobbyId);
    match.players.set(socket.id, { kills: 0, deaths: 0, assists: 0, connectedAt: Date.now() });
    io.to(lobbyId).emit('lobby_state', {
      lobbyId,
      players: Array.from(match.players.keys()),
      spectators: Array.from(match.spectators.values()),
      mapName: match.mapName,
      matchTitle: match.matchTitle,
    });
  });

  // Handle Battle Actions (Astras)
  socket.on('unleash_astra', (astraData) => {
    console.log(`⚔️ ${astraData.playerName} unleashed ${astraData.astraName}!`);
    socket.broadcast.emit('astra_strike', astraData);
  });

  socket.on('authoritative_action', ({ lobbyId, payload }) => {
    const match = matches.get(lobbyId);
    if (!match) return;
    if (!match.players.has(socket.id)) {
      telemetry.integrityViolations += 1;
      socket.emit('integrity_violation', { reason: 'UNAUTHORIZED_PLAYER' });
      return;
    }
    if (!actionAllowed(socket.id)) {
      telemetry.actionRateLimited += 1;
      telemetry.integrityViolations += 1;
      socket.emit('integrity_violation', { reason: 'ACTION_RATE_LIMIT' });
      return;
    }
    if (!validateActionPayload(payload)) {
      telemetry.invalidPayloads += 1;
      telemetry.integrityViolations += 1;
      socket.emit('integrity_violation', { reason: 'INVALID_ACTION_PAYLOAD' });
      return;
    }

    const serverFrame = {
      playerId: socket.id,
      actionType: payload.actionType,
      serverTick: Math.floor((Date.now() - match.startedAt) / (1000 / TICK_RATE_HZ)),
      acceptedAt: Date.now(),
    };
    appendReplayFrame(match, serverFrame);
    io.to(lobbyId).emit('authoritative_update', serverFrame);
  });

  socket.on('submit_ranked_match_result', ({ lobbyId, didWin, combatScore = 100 }) => {
    const match = matches.get(lobbyId);
    if (!match || !match.isRanked || !match.players.has(socket.id)) return;
    const profileId = socketProfileIds.get(socket.id) || profileKeyFromName(`guest_${socket.id}`);
    const profile = ensureRankedProfile(profileId);
    updateMmr(profile, Boolean(didWin));
    profile.smurfScore = calculateSmurfScore(profile, Number(combatScore) || 100);
    if (profile.smurfScore > 250) profile.antiCheatFlags += 1;
    persistRankedProfilesToDisk();
    socket.emit('ranked_profile_update', profile);
  });

  socket.on('get_ranked_profile', () => {
    const profileId = socketProfileIds.get(socket.id) || profileKeyFromName(`guest_${socket.id}`);
    const profile = ensureRankedProfile(profileId);
    socket.emit('ranked_profile', profile);
  });

  socket.on('get_liveops_balance_signals', () => {
    socket.emit('liveops_balance_signals', [
      { entityId: 'Astra_Matsya', winRate: 55.2, pickRate: 31.7, action: 'nerf' },
      { entityId: 'Astra_Gandiva', winRate: 46.4, pickRate: 9.1, action: 'buff' },
      { entityId: 'Astra_Pashupatastra', winRate: 50.1, pickRate: 17.4, action: 'hold' },
    ]);
  });

  socket.on('request_replay_clip', ({ lobbyId, seconds = 10 }) => {
    const match = matches.get(lobbyId);
    if (!match) return;
    const frameCount = Math.max(1, Math.min(seconds, 20)) * TICK_RATE_HZ;
    socket.emit('replay_clip', {
      lobbyId,
      frames: match.replayBuffer.slice(-frameCount),
    });
  });

  socket.on('disconnect', () => {
    console.log('🏳️ A King has left the battlefield.');
    delete kingdoms[socket.id];
    socketProfileIds.delete(socket.id);
    for (const queueType of Object.keys(matchmakingQueues)) {
      matchmakingQueues[queueType] = matchmakingQueues[queueType].filter((entry) => entry.socketId !== socket.id);
    }
    actionWindow.delete(socket.id);
    for (const [lobbyId, match] of matches.entries()) {
      const playerLeft = match.players.delete(socket.id);
      const spectatorLeft = match.spectators.delete(socket.id);
      if (playerLeft || spectatorLeft) {
        io.to(lobbyId).emit('lobby_state', {
          lobbyId,
          players: Array.from(match.players.keys()),
          spectators: Array.from(match.spectators.values()),
          mapName: match.mapName,
          matchTitle: match.matchTitle,
        });
      }
      if (match.players.size === 0 && match.spectators.size === 0) {
        matches.delete(lobbyId);
      }
    }
    io.emit('kingdom_list_update', Object.values(kingdoms));
  });
});

server.listen(PORT, () => {
  console.log(`\n=========================================`);
  console.log(`🕉️  TENJIKU DEVA YUDDHA: MULTIPLAYER HUB`);
  console.log(`📍 LAN Server running on port: ${PORT}`);
  console.log(`⚙️  Authoritative tick-rate: ${TICK_RATE_HZ}Hz`);
  console.log(`📡 Connect your kingdoms to this IPv4 address.`);
  console.log(`=========================================\n`);
});

function shutdown(signal) {
  console.log(`\nReceived ${signal}. Shutting down multiplayer hub safely...`);
  persistRankedProfilesToDisk();
  io.close(() => {
    server.close(() => {
      process.exit(0);
    });
  });
}

process.on('SIGINT', () => shutdown('SIGINT'));
process.on('SIGTERM', () => shutdown('SIGTERM'));
