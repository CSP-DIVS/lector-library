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
      // Support both camelCase and PascalCase from backend
      const data = response?.data || {};
      const success = (data.success ?? data.Success) === true;
      const token = data.token ?? data.Token;
      const user = data.user ?? data.User;
      const message = data.message ?? data.Message;

      if (success && token && user) {
        localStorage.setItem('token', token);
        localStorage.setItem('user', JSON.stringify(user));
        toast({ title: 'Welcome', message: 'Login successful', color: 'var(--color-success)' });
        setTimeout(() => onLoginSuccess(user), 350);
      } else {
        setError(message || 'Login failed');
        toast({ title: 'Login failed', message: message || 'Invalid credentials', color: 'var(--color-error)' });
      }
    } catch (err) {
      const msg = err.response?.data?.message || err.response?.data?.Message || 'An error occurred';
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
