import { useEffect, useState } from 'react';

let listeners = [];
export const toast = (payload) => {
  listeners.forEach(fn => fn(payload));
};

const Toast = () => {
  const [items, setItems] = useState([]);
  useEffect(() => {
    const sub = (p) => {
      const id = Math.random().toString(36).slice(2);
      setItems(prev => [...prev, { id, ...p }]);
      setTimeout(() => setItems(prev => prev.filter(x => x.id !== id)), p.duration || 3000);
    };
    listeners.push(sub);
    return () => { listeners = listeners.filter(l => l !== sub); };
  }, []);

  return (
    <div style={{ position: 'fixed', top: 16, right: 16, display: 'flex', flexDirection: 'column', gap: 10, zIndex: 50 }}>
      {items.map(t => (
        <div key={t.id} className="card shadow-hover" style={{ 
          padding: 16, 
          minWidth: 300, 
          borderLeft: `4px solid ${t.color || 'var(--color-primary)'}`,
          background: 'var(--color-surface)',
          border: '1px solid var(--color-border)',
          borderRadius: '12px',
          boxShadow: '0 10px 25px rgba(0, 0, 0, 0.15)',
          color: 'var(--color-text)',
          animation: 'slideInFromRight 0.3s ease-out'
        }}>
          <div style={{ 
            fontWeight: 700, 
            marginBottom: 6, 
            color: 'var(--color-text)',
            fontSize: '0.95rem'
          }}>{t.title || 'Notice'}</div>
          <div style={{ 
            color: 'var(--color-text-secondary)',
            fontSize: '0.9rem',
            lineHeight: '1.4'
          }}>{t.message}</div>
        </div>
      ))}
    </div>
  );
};

export default Toast;


