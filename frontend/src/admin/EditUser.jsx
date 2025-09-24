import React, { useState } from "react";

export default function EditUser({ user, onCancel, onSave }) {
  const [form, setForm] = useState({
    fullName: user?.fullName || user?.FullName || "",
    email: user?.email || user?.Email || "",
  });

  const change = (e) => setForm((s) => ({ ...s, [e.target.name]: e.target.value }));

  return (
    <form onSubmit={(e) => { e.preventDefault(); onSave && onSave(form); }}>
      <div>
        <label>Full name</label>
        <input name="fullName" value={form.fullName} onChange={change} />
      </div>
      <div>
        <label>Email</label>
        <input name="email" value={form.email} onChange={change} />
      </div>
      <button type="submit">Save</button>
      <button type="button" onClick={onCancel}>Cancel</button>
    </form>
  );
}