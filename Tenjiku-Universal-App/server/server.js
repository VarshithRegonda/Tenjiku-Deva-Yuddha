const express = require('express');
const http = require('http');
const { Server } = require('socket.io');

const app = express();
const server = http.createServer(app);
const io = new Server(server, {
  cors: {
    origin: "*",
    methods: ["GET", "POST"]
  }
});

const PORT = process.env.PORT || 8080;

// Kingdoms state storage (Backup Server)
let kingdoms = {};

io.on('connection', (socket) => {
  console.log('👑 A High King has connected:', socket.id);

  // Join the Dharma Grid
  socket.on('join_kingdom', (data) => {
    kingdoms[socket.id] = data;
    console.log(`🏰 ${data.kingdomName} led by ${data.playerName} has joined the server.`);
    io.emit('kingdom_list_update', Object.values(kingdoms));
  });

  // Handle Battle Actions (Astras)
  socket.on('unleash_astra', (astraData) => {
    console.log(`⚔️ ${astraData.playerName} unleashed ${astraData.astraName}!`);
    socket.broadcast.emit('astra_strike', astraData);
  });

  socket.on('disconnect', () => {
    console.log('🏳️ A King has left the battlefield.');
    delete kingdoms[socket.id];
    io.emit('kingdom_list_update', Object.values(kingdoms));
  });
});

server.listen(PORT, () => {
  console.log(`\n=========================================`);
  console.log(`🕉️  TENJIKU DEVA YUDDHA: MULTIPLAYER HUB`);
  console.log(`📍 LAN Server running on port: ${PORT}`);
  console.log(`📡 Connect your kingdoms to this IPv4 address.`);
  console.log(`=========================================\n`);
});
