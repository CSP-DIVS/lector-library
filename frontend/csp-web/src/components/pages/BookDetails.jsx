import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import './BookDetails.css';
import api, { reservationApi, lendingApi } from '../../lib/api';

const BookDetails = ({ user }) => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [book, setBook] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [reserving, setReserving] = useState(false);
  const [borrowing, setBorrowing] = useState(false);

  useEffect(() => {
    if (id) {
      fetchBookDetails();
    }
  }, [id]);

  const fetchBookDetails = async () => {
    try {
      setLoading(true);
      const response = await api.get(`/books/${id}`);
      setBook(response.data);
    } catch (error) {
      console.error('Error fetching book details:', error);
      setError('Failed to load book details');
    } finally {
      setLoading(false);
    }
  };

  const handleReserve = async () => {
    if (!user || !user.id) {
      alert('Please log in to reserve a book');
      return;
    }

    if (book.availableCopies <= 0) {
      alert('This book is currently unavailable for reservation');
      return;
    }

    if (window.confirm(`Reserve "${book.title}"?\n\nYou will be notified when the book is available for pickup.`)) {
      try {
        setReserving(true);
        const response = await reservationApi.createReservation(book.id, user.id);
        
        if (response.data.success) {
          alert(`Success! ${response.data.message}`);
          // Refresh book details to update availability
          fetchBookDetails();
        } else {
          alert(`Failed to reserve: ${response.data.message}`);
        }
      } catch (error) {
        console.error('Error creating reservation:', error);
        const errorMessage = error.response?.data?.message || 'Failed to create reservation. Please try again.';
        alert(errorMessage);
      } finally {
        setReserving(false);
      }
    }
  };

  const handleIssueBook = async () => {
    // Prompt for member ID/email
    const memberIdentifier = prompt('Enter member email or ID to issue this book:');
    if (!memberIdentifier) return;

    if (book.availableCopies <= 0) {
      alert('No copies available to issue');
      return;
    }

    try {
      setBorrowing(true);
      // First, get user by email or ID
      const searchResponse = await api.get('/users', {
        params: { search: memberIdentifier, pageSize: 1 }
      });

      if (!searchResponse.data.items || searchResponse.data.items.length === 0) {
        alert('Member not found. Please check the email or ID.');
        return;
      }

      const member = searchResponse.data.items[0];
      
      if (window.confirm(`Issue "${book.title}" to ${member.username} (${member.email})?\n\nLoan period: 14 days`)) {
        const borrowResponse = await lendingApi.borrowBook(book.id, member.id, 14);
        
        if (borrowResponse.data.success) {
          alert(`✓ Success!\n\nBook issued to ${member.username}\nDue date: ${new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toLocaleDateString()}`);
          fetchBookDetails();
        } else {
          alert(`Failed: ${borrowResponse.data.message}`);
        }
      }
    } catch (error) {
      console.error('Error issuing book:', error);
      const errorMessage = error.response?.data?.message || 'Failed to issue book. Please try again.';
      alert(errorMessage);
    } finally {
      setBorrowing(false);
    }
  };

  const handleBack = () => {
    navigate(-1);
  };

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>Loading book details...</p>
      </div>
    );
  }

  if (error || !book) {
    return (
      <div className="error-container">
        <div className="error-icon">❌</div>
        <h3>Book Not Found</h3>
        <p>{error || 'The book you are looking for does not exist.'}</p>
        <button onClick={handleBack} className="btn btn-primary">
          Go Back
        </button>
      </div>
    );
  }

  return (
    <div className="book-details-page">
      <div className="book-details-header">
        <button onClick={handleBack} className="btn btn-outline back-button">
          ← Back
        </button>
        <h1>Book Details</h1>
      </div>

      <div className="book-details-content">
        <div className="book-cover-section">
          <div className="book-cover-large">
            <div className="book-cover-placeholder">
              <span className="book-initial">{book.title.charAt(0)}</span>
            </div>
            <div className={`availability-badge-large ${book.status.toLowerCase()}`}>
              {book.status}
            </div>
          </div>
        </div>

        <div className="book-info-section">
          <h2 className="book-title">{book.title}</h2>
          <p className="book-author">by {book.author}</p>
          
          <div className="book-meta">
            <div className="meta-item">
              <label>ISBN:</label>
              <span>{book.isbn}</span>
            </div>
            <div className="meta-item">
              <label>Category:</label>
              <span>{book.category}</span>
            </div>
            <div className="meta-item">
              <label>Published Year:</label>
              <span>{book.publishedYear}</span>
            </div>
            <div className="meta-item">
              <label>Added to Library:</label>
              <span>{new Date(book.createdAt).toLocaleDateString()}</span>
            </div>
          </div>

          <div className="availability-section">
            <h3>Availability</h3>
            <div className="availability-info">
              <div className="availability-stats">
                <div className="stat-item">
                  <span className="stat-label">Available Copies:</span>
                  <span className="stat-value available">{book.availableCopies}</span>
                </div>
                <div className="stat-item">
                  <span className="stat-label">Total Copies:</span>
                  <span className="stat-value total">{book.totalCopies}</span>
                </div>
              </div>
              
              <div className="availability-bar-large">
                <div 
                  className="availability-fill"
                  style={{ 
                    width: `${(book.availableCopies / book.totalCopies) * 100}%` 
                  }}
                ></div>
              </div>
              
              <p className="availability-text">
                {book.availableCopies > 0 
                  ? `${book.availableCopies} copy${book.availableCopies > 1 ? 'ies' : ''} available for borrowing`
                  : 'No copies currently available'
                }
              </p>
            </div>
          </div>

          {user.role === 'Member' && (
            <div className="member-actions">
              <button
                onClick={handleReserve}
                disabled={book.availableCopies === 0 || reserving}
                className={`btn ${book.availableCopies > 0 ? 'btn-primary' : 'btn-disabled'}`}
              >
                {reserving ? 'Reserving...' : (book.availableCopies > 0 ? 'Reserve This Book' : 'Currently Unavailable')}
              </button>
              <p className="action-note">
                {book.availableCopies > 0 
                  ? 'Click to reserve this book for pickup'
                  : 'This book is currently checked out. You can still reserve it to join the queue.'
                }
              </p>
            </div>
          )}

          {user.role === 'Librarian' || user.role === 'Administrator' ? (
            <div className="staff-actions">
              <button
                onClick={handleIssueBook}
                disabled={book.availableCopies === 0 || borrowing}
                className={`btn ${book.availableCopies > 0 ? 'btn-primary' : 'btn-disabled'}`}
              >
                {borrowing ? 'Processing...' : (book.availableCopies > 0 ? 'Issue Book to Member' : 'No Copies Available')}
              </button>
              <button
                onClick={() => navigate(`/admin/books/${book.id}/edit`)}
                className="btn btn-outline"
              >
                Edit Book Details
              </button>
              <button
                onClick={() => navigate('/admin/books')}
                className="btn btn-secondary"
              >
                Manage All Books
              </button>
            </div>
          ) : null}
        </div>
      </div>
    </div>
  );
};

export default BookDetails;
