import { useEffect, useMemo, useState } from 'react';
import api from '../lib/api';

const MemberManagement = () => {
  const [items, setItems] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [q, setQ] = useState('');
  const [form, setForm] = useState({ username: '', email: '', password: '' });
  const [edit, setEdit] = useState(null);
  const [error, setError] = useState('');

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
    } catch (err) {
      setError(err?.response?.data?.message || 'Operation failed');
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
      <form onSubmit={submit} style={{ marginBottom: 16 }}>
        <input placeholder="Username" value={form.username} onChange={(e) => setForm({ ...form, username: e.target.value })} />
        <input placeholder="Email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
        {!edit && <input placeholder="Password" type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />}
        <button type="submit" disabled={!canSubmit}>{edit ? 'Save' : 'Register'}</button>
        {edit && <button type="button" onClick={() => { setEdit(null); setForm({ username: '', email: '', password: '' }); }}>Cancel</button>}
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
              <td>{m.isActive ? 'Active' : 'Inactive'}</td>
              <td>
                <button onClick={() => startEdit(m)}>Edit</button>
                <button onClick={async () => {
                  const prev = [...items];
                  setItems(prev.map(x => x.id === m.id ? { ...x, isActive: !m.isActive } : x));
                  try {
                    await api.put(`/api/users/${m.id}/status`, null, { params: { isActive: !m.isActive } });
                  } catch (e) {
                    setItems(prev);
                    setError('Status update failed');
                  }
                }}>{m.isActive ? 'Deactivate' : 'Reactivate'}</button>
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
    </div>
  );
};

export default MemberManagement;


