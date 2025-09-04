import { useEffect, useState } from "react";
import api from "./lib/api";

export default function App() {
  const [msg, setMsg] = useState("");
  useEffect(() => {
    api.get("/health/db").then(r => setMsg(JSON.stringify(r.data))).catch(() => setMsg("API not reachable"));
  }, []);
  return <div style={{ padding: 24 }}>
    <h1>CSP Frontend</h1>
    <p>{msg}</p>
  </div>;
}