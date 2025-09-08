const styles = {
  base: {
    display: 'inline-flex', alignItems: 'center', justifyContent: 'center', gap: 8,
    padding: '10px 14px', borderRadius: '10px', fontWeight: 600, fontSize: '0.9rem',
    transition: 'all var(--transition)', cursor: 'pointer', border: '2px solid transparent'
  },
  primary: { 
    background: 'var(--color-primary)', 
    color: 'white',
    borderColor: 'var(--color-primary)'
  },
  outline: { 
    background: 'transparent', 
    color: 'var(--color-text)', 
    borderColor: 'var(--color-border)'
  },
  danger: { 
    background: 'var(--color-error)', 
    color: 'white',
    borderColor: 'var(--color-error)'
  },
  ghost: { 
    background: 'transparent', 
    color: 'var(--color-text-secondary)',
    borderColor: 'transparent'
  }
};

const Button = ({ variant = 'primary', children, style, ...rest }) => (
  <button {...rest} style={{ ...styles.base, ...styles[variant], ...style }}>
    {children}
  </button>
);

export default Button;


