import { useEffect, useMemo, useState } from 'react';
import api from '../lib/api';
import StatusBadge from './ui/StatusBadge';
import Modal from './ui/Modal';
import Button from './ui/Button';
import { toast } from './ui/Toast';

const MemberManagement = () => {
  const [items, setItems] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [q, setQ] = useState('');
  const [form, setForm] = useState({ username: '', email: '', password: '' });
  const [edit, setEdit] = useState(null);
  const [error, setError] = useState('');
  const [confirm, setConfirm] = useState({ open: false, target: null });

  const canSubmit = useMemo(() => form.username && form.email && (edit ? true : form.password), [form, edit]);

  const load = async () => {
    const r = await api.get('/api/users/members', { params: { q, page, pageSize } });
    setItems(r.data.items || []);
    setTotal(r.data.total || 0);
  };

  useEffect(() => { load(); }, [q, page]);

  const submit = async (e) => {
    e.preventDefault();
    setError('');
    try {
      if (edit) {
        await api.put(`/api/users/members/${edit.id}`, { username: form.username, email: form.email });
        setEdit(null);
      } else {
        await api.post('/api/users/members', form);
      }
      setForm({ username: '', email: '', password: '' });
      await load();
      toast({ title: 'Success', message: edit ? 'Member updated' : 'Member registered', color: 'var(--color-success)' });
    } catch (err) {
      setError(err?.response?.data?.message || 'Operation failed');
      toast({ title: 'Error', message: 'Operation failed', color: 'var(--color-error)' });
    }
  };

  const startEdit = (m) => {
    setEdit(m);
    setForm({ username: m.username, email: m.email, password: '' });
  };

  return (
    <div style={{ padding: 16 }}>
      <h2>Member Management</h2>
      <div style={{ marginBottom: 16 }}>
        <input placeholder="Search members" value={q} onChange={(e) => setQ(e.target.value)} />
      </div>
      <form onSubmit={submit} style={{ marginBottom: 16, display: 'grid', gap: 8, gridTemplateColumns: 'repeat(6, 1fr)' }}>
        <input style={{ gridColumn: 'span 2' }} placeholder="Username" value={form.username} onChange={(e) => setForm({ ...form, username: e.target.value })} />
        <input style={{ gridColumn: 'span 3' }} placeholder="Email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
        {!edit && <input style={{ gridColumn: 'span 1' }} placeholder="Password" type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />}
        <div style={{ gridColumn: 'span 6', display: 'flex', gap: 8 }}>
          <Button type="submit" disabled={!canSubmit}>{edit ? 'Save' : 'Register'}</Button>
          {edit && <Button type="button" variant="outline" onClick={() => { setEdit(null); setForm({ username: '', email: '', password: '' }); }}>Cancel</Button>}
        </div>
      </form>
      {error && <div style={{ color: 'red' }}>{error}</div>}
      <table width="100%" cellPadding="8" style={{ borderCollapse: 'collapse' }}>
        <thead>
          <tr>
            <th align="left">Id</th>
            <th align="left">Username</th>
            <th align="left">Email</th>
            <th align="left">Status</th>
            <th align="left">Actions</th>
          </tr>
        </thead>
        <tbody>
          {items.map(m => (
            <tr key={m.id}>
              <td>{m.id}</td>
              <td>{m.username}</td>
              <td>{m.email}</td>
              <td><StatusBadge active={m.isActive} /></td>
              <td>
                <Button variant="outline" onClick={() => startEdit(m)}>Edit</Button>
                <Button variant={m.isActive ? 'danger' : 'primary'} onClick={() => setConfirm({ open: true, target: m })}>{m.isActive ? 'Deactivate' : 'Reactivate'}</Button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <div style={{ marginTop: 8 }}>
        <button disabled={page === 1} onClick={() => setPage(p => p - 1)}>Prev</button>
        <span style={{ margin: '0 8px' }}>Page {page}</span>
        <button disabled={(page * pageSize) >= total} onClick={() => setPage(p => p + 1)}>Next</button>
      </div>
      <Modal open={confirm.open} onClose={() => setConfirm({ open: false, target: null })} title={confirm.target?.isActive ? 'Deactivate member' : 'Reactivate member'}
        footer={[
          <Button key="cancel" variant="outline" onClick={() => setConfirm({ open: false, target: null })}>Cancel</Button>,
          <Button key="ok" variant={confirm.target?.isActive ? 'danger' : 'primary'} onClick={async () => {
            const m = confirm.target; if (!m) return;
            const prev = [...items];
            setItems(prev.map(x => x.id === m.id ? { ...x, isActive: !m.isActive } : x));
            setConfirm({ open: false, target: null });
            try {
              await api.put(`/api/users/${m.id}/status`, null, { params: { isActive: !m.isActive } });
              toast({ title: 'Success', message: 'Status updated', color: 'var(--color-success)' });
            } catch (e) {
              setItems(prev);
              setError('Status update failed');
              toast({ title: 'Error', message: 'Status update failed', color: 'var(--color-error)' });
            }
          }}>Confirm</Button>
        ]}
      >
        <p style={{ color: '#9ca3af' }}>This action will {confirm.target?.isActive ? 'deactivate' : 'reactivate'} the member account.</p>
      </Modal>
    </div>
  );
};

export default MemberManagement;


