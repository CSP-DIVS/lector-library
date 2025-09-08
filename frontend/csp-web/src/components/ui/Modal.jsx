const Modal = ({ open, onClose, title, children, footer }) => {
  if (!open) return null;
  return (
    <div role="dialog" aria-modal="true" onClick={onClose} style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,.5)', display: 'grid', placeItems: 'center', zIndex: 40 }}>
      <div className="card" onClick={(e) => e.stopPropagation()} style={{ width: 'min(92vw, 720px)', padding: 20 }}>
        {title && <div style={{ fontSize: 18, fontWeight: 700, marginBottom: 12 }}>{title}</div>}
        <div>{children}</div>
        {footer && <div style={{ marginTop: 16, display: 'flex', gap: 10, justifyContent: 'flex-end' }}>{footer}</div>}
      </div>
    </div>
  );
};

export default Modal;


