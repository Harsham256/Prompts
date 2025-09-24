import React from "react";

export default function ResultDisplay({ result }) {
  if (!result) return null;
  return (
    <div style={{ marginTop: 12 }}>
      <h3>Verification Result</h3>
      <div>
        <strong>Status:</strong> {result.Status || result.status || "Unknown"}
      </div>
      <div>
        <strong>Reason:</strong> {result.Reason || result.reason || result.DecisionReason || "—"}
      </div>
      <pre style={{ background: "#fafafa", padding: 8, marginTop: 8 }}>{JSON.stringify(result, null, 2)}</pre>
    </div>
  );
}