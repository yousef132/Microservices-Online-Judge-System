import React, { useRef, useEffect } from "react";
import Editor from "@monaco-editor/react";
import * as Y from "yjs";
import { SocketIOProvider as YSocketIO } from "y-socket.io";
import { MonacoBinding } from "y-monaco";

export default function CollaborativeEditor({ socket, room, userId }) {
  const providerRef = useRef(null);
  const bindingRef = useRef(null);

  function handleEditorDidMount(editor, monaco) {
    try {
      const ydoc = new Y.Doc();

      const provider = new YSocketIO(
        window.location.origin,
        room,
        ydoc,
        {
          autoConnect: false,
        },
        {
          path: "/collaboration/socket.io",
        },
      );

      const ytext = ydoc.getText("monaco");
      const metadata = ydoc.getMap("metadata");

      provider.on("sync", (isSynced) => {
        if (isSynced && !bindingRef.current) {
          if (!metadata.get("isSeeded")) {
            const initialCode = `console.log("Hello World");`;
            ytext.insert(0, initialCode);
            metadata.set("isSeeded", true);
          }

          const binding = new MonacoBinding(
            ytext,
            editor.getModel(),
            new Set([editor]),
            provider.awareness,
          );
          console.log("Binding created after sync:", binding);
          bindingRef.current = binding;

          const userName = "User " + userId.substring(0, 4);
          const userColor =
            "#" + Math.floor(Math.random() * 16777215).toString(16);
          provider.awareness.setLocalStateField("user", {
            id: userId,
            name: userName,
            color: userColor,
          });
        }
      });

      provider.awareness.on("update", () => {
        const states = provider.awareness.getStates();

        let css = "";

        states.forEach((state, clientID) => {
          if (state.user && clientID !== provider.awareness.clientID) {
            const { name, color } = state.user;
            css += `
              .yRemoteSelection-${clientID} {
                background-color: ${color}4D; /* 30% opacity */
              }
              .yRemoteSelectionHead-${clientID} {
                border-left-color: ${color} !important;
              }
              .yRemoteSelectionHead-${clientID}::after {
                content: "${name}";
                background-color: ${color};
                display: block;
                top: "-22px";
                z-index: 100;
                transition: top 0.15s ease-out;
              }
            `;
          }
        });

        let styleElement = document.getElementById("yjs-cursor-styles");
        if (!styleElement) {
          styleElement = document.createElement("style");
          styleElement.id = "yjs-cursor-styles";
          document.head.appendChild(styleElement);
        }
        styleElement.textContent = css;
      });

      provider.connect();
      providerRef.current = provider;
    } catch (err) {
      console.error("Error initializing collaborative editor:", err);
    }
  }

  useEffect(() => {
    return () => {
      if (bindingRef.current) {
        bindingRef.current.destroy();
      }
      if (providerRef.current) {
        providerRef.current.destroy();
      }

      const styleElement = document.getElementById("yjs-cursor-styles");
      if (styleElement) {
        styleElement.remove();
      }
    };
  }, []);

  return (
    <div style={{ height: "100vh", width: "100vw" }}>
      <Editor
        height="100%"
        defaultLanguage="javascript"
        theme="vs-dark"
        onMount={handleEditorDidMount}
      />
    </div>
  );
}
