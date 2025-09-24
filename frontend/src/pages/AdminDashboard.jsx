import React, { useEffect, useState } from "react";
import { getUsers, getAllApplications, approveApplication, rejectApplication, deleteUser } from "../services/api";

export default function AdminDashboard() {
  const [users, setUsers] = useState([]);
  const [apps, setApps] = useState([]);
  const [loading, setLoading] = useState(false);
  const [err, setErr] = useState("");

  useEffect(() => {
    loadAll();
  }, []);

  async function loadAll() {
    setLoading(true);
    setErr("");
    try {
      const [u, a] = await Promise.all([getUsers(), getAllApplications()]);
      setUsers(u.data || []);
      setApps(a.data || []);
    } catch (e) {
      setErr(e?.message || "Failed");
    } finally {
      setLoading(false);
    }
  }

  async function onApprove(id) {
    await approveApplication(id);
    await loadAll();
  }

  async function onReject(id) {
    const reason = prompt("Reason for rejection?");
    if (!reason) return;
    await rejectApplication(id, reason);
    await loadAll();
  }

  async function onDeleteUser(id) {
    if (!window.confirm("Delete user?")) return;
    await deleteUser(id);
    await loadAll();
  }

  return (
    <div style={{ padding: 16 }}>
      <h1>Admin Dashboard</h1>
      {err && <div style={{ color: "red" }}>{err}</div>}
      {loading && <div>Loading...</div>}

      <section style={{ marginTop: 12 }}>
        <h3>Pending Applications</h3>
        {apps.length === 0 && <div>No applications</div>}
        <ul>
          {apps.map((a) => (
            <li key={a.id} style={{ marginBottom: 8 }}>
              <div>
                <strong>App #{a.id}</strong> — Land: {a.landId || a.LandId} — User: {a.userId}
              </div>
              <div>
                <button onClick={() => onApprove(a.id)}>Approve</button>{" "}
                <button onClick={() => onReject(a.id)}>Reject</button>
              </div>
            </li>
          ))}
        </ul>
      </section>

      <section style={{ marginTop: 20 }}>
        <h3>Users</h3>
        <ul>
          {users.map((u) => (
            <li key={u.id} style={{ marginBottom: 8 }}>
              {u.fullName || u.FullName} — {u.email || u.Email}
              {" "}
              <button onClick={() => onDeleteUser(u.id)} style={{ marginLeft: 8 }}>Delete</button>
            </li>
          ))}
        </ul>
      </section>
    </div>
  );
}