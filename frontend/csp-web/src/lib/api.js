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

// Lending API functions
export const lendingApi = {
  borrowBook: (bookId, userId, loanDurationDays = 14) => 
    api.post('/lendings/borrow', { bookId, userId, loanDurationDays }),
  
  returnBook: (lendingId, fineAmount = null) => 
    api.post('/lendings/return', { lendingId, fineAmount }),
  
  renewLoan: (lendingId) => 
    api.post('/lendings/renew', { lendingId }),
  
  getActiveLoans: (userId = null, page = 1, pageSize = 10) => 
    api.get('/lendings/active', { params: { userId, page, pageSize } }),
  
  getLoanHistory: (userId = null, page = 1, pageSize = 10) => 
    api.get('/lendings/history', { params: { userId, page, pageSize } }),
  
  getLendingById: (id) => 
    api.get(`/lendings/${id}`)
};

// Reservation API functions
export const reservationApi = {
  createReservation: (bookId, userId) => 
    api.post('/reservations', { bookId, userId }),
  
  cancelReservation: (reservationId) => 
    api.delete(`/reservations/${reservationId}`),
  
  fulfillReservation: (reservationId, loanDurationDays = 14) => 
    api.post('/reservations/fulfill', { reservationId, loanDurationDays }),
  
  getMyReservations: (page = 1, pageSize = 10) => 
    api.get('/reservations/my-reservations', { params: { page, pageSize } }),
  
  getAllReservations: (page = 1, pageSize = 10) => 
    api.get('/reservations', { params: { page, pageSize } }),
  
  getReservationById: (id) => 
    api.get(`/reservations/${id}`)
};

export default api;