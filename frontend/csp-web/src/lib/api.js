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
  // Legacy endpoints (if present on backend)
  adjustFine: (lendingId, adjustmentData) => 
    api.put(`/fines/${lendingId}/adjust`, adjustmentData),
  getUserFines: (userId) =>
    api.get(`/fines/user/${userId}`),

  fulfillReservation: (reservationId, loanDurationDays = 14) => 
    api.post('/reservations/fulfill', { reservationId, loanDurationDays }),
  
  getMyReservations: (page = 1, pageSize = 10) => 
    api.get('/reservations/my-reservations', { params: { page, pageSize } }),
  
  getAllReservations: (page = 1, pageSize = 10) => 
    api.get('/reservations', { params: { page, pageSize } }),
  
  getReservationById: (id) => 
    api.get(`/reservations/${id}`)
};

// Fines API functions
export const finesApi = {
  // For members: get my active/overdue lendings including FineAmount
  getMyActiveLoans: (page = 1, pageSize = 10) =>
    lendingApi.getActiveLoans(null, page, pageSize),

  // For admins: trigger recalculation manually
  triggerRecalculate: () =>
    api.post('/maintenance/trigger-fine-calculation'),
  // Convenience: filter fines client-side until we expose a dedicated endpoint
  mapLoansToFines: (items = []) => items
    .filter(x => (x.status === 'Overdue' || (x.fineAmount ?? 0) > 0) && !x.finePaid)
    .map(x => ({
      id: x.id,
      reason: 'Overdue Book',
      bookTitle: x.bookTitle,
      bookAuthor: x.bookAuthor,
      amount: Number(x.fineAmount ?? 0),
      dueDate: new Date(x.dueDate).toISOString().slice(0,10),
      overdueDate: new Date(x.dueDate).toISOString().slice(0,10),
      daysOverdue: x.overdueDays ?? 0,
      status: (x.finePaid ? 'Paid' : 'Outstanding')
    }))
};

export default api;