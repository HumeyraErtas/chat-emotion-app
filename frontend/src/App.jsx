import { useState, useEffect } from "react";
import "./App.css";

const API_URL = "https://chat-emotion-app-2.onrender.com/api/chat"; // Backend URL

export default function App() {
  const [messages, setMessages] = useState([]);
  const [text, setText] = useState("");

  const fetchMessages = async () => {
    try {
      const res = await fetch(`${API_URL}/messages`);
      const data = await res.json();
      setMessages(data);
    } catch (err) {
      console.error("Fetch Error:", err);
    }
  };

  useEffect(() => {
    fetchMessages();
  }, []);

  const sendMessage = async () => {
    if (!text.trim()) return;

    const newMessage = {
      userId: 1,
      text: text,
      emotion: "NEUTRAL",
    };

    try {
      const res = await fetch(`${API_URL}/message`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newMessage),
      });

      const savedMessage = await res.json();

      setMessages([savedMessage, ...messages]);
      setText("");
    } catch (err) {
      console.error("POST Error:", err);
    }
  };

  const getBubbleColor = (emotion) => {
    switch (emotion) {
      case "POSITIVE":
        return "#4CAF50";
      case "NEGATIVE":
        return "#E53935";
      default:
        return "#607D8B";
    }
  };

  return (
    <div className="app">
      <h2>Chat AI</h2>

      <div className="chat-box">
        {messages.map((msg) => (
          <div key={msg.id} className="bubble-wrapper">
            <div
              className="bubble"
              style={{ backgroundColor: getBubbleColor(msg.emotion) }}
            >
              {msg.text}
              <span className="emotion-tag">{msg.emotion}</span>
            </div>
          </div>
        ))}
      </div>

      <div className="input-area">
        <input
          value={text}
          onChange={(e) => setText(e.target.value)}
          placeholder="Mesaj yaz..."
        />
        <button onClick={sendMessage}>Gönder</button>
      </div>
    </div>
  );
}
