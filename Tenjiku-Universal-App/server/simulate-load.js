const { io } = require("socket.io-client");

const SERVER_URL = process.env.SERVER_URL || "http://localhost:8080";
const CLIENTS = Number(process.env.LOAD_CLIENTS || 20);
const TEST_DURATION_MS = Number(process.env.LOAD_DURATION_MS || 12000);
const queues = ["cross_input", "controller_only", "touch_only"];

const sockets = [];
let queueJoined = 0;
let matchesFound = 0;
let integrityViolations = 0;

function connectClient(index) {
  const socket = io(SERVER_URL, {
    transports: ["websocket"],
    timeout: 5000,
  });

  socket.on("connect", () => {
    socket.emit("join_kingdom", {
      kingdomName: `LOAD_KINGDOM_${index}`,
      playerName: `LOAD_PLAYER_${index}`,
    });
    socket.emit("join_matchmaking_queue", {
      queueType: queues[index % queues.length],
      mmr: 900 + (index % 8) * 40,
    });
  });

  socket.on("queue_joined", () => {
    queueJoined += 1;
  });

  socket.on("ranked_match_found", ({ lobbyId }) => {
    matchesFound += 1;
    socket.emit("authoritative_action", {
      lobbyId,
      payload: { actionType: "LOADTEST_MOVE", clientTick: Date.now() % 100000 },
    });
    socket.emit("submit_ranked_match_result", {
      lobbyId,
      didWin: index % 2 === 0,
      combatScore: 110 + (index % 5) * 10,
    });
  });

  socket.on("integrity_violation", () => {
    integrityViolations += 1;
  });

  sockets.push(socket);
}

for (let i = 0; i < CLIENTS; i += 1) {
  connectClient(i);
}

setTimeout(() => {
  sockets.forEach((socket) => socket.disconnect());
  console.log("=======================================");
  console.log("Esports Server Load Simulation Summary");
  console.log(`Server: ${SERVER_URL}`);
  console.log(`Clients: ${CLIENTS}`);
  console.log(`Queue joins acknowledged: ${queueJoined}`);
  console.log(`Ranked matches found events: ${matchesFound}`);
  console.log(`Integrity violations: ${integrityViolations}`);
  console.log("=======================================");
  process.exit(0);
}, TEST_DURATION_MS);
