import React, { useState } from "react";
import { registerUser } from "../services/api";
import { saveToken, saveUser } from "../utils/tokenHelper";
import { useNavigate } from "react-router-dom";

export default function RegisterForm() {
  const [form, setForm] = useState({
    fullName: "",
    email: "",
    password: "",
    aadhaar: "",
    pan: "",
    landId: "",
    latitude: "",
    longitude: "",
  });

  const navigate = useNavigate();

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      // Use the imported registerUser function
      const response = await registerUser({
        ...form,
        latitude: parseFloat(form.latitude),
        longitude: parseFloat(form.longitude),
      });
      // Save token and user if needed
      if (response.data.token) saveToken(response.data.token);
      if (response.data.user) saveUser(response.data.user);
      alert("Registered successfully!");
      navigate("/login");
    } catch (error) {
      alert("Registration failed: " + (error.response?.data?.message || error.message));
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      {Object.keys(form).map((key) => (
        <div key={key}>
          <label>{key}</label>
          <input
            name={key}
            value={form[key]}
            onChange={handleChange}
            required
          />
        </div>
      ))}
      <button type="submit">Register</button>
    </form>
  );
}