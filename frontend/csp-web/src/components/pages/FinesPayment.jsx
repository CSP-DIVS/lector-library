import { useState, useEffect } from 'react';
import './FinesPayment.css';

const FinesPayment = ({ user }) => {
  const [fines, setFines] = useState([]);
  const [paymentHistory, setPaymentHistory] = useState([]);
  const [activeTab, setActiveTab] = useState('outstanding');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchFinesData();
  }, []);

  const fetchFinesData = async () => {
    try {
      // Mock data - replace with actual API calls
      const mockFines = getMockFinesForRole(user.role);
      const mockPayments = getMockPaymentHistoryForRole(user.role);
      
      setFines(mockFines);
      setPaymentHistory(mockPayments);
    } catch (error) {
      console.error('Error fetching fines data:', error);
    } finally {
      setLoading(false);
    }
  };

  const totalOutstanding = fines
    .filter(fine => fine.status === 'Outstanding')
    .reduce((sum, fine) => sum + fine.amount, 0);

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>Loading fines information...</p>
      </div>
    );
  }

  return (
    <div className="fines-payment-page">
      <div className="page-header">
        <h1>
          {user.role === 'Member' ? 'My Fines & Payments' : 'Fines & Payment Management'}
        </h1>
        <p className="page-subtitle">
          {user.role === 'Member' 
            ? 'View and pay your library fines'
            : 'Manage member fines and process payments'
          }
        </p>
      </div>

      {user.role === 'Member' && totalOutstanding > 0 && (
        <div className="outstanding-summary">
          <div className="summary-content">
            <div className="summary-info">
              <h3>Outstanding Balance</h3>
              <div className="total-amount">${totalOutstanding.toFixed(2)}</div>
              <p>{fines.filter(f => f.status === 'Outstanding').length} unpaid fine(s)</p>
            </div>
            <div className="summary-actions">
              <button className="btn btn-primary pay-all-btn">
                Pay All Fines
              </button>
            </div>
          </div>
        </div>
      )}

      <div className="tabs-container">
        <div className="tabs">
          <button
            className={`tab ${activeTab === 'outstanding' ? 'active' : ''}`}
            onClick={() => setActiveTab('outstanding')}
          >
            {user.role === 'Member' ? 'Outstanding Fines' : 'All Fines'}
            <span className="tab-count">
              {fines.filter(f => user.role === 'Member' ? f.status === 'Outstanding' : true).length}
            </span>
          </button>
          <button
            className={`tab ${activeTab === 'history' ? 'active' : ''}`}
            onClick={() => setActiveTab('history')}
          >
            Payment History
            <span className="tab-count">{paymentHistory.length}</span>
          </button>
        </div>
      </div>

      <div className="tab-content">
        {activeTab === 'outstanding' && (
          <div className="fines-section">
            <div className="section-header">
              <h2>
                {user.role === 'Member' ? 'Outstanding Fines' : 'Member Fines'}
              </h2>
              {user.role !== 'Member' && (
                <div className="section-actions">
                  <button className="btn btn-outline">Export Report</button>
                </div>
              )}
            </div>
            
            <div className="fines-list">
              {fines
                .filter(fine => user.role === 'Member' ? fine.status === 'Outstanding' : true)
                .map(fine => (
                <div key={fine.id} className="fine-card">
                  <div className="fine-info">
                    <div className="fine-header">
                      <h3 className="fine-reason">{fine.reason}</h3>
                      <span className={`status-badge ${fine.status.toLowerCase()}`}>
                        {fine.status}
                      </span>
                    </div>
                    
                    <div className="fine-details">
                      <div className="book-info">
                        <span className="book-title">{fine.bookTitle}</span>
                        <span className="book-author">by {fine.bookAuthor}</span>
                      </div>
                      {user.role !== 'Member' && (
                        <p className="member-name">Member: {fine.memberName}</p>
                      )}
                      
                      <div className="fine-dates">
                        <div className="date-item">
                          <span className="date-label">Due Date:</span>
                          <span className="date-value">{fine.dueDate}</span>
                        </div>
                        <div className="date-item">
                          <span className="date-label">Overdue Since:</span>
                          <span className="date-value">{fine.overdueDate}</span>
                        </div>
                        <div className="date-item">
                          <span className="date-label">Days Overdue:</span>
                          <span className="date-value">{fine.daysOverdue} days</span>
                        </div>
                      </div>
                    </div>
                  </div>
                  
                  <div className="fine-amount-section">
                    <div className="amount-info">
                      <div className="amount-label">Fine Amount</div>
                      <div className="amount-value">${fine.amount.toFixed(2)}</div>
                    </div>
                    
                    <div className="fine-actions">
                      {user.role === 'Member' ? (
                        fine.status === 'Outstanding' && (
                          <button className="btn btn-primary">Pay Fine</button>
                        )
                      ) : (
                        <div className="staff-actions">
                          {fine.status === 'Outstanding' && (
                            <>
                              <button className="btn btn-primary">Process Payment</button>
                              <button className="btn btn-outline">Send Notice</button>
                              <button className="btn btn-secondary">Waive Fine</button>
                            </>
                          )}
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {activeTab === 'history' && (
          <div className="payment-history-section">
            <div className="section-header">
              <h2>Payment History</h2>
            </div>
            
            <div className="payments-list">
              {paymentHistory.map(payment => (
                <div key={payment.id} className="payment-card">
                  <div className="payment-info">
                    <div className="payment-header">
                      <h3 className="payment-description">{payment.description}</h3>
                      <span className="payment-amount">${payment.amount.toFixed(2)}</span>
                    </div>
                    
                    <div className="payment-details">
                      {user.role !== 'Member' && (
                        <p className="member-name">Member: {payment.memberName}</p>
                      )}
                      <div className="payment-meta">
                        <span className="payment-date">{payment.date}</span>
                        <span className="payment-method">{payment.method}</span>
                        <span className="payment-id">ID: {payment.transactionId}</span>
                      </div>
                    </div>
                  </div>
                  
                  <div className="payment-actions">
                    <button className="btn btn-outline">View Receipt</button>
                    {user.role !== 'Member' && (
                      <button className="btn btn-outline">Refund</button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      {((activeTab === 'outstanding' && fines.length === 0) || 
        (activeTab === 'history' && paymentHistory.length === 0)) && (
        <div className="empty-state">
          <div className="empty-icon">
            {activeTab === 'outstanding' ? '💰' : '📄'}
          </div>
          <h3>
            {activeTab === 'outstanding' ? 'No outstanding fines' : 'No payment history'}
          </h3>
          <p>
            {user.role === 'Member' 
              ? activeTab === 'outstanding' 
                ? 'You have no outstanding fines. Keep up the good work!'
                : 'No payment history found.'
              : activeTab === 'outstanding'
                ? 'No fines to manage at this time.'
                : 'No payment records found.'
            }
          </p>
        </div>
      )}
    </div>
  );
};

const getMockFinesForRole = (role) => {
  if (role === 'Member') {
    return [
      {
        id: 1,
        reason: 'Overdue Book',
        bookTitle: 'To Kill a Mockingbird',
        bookAuthor: 'Harper Lee',
        amount: 3.50,
        dueDate: '2024-09-01',
        overdueDate: '2024-09-02',
        daysOverdue: 7,
        status: 'Outstanding'
      }
    ];
  } else {
    return [
      {
        id: 1,
        reason: 'Overdue Book',
        bookTitle: '1984',
        bookAuthor: 'George Orwell',
        memberName: 'Jane Doe',
        amount: 7.00,
        dueDate: '2024-08-25',
        overdueDate: '2024-08-26',
        daysOverdue: 14,
        status: 'Outstanding'
      },
      {
        id: 2,
        reason: 'Lost Book',
        bookTitle: 'The Great Gatsby',
        bookAuthor: 'F. Scott Fitzgerald',
        memberName: 'John Smith',
        amount: 25.00,
        dueDate: '2024-08-15',
        overdueDate: '2024-08-16',
        daysOverdue: 24,
        status: 'Outstanding'
      },
      {
        id: 3,
        reason: 'Overdue Book',
        bookTitle: 'Pride and Prejudice',
        bookAuthor: 'Jane Austen',
        memberName: 'Alice Wilson',
        amount: 2.50,
        dueDate: '2024-08-20',
        overdueDate: '2024-08-21',
        daysOverdue: 19,
        status: 'Paid'
      }
    ];
  }
};

const getMockPaymentHistoryForRole = (role) => {
  if (role === 'Member') {
    return [
      {
        id: 1,
        description: 'Overdue fine - "The Hobbit"',
        amount: 5.00,
        date: '2024-08-15',
        method: 'Credit Card',
        transactionId: 'TXN-001234'
      },
      {
        id: 2,
        description: 'Late return fine - "Dune"',
        amount: 2.50,
        date: '2024-07-28',
        method: 'Cash',
        transactionId: 'TXN-001123'
      }
    ];
  } else {
    return [
      {
        id: 1,
        description: 'Overdue fine payment',
        memberName: 'Alice Wilson',
        amount: 2.50,
        date: '2024-08-25',
        method: 'Credit Card',
        transactionId: 'TXN-001256'
      },
      {
        id: 2,
        description: 'Lost book replacement fee',
        memberName: 'Bob Johnson',
        amount: 30.00,
        date: '2024-08-20',
        method: 'Debit Card',
        transactionId: 'TXN-001245'
      },
      {
        id: 3,
        description: 'Overdue fine payment',
        memberName: 'Charlie Brown',
        amount: 7.50,
        date: '2024-08-18',
        method: 'Cash',
        transactionId: 'TXN-001234'
      }
    ];
  }
};

export default FinesPayment;
