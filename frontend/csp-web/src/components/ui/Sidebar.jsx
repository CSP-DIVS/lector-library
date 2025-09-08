const Sidebar = ({ user, onSelect }) => (
  <aside style={{ width: 240, padding: 16 }}>
    <div className="card" style={{ padding: 16 }}>
      <div style={{ fontWeight: 800, marginBottom: 12 }}>Lector Library</div>
      <nav style={{ display: 'grid', gap: 8 }}>
        <button className="action-btn" style={{ padding: 10 }} onClick={() => onSelect('home')}>Home</button>
        {user.role === 'Administrator' && (
          <button className="action-btn" style={{ padding: 10 }} onClick={() => onSelect('members')}>Member Management</button>
        )}
        {user.role === 'Member' && (
          <button className="action-btn" style={{ padding: 10 }} onClick={() => onSelect('profile')}>My Profile</button>
        )}
      </nav>
    </div>
  </aside>
);

export default Sidebar;


