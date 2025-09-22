import { useState } from 'react';
import './Modal.css';

const AddBookModal = ({ isOpen, onClose, onSubmit }) => {
  const [formData, setFormData] = useState({
    title: '',
    author: '',
    isbn: '',
    category: '',
    publishedYear: new Date().getFullYear(),
    totalCopies: 1
  });
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const categories = [
    'Fiction',
    'Non-Fiction',
    'Science Fiction',
    'Fantasy',
    'Mystery',
    'Romance',
    'Thriller',
    'Biography',
    'History',
    'Science',
    'Technology',
    'Philosophy',
    'Religion',
    'Art',
    'Music',
    'Poetry',
    'Drama',
    'Comedy',
    'Horror',
    'Adventure',
    'Children',
    'Young Adult',
    'Reference',
    'Textbook',
    'Other'
  ];

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    
    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({
        ...prev,
        [name]: ''
      }));
    }
  };

  const validateForm = () => {
    const newErrors = {};

    if (!formData.title.trim()) {
      newErrors.title = 'Title is required';
    }

    if (!formData.author.trim()) {
      newErrors.author = 'Author is required';
    }

    if (!formData.isbn.trim()) {
      newErrors.isbn = 'ISBN is required';
    } else if (formData.isbn.length < 10 || formData.isbn.length > 20) {
      newErrors.isbn = 'ISBN must be between 10 and 20 characters';
    }

    if (!formData.category) {
      newErrors.category = 'Category is required';
    }

    if (!formData.publishedYear || formData.publishedYear < 1000 || formData.publishedYear > new Date().getFullYear() + 1) {
      newErrors.publishedYear = `Published year must be between 1000 and ${new Date().getFullYear() + 1}`;
    }

    if (!formData.totalCopies || formData.totalCopies < 1) {
      newErrors.totalCopies = 'Total copies must be at least 1';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);
    try {
      await onSubmit(formData);
      handleClose();
    } catch (error) {
      console.error('Error adding book:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleClose = () => {
    setFormData({
      title: '',
      author: '',
      isbn: '',
      category: '',
      publishedYear: new Date().getFullYear(),
      totalCopies: 1
    });
    setErrors({});
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <div className="modal-header">
          <h2>Add New Book</h2>
          <button 
            type="button" 
            className="modal-close" 
            onClick={handleClose}
            disabled={isSubmitting}
          >
            ×
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label htmlFor="title">Title *</label>
            <input
              type="text"
              id="title"
              name="title"
              value={formData.title}
              onChange={handleInputChange}
              className={errors.title ? 'error' : ''}
              disabled={isSubmitting}
            />
            {errors.title && <span className="error-message">{errors.title}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="author">Author *</label>
            <input
              type="text"
              id="author"
              name="author"
              value={formData.author}
              onChange={handleInputChange}
              className={errors.author ? 'error' : ''}
              disabled={isSubmitting}
            />
            {errors.author && <span className="error-message">{errors.author}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="isbn">ISBN *</label>
            <input
              type="text"
              id="isbn"
              name="isbn"
              value={formData.isbn}
              onChange={handleInputChange}
              className={errors.isbn ? 'error' : ''}
              disabled={isSubmitting}
              placeholder="e.g., 978-0-06-112008-4"
            />
            {errors.isbn && <span className="error-message">{errors.isbn}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="category">Category *</label>
            <select
              id="category"
              name="category"
              value={formData.category}
              onChange={handleInputChange}
              className={errors.category ? 'error' : ''}
              disabled={isSubmitting}
            >
              <option value="">Select a category</option>
              {categories.map(category => (
                <option key={category} value={category}>{category}</option>
              ))}
            </select>
            {errors.category && <span className="error-message">{errors.category}</span>}
          </div>

          <div className="form-row">
            <div className="form-group">
              <label htmlFor="publishedYear">Published Year *</label>
              <input
                type="number"
                id="publishedYear"
                name="publishedYear"
                value={formData.publishedYear}
                onChange={handleInputChange}
                className={errors.publishedYear ? 'error' : ''}
                disabled={isSubmitting}
                min="1000"
                max={new Date().getFullYear() + 1}
              />
              {errors.publishedYear && <span className="error-message">{errors.publishedYear}</span>}
            </div>

            <div className="form-group">
              <label htmlFor="totalCopies">Total Copies *</label>
              <input
                type="number"
                id="totalCopies"
                name="totalCopies"
                value={formData.totalCopies}
                onChange={handleInputChange}
                className={errors.totalCopies ? 'error' : ''}
                disabled={isSubmitting}
                min="1"
              />
              {errors.totalCopies && <span className="error-message">{errors.totalCopies}</span>}
            </div>
          </div>

          <div className="modal-actions">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={handleClose}
              disabled={isSubmitting}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="btn btn-primary"
              disabled={isSubmitting}
            >
              {isSubmitting ? 'Adding...' : 'Add Book'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default AddBookModal;
