import React, { useState } from "react";

export default function LandDetailsForm({ initial = {}, onSubmit }) {
  const [form, setForm] = useState({
    landId: initial.landId || "",
    ownerName: initial.ownerName || "",
    hasSiblingApproval: initial.hasSiblingApproval || false,
  });

  const change = (e) => {
    const { name, value, type, checked } = e.target;
    setForm((s) => ({ ...s, [name]: type === "checkbox" ? checked : value }));
  };

  const submit = (e) => {
    e.preventDefault();
    onSubmit && onSubmit(form);
  };

  return (
    <form onSubmit={submit} style={{ maxWidth: 520 }}>
      <h3>Land Details</h3>
      <div>
        <label>Land ID</label>
        <input name="landId" value={form.landId} onChange={change} required />
      </div>
      <div>
        <label>Owner name on land docs</label>
        <input name="ownerName" value={form.ownerName} onChange={change} required />
      </div>
      <div>
        <label>
          <input name="hasSiblingApproval" type="checkbox" checked={form.hasSiblingApproval} onChange={change} />
          Sibling approval (if inherited)
        </label>
      </div>
      <button type="submit">Apply</button>
    </form>
  );
}