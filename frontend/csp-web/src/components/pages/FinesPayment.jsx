import { useState, useEffect } from 'react';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import './FinesPayment.css';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5192';

const FinesPayment = ({ user }) => {
  const [fines, setFines] = useState([]);
  const [paymentHistory, setPaymentHistory] = useState([]);
  const [statistics, setStatistics] = useState(null);
  const [activeTab, setActiveTab] = useState('outstanding');
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);

  useEffect(() => {
    fetchFinesData();
  }, [user.role]);

  const fetchFinesData = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem('token');
      
      // Fetch fines based on user role
      const finesEndpoint = user.role === 'Member' 
        ? `${API_BASE_URL}/api/fines/my-fines`
        : `${API_BASE_URL}/api/fines`;
      
      console.log('Fetching fines from:', finesEndpoint);
      
      const finesResponse = await fetch(finesEndpoint, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });
      
      console.log('Fines response status:', finesResponse.status);
      
      if (finesResponse.ok) {
        const finesData = await finesResponse.json();
        console.log('Fines data received:', finesData);
        console.log('Fines items:', finesData.items);
        console.log('Number of fines:', finesData.items?.length || 0);
        setFines(finesData.items || []);
      } else {
        const errorText = await finesResponse.text();
        console.error('Failed to fetch fines. Status:', finesResponse.status);
        console.error('Error response:', errorText);
      }

      // Fetch payment history based on user role
      const paymentsEndpoint = user.role === 'Member'
        ? `${API_BASE_URL}/api/fines/my-payments`
        : `${API_BASE_URL}/api/fines/payments`;
      
      const paymentsResponse = await fetch(paymentsEndpoint, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });
      
      if (paymentsResponse.ok) {
        const paymentsData = await paymentsResponse.json();
        setPaymentHistory(paymentsData.items || []);
      }

      // Fetch statistics if member
      if (user.role === 'Member') {
        const statsResponse = await fetch(`${API_BASE_URL}/api/fines/my-statistics`, {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });
        
        if (statsResponse.ok) {
          const statsData = await statsResponse.json();
          setStatistics(statsData);
        }
      }
    } catch (error) {
      console.error('Error fetching fines data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handlePayFine = async (fineId) => {
    if (!confirm('Are you sure you want to pay this fine?')) {
      return;
    }

    try {
      setProcessing(true);
      const token = localStorage.getItem('token');
      
      const response = await fetch(`${API_BASE_URL}/api/fines/pay`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          fineId: fineId
        })
      });

      if (response.ok) {
        alert('Payment processed successfully!');
        fetchFinesData(); // Refresh data
      } else {
        const error = await response.json();
        alert(`Payment failed: ${error.message}`);
      }
    } catch (error) {
      console.error('Error processing payment:', error);
      alert('Failed to process payment. Please try again.');
    } finally {
      setProcessing(false);
    }
  };

  const handleWaiveFine = async (fineId) => {
    const reason = prompt('Enter reason for waiving fine:');
    if (!reason) return;

    try {
      setProcessing(true);
      const token = localStorage.getItem('token');
      
      const response = await fetch(`${API_BASE_URL}/api/fines/waive`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          fineId: fineId,
          reason: reason
        })
      });

      if (response.ok) {
        alert('Fine waived successfully!');
        fetchFinesData(); // Refresh data
      } else {
        const error = await response.json();
        alert(`Failed to waive fine: ${error.message}`);
      }
    } catch (error) {
      console.error('Error waiving fine:', error);
      alert('Failed to waive fine. Please try again.');
    } finally {
      setProcessing(false);
    }
  };

  const handleAdjustFineAmount = async (fineId, currentAmount) => {
    const newAmountStr = prompt(`Enter new fine amount (Current: Rs ${currentAmount.toFixed(2)}):`);
    if (!newAmountStr) return;

    const newAmount = parseFloat(newAmountStr);
    if (isNaN(newAmount) || newAmount < 0) {
      alert('Please enter a valid amount');
      return;
    }

    const reason = prompt('Enter reason for adjustment:');
    if (!reason) return;

    try {
      setProcessing(true);
      const token = localStorage.getItem('token');
      
      const response = await fetch(`${API_BASE_URL}/api/fines/adjust-amount`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          fineId: fineId,
          newAmount: newAmount,
          reason: reason
        })
      });

      if (response.ok) {
        alert('Fine amount adjusted successfully!');
        fetchFinesData(); // Refresh data
      } else {
        const error = await response.json();
        alert(`Failed to adjust fine amount: ${error.message}`);
      }
    } catch (error) {
      console.error('Error adjusting fine amount:', error);
      alert('Failed to adjust fine amount. Please try again.');
    } finally {
      setProcessing(false);
    }
  };

  const handleExportReport = () => {
    try {
      const doc = new jsPDF();
      
      // Add title
      doc.setFontSize(20);
      doc.setFont('helvetica', 'bold');
      doc.text('Lector Library Fines Report', 105, 20, { align: 'center' });
      
      // Add generation date
      doc.setFontSize(10);
      doc.setFont('helvetica', 'normal');
      doc.text(`Generated on: ${new Date().toLocaleString()}`, 105, 28, { align: 'center' });
      
      // Add summary statistics
      doc.setFontSize(12);
      doc.setFont('helvetica', 'bold');
      doc.text('Summary', 14, 40);
      
      const outstandingFines = fines.filter(f => f.status === 'Outstanding');
      const paidFines = fines.filter(f => f.status === 'Paid');
      const waivedFines = fines.filter(f => f.status === 'Waived');
      const totalOutstanding = outstandingFines.reduce((sum, f) => sum + f.amount, 0);
      const totalPaid = paidFines.reduce((sum, f) => sum + f.amount, 0);
      const totalWaived = waivedFines.reduce((sum, f) => sum + f.amount, 0);
      
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(10);
      doc.text(`Total Fines: ${fines.length}`, 14, 48);
      doc.text(`Outstanding: ${outstandingFines.length} (Rs ${totalOutstanding.toFixed(2)})`, 14, 54);
      doc.text(`Paid: ${paidFines.length} (Rs ${totalPaid.toFixed(2)})`, 14, 60);
      doc.text(`Waived: ${waivedFines.length} (Rs ${totalWaived.toFixed(2)})`, 14, 66);
      
      // Prepare table data
      const tableData = fines.map(fine => [
        fine.memberName,
        fine.bookTitle,
        fine.reason,
        new Date(fine.dueDate).toLocaleDateString(),
        fine.daysOverdue,
        `Rs ${fine.amount.toFixed(2)}`,
        fine.status
      ]);
      
      // Add fines table using autoTable
      autoTable(doc, {
        startY: 75,
        head: [['Member', 'Book', 'Reason', 'Due Date', 'Days', 'Amount', 'Status']],
        body: tableData,
        theme: 'grid',
        styles: {
          fontSize: 8,
          cellPadding: 3,
        },
        headStyles: {
          fillColor: [41, 128, 185],
          textColor: 255,
          fontStyle: 'bold',
        },
        columnStyles: {
          0: { cellWidth: 30 },  // Member
          1: { cellWidth: 35 },  // Book
          2: { cellWidth: 40 },  // Reason
          3: { cellWidth: 25 },  // Due Date
          4: { cellWidth: 15 },  // Days
          5: { cellWidth: 22 },  // Amount
          6: { cellWidth: 20 },  // Status
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
      const fileName = `Fines_Report_${new Date().toISOString().split('T')[0]}.pdf`;
      doc.save(fileName);
      
      alert('Report exported successfully!');
    } catch (error) {
      console.error('Error generating report:', error);
      alert('Failed to generate report. Please try again.');
    }
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
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
              <div className="total-amount">Rs {totalOutstanding.toFixed(2)}</div>
              <p>{fines.filter(f => f.status === 'Outstanding').length} unpaid fine(s)</p>
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
                  <button className="btn btn-outline" onClick={handleExportReport}>
                    Export Report
                  </button>
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
                          <span className="date-value">{formatDate(fine.dueDate)}</span>
                        </div>
                        <div className="date-item">
                          <span className="date-label">Overdue Since:</span>
                          <span className="date-value">{formatDate(fine.overdueDate)}</span>
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
                      <div className="amount-value">Rs {fine.amount.toFixed(2)}</div>
                    </div>
                    
                    <div className="fine-actions">
                      {user.role === 'Member' ? (
                        null
                      ) : (
                        <div className="staff-actions">
                          {fine.status === 'Outstanding' && (
                            <>
                              <button 
                                className="btn btn-primary"
                                onClick={() => handlePayFine(fine.id)}
                                disabled={processing}
                              >
                                Process Payment
                              </button>
                              <button 
                                className="btn btn-outline"
                                onClick={() => handleAdjustFineAmount(fine.id, fine.amount)}
                                disabled={processing}
                              >
                                Adjust Amount
                              </button>
                              <button 
                                className="btn btn-secondary"
                                onClick={() => handleWaiveFine(fine.id)}
                                disabled={processing}
                              >
                                Waive Fine
                              </button>
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
                      <span className="payment-amount">Rs {payment.amount.toFixed(2)}</span>
                    </div>
                    
                    <div className="payment-details">
                      {user.role !== 'Member' && (
                        <p className="member-name">Member: {payment.memberName}</p>
                      )}
                      <div className="payment-meta">
                        <span className="payment-date">{formatDate(payment.paymentDate)}</span>
                        <span className="payment-id">Transaction ID: {payment.transactionId}</span>
                      </div>
                    </div>
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

export default FinesPayment;
