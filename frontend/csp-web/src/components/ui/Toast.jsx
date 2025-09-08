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
        <div key={t.id} className="card shadow-hover" style={{ padding: 12, minWidth: 260, borderLeft: `4px solid ${t.color || 'var(--color-primary)'}` }}>
          <div style={{ fontWeight: 700, marginBottom: 6 }}>{t.title || 'Notice'}</div>
          <div style={{ color: 'var(--color-muted)' }}>{t.message}</div>
        </div>
      ))}
    </div>
  );
};

export default Toast;


