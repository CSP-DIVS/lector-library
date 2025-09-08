const StatusBadge = ({ active }) => (
  <span className={`badge ${active ? 'badge-success' : 'badge-error'}`}>
    <span style={{ width: 8, height: 8, borderRadius: 999, background: active ? '#10b981' : '#ef4444' }} />
    {active ? 'Active' : 'Inactive'}
  </span>
);

export default StatusBadge;


