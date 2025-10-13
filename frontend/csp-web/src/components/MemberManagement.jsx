import { useEffect, useMemo, useState, Fragment } from 'react';
import api, { finesApi } from '../lib/api';
import StatusBadge from './ui/StatusBadge';
import Modal from './ui/Modal';
import Button from './ui/Button';
import { toast } from './ui/Toast';
import './MemberManagement.css';
import AdjustFineModal from './ui/AdjustFineModal';

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
  // Inline fines panel state
  const [expanded, setExpanded] = useState({}); // userId -> boolean
  const [userFines, setUserFines] = useState({}); // userId -> fines[]
  const [adjustFineModal, setAdjustFineModal] = useState({ isOpen: false, fine: null, userId: null });

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

  // Load real fines data from API
  const loadUserFines = async (userId) => {
    try {
      const response = await finesApi.getUserFines(userId);
      const data = Array.isArray(response?.data) ? response.data : (response?.data?.items || []);
      return data.map(fine => ({
        id: fine.lendingId,
        lendingId: fine.lendingId,
        reason: fine.status === 'Overdue' ? 'Overdue Book' : 'Late Return Fee',
        bookTitle: fine.bookTitle,
        bookAuthor: fine.bookAuthor,
        memberName: items.find(u => u.id === userId)?.username || 'Unknown',
        amount: fine.fineAmount,
        dueDate: fine.dueDate.split('T')[0], // Format date
        borrowDate: fine.borrowDate.split('T')[0], // Format date
        daysOverdue: fine.overdueDays,
        status: fine.finePaid ? 'Paid' : 'Outstanding'
      }));
    } catch (error) {
      console.error('Error loading user fines:', error);
      // Return empty array if API fails
      return [];
    }
  };

  const toggleFinesPanel = async (u) => {
    setExpanded(prev => ({ ...prev, [u.id]: !prev[u.id] }));
    
    // Load fines when expanding if not already cached
    if (!expanded[u.id] && !userFines[u.id]) {
      const fines = await loadUserFines(u.id);
      setUserFines(prev => ({ ...prev, [u.id]: fines }));
    }
  };

  const handleOpenAdjustFine = (fine, userId) => {
    setAdjustFineModal({ isOpen: true, fine, userId });
  };

  const handleAdjustFineSubmit = async (lendingId, adjustmentData) => {
    const uid = adjustFineModal.userId;
    try {
      const response = await finesApi.adjustFine(lendingId, adjustmentData);
      if (response.data?.success) {
        // Update local inline fines state
        setUserFines(prev => ({
          ...prev,
          [uid]: (prev[uid] || []).map(f =>
            f.id === lendingId
              ? {
                  ...f,
                  amount: adjustmentData.newAmount,
                  status: adjustmentData.newAmount === 0 ? 'Paid' : f.status
                }
              : f
          )
        }));
        toast({ title: 'Success', message: response.data.message || 'Fine updated', color: 'var(--color-success)' });
      }
    } catch (error) {
      // Let modal surface the error message via throw
      throw error;
    }
  };

  const closeAdjustFineModal = () => {
    setAdjustFineModal({ isOpen: false, fine: null, userId: null });
  };

  return (
    <div className="user-management">
      <div className="page-header">
        <div className="header-content">
          <h1 className="page-title">User Management</h1>
          <p className="page-description">Manage administrators, librarians, and members</p>
        </div>
        <Button 
          variant="primary" 
          onClick={() => setShowAddForm(true)}
          className="add-user-btn"
        >
          <span className="btn-icon">👤</span>
          Add New User
        </Button>
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
              {items.map(u => (
                <Fragment key={`row-${u.id}`}>
                <tr key={u.id}>
                  <td>
                    <div className="user-info">
                      <div className="user-avatar">
                        {(u.firstName?.[0] || u.username?.[0] || '?').toUpperCase()}
                      </div>
                      <div className="user-details">
                        <div className="user-name">
                          {u.username}
                        </div>
                        <div className="user-id">ID: {u.id}</div>
                      </div>
                    </div>
                  </td>
                  <td className="username-cell">{u.username}</td>
                  <td className="email-cell">{u.email}</td>
                  <td>
                    <span 
                      className="role-badge" 
                      style={{ backgroundColor: getRoleBadgeColor(u.role) }}
                    >
                      {u.role}
                    </span>
                  </td>
                  <td>
                    <StatusBadge active={u.isActive} />
                  </td>
                  <td>
                    <div className="action-buttons">
                      <Button 
                        variant="outline" 
                        size="sm"
                        onClick={() => startEdit(u)}
                      >
                        Edit
                      </Button>
                      <Button 
                        variant={u.isActive ? 'danger' : 'primary'} 
                        size="sm"
                        onClick={() => setConfirm({ open: true, target: u })}
                      >
                        {u.isActive ? 'Deactivate' : 'Activate'}
                      </Button>
                      {/* Admin-only: inline fines panel toggle */}
                      {user?.role === 'Administrator' && (
                        <Button 
                          variant="outline" 
                          size="sm"
                          onClick={() => toggleFinesPanel(u)}
                        >
                          {expanded[u.id] ? 'Hide Fines' : 'View Fines'}
                        </Button>
                      )}
                    </div>
                  </td>
                </tr>
                {expanded[u.id] && (
                  <tr key={`${u.id}-fines`}>
                    <td colSpan={6}>
                      <div className="member-fines-panel" style={{ padding: '12px 16px', background: 'var(--color-surface)', border: '1px solid var(--color-border)', borderRadius: 8 }}>
                        <div className="panel-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
                          <h4 style={{ margin: 0 }}>Fines for {u.username}</h4>
                        </div>
                        <div className="fines-inline-list" style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
                          {(userFines[u.id] || []).map(f => (
                            <div key={f.id} className="fine-inline-card" style={{ display: 'flex', justifyContent: 'space-between', gap: 12, padding: 12, border: '1px solid var(--color-border)', borderRadius: 8 }}>
                              <div className="fine-inline-info" style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
                                <strong>{f.reason}</strong>
                                <span>{f.bookTitle} by {f.bookAuthor}</span>
                                <span>Status: {f.status}</span>
                                <span>Due: {f.dueDate} • Overdue since: {f.overdueDate} • {f.daysOverdue} days</span>
                              </div>
                              <div className="fine-inline-actions" style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                                <div style={{ textAlign: 'right', marginRight: 8 }}>
                                  <div className="amount-label" style={{ fontSize: 12, color: 'var(--color-text-secondary)' }}>Amount</div>
                                  <div className="amount-value" style={{ fontWeight: 700, color: 'var(--color-error)' }}>${f.amount.toFixed(2)}</div>
                                </div>
                                {user?.role === 'Administrator' && f.status === 'Outstanding' && (
                                  <Button 
                                    variant="secondary"
                                    size="sm"
                                    onClick={() => handleOpenAdjustFine(f, u.id)}
                                  >
                                    Adjust Fine
                                  </Button>
                                )}
                              </div>
                            </div>
                          ))}
                          {(userFines[u.id] || []).length === 0 && (
                            <div style={{ color: 'var(--color-text-secondary)' }}>No fines found for this member.</div>
                          )}
                        </div>
                      </div>
                    </td>
                  </tr>
                )}
                </Fragment>
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
              const userTarget = confirm.target;
              if (!userTarget) return;

              const prev = [...items];
              setItems(prev.map(x => x.id === userTarget.id ? { ...x, isActive: !userTarget.isActive } : x));
              setConfirm({ open: false, target: null });

              try {
                await api.put(`/users/${userTarget.id}/status`, null, {
                  params: { isActive: !userTarget.isActive }
                });
                toast({
                  title: 'Success',
                  message: `User ${userTarget.isActive ? 'deactivated' : 'activated'}`,
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

      {/* Adjust Fine Modal for inline fines */}
      <AdjustFineModal
        isOpen={adjustFineModal.isOpen}
        onClose={closeAdjustFineModal}
        fine={adjustFineModal.fine}
        onAdjustFine={handleAdjustFineSubmit}
      />
    </div>
  );
};

export default MemberManagement;


