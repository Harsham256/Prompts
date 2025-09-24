import React, { useState } from "react";
import { uploadDocument } from "../services/api";
import { readFileAsDataURL } from "../utils/ocr";

export default function DocumentUpload({ onUploaded }) {
  const [file, setFile] = useState(null);
  const [preview, setPreview] = useState("");
  const [serverResult, setServerResult] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const onFileChange = async (e) => {
    setError("");
    const f = e.target.files[0];
    setFile(f);
    if (!f) {
      setPreview("");
      return;
    }
    if (f.type.includes("image") || f.type.includes("pdf")) {
      const url = URL.createObjectURL(f);
      setPreview(url);
    } else {
      const data = await readFileAsDataURL(f);
      setPreview(data);
    }
  };

  const onUpload = async () => {
    if (!file) return setError("Choose a file first");
    setError("");
    setLoading(true);
    try {
      const { data } = await uploadDocument(file);
      setServerResult(data);
      onUploaded && onUploaded(data);
    } catch (err) {
      setError(err?.response?.data || err.message || "Upload failed");
    } finally {
      setLoading(false);
    }
  };

  const onConfirmAndApply = () => {
    onUploaded && onUploaded(serverResult || { LandId: "NotFound" }, file);
  };

  return (
    <div style={{ maxWidth: 720 }}>
      <h3>Upload Document</h3>
      <input type="file" accept="image/*,.pdf" onChange={onFileChange} />
      {error && <div style={{ color: "red" }}>{error}</div>}
      {preview && (
        <div style={{ marginTop: 8 }}>
          {file?.type && file.type.includes("pdf") ? (
            <iframe title="pdf-preview" src={preview} width="100%" height="500px"></iframe>
          ) : (
            <img src={preview} alt="preview" style={{ maxWidth: "100%", maxHeight: 500 }} />
          )}
        </div>
      )}
      <div style={{ marginTop: 8 }}>
        <button onClick={onUpload} disabled={!file || loading}>
          {loading ? "Uploading..." : "Extract & Send to Admin"}
        </button>
        &nbsp;
        <button onClick={onConfirmAndApply} disabled={!serverResult}>
          Confirm & Apply (send application)
        </button>
      </div>

      {serverResult && (
        <div style={{ marginTop: 10 }}>
          <h4>Server result</h4>
          <pre style={{ whiteSpace: "pre-wrap", background: "#f6f6f6", padding: 8 }}>
            {JSON.stringify(serverResult, null, 2)}
          </pre>
        </div>
      )}
    </div>
  );
}