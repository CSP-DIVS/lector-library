import './Header.css';

const Header = ({ user, onLogout }) => {
  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    onLogout();
  };

  return (
    <header className="header">
      <div className="header-content">
        <div className="header-title">
          <h1>Lector - Library Management System</h1>
          <span className="role-badge role-{user.role?.toLowerCase() || 'member'}">
            {user.role || 'Member'}
          </span>
        </div>
        <div className="header-actions">
          <div className="user-info">
            <span className="welcome-text">Welcome, {user.username}!</span>
            <div className="user-avatar">
              {user.username?.charAt(0).toUpperCase() || 'U'}
            </div>
          </div>
          <button onClick={handleLogout} className="logout-btn">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
              <polyline points="16,17 21,12 16,7"/>
              <line x1="21" y1="12" x2="9" y2="12"/>
            </svg>
            Logout
          </button>
        </div>
      </div>
    </header>
  );
};

export default Header;
