import { useEffect, useMemo, useState } from 'react';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import api from '../lib/api';
import StatusBadge from './ui/StatusBadge';
import Modal from './ui/Modal';
import Button from './ui/Button';
import { toast } from './ui/Toast';
import './MemberManagement.css';

const MemberManagement = ({ user }) => {
  const [items, setItems] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [q, setQ] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [form, setForm] = useState({ 
    username: '', 
    email: '', 
    password: '', 
    role: 'Member'
  });
  const [edit, setEdit] = useState(null);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [confirm, setConfirm] = useState({ open: false, target: null });
  const [showAddForm, setShowAddForm] = useState(false);
  const [loading, setLoading] = useState(false);

  const roles = ['Administrator', 'Librarian', 'Member'];
  
  const canSubmit = useMemo(() => 
    form.username && 
    form.email && 
    form.role && 
    (edit ? true : form.password), 
    [form, edit]
  );

  const load = async () => {
    setLoading(true);
    try {
      const params = { q, page, pageSize };
      if (roleFilter) params.role = roleFilter;
      if (statusFilter === 'active') params.isActive = true;
      if (statusFilter === 'inactive') params.isActive = false;
      
      // Try different API endpoints based on what's available
      let r;
      try {
        r = await api.get('/users', { params });
      } catch (firstErr) {
        // Fallback to members endpoint if users endpoint doesn't exist
        try {
          r = await api.get('/users/members', { params });
        } catch (secondErr) {
          throw new Error('Unable to fetch users from any endpoint');
        }
      }
      
      let users = r.data.items || r.data || [];
      
      // Client-side filtering if server doesn't support it
      if (roleFilter && users.length > 0) {
        users = users.filter(user => user.role === roleFilter);
      }
      
      if (statusFilter && users.length > 0) {
        if (statusFilter === 'active') {
          users = users.filter(user => user.isActive === true);
        } else if (statusFilter === 'inactive') {
          users = users.filter(user => user.isActive === false);
        }
      }
      
      setItems(users);
      setTotal(users.length);
    } catch (err) {
      setError('Failed to load users');
      console.error('Load users error:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [q, page, roleFilter, statusFilter]);

  const submit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);
    
    try {
      if (edit) {
        // Try different update endpoints
        try {
          await api.put(`/users/${edit.id}`, {
            username: form.username,
            email: form.email,
            role: form.role
          });
        } catch (err) {
          // Fallback to members endpoint
          await api.put(`/users/members/${edit.id}`, {
            username: form.username,
            email: form.email
          });
        }
        setEdit(null);
        setSuccess('User updated successfully');
      } else {
        // Try different create endpoints
        try {
          await api.post('/users', form);
        } catch (err) {
          // Fallback to members endpoint
          await api.post('/users/members', form);
        }
        setSuccess(`${form.role} created successfully`);
      }
      
      resetForm();
      await load();
      toast({ 
        title: 'Success', 
        message: edit ? 'User updated' : `${form.role} created`, 
        color: 'var(--color-success)' 
      });
    } catch (err) {
      const message = err?.response?.data?.message || 'Operation failed';
      setError(message);
      toast({ title: 'Error', message, color: 'var(--color-error)' });
    } finally {
      setLoading(false);
    }
  };

  const resetForm = () => {
    setForm({ 
      username: '', 
      email: '', 
      password: '', 
      role: 'Member'
    });
    setShowAddForm(false);
    setError('');
    setSuccess('');
  };

  const startEdit = (user) => {
    setEdit(user);
    setForm({
      username: user.username,
      email: user.email,
      password: '',
      role: user.role
    });
    setShowAddForm(true);
    setError('');
    setSuccess('');
  };

  const getRoleBadgeColor = (role) => {
    switch (role) {
      case 'Administrator': return '#dc2626';
      case 'Librarian': return '#2563eb';
      case 'Member': return '#16a34a';
      default: return '#6b7280';
    }
  };

  const handleExportReport = () => {
    try {
      const doc = new jsPDF();
      
      // Add title
      doc.setFontSize(20);
      doc.setFont('helvetica', 'bold');
      doc.text('Lector Library User Details', 105, 20, { align: 'center' });
      
      // Add generation date
      doc.setFontSize(10);
      doc.setFont('helvetica', 'normal');
      doc.text(`Generated on: ${new Date().toLocaleString()}`, 105, 28, { align: 'center' });
      
      // Add summary statistics
      doc.setFontSize(12);
      doc.setFont('helvetica', 'bold');
      doc.text('Summary', 14, 40);
      
      const admins = items.filter(u => u.role === 'Administrator');
      const librarians = items.filter(u => u.role === 'Librarian');
      const members = items.filter(u => u.role === 'Member');
      const activeUsers = items.filter(u => u.isActive);
      const inactiveUsers = items.filter(u => !u.isActive);
      
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(10);
      doc.text(`Total Users: ${items.length}`, 14, 48);
      doc.text(`Administrators: ${admins.length}`, 14, 54);
      doc.text(`Librarians: ${librarians.length}`, 14, 60);
      doc.text(`Members: ${members.length}`, 14, 66);
      doc.text(`Active: ${activeUsers.length} | Inactive: ${inactiveUsers.length}`, 14, 72);
      
      // Prepare table data
      const tableData = items.map(user => [
        user.username,
        user.email,
        user.role,
        user.isActive ? 'Active' : 'Inactive'
      ]);
      
      // Add users table
      autoTable(doc, {
        startY: 80,
        head: [['Username', 'Email', 'Role', 'Status']],
        body: tableData,
        theme: 'grid',
        styles: {
          fontSize: 9,
          cellPadding: 3,
        },
        headStyles: {
          fillColor: [99, 102, 241],
          textColor: 255,
          fontStyle: 'bold',
        },
        columnStyles: {
          0: { cellWidth: 40 },  // Username
          1: { cellWidth: 70 },  // Email
          2: { cellWidth: 35 },  // Role
          3: { cellWidth: 30 },  // Status
        },
        alternateRowStyles: {
          fillColor: [245, 245, 245]
        },
        didDrawPage: (data) => {
          // Footer
          const pageCount = doc.internal.getNumberOfPages();
          doc.setFontSize(8);
          doc.setFont('helvetica', 'normal');
          doc.text(
            `Page ${data.pageNumber} of ${pageCount}`,
            doc.internal.pageSize.width / 2,
            doc.internal.pageSize.height - 10,
            { align: 'center' }
          );
        }
      });
      
      // Save the PDF
      const fileName = `User_Management_Report_${new Date().toISOString().split('T')[0]}.pdf`;
      doc.save(fileName);
      
      toast.success('Report exported successfully!');
    } catch (error) {
      console.error('Error generating report:', error);
      toast.error('Failed to generate report. Please try again.');
    }
  };

  return (
    <div className="user-management">
      <div className="page-header">
        <div className="header-content">
          <h1 className="page-title">User Management</h1>
          <p className="page-description">Manage administrators, librarians, and members</p>
        </div>
        <div className="header-actions">
          <Button 
            variant="outline" 
            onClick={handleExportReport}
            disabled={items.length === 0}
            className="export-btn"
          >
            📄 Export Report
          </Button>
          <Button 
            variant="primary" 
            onClick={() => setShowAddForm(true)}
            className="add-user-btn"
          >
            <span className="btn-icon">👤</span>
            Add New User
          </Button>
        </div>
      </div>

      {/* Filters */}
      <div className="filters-section">
        <div className="search-container">
          <input 
            type="text"
            placeholder="Search by name, username, or email..." 
            value={q} 
            onChange={(e) => setQ(e.target.value)}
            className="search-input"
          />
        </div>
        <div className="filter-controls">
          <select 
            value={roleFilter} 
            onChange={(e) => setRoleFilter(e.target.value)}
            className="filter-select"
          >
            <option value="">All Roles</option>
            {roles.map(role => (
              <option key={role} value={role}>{role}</option>
            ))}
          </select>
          <select 
            value={statusFilter} 
            onChange={(e) => setStatusFilter(e.target.value)}
            className="filter-select"
          >
            <option value="">All Status</option>
            <option value="active">Active</option>
            <option value="inactive">Inactive</option>
          </select>
        </div>
      </div>

      {/* Add/Edit User Form */}
      {showAddForm && (
        <div className="add-user-section">
          <div className="form-header">
            <h3 className="form-title">
              {edit ? 'Edit User' : 'Add New User'}
            </h3>
            <Button 
              variant="outline" 
              onClick={() => {
                setEdit(null);
                resetForm();
              }}
              className="close-form-btn"
            >
              ✕
            </Button>
          </div>

          {error && (
            <div className="alert alert-error">
              <span className="alert-icon">⚠️</span>
              {error}
            </div>
          )}

          {success && (
            <div className="alert alert-success">
              <span className="alert-icon">✅</span>
              {success}
            </div>
          )}

          <form onSubmit={submit} className="user-form">
            <div className="form-grid">
              <div className="form-group">
                <label>Username *</label>
                <input 
                  type="text"
                  placeholder="Enter username" 
                  value={form.username} 
                  onChange={(e) => setForm({ ...form, username: e.target.value })}
                  className="form-input"
                  required
                />
              </div>

              <div className="form-group">
                <label>Email *</label>
                <input 
                  type="email"
                  placeholder="Enter email address" 
                  value={form.email} 
                  onChange={(e) => setForm({ ...form, email: e.target.value })}
                  className="form-input"
                  required
                />
              </div>

              <div className="form-group">
                <label>Role *</label>
                <select 
                  value={form.role} 
                  onChange={(e) => setForm({ ...form, role: e.target.value })}
                  className="form-select"
                  required
                >
                  {roles.map(role => (
                    <option key={role} value={role}>{role}</option>
                  ))}
                </select>
              </div>

              {!edit && (
                <div className="form-group">
                  <label>Password *</label>
                  <input 
                    type="password"
                    placeholder="Enter password" 
                    value={form.password} 
                    onChange={(e) => setForm({ ...form, password: e.target.value })}
                    className="form-input"
                    required
                  />
                </div>
              )}
            </div>

            <div className="form-actions">
              <Button 
                type="submit" 
                disabled={!canSubmit || loading}
                variant="primary"
              >
                {loading ? 'Processing...' : (edit ? 'Update User' : 'Create User')}
              </Button>
              <Button 
                type="button" 
                variant="outline" 
                onClick={() => {
                  setEdit(null);
                  resetForm();
                }}
              >
                Cancel
              </Button>
            </div>
          </form>
        </div>
      )}

      {/* Users Table */}
      <div className="users-table-container">
        {loading ? (
          <div className="loading-state">
            <div className="loading-spinner"></div>
            <p>Loading users...</p>
          </div>
        ) : (
          <table className="users-table">
            <thead>
              <tr>
                <th>User</th>
                <th>Username</th>
                <th>Email</th>
                <th>Role</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {items.map(user => (
                <tr key={user.id}>
                  <td>
                    <div className="user-info">
                      <div className="user-avatar">
                        {(user.firstName?.[0] || user.username?.[0] || '?').toUpperCase()}
                      </div>
                      <div className="user-details">
                        <div className="user-name">
                          {user.username}
                        </div>
                        <div className="user-id">ID: {user.id}</div>
                      </div>
                    </div>
                  </td>
                  <td className="username-cell">{user.username}</td>
                  <td className="email-cell">{user.email}</td>
                  <td>
                    <span 
                      className="role-badge" 
                      style={{ backgroundColor: getRoleBadgeColor(user.role) }}
                    >
                      {user.role}
                    </span>
                  </td>
                  <td>
                    <StatusBadge active={user.isActive} />
                  </td>
                  <td>
                    <div className="action-buttons">
                      <Button 
                        variant="outline" 
                        size="sm"
                        onClick={() => startEdit(user)}
                      >
                        Edit
                      </Button>
                      <Button 
                        variant={user.isActive ? 'danger' : 'primary'} 
                        size="sm"
                        onClick={() => setConfirm({ open: true, target: user })}
                      >
                        {user.isActive ? 'Deactivate' : 'Activate'}
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        {items.length === 0 && !loading && (
          <div className="empty-state">
            <div className="empty-icon">👥</div>
            <h3>No users found</h3>
            <p>Try adjusting your search or filters, or add a new user.</p>
          </div>
        )}
      </div>

      {/* Pagination */}
      {total > pageSize && (
        <div className="pagination">
          <Button 
            variant="outline"
            disabled={page === 1} 
            onClick={() => setPage(p => p - 1)}
          >
            Previous
          </Button>
          <span className="pagination-info">
            Page {page} of {Math.ceil(total / pageSize)} • {total} total users
          </span>
          <Button 
            variant="outline"
            disabled={(page * pageSize) >= total} 
            onClick={() => setPage(p => p + 1)}
          >
            Next
          </Button>
        </div>
      )}

      {/* Confirmation Modal */}
      <Modal 
        open={confirm.open} 
        onClose={() => setConfirm({ open: false, target: null })} 
        title={`${confirm.target?.isActive ? 'Deactivate' : 'Activate'} User`}
        footer={[
          <Button 
            key="cancel" 
            variant="outline" 
            onClick={() => setConfirm({ open: false, target: null })}
          >
            Cancel
          </Button>,
          <Button 
            key="ok" 
            variant={confirm.target?.isActive ? 'danger' : 'primary'} 
            onClick={async () => {
              const user = confirm.target;
              if (!user) return;
              
              const prev = [...items];
              setItems(prev.map(x => x.id === user.id ? { ...x, isActive: !user.isActive } : x));
              setConfirm({ open: false, target: null });
              
              try {
                await api.put(`/users/${user.id}/status`, null, { 
                  params: { isActive: !user.isActive } 
                });
                toast({ 
                  title: 'Success', 
                  message: `User ${user.isActive ? 'deactivated' : 'activated'}`, 
                  color: 'var(--color-success)' 
                });
              } catch (e) {
                setItems(prev);
                const message = 'Status update failed';
                setError(message);
                toast({ title: 'Error', message, color: 'var(--color-error)' });
              }
            }}
          >
            {confirm.target?.isActive ? 'Deactivate' : 'Activate'}
          </Button>
        ]}
      >
        <p style={{ color: 'var(--color-text)', fontSize: '1rem', lineHeight: '1.5' }}>
          Are you sure you want to {confirm.target?.isActive ? 'deactivate' : 'activate'} this user? 
          {confirm.target?.isActive && ' This will prevent them from accessing the system.'}
        </p>
      </Modal>
    </div>
  );
};

export default MemberManagement;


