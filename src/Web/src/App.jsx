import React, { useState, useEffect } from "react";
import { BrowserRouter, Routes, Route, Navigate, useParams } from "react-router-dom";
import CollaborativeEditor from "./components/CollaborativeEditor";
import Header from "./components/Header";
import { Users, Info } from "lucide-react";
import io from "socket.io-client";

const myUserId = Math.random().toString(36).substring(2, 9);

const socket = io({
  path: "/collaboration/socket.io",
  transports: ["websocket"],
  autoConnect: false,
});

function Room() {
  const { roomId } = useParams();
  const [participants, setParticipants] = useState([
    { id: myUserId, me: true },
  ]);
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    socket.connect();

    socket.on("connect", () => {
      setIsConnected(true);
      socket.emit("join-session", { sessionId: roomId, userId: myUserId });
    });

    socket.on("room-participants", ({ users }) => {
      setParticipants(users.map((id) => ({ id, me: id === myUserId })));
    });

    socket.on("disconnect", () => {
      setIsConnected(false);
    });

    socket.on("user-joined", ({ userId }) => {
      setParticipants((prev) => {
        if (!prev.find((p) => p.id === userId)) {
          return [...prev, { id: userId, me: false }];
        }
        return prev;
      });
    });

    socket.on("user-left", ({ userId }) => {
      setParticipants((prev) => prev.filter((p) => p.id !== userId));
    });

    return () => {
      socket.disconnect();
    };
  }, [roomId]);

  return (
    <div className="app-container">
      <Header isConnected={isConnected} room={roomId} />

      <main className="main-content">
        <div
          className="panel editor-wrapper animate-fade-in"
          style={{ animationDelay: "0.1s" }}
        >
          <div className="panel-header">
            <span style={{ color: "#a855f7" }}>main</span>
          </div>
          <CollaborativeEditor socket={socket} room={roomId} userId={myUserId} />
        </div>

        <aside
          className="panel sidebar animate-fade-in"
          style={{ animationDelay: "0.2s" }}
        >
          <div className="panel-header">
            <Users size={16} />
            <span>Participants ({participants.length})</span>
          </div>

          <div className="participant-list">
            {participants.map((p) => (
              <div key={p.id} className="participant">
                <div className="avatar">
                  {p.id.substring(0, 2).toUpperCase()}
                </div>
                <div className="participant-info">
                  <span className="participant-name">
                    User {p.id.substring(0, 4)} {p.me && "(You)"}
                  </span>
                  <span className="participant-status">Online</span>
                </div>
              </div>
            ))}

            {participants.length === 1 && (
              <div
                style={{
                  marginTop: "20px",
                  padding: "12px",
                  background: "rgba(99, 102, 241, 0.1)",
                  borderRadius: "8px",
                  fontSize: "0.85rem",
                  display: "flex",
                  gap: "8px",
                  color: "#a0a4b8",
                }}
              >
                <Info size={16} color="#6366f1" style={{ flexShrink: 0 }} />
                <span>
                  You are the only one here. Give the URL to someone else or open it in another tab to pair program!
                </span>
              </div>
            )}
          </div>
        </aside>
      </main>
    </div>
  );
}

function App() {
  const generateRandomRoom = () => Math.random().toString(36).substring(2, 10);

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to={`/room/${generateRandomRoom()}`} replace />} />
        <Route path="/room/:roomId" element={<Room />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
