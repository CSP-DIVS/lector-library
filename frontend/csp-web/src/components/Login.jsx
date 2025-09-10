import { useState } from 'react';
import api from '../lib/api';
import './Login.css';
import LoadingSpinner from './ui/LoadingSpinner';
import { toast } from './ui/Toast';

const Login = ({ onLoginSuccess }) => {
  const [formData, setFormData] = useState({
    username: '',
    password: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleInputChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      const response = await api.post('/auth/login', formData);
      
      if (response.data.success) {
        localStorage.setItem('token', response.data.token);
        localStorage.setItem('user', JSON.stringify(response.data.user));
        toast({ title: 'Welcome', message: 'Login successful', color: 'var(--color-success)' });
        setTimeout(() => onLoginSuccess(response.data.user), 350);
      } else {
        setError(response.data.message);
        toast({ title: 'Login failed', message: response.data.message, color: 'var(--color-error)' });
      }
    } catch (err) {
      const msg = err.response?.data?.message || 'An error occurred';
      setError(msg);
      toast({ title: 'Error', message: msg, color: 'var(--color-error)' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <div className="login-header">
          <div className="logo-section">
            <div className="logo-icon">📚</div>
            <div className="logo-text">
              <h1 className="logo-title">Lector</h1>
              <p className="logo-subtitle">Library Management System</p>
            </div>
          </div>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="username">Username:</label>
            <input
              type="text"
              id="username"
              name="username"
              value={formData.username}
              onChange={handleInputChange}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password:</label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleInputChange}
              required
            />
          </div>

          {error && <div className="error-message">{error}</div>}

          <button type="submit" disabled={loading} className="submit-btn">
            {loading ? <LoadingSpinner size={20} /> : 'Sign In'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default Login;
