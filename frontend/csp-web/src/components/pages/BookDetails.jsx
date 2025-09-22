import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import './BookDetails.css';
import api from '../../lib/api';

const BookDetails = ({ user }) => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [book, setBook] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

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

  const handleReserve = () => {
    // TODO: Implement reservation functionality
    alert('Reservation functionality will be implemented in a future sprint');
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
                disabled={book.availableCopies === 0}
                className={`btn ${book.availableCopies > 0 ? 'btn-primary' : 'btn-disabled'}`}
              >
                {book.availableCopies > 0 ? 'Reserve This Book' : 'Currently Unavailable'}
              </button>
              <p className="action-note">
                {book.availableCopies > 0 
                  ? 'Click to reserve this book for pickup'
                  : 'This book is currently checked out. Check back later or place a hold.'
                }
              </p>
            </div>
          )}

          {user.role === 'Librarian' || user.role === 'Administrator' ? (
            <div className="staff-actions">
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
