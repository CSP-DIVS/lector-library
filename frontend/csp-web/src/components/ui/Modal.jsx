const Modal = ({ open, onClose, title, children, footer }) => {
  if (!open) return null;
  return (
    <div role="dialog" aria-modal="true" onClick={onClose} style={{ 
      position: 'fixed', 
      inset: 0, 
      background: 'rgba(0,0,0,.5)', 
      display: 'grid', 
      placeItems: 'center', 
      zIndex: 40,
      animation: 'fadeInScale 0.2s ease-out'
    }}>
      <div className="card" onClick={(e) => e.stopPropagation()} style={{ 
        width: 'min(92vw, 720px)', 
        padding: 20, 
        background: 'var(--color-surface)', 
        border: '1px solid var(--color-border)', 
        borderRadius: '16px',
        color: 'var(--color-text)',
        boxShadow: '0 25px 50px rgba(0, 0, 0, 0.25)',
        animation: 'fadeInScale 0.2s ease-out'
      }}>
        {title && <div style={{ 
          fontSize: 18, 
          fontWeight: 700, 
          marginBottom: 12, 
          color: 'var(--color-text)',
          borderBottom: '1px solid var(--color-border)',
          paddingBottom: '12px'
        }}>{title}</div>}
        <div style={{ color: 'var(--color-text)', lineHeight: '1.5' }}>{children}</div>
        {footer && <div style={{ 
          marginTop: 16, 
          display: 'flex', 
          gap: 10, 
          justifyContent: 'flex-end',
          paddingTop: '16px',
          borderTop: '1px solid var(--color-border)'
        }}>{footer}</div>}
      </div>
    </div>
  );
};

export default Modal;


