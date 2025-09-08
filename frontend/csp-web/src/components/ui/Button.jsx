const styles = {
  base: {
    display: 'inline-flex', alignItems: 'center', justifyContent: 'center', gap: 8,
    padding: '10px 14px', borderRadius: '10px', fontWeight: 700,
    transition: 'all var(--transition)', cursor: 'pointer', border: '1px solid transparent'
  },
  primary: { background: 'var(--color-primary)', color: 'white' },
  outline: { background: 'transparent', color: 'var(--color-text)', borderColor: 'rgba(255,255,255,.12)' },
  danger: { background: 'var(--color-error)', color: 'white' },
  ghost: { background: 'transparent', color: 'var(--color-muted)' }
};

const Button = ({ variant = 'primary', children, style, ...rest }) => (
  <button {...rest} style={{ ...styles.base, ...styles[variant], ...style }}>
    {children}
  </button>
);

export default Button;


