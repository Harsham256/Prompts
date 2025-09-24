import axios from "axios";
import { getToken } from "../utils/tokenHelper";

// For Vite projects, use import.meta.env; for CRA, use process.env
const API_BASE = import.meta.env.VITE_API_BASE || "http://localhost:5067/api";

const instance = axios.create({
  baseURL: API_BASE,
  timeout: 30000,
});

instance.interceptors.request.use((config) => {
  const token = getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

export async function registerUser(payload) {
  return instance.post("/auth/register", payload);
}

export async function loginUser(payload) {
  return instance.post("/auth/login", payload);
}

export async function uploadDocument(file) {
  const form = new FormData();
  form.append("file", file);
  return instance.post("/document/extract", form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
}

export async function applyForLoan(body) {
  return instance.post("/loan/apply", body);
}

export async function getUsers() {
  return instance.get("/admin/users");
}
export async function addUser(user) {
  return instance.post("/admin/users", user);
}
export async function updateUser(id, user) {
  return instance.put(`/admin/users/${id}`, user);
}
export async function deleteUser(id) {
  return instance.delete(`/admin/users/${id}`);
}
export async function getAllApplications() {
  return instance.get("/admin/applications");
}
export async function approveApplication(id) {
  return instance.post(`/admin/applications/${id}/approve`);
}
export async function rejectApplication(id, reason) {
  return instance.post(`/admin/applications/${id}/reject`, { reason });
}

export default instance;