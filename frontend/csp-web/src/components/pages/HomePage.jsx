import { useState, useEffect } from 'react';
import api from '../../lib/api';
import './HomePage.css';

const HomePage = ({ user }) => {
  const [stats, setStats] = useState({});
  const [recentActivities, setRecentActivities] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchDashboardData();
  }, [user.role]);

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      // Mock data for now - replace with actual API calls
      const mockStats = getMockStatsForRole(user.role);
      const mockActivities = getMockActivitiesForRole(user.role);
      
      setStats(mockStats);
      setRecentActivities(mockActivities);
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>Loading dashboard...</p>
      </div>
    );
  }

  return (
    <div className="home-page">
      <div className="page-header">
        <h1>Welcome back, {user.username}!</h1>
        <p className="page-subtitle">
          {getRoleDescription(user.role)}
        </p>
      </div>

      <div className="dashboard-grid">
        <div className="stats-section">
          <h2>Overview</h2>
          <div className="stats-grid">
            {Object.entries(stats).map(([key, value]) => (
              <div key={key} className="stat-card">
                <div className="stat-icon">{getStatIcon(key)}</div>
                <div className="stat-content">
                  <div className="stat-value">{value.value}</div>
                  <div className="stat-label">{value.label}</div>
                  {value.change && (
                    <div className={`stat-change ${value.change > 0 ? 'positive' : 'negative'}`}>
                      {value.change > 0 ? '+' : ''}{value.change}%
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="activities-section">
          <h2>Recent Activities</h2>
          <div className="activities-list">
            {recentActivities.map((activity, index) => (
              <div key={index} className="activity-item">
                <div className="activity-icon">{activity.icon}</div>
                <div className="activity-content">
                  <div className="activity-title">{activity.title}</div>
                  <div className="activity-description">{activity.description}</div>
                  <div className="activity-time">{activity.time}</div>
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="quick-actions-section">
          <h2>Quick Actions</h2>
          <div className="quick-actions-grid">
            {getQuickActionsForRole(user.role).map((action, index) => (
              <button key={index} className="quick-action-btn" onClick={action.onClick}>
                <div className="action-icon">{action.icon}</div>
                <div className="action-label">{action.label}</div>
              </button>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

const getRoleDescription = (role) => {
  switch (role) {
    case 'Administrator':
      return 'Manage the entire library system, users, and operations.';
    case 'Librarian':
      return 'Handle daily library operations, book management, and member services.';
    case 'Member':
      return 'Browse books, manage your loans, and track your library activities.';
    default:
      return 'Welcome to the Lector Library Management System.';
  }
};

const getMockStatsForRole = (role) => {
  if (role === 'Administrator') {
    return {
      totalUsers: { label: 'Total Users', value: '1,247', change: 12 },
      totalBooks: { label: 'Total Books', value: '15,832', change: 3 },
      activeLoans: { label: 'Active Loans', value: '3,456', change: -2 },
      overdueBooks: { label: 'Overdue Books', value: '89', change: -15 },
      monthlyRevenue: { label: 'Monthly Revenue', value: '$2,340', change: 8 },
      newRegistrations: { label: 'New Registrations', value: '156', change: 25 }
    };
  } else if (role === 'Librarian') {
    return {
      dailyCheckouts: { label: 'Daily Checkouts', value: '67', change: 5 },
      dailyReturns: { label: 'Daily Returns', value: '54', change: -3 },
      pendingRequests: { label: 'Pending Requests', value: '23', change: 0 },
      overdueToday: { label: 'Overdue Today', value: '12', change: -8 },
      reservations: { label: 'Active Reservations', value: '145', change: 7 },
      finesCollected: { label: 'Fines Collected', value: '$89', change: 15 }
    };
  } else {
    return {
      booksLoaned: { label: 'Books on Loan', value: '3', change: 0 },
      booksReserved: { label: 'Books Reserved', value: '2', change: 1 },
      dueThisWeek: { label: 'Due This Week', value: '1', change: 0 },
      totalRead: { label: 'Books Read This Year', value: '24', change: 20 },
      outstandingFines: { label: 'Outstanding Fines', value: '$0', change: -100 },
      favoriteGenre: { label: 'Favorite Genre', value: 'Fiction', change: null }
    };
  }
};

const getMockActivitiesForRole = (role) => {
  if (role === 'Administrator') {
    return [
      { icon: '👤', title: 'New User Registration', description: 'John Smith registered as a new member', time: '2 hours ago' },
      { icon: '📚', title: 'Book Added', description: 'Added "The Great Gatsby" to the collection', time: '4 hours ago' },
      { icon: '⚠️', title: 'System Alert', description: 'Server maintenance scheduled for tonight', time: '6 hours ago' },
      { icon: '💰', title: 'Payment Received', description: 'Fine payment of $15 received from Jane Doe', time: '1 day ago' }
    ];
  } else if (role === 'Librarian') {
    return [
      { icon: '📖', title: 'Book Checked Out', description: 'Member borrowed "To Kill a Mockingbird"', time: '30 minutes ago' },
      { icon: '🔄', title: 'Book Returned', description: 'Member returned "1984" (on time)', time: '1 hour ago' },
      { icon: '📋', title: 'Reservation Fulfilled', description: 'Reserved book picked up by member', time: '2 hours ago' },
      { icon: '⏰', title: 'Overdue Notice', description: 'Sent overdue notice to 3 members', time: '3 hours ago' }
    ];
  } else {
    return [
      { icon: '📚', title: 'Book Due Soon', description: '"The Catcher in the Rye" is due in 2 days', time: 'Today' },
      { icon: '✅', title: 'Book Returned', description: 'Successfully returned "Pride and Prejudice"', time: '2 days ago' },
      { icon: '🔖', title: 'Book Reserved', description: 'Reserved "Dune" - position #3 in queue', time: '3 days ago' },
      { icon: '⭐', title: 'Review Submitted', description: 'Rated "The Hobbit" 5 stars', time: '1 week ago' }
    ];
  }
};

const getQuickActionsForRole = (role) => {
  if (role === 'Administrator') {
    return [
      { icon: '👥', label: 'Add New User', onClick: () => {} },
      { icon: '📊', label: 'View Reports', onClick: () => {} },
      { icon: '⚙️', label: 'System Settings', onClick: () => {} },
      { icon: '📧', label: 'Send Notifications', onClick: () => {} }
    ];
  } else if (role === 'Librarian') {
    return [
      { icon: '📖', label: 'Check Out Book', onClick: () => {} },
      { icon: '🔄', label: 'Process Returns', onClick: () => {} },
      { icon: '📋', label: 'Manage Reservations', onClick: () => {} },
      { icon: '💰', label: 'Process Fines', onClick: () => {} }
    ];
  } else {
    return [
      { icon: '🔍', label: 'Search Books', onClick: () => {} },
      { icon: '📖', label: 'My Loans', onClick: () => {} },
      { icon: '🔖', label: 'My Reservations', onClick: () => {} },
      { icon: '💳', label: 'Pay Fines', onClick: () => {} }
    ];
  }
};

const getStatIcon = (key) => {
  const iconMap = {
    totalUsers: '👥',
    totalBooks: '📚',
    activeLoans: '📖',
    overdueBooks: '⏰',
    monthlyRevenue: '💰',
    newRegistrations: '✨',
    dailyCheckouts: '📤',
    dailyReturns: '📥',
    pendingRequests: '📋',
    overdueToday: '⚠️',
    reservations: '🔖',
    finesCollected: '💵',
    booksLoaned: '📚',
    booksReserved: '🔖',
    dueThisWeek: '⏰',
    totalRead: '✅',
    outstandingFines: '💳',
    favoriteGenre: '❤️'
  };
  return iconMap[key] || '📊';
};

export default HomePage;
