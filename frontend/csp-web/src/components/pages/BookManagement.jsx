import { useState, useEffect, useMemo, useCallback } from 'react';
import './BookManagement.css';
import AddBookModal from '../ui/AddBookModal';
import EditBookModal from '../ui/EditBookModal';
import api from '../../lib/api';

/**
 * @file BookManagement.jsx
 * @description This component provides a comprehensive interface for managing the library's book collection.
 * It allows users to view, search, and filter books. Authorized users (Librarians, Administrators)
 * can also add, edit, update the status of, and delete books.
 * @param {{ user: { role: string } }} props - The component props.
 * @param {object} props.user - The currently logged-in user object, used to determine permissions.
 */
const BookManagement = ({ user }) => {
  // State for book data and loading status
  const [books, setBooks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [isSearching, setIsSearching] = useState(false);

  // State for handling UI elements like modals and notifications
  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [editingBook, setEditingBook] = useState(null); // Holds the book object for the edit modal
  const [message, setMessage] = useState({ type: '', text: '' });

  // State for search and filter inputs
  const [searchTerm, setSearchTerm] = useState('');
  const [filterCategory, setFilterCategory] = useState('all');

  // Memoized derived state for user permissions
  const canManageBooks = useMemo(() => 
    user.role === 'Administrator' || user.role === 'Librarian',
    [user.role]
  );

  // Memoized list of unique categories for the filter dropdown
  const categories = useMemo(() => [
    ...new Set(books.map(book => book.category))
  ], [books]);

  /**
   * Fetches books from the API based on the current search and filter state.
   * Wrapped in useCallback to prevent re-creation on every render, optimizing performance.
   */
  const fetchBooks = useCallback(async (searchQuery, categoryQuery) => {
    // Use passed arguments or state
    const currentSearch = searchQuery !== undefined ? searchQuery : searchTerm;
    const currentCategory = categoryQuery !== undefined ? categoryQuery : filterCategory;

    setIsSearching(true);
    if (books.length === 0) setLoading(true);

    try {
      const response = await api.get('/books', {
        params: {
          search: currentSearch || undefined,
          category: currentCategory !== 'all' ? currentCategory : undefined,
          page: 1,
          pageSize: 100,
        }
      });
      setBooks(response.data.items || []);
    } catch (error) {
      console.error('Error fetching books:', error);
      setMessage({ type: 'error', text: 'Failed to fetch books. Please try again.' });
    } finally {
      setLoading(false);
      setIsSearching(false);
    }
  }, [books.length, searchTerm, filterCategory]);


  /**
   * Effect to trigger a debounced fetch when search or filter criteria change.
   */
  useEffect(() => {
    const handler = setTimeout(() => {
      // Avoid fetching on initial mount if searchTerm is empty
      if (searchTerm === '' && filterCategory === 'all' && books.length > 0) {
        // If filters are cleared, we might want to refetch or just clear search state
        return;
      }
      fetchBooks(searchTerm, filterCategory);
    }, 500); // Debounce time

    return () => {
      clearTimeout(handler);
    };
  }, [searchTerm, filterCategory, fetchBooks, books.length]);

  // Initial data fetch
  useEffect(() => {
    fetchBooks();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []); // Runs only once on mount


  // --- CRUD Handlers ---

  const handleAddBook = async (bookData) => {
    try {
      await api.post('/books', bookData);
      setMessage({ type: 'success', text: 'Book added successfully.' });
      fetchBooks(); // Refresh book list
      setShowAddModal(false);
    } catch (error) {
      console.error('Error adding book:', error);
      setMessage({ type: 'error', text: error.response?.data?.message || 'Failed to add book.' });
    }
  };

  const handleEditBook = async (bookId, bookData) => {
    try {
      await api.put(`/books/${bookId}`, bookData);
      setMessage({ type: 'success', text: 'Book updated successfully.' });
      fetchBooks();
      closeEditModal();
    } catch (error) {
      console.error('Error updating book:', error);
      setMessage({ type: 'error', text: error.response?.data?.message || 'Failed to update book.' });
    }
  };

  const handleToggleBookStatus = async (bookId, newStatus) => {
    try {
      await api.put(`/books/${bookId}/status`, { isActive: newStatus });
      setMessage({ type: 'success', text: `Book ${newStatus ? 'reactivated' : 'deactivated'} successfully.` });
      fetchBooks();
    } catch (error) {
      console.error('Error updating book status:', error);
      setMessage({ type: 'error', text: error.response?.data?.message || 'Failed to update status.' });
    }
  };

  const handleDeleteBook = async (bookId) => {
    if (!window.confirm('Are you sure you want to delete this book? This action is permanent.')) {
      return;
    }
    try {
      await api.delete(`/books/${bookId}`);
      setMessage({ type: 'success', text: 'Book deleted successfully.' });
      fetchBooks();
    } catch (error) {
      console.error('Error deleting book:', error);
      setMessage({ type: 'error', text: error.response?.data?.message || 'Failed to delete book.' });
    }
  };

  // --- Modal Handlers ---

  const openEditModal = (book) => {
    setEditingBook(book);
    setShowEditModal(true);
  };

  const closeEditModal = () => {
    setEditingBook(null);
    setShowEditModal(false);
  };
  
  const handleSearchChange = useCallback((e) => {
    setSearchTerm(e.target.value);
  }, []);


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
          {canManageBooks ? 'Manage the library book collection and inventory' : 'Browse the library book collection'}
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
          <div className="search-input-container">
            <input
              type="text"
              placeholder="Search by title, author, or ISBN..."
              value={searchTerm}
              onChange={handleSearchChange}
              className="search-input"
            />
            {isSearching && <div className="search-spinner"></div>}
          </div>
          <select
            value={filterCategory}
            onChange={(e) => setFilterCategory(e.target.value)}
            className="genre-filter"
          >
            <option value="all">. All  Categories</option>
            {categories.map(category => (
              <option key={category} value={category}>{category}</option>
            ))}
          </select>
        </div>
        
        {canManageBooks && (
          <div className="toolbar-actions">
            <button onClick={() => setShowAddModal(true)} className="btn btn-primary">
              + Add New Book
            </button>
          </div>
        )}
      </div>

      <div className="books-grid">
        {books.map(book => (
          <div key={book.id} className={`book-card ${!book.isActive ? 'inactive-book' : ''}`}>
            <div className="book-header">
              <h3 className="book-title">{book.title}</h3>
              <div className="status-badges">
                <span className={`status-badge ${book.status.toLowerCase()}`}>{book.status}</span>
                {!book.isActive && <span className="status-badge inactive">Inactive</span>}
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
                  <span className="available-copies">{book.availableCopies} available</span>
                  <span className="total-copies">of {book.totalCopies} total</span>
                </div>
                <div className="availability-bar">
                  <div 
                    className="availability-fill"
                    style={{ width: `${book.totalCopies > 0 ? (book.availableCopies / book.totalCopies) * 100 : 0}%` }}
                  ></div>
                </div>
              </div>
            </div>
            <div className="book-actions">
              {user.role === 'Member' ? (
                <div className="member-actions">
                  <button className="btn btn-secondary" disabled={book.availableCopies === 0}>
                    {book.availableCopies > 0 ? 'Reserve' : 'Unavailable'}
                  </button>
                </div>
              ) : (
                <div className="staff-actions">
                  <button className="btn btn-outline" onClick={() => openEditModal(book)}>Edit</button>
                  {canManageBooks && (
                    <>
                      <button 
                        className={`btn ${book.isActive ? 'btn-warning' : 'btn-success'}`}
                        onClick={() => handleToggleBookStatus(book.id, !book.isActive)}
                      >
                        {book.isActive ? 'Deactivate' : 'Reactivate'}
                      </button>
                      <button className="btn btn-danger" onClick={() => handleDeleteBook(book.id)}>Delete</button>
                    </>
                  )}
                </div>
              )}
            </div>
          </div>
        ))}
      </div>

      {books.length === 0 && !loading && (
        <div className="empty-state">
          <div className="empty-icon">📚</div>
          <h3>No books found</h3>
          <p>Try adjusting your search criteria or add new books to the collection.</p>
        </div>
      )}

      <AddBookModal isOpen={showAddModal} onClose={() => setShowAddModal(false)} onSubmit={handleAddBook} />
      <EditBookModal isOpen={showEditModal} onClose={closeEditModal} onSubmit={handleEditBook} book={editingBook} />
    </div>
  );
};

export default BookManagement;
