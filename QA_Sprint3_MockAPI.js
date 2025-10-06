// 🔧 Sprint 3 Mock API Server for Early QA Testing
// This allows you to test frontend components before backend is ready

const express = require('express');
const cors = require('cors');
const app = express();

app.use(cors());
app.use(express.json());

// Mock data
const mockLoans = [
  {
    id: 1,
    bookId: 4,
    bookTitle: "1984",
    bookAuthor: "George Orwell",
    memberName: "John Doe",
    memberId: 2,
    issueDate: "2025-09-22",
    dueDate: "2025-10-15",
    returnDate: null,
    isReturned: false,
    isOverdue: true,
    fineAmount: 3.00
  },
  {
    id: 2,
    bookId: 5,
    bookTitle: "Brave New World", 
    bookAuthor: "Aldous Huxley",
    memberName: "Jane Smith",
    memberId: 3,
    issueDate: "2025-10-03",
    dueDate: "2025-10-17",
    returnDate: null,
    isReturned: false,
    isOverdue: false,
    fineAmount: 0.00
  }
];

const mockReservations = [
  {
    id: 1,
    bookId: 4,
    bookTitle: "1984",
    bookAuthor: "George Orwell",
    memberName: "Jane Smith",
    memberId: 3,
    reservationDate: "2025-10-04",
    status: "Active",
    queuePosition: 1,
    estimatedAvailable: "2025-10-15"
  },
  {
    id: 2,
    bookId: 4,
    bookTitle: "1984",
    bookAuthor: "George Orwell", 
    memberName: "Bob Wilson",
    memberId: 4,
    reservationDate: "2025-10-05",
    status: "Active",
    queuePosition: 2,
    estimatedAvailable: "2025-10-22"
  }
];

const mockFines = [
  {
    id: 1,
    loanId: 1,
    memberId: 2,
    memberName: "John Doe",
    bookTitle: "1984",
    amount: 3.00,
    description: "Late return fee: 6 days overdue",
    isPaid: false,
    dueDate: "2025-09-29"
  }
];

// 📚 Loan Management Endpoints
app.get('/api/loans', (req, res) => {
  const { memberId, status } = req.query;
  let loans = [...mockLoans];
  
  if (memberId) {
    loans = loans.filter(loan => loan.memberId == memberId);
  }
  
  if (status === 'overdue') {
    loans = loans.filter(loan => loan.isOverdue);
  }
  
  res.json({
    items: loans,
    total: loans.length,
    page: 1,
    pageSize: 10
  });
});

app.post('/api/loans', (req, res) => {
  const { bookId, memberId } = req.body;
  
  // Simulate validation
  if (!bookId || !memberId) {
    return res.status(400).json({
      success: false,
      message: "Book ID and Member ID are required"
    });
  }
  
  // Simulate successful loan creation
  const newLoan = {
    id: mockLoans.length + 1,
    bookId,
    memberId,
    issueDate: new Date().toISOString().split('T')[0],
    dueDate: new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    returnDate: null,
    isReturned: false,
    isOverdue: false,
    fineAmount: 0.00
  };
  
  mockLoans.push(newLoan);
  
  res.json({
    success: true,
    message: "Book issued successfully",
    loan: newLoan
  });
});

app.put('/api/loans/:id/return', (req, res) => {
  const loanId = parseInt(req.params.id);
  const loan = mockLoans.find(l => l.id === loanId);
  
  if (!loan) {
    return res.status(404).json({
      success: false,
      message: "Loan not found"
    });
  }
  
  if (loan.isReturned) {
    return res.status(400).json({
      success: false,
      message: "Book already returned"
    });
  }
  
  // Process return
  loan.isReturned = true;
  loan.returnDate = new Date().toISOString().split('T')[0];
  
  // Calculate fine if overdue
  if (loan.isOverdue) {
    const overdueDays = Math.floor((new Date() - new Date(loan.dueDate)) / (1000 * 60 * 60 * 24));
    loan.fineAmount = overdueDays * 0.50;
  }
  
  res.json({
    success: true,
    message: "Book returned successfully",
    loan,
    fine: loan.fineAmount > 0 ? {
      amount: loan.fineAmount,
      description: `Late return fee: ${Math.floor(loan.fineAmount / 0.50)} days overdue`
    } : null
  });
});

// 🔖 Reservation Management Endpoints  
app.get('/api/reservations', (req, res) => {
  const { memberId } = req.query;
  let reservations = [...mockReservations];
  
  if (memberId) {
    reservations = reservations.filter(r => r.memberId == memberId);
  }
  
  res.json({
    items: reservations,
    total: reservations.length,
    page: 1,
    pageSize: 10
  });
});

app.post('/api/reservations', (req, res) => {
  const { bookId, memberId } = req.body;
  
  // Simulate validation
  if (!bookId || !memberId) {
    return res.status(400).json({
      success: false,
      message: "Book ID and Member ID are required"
    });
  }
  
  // Check if already reserved
  const existingReservation = mockReservations.find(r => 
    r.bookId === bookId && r.memberId === memberId && r.status === 'Active'
  );
  
  if (existingReservation) {
    return res.status(400).json({
      success: false,
      message: "You have already reserved this book"
    });
  }
  
  // Create new reservation
  const queuePosition = mockReservations.filter(r => 
    r.bookId === bookId && r.status === 'Active'
  ).length + 1;
  
  const newReservation = {
    id: mockReservations.length + 1,
    bookId,
    memberId,
    reservationDate: new Date().toISOString().split('T')[0],
    status: 'Active',
    queuePosition,
    estimatedAvailable: new Date(Date.now() + queuePosition * 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
  };
  
  mockReservations.push(newReservation);
  
  res.json({
    success: true,
    message: "Book reserved successfully",
    reservation: newReservation
  });
});

app.delete('/api/reservations/:id', (req, res) => {
  const reservationId = parseInt(req.params.id);
  const index = mockReservations.findIndex(r => r.id === reservationId);
  
  if (index === -1) {
    return res.status(404).json({
      success: false,
      message: "Reservation not found"
    });
  }
  
  const reservation = mockReservations[index];
  mockReservations.splice(index, 1);
  
  // Update queue positions for remaining reservations
  mockReservations
    .filter(r => r.bookId === reservation.bookId && r.queuePosition > reservation.queuePosition)
    .forEach(r => r.queuePosition--);
  
  res.json({
    success: true,
    message: "Reservation cancelled successfully"
  });
});

// 💰 Fine Management Endpoints
app.get('/api/fines', (req, res) => {
  const { memberId, isPaid } = req.query;
  let fines = [...mockFines];
  
  if (memberId) {
    fines = fines.filter(f => f.memberId == memberId);
  }
  
  if (isPaid !== undefined) {
    fines = fines.filter(f => f.isPaid === (isPaid === 'true'));
  }
  
  res.json({
    items: fines,
    total: fines.length,
    totalAmount: fines.filter(f => !f.isPaid).reduce((sum, f) => sum + f.amount, 0)
  });
});

app.post('/api/fines/:id/pay', (req, res) => {
  const fineId = parseInt(req.params.id);
  const fine = mockFines.find(f => f.id === fineId);
  
  if (!fine) {
    return res.status(404).json({
      success: false,
      message: "Fine not found"
    });
  }
  
  if (fine.isPaid) {
    return res.status(400).json({
      success: false,
      message: "Fine already paid"
    });
  }
  
  fine.isPaid = true;
  fine.paidDate = new Date().toISOString().split('T')[0];
  
  res.json({
    success: true,
    message: "Fine paid successfully",
    fine
  });
});

// 🏥 Health Check
app.get('/api/health', (req, res) => {
  res.json({
    status: 'OK',
    timestamp: new Date().toISOString(),
    service: 'Sprint 3 Mock API',
    endpoints: [
      'GET /api/loans',
      'POST /api/loans', 
      'PUT /api/loans/:id/return',
      'GET /api/reservations',
      'POST /api/reservations',
      'DELETE /api/reservations/:id',
      'GET /api/fines',
      'POST /api/fines/:id/pay'
    ]
  });
});

const PORT = process.env.PORT || 5193;
app.listen(PORT, () => {
  console.log(`🔧 Sprint 3 Mock API Server running on port ${PORT}`);
  console.log(`📋 Available endpoints:`);
  console.log(`   GET  http://localhost:${PORT}/api/health`);
  console.log(`   GET  http://localhost:${PORT}/api/loans`);
  console.log(`   POST http://localhost:${PORT}/api/loans`);
  console.log(`   GET  http://localhost:${PORT}/api/reservations`);
  console.log(`   GET  http://localhost:${PORT}/api/fines`);
  console.log(`\n🧪 Ready for Sprint 3 QA Testing!`);
});