import { useEffect, useState } from 'react';
import api from '../lib/api';

const MyProfile = () => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [profile, setProfile] = useState({ username: '', email: '' });
  const [pwd, setPwd] = useState({ currentPassword: '', newPassword: '' });
  const [msg, setMsg] = useState('');

  useEffect(() => {
    const load = async () => {
      try {
        const r = await api.get('/api/users/my-profile');
        setProfile({ username: r.data.username, email: r.data.email });
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
      await api.put('/api/users/my-profile', profile);
      setMsg('Profile updated successfully');
      localStorage.setItem('user', JSON.stringify({ ...(JSON.parse(localStorage.getItem('user')||'{}')), username: profile.username, email: profile.email }));
    } catch (e) {
      setError('Update failed');
    }
  };

  const changePassword = async (e) => {
    e.preventDefault();
    setError('');
    setMsg('');
    try {
      await api.put('/api/users/my-password', pwd);
      setMsg('Password changed successfully');
      setPwd({ currentPassword: '', newPassword: '' });
    } catch (e) {
      setError('Password change failed');
    }
  };

  if (loading) return <div style={{ padding: 16 }}>Loading...</div>;

  return (
    <div style={{ padding: 16 }}>
      <h2>My Profile</h2>
      {error && <div style={{ color: 'red' }}>{error}</div>}
      {msg && <div style={{ color: 'green' }}>{msg}</div>}

      <form onSubmit={saveProfile} style={{ marginBottom: 24 }}>
        <div>
          <label>Username</label>
          <input value={profile.username} onChange={(e) => setProfile({ ...profile, username: e.target.value })} />
        </div>
        <div>
          <label>Email</label>
          <input value={profile.email} onChange={(e) => setProfile({ ...profile, email: e.target.value })} />
        </div>
        <button type="submit">Save</button>
      </form>

      <h3>Change Password</h3>
      <form onSubmit={changePassword}>
        <div>
          <label>Current Password</label>
          <input type="password" value={pwd.currentPassword} onChange={(e) => setPwd({ ...pwd, currentPassword: e.target.value })} />
        </div>
        <div>
          <label>New Password</label>
          <input type="password" value={pwd.newPassword} onChange={(e) => setPwd({ ...pwd, newPassword: e.target.value })} />
        </div>
        <button type="submit">Change Password</button>
      </form>
    </div>
  );
};

export default MyProfile;


