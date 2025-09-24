import React, { useState } from "react";
import DocumentUpload from "../components/DocumentUpload";
import LandDetailsForm from "../components/LandDetailsForm";
import ResultDisplay from "../components/ResultDisplay";
import { applyForLoan } from "../services/api";

export default function UserDashboard() {
  const [extracted, setExtracted] = useState(null);
  const [applicationResult, setApplicationResult] = useState(null);
  const [applying, setApplying] = useState(false);
  const [error, setError] = useState("");

  async function handleUploaded(serverResult, file) {
    setExtracted(serverResult);
  }

  async function handleApply(landForm) {
    setError("");
    setApplying(true);
    try {
      const body = {
        LandId: landForm.landId,
        OwnerName: landForm.ownerName,
        HasSiblingApproval: !!landForm.hasSiblingApproval,
        UserId: 0
      };
      const { data } = await applyForLoan(body);
      setApplicationResult(data);
    } catch (err) {
      setError(err?.response?.data?.message || err.message || "Apply failed");
    } finally {
      setApplying(false);
    }
  }

  return (
    <div style={{ padding: 16 }}>
      <h1>User Dashboard</h1>
      <p>Upload your land document, preview, and confirm. Admin will approve for verification.</p>

      <DocumentUpload onUploaded={handleUploaded} />

      {extracted && (
        <div style={{ marginTop: 12 }}>
          <h4>Extracted land details</h4>
          <pre style={{ background: "#f5f5f5", padding: 8 }}>{JSON.stringify(extracted, null, 2)}</pre>
          <LandDetailsForm
            initial={{ landId: extracted?.LandId || "" }}
            onSubmit={(form) => handleApply(form)}
          />
          {error && <div style={{ color: "red" }}>{error}</div>}
          {applicationResult && <ResultDisplay result={applicationResult} />}
          {applying && <div>Applying...</div>}
        </div>
      )}
    </div>
  );
}