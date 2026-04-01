const express = require("express");
const http = require("http");
const { Server } = require("socket.io");
const { YSocketIO } = require("y-socket.io/dist/server");
const cors = require("cors");

const app = express();
app.use(cors());

app.get("/collaboration/health", (req, res) => {
  res.status(200).json({ status: "healthy", service: "collaboration.api" });
});

const server = http.createServer(app);
const io = new Server(server, {
  cors: {
    origin: "*",
    methods: ["GET", "POST"],
  },
  path: "/collaboration/socket.io",
});

const ysocketio = new YSocketIO(io, {
  gcEnabled: true,
  levelPersistenceDir: "./.yjs-storage",
});
ysocketio.initialize();

const roomUsers = {};

io.on("connection", (socket) => {
  socket.on("join-session", ({ roomId, userId }) => {
    socket.join(roomId);
    socket.roomId = roomId;
    socket.userId = userId;

    if (!roomUsers[roomId]) {
      roomUsers[roomId] = [];
    }

    if (!roomUsers[roomId].includes(userId)) {
      roomUsers[roomId].push(userId);
    }

    socket.emit("room-participants", { users: roomUsers[roomId] });
    socket.to(roomId).emit("user-joined", { userId });
  });

  socket.on("disconnect", () => {
    const { roomId, userId } = socket;

    if (roomUsers[roomId]) {
      roomUsers[roomId] = roomUsers[roomId].filter((id) => id !== userId);
      socket.to(roomId).emit("user-left", { userId });

      setTimeout(() => {
        if (roomUsers[roomId] && roomUsers[roomId].length === 0) {
          delete roomUsers[roomId];
          console.log(`Room ${roomId} garbage collected.`);
        }
      }, 20000);
    }
  });
});

const PORT = process.env.PORT || 8080;
server.listen(PORT, () => {
  console.log(`Collaboration service listening on port ${PORT}`);
});
