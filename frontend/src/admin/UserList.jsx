import React from "react";

export default function UserList({ users = [], onEdit, onDelete }) {
  return (
    <div>
      <h4>Users</h4>
      <ul>
        {users.map((u) => (
          <li key={u.id}>
            {u.fullName || u.FullName} — {u.email || u.Email}
            <button onClick={() => onEdit && onEdit(u)}>Edit</button>
            <button onClick={() => onDelete && onDelete(u.id)}>Delete</button>
          </li>
        ))}
      </ul>
    </div>
  );
}