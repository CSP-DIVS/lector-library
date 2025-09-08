import { useState, useEffect } from 'react';
import './BookManagement.css';

const BookManagement = ({ user }) => {
  const [books, setBooks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showAddForm, setShowAddForm] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterGenre, setFilterGenre] = useState('all');

  const canManageBooks = user.role === 'Administrator' || user.role === 'Librarian';

  useEffect(() => {
    fetchBooks();
  }, []);

  const fetchBooks = async () => {
    try {
      // Mock data - replace with actual API call
      const mockBooks = [
        {
          id: 1,
          title: 'To Kill a Mockingbird',
          author: 'Harper Lee',
          isbn: '978-0-06-112008-4',
          genre: 'Fiction',
          totalCopies: 5,
          availableCopies: 3,
          publishedYear: 1960,
          status: 'Available'
        },
        {
          id: 2,
          title: '1984',
          author: 'George Orwell',
          isbn: '978-0-452-28423-4',
          genre: 'Dystopian Fiction',
          totalCopies: 4,
          availableCopies: 0,
          publishedYear: 1949,
          status: 'Unavailable'
        },
        {
          id: 3,
          title: 'Pride and Prejudice',
          author: 'Jane Austen',
          isbn: '978-0-14-143951-8',
          genre: 'Romance',
          totalCopies: 3,
          availableCopies: 2,
          publishedYear: 1813,
          status: 'Available'
        }
      ];
      setBooks(mockBooks);
    } catch (error) {
      console.error('Error fetching books:', error);
    } finally {
      setLoading(false);
    }
  };

  const filteredBooks = books.filter(book => {
    const matchesSearch = book.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         book.author.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         book.isbn.includes(searchTerm);
    const matchesGenre = filterGenre === 'all' || book.genre === filterGenre;
    return matchesSearch && matchesGenre;
  });

  const genres = [...new Set(books.map(book => book.genre))];

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
          <div key={book.id} className="book-card">
            <div className="book-header">
              <h3 className="book-title">{book.title}</h3>
              <span className={`status-badge ${book.status.toLowerCase()}`}>
                {book.status}
              </span>
            </div>
            
            <div className="book-details">
              <p className="book-author">by {book.author}</p>
              <p className="book-info">
                <span>ISBN: {book.isbn}</span>
                <span>Genre: {book.genre}</span>
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
                  <button className="btn btn-outline">Edit</button>
                  <button className="btn btn-outline">Check Out</button>
                  {canManageBooks && (
                    <button className="btn btn-danger">Delete</button>
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
    </div>
  );
};

export default BookManagement;
