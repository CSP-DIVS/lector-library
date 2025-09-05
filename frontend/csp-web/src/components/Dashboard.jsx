import { useState, useEffect } from 'react';
import api from '../lib/api';
import './Dashboard.css';

const Dashboard = ({ user, onLogout }) => {
  const [healthCheck, setHealthCheck] = useState('');

  useEffect(() => {
    // Test API connection
    api.get('/health/db')
      .then(r => setHealthCheck(`Database connected: ${JSON.stringify(r.data)}`))
      .catch(() => setHealthCheck('API not reachable'));
  }, []);

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    onLogout();
  };

  return (
    <div className="dashboard-container">
      <div className="dashboard-header">
        <h1>Library Management System</h1>
        <div className="user-info">
          <span>Welcome, {user.username}!</span>
          <button onClick={handleLogout} className="logout-btn">
            Logout
          </button>
        </div>
      </div>

      <div className="dashboard-content">
        <div className="info-card">
          <h3>User Information</h3>
          <p><strong>Username:</strong> {user.username}</p>
          <p><strong>Email:</strong> {user.email}</p>
          <p><strong>User ID:</strong> {user.id}</p>
        </div>

        <div className="info-card">
          <h3>System Status</h3>
          <p><strong>Database:</strong> {healthCheck}</p>
          <p><strong>Frontend:</strong> Connected</p>
          <p><strong>Login Status:</strong> Authenticated</p>
        </div>

        <div className="info-card">
          <h3>Quick Actions</h3>
          <div className="action-buttons">
            <button className="action-btn">Manage Books</button>
            <button className="action-btn">View Members</button>
            <button className="action-btn">Check Borrowings</button>
            <button className="action-btn">Generate Reports</button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
