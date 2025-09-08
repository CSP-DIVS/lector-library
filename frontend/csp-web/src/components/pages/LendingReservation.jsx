import { useState, useEffect } from 'react';
import './LendingReservation.css';

const LendingReservation = ({ user }) => {
  const [activeTab, setActiveTab] = useState('loans');
  const [loans, setLoans] = useState([]);
  const [reservations, setReservations] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchLendingData();
  }, []);

  const fetchLendingData = async () => {
    try {
      // Mock data - replace with actual API calls
      const mockLoans = getMockLoansForRole(user.role);
      const mockReservations = getMockReservationsForRole(user.role);
      
      setLoans(mockLoans);
      setReservations(mockReservations);
    } catch (error) {
      console.error('Error fetching lending data:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>Loading lending information...</p>
      </div>
    );
  }

  return (
    <div className="lending-reservation-page">
      <div className="page-header">
        <h1>
          {user.role === 'Member' ? 'My Books & Reservations' : 'Lending & Reservation Management'}
        </h1>
        <p className="page-subtitle">
          {user.role === 'Member' 
            ? 'Track your borrowed books and reservations'
            : 'Manage book loans and reservations for all members'
          }
        </p>
      </div>

      <div className="tabs-container">
        <div className="tabs">
          <button
            className={`tab ${activeTab === 'loans' ? 'active' : ''}`}
            onClick={() => setActiveTab('loans')}
          >
            {user.role === 'Member' ? 'My Loans' : 'Active Loans'}
            <span className="tab-count">{loans.length}</span>
          </button>
          <button
            className={`tab ${activeTab === 'reservations' ? 'active' : ''}`}
            onClick={() => setActiveTab('reservations')}
          >
            {user.role === 'Member' ? 'My Reservations' : 'Reservations'}
            <span className="tab-count">{reservations.length}</span>
          </button>
        </div>
      </div>

      <div className="tab-content">
        {activeTab === 'loans' && (
          <div className="loans-section">
            <div className="section-header">
              <h2>
                {user.role === 'Member' ? 'Books on Loan' : 'Active Loans'}
              </h2>
            </div>
            
            <div className="items-list">
              {loans.map(loan => (
                <div key={loan.id} className="loan-card">
                  <div className="item-info">
                    <div className="book-details">
                      <h3 className="book-title">{loan.bookTitle}</h3>
                      <p className="book-author">by {loan.bookAuthor}</p>
                      {user.role !== 'Member' && (
                        <p className="member-name">Member: {loan.memberName}</p>
                      )}
                    </div>
                    
                    <div className="loan-details">
                      <div className="loan-dates">
                        <div className="date-item">
                          <span className="date-label">Borrowed:</span>
                          <span className="date-value">{loan.borrowDate}</span>
                        </div>
                        <div className="date-item">
                          <span className="date-label">Due:</span>
                          <span className={`date-value ${loan.isOverdue ? 'overdue' : ''}`}>
                            {loan.dueDate}
                          </span>
                        </div>
                      </div>
                      
                      {loan.isOverdue && (
                        <div className="overdue-notice">
                          <span className="overdue-icon">⚠️</span>
                          <span>Overdue by {loan.overdueDays} days</span>
                          {loan.fine && <span className="fine-amount">${loan.fine}</span>}
                        </div>
                      )}
                    </div>
                  </div>
                  
                  <div className="loan-actions">
                    {user.role === 'Member' ? (
                      <div className="member-actions">
                        <button className="btn btn-outline">Renew</button>
                      </div>
                    ) : (
                      <div className="staff-actions">
                        <button className="btn btn-primary">Process Return</button>
                        <button className="btn btn-outline">Send Reminder</button>
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {activeTab === 'reservations' && (
          <div className="reservations-section">
            <div className="section-header">
              <h2>
                {user.role === 'Member' ? 'My Reservations' : 'Pending Reservations'}
              </h2>
            </div>
            
            <div className="items-list">
              {reservations.map(reservation => (
                <div key={reservation.id} className="reservation-card">
                  <div className="item-info">
                    <div className="book-details">
                      <h3 className="book-title">{reservation.bookTitle}</h3>
                      <p className="book-author">by {reservation.bookAuthor}</p>
                      {user.role !== 'Member' && (
                        <p className="member-name">Member: {reservation.memberName}</p>
                      )}
                    </div>
                    
                    <div className="reservation-details">
                      <div className="reservation-info">
                        <div className="info-item">
                          <span className="info-label">Reserved:</span>
                          <span className="info-value">{reservation.reservedDate}</span>
                        </div>
                        <div className="info-item">
                          <span className="info-label">Position:</span>
                          <span className="info-value">#{reservation.queuePosition}</span>
                        </div>
                        <div className="info-item">
                          <span className="info-label">Status:</span>
                          <span className={`status-badge ${reservation.status.toLowerCase()}`}>
                            {reservation.status}
                          </span>
                        </div>
                      </div>
                      
                      {reservation.estimatedAvailable && (
                        <div className="estimated-date">
                          <span className="estimate-label">Estimated available:</span>
                          <span className="estimate-value">{reservation.estimatedAvailable}</span>
                        </div>
                      )}
                    </div>
                  </div>
                  
                  <div className="reservation-actions">
                    {user.role === 'Member' ? (
                      <div className="member-actions">
                        <button className="btn btn-danger">Cancel Reservation</button>
                      </div>
                    ) : (
                      <div className="staff-actions">
                        {reservation.status === 'Available' ? (
                          <button className="btn btn-primary">Fulfill Reservation</button>
                        ) : (
                          <button className="btn btn-outline">Notify Member</button>
                        )}
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      {((activeTab === 'loans' && loans.length === 0) || 
        (activeTab === 'reservations' && reservations.length === 0)) && (
        <div className="empty-state">
          <div className="empty-icon">
            {activeTab === 'loans' ? '📚' : '🔖'}
          </div>
          <h3>
            {activeTab === 'loans' ? 'No active loans' : 'No reservations'}
          </h3>
          <p>
            {user.role === 'Member' 
              ? `You don't have any ${activeTab === 'loans' ? 'books on loan' : 'reservations'} at the moment.`
              : `No ${activeTab} to manage at this time.`
            }
          </p>
        </div>
      )}
    </div>
  );
};

const getMockLoansForRole = (role) => {
  if (role === 'Member') {
    return [
      {
        id: 1,
        bookTitle: 'The Catcher in the Rye',
        bookAuthor: 'J.D. Salinger',
        borrowDate: '2024-08-15',
        dueDate: '2024-09-15',
        isOverdue: false,
        overdueDays: 0
      },
      {
        id: 2,
        bookTitle: 'To Kill a Mockingbird',
        bookAuthor: 'Harper Lee',
        borrowDate: '2024-08-01',
        dueDate: '2024-09-01',
        isOverdue: true,
        overdueDays: 7,
        fine: 3.50
      }
    ];
  } else {
    return [
      {
        id: 1,
        bookTitle: 'The Great Gatsby',
        bookAuthor: 'F. Scott Fitzgerald',
        memberName: 'John Smith',
        borrowDate: '2024-08-20',
        dueDate: '2024-09-20',
        isOverdue: false
      },
      {
        id: 2,
        bookTitle: '1984',
        bookAuthor: 'George Orwell',
        memberName: 'Jane Doe',
        borrowDate: '2024-07-25',
        dueDate: '2024-08-25',
        isOverdue: true,
        overdueDays: 14,
        fine: 7.00
      },
      {
        id: 3,
        bookTitle: 'Pride and Prejudice',
        bookAuthor: 'Jane Austen',
        memberName: 'Bob Johnson',
        borrowDate: '2024-08-10',
        dueDate: '2024-09-10',
        isOverdue: false
      }
    ];
  }
};

const getMockReservationsForRole = (role) => {
  if (role === 'Member') {
    return [
      {
        id: 1,
        bookTitle: 'Dune',
        bookAuthor: 'Frank Herbert',
        reservedDate: '2024-08-28',
        queuePosition: 3,
        status: 'Pending',
        estimatedAvailable: '2024-09-15'
      },
      {
        id: 2,
        bookTitle: 'The Hobbit',
        bookAuthor: 'J.R.R. Tolkien',
        reservedDate: '2024-08-30',
        queuePosition: 1,
        status: 'Available',
        estimatedAvailable: 'Now'
      }
    ];
  } else {
    return [
      {
        id: 1,
        bookTitle: 'The Lord of the Rings',
        bookAuthor: 'J.R.R. Tolkien',
        memberName: 'Alice Wilson',
        reservedDate: '2024-08-25',
        queuePosition: 1,
        status: 'Available'
      },
      {
        id: 2,
        bookTitle: 'Harry Potter and the Sorcerer\'s Stone',
        bookAuthor: 'J.K. Rowling',
        memberName: 'Charlie Brown',
        reservedDate: '2024-08-27',
        queuePosition: 2,
        status: 'Pending',
        estimatedAvailable: '2024-09-10'
      }
    ];
  }
};

export default LendingReservation;
