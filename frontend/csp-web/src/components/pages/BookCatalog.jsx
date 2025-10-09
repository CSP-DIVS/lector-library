import { useState, useEffect } from 'react';
import './BookCatalog.css';
import api, { reservationApi } from '../../lib/api';

const BookCatalog = ({ user }) => {
  const [books, setBooks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [searching, setSearching] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterCategory, setFilterCategory] = useState('all');
  const [filterAuthor, setFilterAuthor] = useState('');
  const [categories, setCategories] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [message, setMessage] = useState({ type: '', text: '' });
  const [selectedBook, setSelectedBook] = useState(null);
  const [isInitialLoad, setIsInitialLoad] = useState(true);
  const [reserving, setReserving] = useState(false);

  const pageSize = 12;

  // Initial load
  useEffect(() => {
    fetchBooks();
    setIsInitialLoad(false);
  }, []);

  // Debounced search effect (skip initial load)
  useEffect(() => {
    if (isInitialLoad) return;
    
    setSearching(true);
    const timeoutId = setTimeout(() => {
      fetchBooks();
    }, 300); // 300ms delay

    return () => clearTimeout(timeoutId);
  }, [searchTerm, filterCategory, filterAuthor, currentPage]);

  const fetchBooks = async () => {
    try {
      setLoading(true);
      const response = await api.get('/books', {
        params: {
          search: searchTerm || undefined,
          category: filterCategory !== 'all' ? filterCategory : undefined,
          author: filterAuthor || undefined,
          page: currentPage,
          pageSize: pageSize
        }
      });
      
      setBooks(response.data.items || []);
      setTotalPages(Math.ceil(response.data.total / pageSize));
      
      // Extract unique categories for filter
      const uniqueCategories = [...new Set(response.data.items?.map(book => book.category) || [])];
      setCategories(uniqueCategories);
    } catch (error) {
      console.error('Error fetching books:', error);
      setMessage({ type: 'error', text: 'Failed to fetch books' });
    } finally {
      setLoading(false);
      setSearching(false);
    }
  };

  const handleSearch = (e) => {
    e.preventDefault();
    setCurrentPage(1);
    fetchBooks();
  };

  const handleBookClick = (book) => {
    setSelectedBook(book);
  };

  const handleReserve = async (book) => {
    if (!user || !user.id) {
      alert('Please log in to reserve a book');
      return;
    }

    if (window.confirm(`Reserve "${book.title}"?\n\nYou will be notified when the book is available for pickup.`)) {
      try {
        setReserving(true);
        const response = await reservationApi.createReservation(book.id, user.id);
        
        if (response.data.success) {
          alert(`Success! ${response.data.message}`);
          // Close modal and refresh book list
          setSelectedBook(null);
          fetchBooks();
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

  const handlePageChange = (page) => {
    setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const clearFilters = () => {
    setSearchTerm('');
    setFilterCategory('all');
    setFilterAuthor('');
    setCurrentPage(1);
  };

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>Loading books...</p>
      </div>
    );
  }

  return (
    <div className="book-catalog-page">
      <div className="page-header">
        <h1>Book Catalog</h1>
        <p className="page-subtitle">Discover and explore our collection of books</p>
      </div>

      {message.text && (
        <div className={`message ${message.type}`}>
          {message.text}
          <button onClick={() => setMessage({ type: '', text: '' })}>×</button>
        </div>
      )}

      <div className="catalog-filters">
        <form onSubmit={handleSearch} className="search-form">
          <div className="search-input-group">
            <div className="search-input-wrapper">
              <input
                type="text"
                placeholder="Search by title, author, or ISBN..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="search-input"
              />
              {searching && (
                <div className="search-loading">
                  <div className="search-spinner"></div>
                </div>
              )}
            </div>
            <button type="submit" className="btn btn-primary">
              Search
            </button>
          </div>
        </form>

        <div className="filter-controls">
          <div className="filter-group">
            <label htmlFor="category-filter">Category:</label>
            <select
              id="category-filter"
              value={filterCategory}
              onChange={(e) => setFilterCategory(e.target.value)}
              className="filter-select"
            >
              <option value="all">All Categories</option>
              {categories.map(category => (
                <option key={category} value={category}>{category}</option>
              ))}
            </select>
          </div>

          <div className="filter-group">
            <label htmlFor="author-filter">Author:</label>
            <input
              type="text"
              id="author-filter"
              placeholder="Filter by author..."
              value={filterAuthor}
              onChange={(e) => setFilterAuthor(e.target.value)}
              className="filter-input"
            />
          </div>

          <button 
            type="button" 
            onClick={clearFilters}
            className="btn btn-outline"
          >
            Clear Filters
          </button>
        </div>
      </div>

      <div className="books-grid">
        {books.map(book => (
          <div 
            key={book.id} 
            className="book-card"
            onClick={() => handleBookClick(book)}
          >
            <div className="book-cover">
              <div className="book-cover-placeholder">
                <span className="book-initial">{book.title.charAt(0)}</span>
              </div>
              <div className={`availability-badge ${book.status.toLowerCase()}`}>
                {book.status}
              </div>
            </div>
            
            <div className="book-info">
              <h3 className="book-title">{book.title}</h3>
              <p className="book-author">by {book.author}</p>
              <p className="book-category">{book.category}</p>
              <p className="book-year">{book.publishedYear}</p>
              
              <div className="book-availability">
                <span className="available-count">
                  {book.availableCopies} of {book.totalCopies} available
                </span>
                <div className="availability-bar">
                  <div 
                    className="availability-fill"
                    style={{ 
                      width: `${(book.availableCopies / book.totalCopies) * 100}%` 
                    }}
                  ></div>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>

      {books.length === 0 && (
        <div className="empty-state">
          <div className="empty-icon">📚</div>
          <h3>No books found</h3>
          <p>Try adjusting your search criteria or browse all books.</p>
        </div>
      )}

      {totalPages > 1 && (
        <div className="pagination">
          <button
            onClick={() => handlePageChange(currentPage - 1)}
            disabled={currentPage === 1}
            className="btn btn-outline"
          >
            Previous
          </button>
          
          <div className="page-numbers">
            {Array.from({ length: totalPages }, (_, i) => i + 1).map(page => (
              <button
                key={page}
                onClick={() => handlePageChange(page)}
                className={`btn ${page === currentPage ? 'btn-primary' : 'btn-outline'}`}
              >
                {page}
              </button>
            ))}
          </div>
          
          <button
            onClick={() => handlePageChange(currentPage + 1)}
            disabled={currentPage === totalPages}
            className="btn btn-outline"
          >
            Next
          </button>
        </div>
      )}

      {selectedBook && (
        <div className="modal-overlay" onClick={() => setSelectedBook(null)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Book Details</h2>
              <button 
                className="modal-close" 
                onClick={() => setSelectedBook(null)}
              >
                ×
              </button>
            </div>
            <div className="book-details-modal">
              <div className="book-cover-section">
                <div className="book-cover-large">
                  <div className="book-cover-placeholder">
                    <span className="book-initial">{selectedBook.title.charAt(0)}</span>
                  </div>
                  <div className={`availability-badge-large ${selectedBook.status.toLowerCase()}`}>
                    {selectedBook.status}
                  </div>
                </div>
              </div>
              <div className="book-info-section">
                <h3 className="book-title">{selectedBook.title}</h3>
                <p className="book-author">by {selectedBook.author}</p>
                <div className="book-meta">
                  <div className="meta-item">
                    <label>ISBN:</label>
                    <span>{selectedBook.isbn}</span>
                  </div>
                  <div className="meta-item">
                    <label>Category:</label>
                    <span>{selectedBook.category}</span>
                  </div>
                  <div className="meta-item">
                    <label>Published Year:</label>
                    <span>{selectedBook.publishedYear}</span>
                  </div>
                </div>
                <div className="availability-section">
                  <h4>Availability</h4>
                  <div className="availability-info">
                    <div className="availability-stats">
                      <div className="stat-item">
                        <span className="stat-label">Available Copies:</span>
                        <span className="stat-value available">{selectedBook.availableCopies}</span>
                      </div>
                      <div className="stat-item">
                        <span className="stat-label">Total Copies:</span>
                        <span className="stat-value total">{selectedBook.totalCopies}</span>
                      </div>
                    </div>
                    <div className="availability-bar-large">
                      <div 
                        className="availability-fill"
                        style={{ 
                          width: `${(selectedBook.availableCopies / selectedBook.totalCopies) * 100}%` 
                        }}
                      ></div>
                    </div>
                    <p className="availability-text">
                      {selectedBook.availableCopies > 0 
                        ? `${selectedBook.availableCopies} copy${selectedBook.availableCopies > 1 ? 'ies' : ''} available for borrowing`
                        : 'No copies currently available'
                      }
                    </p>
                  </div>
                </div>
                {user.role === 'Member' && (
                  <div className="member-actions">
                    <button
                      onClick={() => handleReserve(selectedBook)}
                      disabled={selectedBook.availableCopies === 0 || reserving}
                      className={`btn ${selectedBook.availableCopies > 0 ? 'btn-primary' : 'btn-disabled'}`}
                    >
                      {reserving ? 'Reserving...' : (selectedBook.availableCopies > 0 ? 'Reserve This Book' : 'Currently Unavailable')}
                    </button>
                    <p className="action-note">
                      {selectedBook.availableCopies > 0 
                        ? 'Click to reserve this book for pickup'
                        : 'This book is currently checked out. You can still reserve it to join the queue.'
                      }
                    </p>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default BookCatalog;
