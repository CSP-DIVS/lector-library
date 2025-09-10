import { useEffect, useState } from 'react';
import api from '../lib/api';
import './MyProfile.css';

const MyProfile = ({ user }) => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [profile, setProfile] = useState({ username: '', email: '' });
  const [pwd, setPwd] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [msg, setMsg] = useState('');
  const [activeTab, setActiveTab] = useState('profile');
  const [stats, setStats] = useState({
    booksRead: 24,
    currentLoans: 3,
    overdueBooks: 0,
    totalFines: 0,
    memberSince: '2023'
  });

  useEffect(() => {
    const load = async () => {
      try {
        const r = await api.get('/users/my-profile');
        setProfile({ username: r.data.username, email: r.data.email });
        // Mock stats - replace with actual API call
        setStats({
          booksRead: 24,
          currentLoans: 3,
          overdueBooks: 0,
          totalFines: 0,
          memberSince: '2023'
        });
      } catch (e) {
        setError('Failed to load profile');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const saveProfile = async (e) => {
    e.preventDefault();
    setError('');
    setMsg('');
    try {
      await api.put('/users/my-profile', profile);
      setMsg('Profile updated successfully');
      localStorage.setItem('user', JSON.stringify({ ...(JSON.parse(localStorage.getItem('user')||'{}')), username: profile.username, email: profile.email }));
      setTimeout(() => setMsg(''), 3000);
    } catch (e) {
      setError('Update failed');
      setTimeout(() => setError(''), 3000);
    }
  };

  const changePassword = async (e) => {
    e.preventDefault();
    setError('');
    setMsg('');
    
    if (pwd.newPassword !== pwd.confirmPassword) {
      setError('New passwords do not match');
      setTimeout(() => setError(''), 3000);
      return;
    }
    
    if (pwd.newPassword.length < 6) {
      setError('Password must be at least 6 characters long');
      setTimeout(() => setError(''), 3000);
      return;
    }
    
    try {
      await api.put('/users/my-password', {
        currentPassword: pwd.currentPassword,
        newPassword: pwd.newPassword
      });
      setMsg('Password changed successfully');
      setPwd({ currentPassword: '', newPassword: '', confirmPassword: '' });
      setTimeout(() => setMsg(''), 3000);
    } catch (e) {
      setError('Password change failed');
      setTimeout(() => setError(''), 3000);
    }
  };

  if (loading) {
    return (
      <div className="profile-loading">
        <div className="loading-spinner"></div>
        <p>Loading your profile...</p>
      </div>
    );
  }

  return (
    <div className="profile-page">
      <div className="profile-header">
        <div className="profile-avatar">
          <div className="avatar-circle">
            {profile.username?.charAt(0).toUpperCase() || 'U'}
          </div>
          <div className="avatar-status online"></div>
        </div>
        <div className="profile-info">
          <h1 className="profile-name">{profile.username || 'User'}</h1>
          <p className="profile-email">{profile.email}</p>
          <div className="profile-role">
            <span className={`role-badge role-${user?.role?.toLowerCase() || 'member'}`}>
              {user?.role || 'Member'}
            </span>
          </div>
        </div>
        <div className="profile-actions">
          <button className="btn btn-outline">
            <svg className="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path d="M17 3a2.828 2.828 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z"/>
            </svg>
            Edit Profile
          </button>
        </div>
      </div>

      {user?.role === 'Member' && (
        <div className="profile-stats">
          <div className="stat-card">
            <div className="stat-icon">📚</div>
            <div className="stat-content">
              <div className="stat-value">{stats.booksRead}</div>
              <div className="stat-label">Books Read</div>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">📖</div>
            <div className="stat-content">
              <div className="stat-value">{stats.currentLoans}</div>
              <div className="stat-label">Current Loans</div>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">⏰</div>
            <div className="stat-content">
              <div className="stat-value">{stats.overdueBooks}</div>
              <div className="stat-label">Overdue Books</div>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">💰</div>
            <div className="stat-content">
              <div className="stat-value">${stats.totalFines}</div>
              <div className="stat-label">Total Fines</div>
            </div>
          </div>
          <div className="stat-card">
            <div className="stat-icon">📅</div>
            <div className="stat-content">
              <div className="stat-value">{stats.memberSince}</div>
              <div className="stat-label">Member Since</div>
            </div>
          </div>
        </div>
      )}

      <div className="profile-content">
        <div className="profile-tabs">
          <button
            className={`tab ${activeTab === 'profile' ? 'active' : ''}`}
            onClick={() => setActiveTab('profile')}
          >
            <svg className="tab-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
              <circle cx="12" cy="7" r="4"/>
            </svg>
            Profile Settings
          </button>
          <button
            className={`tab ${activeTab === 'security' ? 'active' : ''}`}
            onClick={() => setActiveTab('security')}
          >
            <svg className="tab-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
              <circle cx="12" cy="16" r="1"/>
              <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
            </svg>
            Security
          </button>
        </div>

        <div className="tab-content">
          {error && (
            <div className="alert alert-error">
              <svg className="alert-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <circle cx="12" cy="12" r="10"/>
                <line x1="15" y1="9" x2="9" y2="15"/>
                <line x1="9" y1="9" x2="15" y2="15"/>
              </svg>
              {error}
            </div>
          )}
          
          {msg && (
            <div className="alert alert-success">
              <svg className="alert-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                <polyline points="22,4 12,14.01 9,11.01"/>
              </svg>
              {msg}
            </div>
          )}

          {activeTab === 'profile' && (
            <div className="profile-form-section">
              <h2 className="section-title">Personal Information</h2>
              <form onSubmit={saveProfile} className="profile-form">
                <div className="form-grid">
                  <div className="form-group">
                    <label htmlFor="username">Username</label>
                    <input
                      type="text"
                      id="username"
                      value={profile.username}
                      onChange={(e) => setProfile({ ...profile, username: e.target.value })}
                      className="form-input"
                      required
                    />
                  </div>
                  <div className="form-group">
                    <label htmlFor="email">Email Address</label>
                    <input
                      type="email"
                      id="email"
                      value={profile.email}
                      onChange={(e) => setProfile({ ...profile, email: e.target.value })}
                      className="form-input"
                      required
                    />
                  </div>
                </div>
                <div className="form-actions">
                  <button type="submit" className="btn btn-primary">
                    <svg className="btn-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                      <path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"/>
                      <polyline points="17,21 17,13 7,13 7,21"/>
                      <polyline points="7,3 7,8 15,8"/>
                    </svg>
                    Save Changes
                  </button>
                </div>
              </form>
            </div>
          )}

          {activeTab === 'security' && (
            <div className="security-form-section">
              <h2 className="section-title">Change Password</h2>
              <form onSubmit={changePassword} className="security-form">
                <div className="form-group">
                  <label htmlFor="currentPassword">Current Password</label>
                  <input
                    type="password"
                    id="currentPassword"
                    value={pwd.currentPassword}
                    onChange={(e) => setPwd({ ...pwd, currentPassword: e.target.value })}
                    className="form-input"
                    required
                  />
                </div>
                <div className="form-group">
                  <label htmlFor="newPassword">New Password</label>
                  <input
                    type="password"
                    id="newPassword"
                    value={pwd.newPassword}
                    onChange={(e) => setPwd({ ...pwd, newPassword: e.target.value })}
                    className="form-input"
                    minLength="6"
                    required
                  />
                  <small className="form-hint">Password must be at least 6 characters long</small>
                </div>
                <div className="form-group">
                  <label htmlFor="confirmPassword">Confirm New Password</label>
                  <input
                    type="password"
                    id="confirmPassword"
                    value={pwd.confirmPassword}
                    onChange={(e) => setPwd({ ...pwd, confirmPassword: e.target.value })}
                    className="form-input"
                    required
                  />
                </div>
                <div className="form-actions">
                  <button type="submit" className="btn btn-primary">
                    <svg className="btn-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                      <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
                      <circle cx="12" cy="16" r="1"/>
                      <path d="M7 11V7a5 5 0 0 1 10 0v4"/>
                    </svg>
                    Update Password
                  </button>
                </div>
              </form>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default MyProfile;


