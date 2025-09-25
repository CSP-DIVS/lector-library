import { useState, useEffect } from 'react';
import './BookManagement.css';
import AddBookModal from '../ui/AddBookModal';
import EditBookModal from '../ui/EditBookModal';
import api from '../../lib/api';

const BookManagement = ({ user }) => {
  const [books, setBooks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showAddForm, setShowAddForm] = useState(false);
  const [showEditForm, setShowEditForm] = useState(false);
  const [editingBook, setEditingBook] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterGenre, setFilterGenre] = useState('all');
  const [message, setMessage] = useState({ type: '', text: '' });

  const canManageBooks = user.role === 'Administrator' || user.role === 'Librarian';

  useEffect(() => {
    fetchBooks();
  }, []);

  const fetchBooks = async () => {
    try {
      setLoading(true);
      const response = await api.get('/books', {
        params: {
          search: searchTerm || undefined,
          category: filterGenre !== 'all' ? filterGenre : undefined,
          page: 1,
          pageSize: 100
        }
      });
      setBooks(response.data.items || []);
    } catch (error) {
      console.error('Error fetching books:', error);
      setMessage({ type: 'error', text: 'Failed to fetch books' });
    } finally {
      setLoading(false);
    }
  };

  const filteredBooks = books.filter(book => {
    const matchesSearch = book.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         book.author.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         book.isbn.includes(searchTerm);
    const matchesGenre = filterGenre === 'all' || book.category === filterGenre;
    return matchesSearch && matchesGenre;
  });

  const genres = [...new Set(books.map(book => book.category))];

  const handleAddBook = async (bookData) => {
    try {
      await api.post('/books', bookData);
      setMessage({ type: 'success', text: 'Book added successfully' });
      await fetchBooks();
    } catch (error) {
      console.error('Error adding book:', error);
      setMessage({ type: 'error', text: error.response?.data?.message || 'Failed to add book' });
    }
  };

  const handleEditBook = async (bookId, bookData) => {
    try {
      await api.put(`/books/${bookId}`, bookData);
      setMessage({ type: 'success', text: 'Book updated successfully' });
      await fetchBooks();
    } catch (error) {
      console.error('Error updating book:', error);
      setMessage({ type: 'error', text: error.response?.data?.message || 'Failed to update book' });
    }
  };

  const handleToggleBookStatus = async (bookId, isActive) => {
    try {
      await api.put(`/books/${bookId}/status`, isActive);
      setMessage({ type: 'success', text: isActive ? 'Book reactivated successfully' : 'Book deactivated successfully' });
      await fetchBooks();
    } catch (error) {
      console.error('Error updating book status:', error);
      setMessage({ type: 'error', text: error.response?.data?.message || 'Failed to update book status' });
    }
  };

  const handleDeleteBook = async (bookId) => {
    if (!window.confirm('Are you sure you want to delete this book?')) {
      return;
    }

    try {
      await api.delete(`/books/${bookId}`);
      setMessage({ type: 'success', text: 'Book deleted successfully' });
      await fetchBooks();
    } catch (error) {
      console.error('Error deleting book:', error);
      setMessage({ type: 'error', text: error.response?.data?.message || 'Failed to delete book' });
    }
  };

  const openEditModal = (book) => {
    setEditingBook(book);
    setShowEditForm(true);
  };

  const closeEditModal = () => {
    setEditingBook(null);
    setShowEditForm(false);
  };

  // Auto-refresh when search or filter changes
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      fetchBooks();
    }, 300);
    return () => clearTimeout(timeoutId);
  }, [searchTerm, filterGenre]);

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>Loading books...</p>
      </div>
    );
  }

  return (
    <div className="book-management-page">
      <div className="page-header">
        <h1>Book Management</h1>
        <p className="page-subtitle">
          {canManageBooks 
            ? 'Manage the library book collection and inventory'
            : 'Browse the library book collection'
          }
        </p>
      </div>

      {message.text && (
        <div className={`message ${message.type}`}>
          {message.text}
          <button onClick={() => setMessage({ type: '', text: '' })}>×</button>
        </div>
      )}

      <div className="management-toolbar">
        <div className="search-filters">
          <div className="search-box">
            <input
              type="text"
              placeholder="Search books by title, author, or ISBN..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="search-input"
            />
          </div>
          <select
            value={filterGenre}
            onChange={(e) => setFilterGenre(e.target.value)}
            className="genre-filter"
          >
            <option value="all">All Genres</option>
            {genres.map(genre => (
              <option key={genre} value={genre}>{genre}</option>
            ))}
          </select>
        </div>
        
        {canManageBooks && (
          <div className="toolbar-actions">
            <button
              onClick={() => setShowAddForm(true)}
              className="btn btn-primary"
            >
              + Add New Book
            </button>
          </div>
        )}
      </div>

      <div className="books-grid">
        {filteredBooks.map(book => (
          <div key={book.id} className={`book-card ${!book.isActive ? 'inactive-book' : ''}`}>
            <div className="book-header">
              <h3 className="book-title">{book.title}</h3>
              <div className="status-badges">
                <span className={`status-badge ${book.status.toLowerCase()}`}>
                  {book.status}
                </span>
                {!book.isActive && (
                  <span className="status-badge inactive">
                    Inactive
                  </span>
                )}
              </div>
            </div>
            
            <div className="book-details">
              <p className="book-author">by {book.author}</p>
              <p className="book-info">
                <span>ISBN: {book.isbn}</span>
                <span>Category: {book.category}</span>
                <span>Published: {book.publishedYear}</span>
              </p>
              
              <div className="availability-info">
                <div className="copies-info">
                  <span className="available-copies">
                    {book.availableCopies} available
                  </span>
                  <span className="total-copies">
                    of {book.totalCopies} total
                  </span>
                </div>
                
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

            <div className="book-actions">
              {user.role === 'Member' ? (
                <div className="member-actions">
                  <button 
                    className="btn btn-secondary"
                    disabled={book.availableCopies === 0}
                  >
                    {book.availableCopies > 0 ? 'Reserve' : 'Unavailable'}
                  </button>
                </div>
              ) : (
                <div className="staff-actions">
                  <button 
                    className="btn btn-outline"
                    onClick={() => openEditModal(book)}
                  >
                    Edit
                  </button>
                  <button className="btn btn-outline">Check Out</button>
                  {canManageBooks && (
                    <>
                      <button 
                        className={`btn ${book.isActive ? 'btn-warning' : 'btn-success'}`}
                        onClick={() => handleToggleBookStatus(book.id, !book.isActive)}
                      >
                        {book.isActive ? 'Deactivate' : 'Reactivate'}
                      </button>
                      <button 
                        className="btn btn-danger"
                        onClick={() => handleDeleteBook(book.id)}
                      >
                        Delete
                      </button>
                    </>
                  )}
                </div>
              )}
            </div>
          </div>
        ))}
      </div>

      {filteredBooks.length === 0 && (
        <div className="empty-state">
          <div className="empty-icon">📚</div>
          <h3>No books found</h3>
          <p>Try adjusting your search criteria or add new books to the collection.</p>
        </div>
      )}

      <AddBookModal
        isOpen={showAddForm}
        onClose={() => setShowAddForm(false)}
        onSubmit={handleAddBook}
      />

      <EditBookModal
        isOpen={showEditForm}
        onClose={closeEditModal}
        onSubmit={handleEditBook}
        book={editingBook}
      />
    </div>
  );
};

export default BookManagement;
