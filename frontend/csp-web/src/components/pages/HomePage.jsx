import { useState, useEffect } from 'react';
import api from '../../lib/api';
import './HomePage.css';

const HomePage = ({ user }) => {
  const [stats, setStats] = useState({});
  const [recentActivities, setRecentActivities] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchDashboardData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [user.role]);

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      
      if (user.role === 'Administrator') {
        await fetchAdminStats();
      } else if (user.role === 'Librarian') {
        await fetchLibrarianStats();
      } else {
        await fetchMemberStats();
      }
    } catch (error) {
      console.error('Error fetching dashboard data:', error);
      // Set empty stats on error
      setStats({});
      setRecentActivities([]);
    } finally {
      setLoading(false);
    }
  };

  const fetchAdminStats = async () => {
    try {
      // Fetch all required data in parallel
      const [usersRes, booksRes, loansRes, finesRes] = await Promise.all([
        api.get('/users', { params: { page: 1, pageSize: 1 } }),
        api.get('/books', { params: { page: 1, pageSize: 1 } }),
        api.get('/lendings/active', { params: { page: 1, pageSize: 1000 } }),
        api.get('/fines', { params: { page: 1, pageSize: 1000 } })
      ]);

      // Handle both PascalCase (C#) and camelCase (JSON) property names
      const totalUsers = usersRes.data.total || usersRes.data.Total || 0;
      const totalBooks = booksRes.data.total || booksRes.data.Total || 0;
      const activeLoans = loansRes.data.total || loansRes.data.Total || 0;
      const allLoans = loansRes.data.items || loansRes.data.Items || [];
      const allFines = finesRes.data.items || finesRes.data.Items || [];

      // Calculate overdue books
      const overdueBooks = allLoans.filter(loan => loan.isOverdue || loan.IsOverdue).length;
      
      // Calculate outstanding fines total (Status can be "Outstanding")
      const outstandingFinesTotal = allFines
        .filter(fine => (fine.status || fine.Status) === 'Outstanding')
        .reduce((sum, fine) => sum + (fine.amount || fine.Amount), 0);

      setStats({
        totalUsers: { label: 'Total Users', value: totalUsers.toString() },
        totalBooks: { label: 'Total Books', value: totalBooks.toString() },
        activeLoans: { label: 'Active Loans', value: activeLoans.toString() },
        overdueBooks: { label: 'Overdue Books', value: overdueBooks.toString() },
        outstandingFines: { label: 'Outstanding Fines', value: `Rs.${outstandingFinesTotal.toFixed(2)}` },
        totalFines: { label: 'Total Fines', value: allFines.length.toString() }
      });

      // Get recent activities from active loans
      const recentLoans = allLoans.slice(0, 4).map((loan) => ({
        icon: (loan.isOverdue || loan.IsOverdue) ? '⚠️' : '📖',
        title: (loan.isOverdue || loan.IsOverdue) ? 'Overdue Book' : 'Active Loan',
        description: `"${loan.bookTitle || loan.BookTitle}" borrowed by ${loan.username || loan.Username}`,
        time: formatDate(loan.borrowDate || loan.BorrowDate)
      }));

      setRecentActivities(recentLoans);
    } catch (error) {
      console.error('Error fetching admin stats:', error);
      throw error;
    }
  };

  const fetchLibrarianStats = async () => {
    try {
      // Fetch all required data in parallel
      const [loansRes, reservationsRes, finesRes] = await Promise.all([
        api.get('/lendings/active', { params: { page: 1, pageSize: 1000 } }),
        api.get('/reservations', { params: { page: 1, pageSize: 1000 } }),
        api.get('/fines', { params: { page: 1, pageSize: 1000 } })
      ]);

      const allLoans = loansRes.data.items || loansRes.data.Items || [];
      const allReservations = reservationsRes.data.items || reservationsRes.data.Items || [];
      const allFines = finesRes.data.items || finesRes.data.Items || [];

      // Calculate stats
      const activeLoans = loansRes.data.total || loansRes.data.Total || 0;
      const overdueLoans = allLoans.filter(loan => loan.isOverdue || loan.IsOverdue).length;
      const pendingReservations = allReservations.filter(r => (r.status || r.Status) === 'Pending').length;
      const paidFinesTotal = allFines
        .filter(fine => (fine.status || fine.Status) === 'Paid')
        .reduce((sum, fine) => sum + (fine.amount || fine.Amount), 0);
      const outstandingFinesCount = allFines.filter(fine => (fine.status || fine.Status) === 'Outstanding').length;

      setStats({
        activeLoans: { label: 'Active Loans', value: activeLoans.toString() },
        overdueLoans: { label: 'Overdue Books', value: overdueLoans.toString() },
        pendingReservations: { label: 'Pending Reservations', value: pendingReservations.toString() },
        totalReservations: { label: 'Total Reservations', value: allReservations.length.toString() },
        finesCollected: { label: 'Fines Collected', value: `Rs.${paidFinesTotal.toFixed(2)}` },
        outstandingFinesCount: { label: 'Outstanding Fines', value: outstandingFinesCount.toString() }
      });

      // Get recent activities
      const recentLoans = allLoans.slice(0, 2).map(loan => ({
        icon: (loan.isOverdue || loan.IsOverdue) ? '⚠️' : '📖',
        title: (loan.isOverdue || loan.IsOverdue) ? 'Overdue Book' : 'Active Loan',
        description: `"${loan.bookTitle || loan.BookTitle}" - ${loan.username || loan.Username}`,
        time: formatDate(loan.borrowDate || loan.BorrowDate)
      }));

      const recentReservations = allReservations.slice(0, 2).map(res => ({
        icon: '🔖',
        title: `Reservation ${res.status || res.Status}`,
        description: `"${res.bookTitle || res.BookTitle}" - ${res.username || res.Username}`,
        time: formatDate(res.reservedDate || res.ReservedDate)
      }));

      setRecentActivities([...recentLoans, ...recentReservations]);
    } catch (error) {
      console.error('Error fetching librarian stats:', error);
      throw error;
    }
  };

  const fetchMemberStats = async () => {
    try {
      // Fetch all required data in parallel
      const [loansRes, reservationsRes, historyRes, fineStatsRes] = await Promise.all([
        api.get('/lendings/active', { params: { page: 1, pageSize: 1000 } }),
        api.get('/reservations/my-reservations', { params: { page: 1, pageSize: 1000 } }),
        api.get('/lendings/history', { params: { page: 1, pageSize: 1000 } }),
        api.get('/fines/my-statistics').catch(() => ({ data: { totalOutstanding: 0, TotalOutstanding: 0, outstandingCount: 0, OutstandingCount: 0, paidCount: 0, PaidCount: 0 } }))
      ]);

      const allLoans = loansRes.data.items || loansRes.data.Items || [];
      const allReservations = reservationsRes.data.items || reservationsRes.data.Items || [];
      const allHistory = historyRes.data.items || historyRes.data.Items || [];
      const fineStats = fineStatsRes.data;

      // Calculate stats
      const booksLoaned = allLoans.length;
      const booksReserved = allReservations.filter(r => (r.status || r.Status) === 'Pending').length;
      
      // Calculate due this week
      const oneWeekFromNow = new Date();
      oneWeekFromNow.setDate(oneWeekFromNow.getDate() + 7);
      const dueThisWeek = allLoans.filter(loan => {
        const dueDate = new Date(loan.dueDate || loan.DueDate);
        return dueDate <= oneWeekFromNow && dueDate >= new Date();
      }).length;

      const totalRead = allHistory.length;
      const outstandingFines = fineStats.totalOutstanding || fineStats.TotalOutstanding || 0;
      const paidCount = fineStats.paidCount || fineStats.PaidCount || 0;

      setStats({
        booksLoaned: { label: 'Books on Loan', value: booksLoaned.toString() },
        booksReserved: { label: 'Books Reserved', value: booksReserved.toString() },
        dueThisWeek: { label: 'Due This Week', value: dueThisWeek.toString() },
        totalRead: { label: 'Books Read (History)', value: totalRead.toString() },
        outstandingFines: { label: 'Outstanding Fines', value: `Rs.${outstandingFines.toFixed(2)}` },
        paidFines: { label: 'Fines Paid', value: paidCount.toString() }
      });

      // Get recent activities
      const activities = [];

      // Add overdue/due soon books
      allLoans.forEach(loan => {
        const isOverdue = loan.isOverdue || loan.IsOverdue;
        const bookTitle = loan.bookTitle || loan.BookTitle;
        const dueDate = loan.dueDate || loan.DueDate;
        const overdueDays = loan.overdueDays || loan.OverdueDays;

        if (isOverdue) {
          activities.push({
            icon: '⚠️',
            title: 'Book Overdue',
            description: `"${bookTitle}" is overdue by ${overdueDays} days`,
            time: formatDate(dueDate)
          });
        } else {
          const daysUntilDue = Math.ceil((new Date(dueDate) - new Date()) / (1000 * 60 * 60 * 24));
          if (daysUntilDue <= 3 && daysUntilDue >= 0) {
            activities.push({
              icon: '📚',
              title: 'Book Due Soon',
              description: `"${bookTitle}" is due in ${daysUntilDue} day(s)`,
              time: formatDate(dueDate)
            });
          }
        }
      });

      // Add recent reservations
      allReservations.slice(0, 2).forEach(res => {
        activities.push({
          icon: '🔖',
          title: `Book Reserved (${res.status || res.Status})`,
          description: `"${res.bookTitle || res.BookTitle}" - Position #${res.queuePosition || res.QueuePosition}`,
          time: formatDate(res.reservedDate || res.ReservedDate)
        });
      });

      // Add recent returns
      allHistory.slice(0, 2).forEach(loan => {
        activities.push({
          icon: '✅',
          title: 'Book Returned',
          description: `"${loan.bookTitle || loan.BookTitle}"`,
          time: formatDate(loan.returnDate || loan.ReturnDate)
        });
      });

      setRecentActivities(activities.slice(0, 4));
    } catch (error) {
      console.error('Error fetching member stats:', error);
      throw error;
    }
  };

  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    const now = new Date();
    const diffTime = Math.abs(now - date);
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays === 0) return 'Today';
    if (diffDays === 1) return '1 day ago';
    if (diffDays < 7) return `${diffDays} days ago`;
    if (diffDays < 30) return `${Math.floor(diffDays / 7)} weeks ago`;
    return date.toLocaleDateString();
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

        <div className="info-section">
          <h2>Quick Info</h2>
          <div className="info-content">
            <p><strong>Library Hours:</strong> Monday-Friday: 8:00 AM - 8:00 PM</p>
            <p><strong>Loan Period:</strong> 14 days (renewable up to 2 times)</p>
            <p><strong>Fine Rate:</strong> Rs.20.00 per day for overdue items</p>
            {recentActivities.length === 0 && (
              <p className="no-activity">No recent activities to display.</p>
            )}
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

const getStatIcon = (key) => {
  const iconMap = {
    totalUsers: '👥',
    totalBooks: '📚',
    activeLoans: '📖',
    overdueBooks: '⏰',
    overdueLoans: '⚠️',
    outstandingFines: '💰',
    outstandingFinesCount: '💰',
    totalFines: '📋',
    pendingReservations: '🔖',
    totalReservations: '📋',
    finesCollected: '💵',
    booksLoaned: '📚',
    booksReserved: '🔖',
    dueThisWeek: '⏰',
    totalRead: '✅',
    paidFines: '✅'
  };
  return iconMap[key] || '📊';
};

export default HomePage;
