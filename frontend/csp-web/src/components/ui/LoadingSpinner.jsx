const LoadingSpinner = ({ size = 24 }) => (
  <div style={{ display: 'inline-block', width: size, height: size }} aria-label="loading">
    <svg viewBox="0 0 50 50" style={{ width: '100%', height: '100%' }}>
      <circle cx="25" cy="25" r="20" stroke="var(--color-primary)" strokeWidth="5" fill="none" strokeLinecap="round" opacity=".2" />
      <circle cx="25" cy="25" r="20" stroke="var(--color-primary)" strokeWidth="5" fill="none" strokeLinecap="round" strokeDasharray="90 150" strokeDashoffset="0">
        <animateTransform attributeName="transform" type="rotate" from="0 25 25" to="360 25 25" dur="1s" repeatCount="indefinite" />
      </circle>
    </svg>
  </div>
);

export default LoadingSpinner;


