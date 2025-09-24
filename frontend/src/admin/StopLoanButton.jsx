import React, { useState } from "react";

export default function StopLoanButton({ appId, onStopped }) {
  const [loading, setLoading] = useState(false);

  async function stop() {
    if (!window.confirm("Stop this loan?")) return;
    setLoading(true);
    try {
      await fetch(`/api/admin/applications/${appId}/stop`, { method: "POST" });
      onStopped && onStopped();
    } catch (e) {
      alert("Failed to stop");
    } finally {
      setLoading(false);
    }
  }

  return <button onClick={stop} disabled={loading}>{loading ? "Stopping..." : "Stop loan"}</button>;
}