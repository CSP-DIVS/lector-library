import { useState, useEffect } from 'react';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import { lendingApi, reservationApi } from '../../lib/api';
import './LendingReservation.css';

const LendingReservation = ({ user }) => {
  const [activeTab, setActiveTab] = useState('loans');
  const [loans, setLoans] = useState([]);
  const [reservations, setReservations] = useState([]);
  const [loanHistory, setLoanHistory] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [processing, setProcessing] = useState(false);
  const [detailModal, setDetailModal] = useState({ isOpen: false, type: null, data: null });

  useEffect(() => {
    fetchLendingData();
  }, [activeTab]);

  const fetchLendingData = async () => {
    setLoading(true);
    setError(null);
    try {
      if (activeTab === 'loans') {
        const response = await lendingApi.getActiveLoans();
        setLoans(response.data.items || []);
      } else if (activeTab === 'history') {
        const response = await lendingApi.getLoanHistory();
        setLoanHistory(response.data.items || []);
      } else {
        const response = user.role === 'Member' 
          ? await reservationApi.getMyReservations()
          : await reservationApi.getAllReservations();
        setReservations(response.data.items || []);
      }
    } catch (error) {
      console.error('Error fetching lending data:', error);
      setError(error.response?.data?.message || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  const handleRenewLoan = async (lendingId) => {
    try {
      const response = await lendingApi.renewLoan(lendingId);
      if (response.data.success) {
        alert(response.data.message);
        fetchLendingData();
      }
    } catch (error) {
      alert(error.response?.data?.message || 'Failed to renew loan');
    }
  };

  const handleReturnBook = async (lendingId) => {
    try {
      const response = await lendingApi.returnBook(lendingId);
      if (response.data.success) {
        alert(response.data.message);
        fetchLendingData();
      }
    } catch (error) {
      alert(error.response?.data?.message || 'Failed to process return');
    }
  };

  const handleCancelReservation = async (reservationId) => {
    if (!confirm('Are you sure you want to cancel this reservation?')) {
      return;
    }
    
    try {
      const response = await reservationApi.cancelReservation(reservationId);
      if (response.data.success) {
        alert(response.data.message);
        fetchLendingData();
      }
    } catch (error) {
      alert(error.response?.data?.message || 'Failed to cancel reservation');
    }
  };

  const handleFulfillReservation = async (reservation) => {
    const confirmMsg = `Fulfill reservation for "${reservation.bookTitle}"?\n\nThis will:\n- Issue the book to ${reservation.username}\n- Create an active loan (14-day period)\n- Mark the reservation as fulfilled\n\nProceed?`;
    
    if (!confirm(confirmMsg)) {
      return;
    }
    
    try {
      setProcessing(true);
      const response = await reservationApi.fulfillReservation(reservation.id);
      if (response.data.success) {
        alert(`✓ Success!\n\n${response.data.message}\n\nThe loan now appears in the Active Loans section.`);
        // Switch to loans tab to show the new loan
        setActiveTab('loans');
        // Refresh will happen due to tab change
      }
    } catch (error) {
      console.error('Error fulfilling reservation:', error);
      alert(error.response?.data?.message || 'Failed to fulfill reservation');
      // Refresh current data
      fetchLendingData();
    } finally {
      setProcessing(false);
    }
  };

  const formatDate = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  };

  const handleViewDetails = async (type, id) => {
    try {
      setProcessing(true);
      let response;
      if (type === 'loan') {
        response = await lendingApi.getLendingById(id);
      } else {
        response = await reservationApi.getReservationById(id);
      }
      
      if (response.data.success) {
        setDetailModal({ isOpen: true, type, data: response.data.data });
      }
    } catch (error) {
      alert(error.response?.data?.message || 'Failed to load details');
    } finally {
      setProcessing(false);
    }
  };

  const closeDetailModal = () => {
    setDetailModal({ isOpen: false, type: null, data: null });
  };

  const handleExportLoansReport = () => {
    try {
      const doc = new jsPDF();
      
      // Add title
      doc.setFontSize(20);
      doc.setFont('helvetica', 'bold');
      doc.text('Lector Library Active Loans Report', 105, 20, { align: 'center' });
      
      // Add generation date
      doc.setFontSize(10);
      doc.setFont('helvetica', 'normal');
      doc.text(`Generated on: ${new Date().toLocaleString()}`, 105, 28, { align: 'center' });
      
      // Add summary statistics
      doc.setFontSize(12);
      doc.setFont('helvetica', 'bold');
      doc.text('Summary', 14, 40);
      
      const overdueLoans = loans.filter(l => l.isOverdue);
      const totalOverdueDays = overdueLoans.reduce((sum, l) => sum + (l.overdueDays || 0), 0);
      
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(10);
      doc.text(`Total Active Loans: ${loans.length}`, 14, 48);
      doc.text(`Overdue Loans: ${overdueLoans.length}`, 14, 54);
      if (overdueLoans.length > 0) {
        doc.text(`Total Overdue Days: ${totalOverdueDays}`, 14, 60);
      }
      
      // Prepare table data
      const tableData = loans.map(loan => [
        loan.username || 'N/A',
        loan.bookTitle,
        loan.bookAuthor,
        formatDate(loan.borrowDate),
        formatDate(loan.dueDate),
        `${loan.renewalCount}/${loan.maxRenewals}`,
        loan.isOverdue ? `Yes (${loan.overdueDays}d)` : 'No'
      ]);
      
      // Add loans table
      autoTable(doc, {
        startY: overdueLoans.length > 0 ? 68 : 62,
        head: [['Member', 'Book Title', 'Author', 'Borrowed', 'Due Date', 'Renewals', 'Overdue']],
        body: tableData,
        theme: 'grid',
        styles: {
          fontSize: 8,
          cellPadding: 3,
        },
        headStyles: {
          fillColor: [52, 152, 219],
          textColor: 255,
          fontStyle: 'bold',
        },
        columnStyles: {
          0: { cellWidth: 28 },  // Member
          1: { cellWidth: 40 },  // Book Title
          2: { cellWidth: 32 },  // Author
          3: { cellWidth: 25 },  // Borrowed
          4: { cellWidth: 25 },  // Due Date
          5: { cellWidth: 20 },  // Renewals
          6: { cellWidth: 22 },  // Overdue
        },
        alternateRowStyles: {
          fillColor: [245, 245, 245]
        },
        didDrawPage: (data) => {
          // Footer
          const pageCount = doc.internal.getNumberOfPages();
          doc.setFontSize(8);
          doc.setFont('helvetica', 'normal');
          doc.text(
            `Page ${data.pageNumber} of ${pageCount}`,
            doc.internal.pageSize.width / 2,
            doc.internal.pageSize.height - 10,
            { align: 'center' }
          );
        }
      });
      
      // Save the PDF
      const fileName = `Active_Loans_Report_${new Date().toISOString().split('T')[0]}.pdf`;
      doc.save(fileName);
      
      alert('Loans report exported successfully!');
    } catch (error) {
      console.error('Error generating loans report:', error);
      alert('Failed to generate loans report. Please try again.');
    }
  };

  const handleExportReservationsReport = () => {
    try {
      const doc = new jsPDF();
      
      // Add title
      doc.setFontSize(20);
      doc.setFont('helvetica', 'bold');
      doc.text('Pending Reservations Report', 105, 20, { align: 'center' });
      
      // Add generation date
      doc.setFontSize(10);
      doc.setFont('helvetica', 'normal');
      doc.text(`Generated on: ${new Date().toLocaleString()}`, 105, 28, { align: 'center' });
      
      // Add summary statistics
      doc.setFontSize(12);
      doc.setFont('helvetica', 'bold');
      doc.text('Summary', 14, 40);
      
      const pendingReservations = reservations.filter(r => r.status === 'Pending');
      const fulfilledToday = reservations.filter(r => {
        const today = new Date().toDateString();
        return r.status === 'Fulfilled' && new Date(r.fulfilledAt).toDateString() === today;
      });
      
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(10);
      doc.text(`Total Reservations: ${reservations.length}`, 14, 48);
      doc.text(`Pending: ${pendingReservations.length}`, 14, 54);
      doc.text(`Fulfilled Today: ${fulfilledToday.length}`, 14, 60);
      
      // Prepare table data
      const tableData = reservations.map(reservation => [
        reservation.username || 'N/A',
        reservation.bookTitle,
        reservation.bookAuthor,
        formatDate(reservation.reservationDate),
        reservation.status,
        reservation.expiresAt ? formatDate(reservation.expiresAt) : 'N/A'
      ]);
      
      // Add reservations table
      autoTable(doc, {
        startY: 68,
        head: [['Member', 'Book Title', 'Author', 'Reserved On', 'Status', 'Expires']],
        body: tableData,
        theme: 'grid',
        styles: {
          fontSize: 8,
          cellPadding: 3,
        },
        headStyles: {
          fillColor: [155, 89, 182],
          textColor: 255,
          fontStyle: 'bold',
        },
        columnStyles: {
          0: { cellWidth: 30 },  // Member
          1: { cellWidth: 45 },  // Book Title
          2: { cellWidth: 35 },  // Author
          3: { cellWidth: 28 },  // Reserved On
          4: { cellWidth: 25 },  // Status
          5: { cellWidth: 25 },  // Expires
        },
        alternateRowStyles: {
          fillColor: [245, 245, 245]
        },
        didDrawPage: (data) => {
          // Footer
          const pageCount = doc.internal.getNumberOfPages();
          doc.setFontSize(8);
          doc.setFont('helvetica', 'normal');
          doc.text(
            `Page ${data.pageNumber} of ${pageCount}`,
            doc.internal.pageSize.width / 2,
            doc.internal.pageSize.height - 10,
            { align: 'center' }
          );
        }
      });
      
      // Save the PDF
      const fileName = `Reservations_Report_${new Date().toISOString().split('T')[0]}.pdf`;
      doc.save(fileName);
      
      alert('Reservations report exported successfully!');
    } catch (error) {
      console.error('Error generating reservations report:', error);
      alert('Failed to generate reservations report. Please try again.');
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

  if (error) {
    return (
      <div className="error-container">
        <p className="error-message">{error}</p>
        <button className="btn btn-primary" onClick={fetchLendingData}>Retry</button>
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
          <button
            className={`tab ${activeTab === 'history' ? 'active' : ''}`}
            onClick={() => setActiveTab('history')}
          >
            History
            <span className="tab-count">{loanHistory.length}</span>
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
              {user.role !== 'Member' && loans.length > 0 && (
                <div className="section-actions">
                  <button className="btn btn-outline" onClick={handleExportLoansReport}>
                    Export Report
                  </button>
                </div>
              )}
            </div>
            
            <div className="items-list">
              {loans.length > 0 ? loans.map(loan => (
                <div key={loan.id} className="loan-card">
                  <div className="item-info">
                    <div className="book-details">
                      <h3 className="book-title">{loan.bookTitle}</h3>
                      <p className="book-author">by {loan.bookAuthor}</p>
                      {user.role !== 'Member' && (
                        <p className="member-name">Member: {loan.username} ({loan.memberEmail})</p>
                      )}
                    </div>
                    
                    <div className="loan-details">
                      <div className="loan-dates">
                        <div className="date-item">
                          <span className="date-label">Borrowed:</span>
                          <span className="date-value">{formatDate(loan.borrowDate)}</span>
                        </div>
                        <div className="date-item">
                          <span className="date-label">Due:</span>
                          <span className={`date-value ${loan.isOverdue ? 'overdue' : ''}`}>
                            {formatDate(loan.dueDate)}
                          </span>
                        </div>
                        {loan.renewalCount > 0 && (
                          <div className="date-item">
                            <span className="date-label">Renewals:</span>
                            <span className="date-value">{loan.renewalCount}/{loan.maxRenewals}</span>
                          </div>
                        )}
                      </div>
                      
                      {loan.isOverdue && (
                        <div className="overdue-notice">
                          <span className="overdue-icon">⚠️</span>
                          <span>Overdue by {loan.overdueDays} days</span>
                          {loan.fineAmount && <span className="fine-amount">${loan.fineAmount.toFixed(2)}</span>}
                        </div>
                      )}
                    </div>
                  </div>
                  
                  <div className="loan-actions">
                    {user.role === 'Member' ? (
                      <div className="member-actions">
                        <button 
                          className="btn btn-outline" 
                          onClick={() => handleRenewLoan(loan.id)}
                          disabled={loan.renewalCount >= loan.maxRenewals}
                        >
                          {loan.renewalCount >= loan.maxRenewals ? 'Max Renewals' : 'Renew'}
                        </button>
                      </div>
                    ) : (
                      <div className="staff-actions">
                        <button 
                          className="btn btn-primary"
                          onClick={() => handleReturnBook(loan.id)}
                        >
                          Process Return
                        </button>
                      </div>
                    )}
                  </div>
                </div>
              )) : (
                <div className="empty-state">
                  <div className="empty-icon">📚</div>
                  <h3>No active loans</h3>
                  <p>
                    {user.role === 'Member' 
                      ? "You don't have any books on loan at the moment."
                      : "No active loans to manage at this time."
                    }
                  </p>
                </div>
              )}
            </div>
          </div>
        )}

        {activeTab === 'reservations' && (
          <div className="reservations-section">
            <div className="section-header">
              <h2>
                {user.role === 'Member' ? 'My Reservations' : 'Pending Reservations'}
              </h2>
              {user.role !== 'Member' && reservations.length > 0 && (
                <div className="section-actions">
                  <button className="btn btn-outline" onClick={handleExportReservationsReport}>
                    Export Report
                  </button>
                </div>
              )}
            </div>
            
            <div className="items-list">
              {reservations.length > 0 ? reservations.map(reservation => (
                <div key={reservation.id} className="reservation-card">
                  <div className="item-info">
                    <div className="book-details">
                      <h3 className="book-title">{reservation.bookTitle}</h3>
                      <p className="book-author">by {reservation.bookAuthor}</p>
                      {user.role !== 'Member' && (
                        <p className="member-name">Member: {reservation.username} ({reservation.memberEmail})</p>
                      )}
                    </div>
                    
                    <div className="reservation-details">
                      <div className="reservation-info">
                        <div className="info-item">
                          <span className="info-label">Reserved:</span>
                          <span className="info-value">{formatDate(reservation.reservedDate)}</span>
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
                        <button 
                          className="btn btn-danger"
                          onClick={() => handleCancelReservation(reservation.id)}
                        >
                          Cancel Reservation
                        </button>
                      </div>
                    ) : (
                      <div className="staff-actions">
                        {(reservation.status === 'Available' || reservation.status === 'Pending') && (
                          <button 
                            className="btn btn-primary"
                            onClick={() => handleFulfillReservation(reservation)}
                            disabled={processing}
                          >
                            {processing ? 'Processing...' : reservation.status === 'Available' ? 'Issue Book & Fulfill' : 'Issue Book Now'}
                          </button>
                        )}
                      </div>
                    )}
                  </div>
                </div>
              )) : (
                <div className="empty-state">
                  <div className="empty-icon">🔖</div>
                  <h3>No reservations</h3>
                  <p>
                    {user.role === 'Member' 
                      ? "You don't have any reservations at the moment."
                      : "No reservations to manage at this time."
                    }
                  </p>
                </div>
              )}
            </div>
          </div>
        )}

        {activeTab === 'history' && (
          <div className="history-section">
            <div className="section-header">
              <h2>Loan History</h2>
            </div>
            
            <div className="items-list">
              {loanHistory.length > 0 ? loanHistory.map(loan => (
                <div key={loan.id} className="history-card">
                  <div className="item-info">
                    <div className="book-details">
                      <h3 className="book-title">{loan.bookTitle}</h3>
                      <p className="book-author">by {loan.bookAuthor}</p>
                      {user.role !== 'Member' && (
                        <p className="member-name">Member: {loan.username} ({loan.memberEmail})</p>
                      )}
                    </div>
                    
                    <div className="history-details">
                      <div className="history-dates">
                        <div className="date-item">
                          <span className="date-label">Borrowed:</span>
                          <span className="date-value">{formatDate(loan.borrowDate)}</span>
                        </div>
                        <div className="date-item">
                          <span className="date-label">Returned:</span>
                          <span className="date-value">{formatDate(loan.returnDate)}</span>
                        </div>
                        {loan.renewalCount > 0 && (
                          <div className="date-item">
                            <span className="date-label">Renewed:</span>
                            <span className="date-value">{loan.renewalCount} times</span>
                          </div>
                        )}
                      </div>
                      
                      <div className="history-status">
                        <span className={`status-badge ${loan.status.toLowerCase()}`}>
                          {loan.status}
                        </span>
                        {loan.fineAmount > 0 && (
                          <span className="fine-badge">Fine: ${loan.fineAmount.toFixed(2)}</span>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              )) : (
                <div className="empty-state">
                  <div className="empty-icon">📜</div>
                  <h3>No loan history</h3>
                  <p>
                    {user.role === 'Member' 
                      ? "You haven't borrowed any books yet."
                      : "No loan history available."
                    }
                  </p>
                </div>
              )}
            </div>
          </div>
        )}
      </div>

      {/* Detail Modal */}
      {detailModal.isOpen && (
        <div className="modal-overlay" onClick={closeDetailModal}>
          <div className="modal-content detail-modal" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>{detailModal.type === 'loan' ? 'Loan Details' : 'Reservation Details'}</h2>
              <button className="modal-close" onClick={closeDetailModal}>×</button>
            </div>
            
            <div className="modal-body">
              {detailModal.type === 'loan' && detailModal.data && (
                <div className="detail-content">
                  <div className="detail-section">
                    <h3>Book Information</h3>
                    <div className="detail-row">
                      <span className="detail-label">Title:</span>
                      <span className="detail-value">{detailModal.data.bookTitle}</span>
                    </div>
                    <div className="detail-row">
                      <span className="detail-label">Author:</span>
                      <span className="detail-value">{detailModal.data.bookAuthor}</span>
                    </div>
                    <div className="detail-row">
                      <span className="detail-label">ISBN:</span>
                      <span className="detail-value">{detailModal.data.isbn || 'N/A'}</span>
                    </div>
                  </div>

                  <div className="detail-section">
                    <h3>Member Information</h3>
                    <div className="detail-row">
                      <span className="detail-label">Name:</span>
                      <span className="detail-value">{detailModal.data.username}</span>
                    </div>
                    <div className="detail-row">
                      <span className="detail-label">Email:</span>
                      <span className="detail-value">{detailModal.data.memberEmail}</span>
                    </div>
                    <div className="detail-row">
                      <span className="detail-label">User ID:</span>
                      <span className="detail-value">{detailModal.data.userId}</span>
                    </div>
                  </div>

                  <div className="detail-section">
                    <h3>Loan Information</h3>
                    <div className="detail-row">
                      <span className="detail-label">Borrow Date:</span>
                      <span className="detail-value">{formatDate(detailModal.data.borrowDate)}</span>
                    </div>
                    <div className="detail-row">
                      <span className="detail-label">Due Date:</span>
                      <span className={`detail-value ${detailModal.data.isOverdue ? 'text-danger' : ''}`}>
                        {formatDate(detailModal.data.dueDate)}
                      </span>
                    </div>
                    {detailModal.data.returnDate && (
                      <div className="detail-row">
                        <span className="detail-label">Return Date:</span>
                        <span className="detail-value">{formatDate(detailModal.data.returnDate)}</span>
                      </div>
                    )}
                    <div className="detail-row">
                      <span className="detail-label">Status:</span>
                      <span className={`status-badge ${detailModal.data.status.toLowerCase()}`}>
                        {detailModal.data.status}
                      </span>
                    </div>
                    <div className="detail-row">
                      <span className="detail-label">Renewals:</span>
                      <span className="detail-value">
                        {detailModal.data.renewalCount} / {detailModal.data.maxRenewals}
                      </span>
                    </div>
                    {detailModal.data.isOverdue && (
                      <div className="detail-row">
                        <span className="detail-label">Overdue Days:</span>
                        <span className="detail-value text-danger">{detailModal.data.overdueDays} days</span>
                      </div>
                    )}
                    {detailModal.data.fineAmount > 0 && (
                      <div className="detail-row">
                        <span className="detail-label">Fine Amount:</span>
                        <span className="detail-value text-danger">${detailModal.data.fineAmount.toFixed(2)}</span>
                      </div>
                    )}
                  </div>
                </div>
              )}

              {detailModal.type === 'reservation' && detailModal.data && (
                <div className="detail-content">
                  <div className="detail-section">
                    <h3>Book Information</h3>
                    <div className="detail-row">
                      <span className="detail-label">Title:</span>
                      <span className="detail-value">{detailModal.data.bookTitle}</span>
                    </div>
                    <div className="detail-row">
                      <span className="detail-label">Author:</span>
                      <span className="detail-value">{detailModal.data.bookAuthor}</span>
                    </div>
                  </div>

                  <div className="detail-section">
                    <h3>Member Information</h3>
                    <div className="detail-row">
                      <span className="detail-label">Name:</span>
                      <span className="detail-value">{detailModal.data.username}</span>
                    </div>
                    <div className="detail-row">
                      <span className="detail-label">Email:</span>
                      <span className="detail-value">{detailModal.data.memberEmail}</span>
                    </div>
                  </div>

                  <div className="detail-section">
                    <h3>Reservation Information</h3>
                    <div className="detail-row">
                      <span className="detail-label">Reserved Date:</span>
                      <span className="detail-value">{formatDate(detailModal.data.reservedDate)}</span>
                    </div>
                    <div className="detail-row">
                      <span className="detail-label">Queue Position:</span>
                      <span className="detail-value">#{detailModal.data.queuePosition}</span>
                    </div>
                    <div className="detail-row">
                      <span className="detail-label">Status:</span>
                      <span className={`status-badge ${detailModal.data.status.toLowerCase()}`}>
                        {detailModal.data.status}
                      </span>
                    </div>
                    {detailModal.data.availableDate && (
                      <div className="detail-row">
                        <span className="detail-label">Available Date:</span>
                        <span className="detail-value">{formatDate(detailModal.data.availableDate)}</span>
                      </div>
                    )}
                    {detailModal.data.fulfilledDate && (
                      <div className="detail-row">
                        <span className="detail-label">Fulfilled Date:</span>
                        <span className="detail-value">{formatDate(detailModal.data.fulfilledDate)}</span>
                      </div>
                    )}
                    {detailModal.data.cancelledDate && (
                      <div className="detail-row">
                        <span className="detail-label">Cancelled Date:</span>
                        <span className="detail-value">{formatDate(detailModal.data.cancelledDate)}</span>
                      </div>
                    )}
                    {detailModal.data.estimatedAvailable && (
                      <div className="detail-row">
                        <span className="detail-label">Estimated Available:</span>
                        <span className="detail-value">{detailModal.data.estimatedAvailable}</span>
                      </div>
                    )}
                  </div>
                </div>
              )}
            </div>
            
            <div className="modal-footer">
              <button className="btn btn-secondary" onClick={closeDetailModal}>Close</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default LendingReservation;
