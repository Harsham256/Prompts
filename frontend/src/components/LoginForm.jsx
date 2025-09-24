import React, { useState } from "react";
import { loginUser } from "../services/api";
import { saveToken, saveUser } from "../utils/tokenHelper";
import { useNavigate } from "react-router-dom";

export default function LoginForm() {
  const [form, setForm] = useState({ email: "", password: "" });
  const [error, setError] = useState("");
  const nav = useNavigate();
  const onChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const onSubmit = async (e) => {
    e.preventDefault();
    setError("");
    try {
      const { data } = await loginUser({ Email: form.email, Password: form.password });
      if (data.token) {
        saveToken(data.token);
        saveUser({ email: form.email });
        nav("/user");
      } else {
        setError("Login succeeded but token missing from backend response.");
      }
    } catch (err) {
      setError(err?.response?.data?.message || err.message || "Login failed");
    }
  };

  return (
    <form onSubmit={onSubmit} style={{ maxWidth: 420 }}>
      <h2>Login</h2>
      {error && <div style={{ color: "red" }}>{error}</div>}
      <div>
        <label>Email</label>
        <input name="email" value={form.email} onChange={onChange} required />
      </div>
      <div>
        <label>Password</label>
        <input name="password" type="password" value={form.password} onChange={onChange} required />
      </div>
      <button type="submit">Login</button>
    </form>
  );
}