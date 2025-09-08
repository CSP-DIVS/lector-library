const StatCard = ({ label, value, hint }) => (
  <div className="info-card shadow-hover" style={{ gridColumn: 'span 4' }}>
    <div style={{ fontSize: 12, color: '#9ca3af', fontWeight: 700, textTransform: 'uppercase', letterSpacing: '.08em' }}>{label}</div>
    <div style={{ fontSize: 28, fontWeight: 900, marginTop: 6 }}>{value}</div>
    {hint && <div style={{ fontSize: 12, color: '#9ca3af', marginTop: 8 }}>{hint}</div>}
  </div>
);

export default StatCard;


