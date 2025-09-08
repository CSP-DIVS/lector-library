import { useState, useEffect } from 'react';
import api from '../lib/api';
import './Dashboard.css';
import MemberManagement from './MemberManagement';
import MyProfile from './MyProfile';

const Dashboard = ({ user, onLogout }) => {
  const [healthCheck, setHealthCheck] = useState('');
  const [showMembers, setShowMembers] = useState(false);
  const [showProfile, setShowProfile] = useState(false);

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

  const role = user.role || 'Member';

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
          <p><strong>Role:</strong> {role}</p>
        </div>

        <div className="info-card">
          <h3>System Status</h3>
          <p><strong>Database:</strong> {healthCheck}</p>
          <p><strong>Frontend:</strong> Connected</p>
          <p><strong>Login Status:</strong> Authenticated</p>
        </div>

        {role === 'Administrator' && (
          <div className="info-card">
            <h3>Admin Actions</h3>
            <div className="action-buttons">
              <button className="action-btn" onClick={() => setShowMembers(true)}>Member Management</button>
            </div>
          </div>
        )}

        {role === 'Librarian' && (
          <div className="info-card">
            <h3>Librarian Actions</h3>
            <div className="action-buttons">
              <button className="action-btn">Circulation</button>
            </div>
          </div>
        )}

        {role === 'Member' && (
          <div className="info-card">
            <h3>Member Actions</h3>
            <div className="action-buttons">
              <button className="action-btn" onClick={() => setShowProfile(true)}>My Profile</button>
            </div>
          </div>
        )}
      </div>

      {role === 'Administrator' && showMembers && (
        <div className="dashboard-content">
          <MemberManagement />
        </div>
      )}

      {role === 'Member' && showProfile && (
        <div className="dashboard-content">
          <MyProfile />
        </div>
      )}
    </div>
  );
};

export default Dashboard;
