import axios from "axios";

// Determine the base URL based on environment
const getBaseURL = () => {
  // In production (built app), use relative URLs
  if (import.meta.env.PROD) {
    return '/api';
  }
  // In development, use the proxy path (Vite will proxy /api to the backend)
  return '/api';
};

const api = axios.create({
  baseURL: getBaseURL()
});
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  
  // Ensure Content-Type is set for requests with data
  if (config.data !== undefined && !config.headers['Content-Type']) {
    config.headers['Content-Type'] = 'application/json';
  }
  
  return config;
});
api.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err?.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    }
    return Promise.reject(err);
  }
);
export default api;