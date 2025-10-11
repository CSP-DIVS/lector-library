import { useState } from 'react';
import Modal from './Modal';
import './Modal.css';

const AdjustFineModal = ({ 
  isOpen, 
  onClose, 
  fine, 
  onAdjustFine 
}) => {
  const [newAmount, setNewAmount] = useState(fine?.amount?.toString() || '0');
  const [reason, setReason] = useState('');
  const [loading, setLoading] = useState(false);
  const [errors, setErrors] = useState({});

  const validateForm = () => {
    const newErrors = {};
    
    // Validate new amount
    const amount = parseFloat(newAmount);
    if (isNaN(amount) || amount < 0) {
      newErrors.newAmount = 'Amount must be a valid number >= 0';
    }
    
    // Validate reason
    if (!reason.trim()) {
      newErrors.reason = 'A reason is required for all fine adjustments';
    }
    
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setLoading(true);
    
    try {
      const adjustmentData = {
        newAmount: parseFloat(newAmount),
        reason: reason.trim()
      };
      
      await onAdjustFine(fine.id, adjustmentData);
      
      // Reset form
      setNewAmount('0');
      setReason('');
      setErrors({});
      
      onClose();
    } catch (error) {
      console.error('Error adjusting fine:', error);
      setErrors({ 
        submit: error.response?.data?.message || 'Failed to adjust fine. Please try again.' 
      });
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    setNewAmount(fine?.amount?.toString() || '0');
    setReason('');
    setErrors({});
    onClose();
  };

  const isWaiver = parseFloat(newAmount) === 0;
  const originalAmount = fine?.amount || 0;

  return (
    <Modal
      open={isOpen}
      onClose={handleClose}
      title="Adjust Fine"
    >
      <form onSubmit={handleSubmit} className="adjust-fine-form">
        {fine && (
          <div className="fine-summary">
            <h4>Fine Details</h4>
            <div className="fine-details">
              <p><strong>Book:</strong> {fine.bookTitle} by {fine.bookAuthor}</p>
              <p><strong>Member:</strong> {fine.memberName}</p>
              <p><strong>Current Amount:</strong> ${originalAmount.toFixed(2)}</p>
              <p><strong>Reason:</strong> {fine.reason}</p>
            </div>
          </div>
        )}

        <div className="form-group">
          <label htmlFor="newAmount">
            New Amount *
          </label>
          <input
            id="newAmount"
            type="number"
            min="0"
            step="0.01"
            value={newAmount}
            onChange={(e) => setNewAmount(e.target.value)}
            className={errors.newAmount ? 'error' : ''}
            placeholder="0.00"
          />
          {errors.newAmount && (
            <span className="error-message">{errors.newAmount}</span>
          )}
          <div className="amount-info">
            {isWaiver ? (
              <span className="waiver-notice">
                ✓ This will completely waive the fine
              </span>
            ) : (
              <span className="adjustment-notice">
                Adjustment: ${(parseFloat(newAmount) - originalAmount).toFixed(2)}
              </span>
            )}
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="reason">
            Reason for Adjustment *
          </label>
          <textarea
            id="reason"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            className={errors.reason ? 'error' : ''}
            placeholder="Enter the reason for this fine adjustment..."
            rows={3}
            maxLength={500}
          />
          {errors.reason && (
            <span className="error-message">{errors.reason}</span>
          )}
          <div className="char-count">
            {reason.length}/500 characters
          </div>
        </div>

        {errors.submit && (
          <div className="error-message submit-error">
            {errors.submit}
          </div>
        )}

        <div className="form-actions">
          <button
            type="button"
            onClick={handleClose}
            className="btn btn-outline"
            disabled={loading}
          >
            Cancel
          </button>
          <button
            type="submit"
            className={`btn ${isWaiver ? 'btn-warning' : 'btn-primary'}`}
            disabled={loading}
          >
            {loading ? (
              <>
                <span className="loading-spinner"></span>
                Processing...
              </>
            ) : (
              isWaiver ? 'Waive Fine' : 'Adjust Fine'
            )}
          </button>
        </div>
      </form>

      <style jsx>{`
        .adjust-fine-form {
          max-width: 500px;
        }

        .fine-summary {
          background: var(--color-background);
          padding: 16px;
          border-radius: 8px;
          margin-bottom: 24px;
          border: 1px solid var(--color-border);
        }

        .fine-summary h4 {
          margin: 0 0 12px 0;
          color: var(--color-text);
          font-size: 16px;
        }

        .fine-details p {
          margin: 8px 0;
          font-size: 14px;
          color: var(--color-text-secondary);
        }

        .form-group {
          margin-bottom: 20px;
        }

        .form-group label {
          display: block;
          margin-bottom: 6px;
          font-weight: 600;
          color: var(--color-text);
        }

        .form-group input,
        .form-group textarea {
          width: 100%;
          padding: 10px 12px;
          border: 1px solid var(--color-border);
          border-radius: 6px;
          font-size: 14px;
          background: var(--color-surface);
          color: var(--color-text);
          transition: border-color 0.2s;
        }

        .form-group input:focus,
        .form-group textarea:focus {
          outline: none;
          border-color: var(--color-primary);
        }

        .form-group input.error,
        .form-group textarea.error {
          border-color: var(--color-error);
        }

        .error-message {
          display: block;
          color: var(--color-error);
          font-size: 12px;
          margin-top: 4px;
        }

        .submit-error {
          background: var(--color-error-light);
          padding: 12px;
          border-radius: 6px;
          margin-bottom: 16px;
        }

        .amount-info {
          margin-top: 8px;
          font-size: 13px;
        }

        .waiver-notice {
          color: var(--color-warning);
          font-weight: 600;
        }

        .adjustment-notice {
          color: var(--color-text-secondary);
        }

        .char-count {
          text-align: right;
          font-size: 12px;
          color: var(--color-text-secondary);
          margin-top: 4px;
        }

        .form-actions {
          display: flex;
          gap: 12px;
          justify-content: flex-end;
          margin-top: 24px;
          padding-top: 16px;
          border-top: 1px solid var(--color-border);
        }

        .btn {
          padding: 10px 20px;
          border: none;
          border-radius: 6px;
          font-size: 14px;
          font-weight: 600;
          cursor: pointer;
          transition: all 0.2s;
          display: flex;
          align-items: center;
          gap: 8px;
        }

        .btn:disabled {
          opacity: 0.6;
          cursor: not-allowed;
        }

        .btn-outline {
          background: transparent;
          border: 1px solid var(--color-border);
          color: var(--color-text);
        }

        .btn-outline:hover:not(:disabled) {
          background: var(--color-background);
        }

        .btn-primary {
          background: var(--color-primary);
          color: white;
        }

        .btn-primary:hover:not(:disabled) {
          background: var(--color-primary-dark);
        }

        .btn-warning {
          background: var(--color-warning);
          color: white;
        }

        .btn-warning:hover:not(:disabled) {
          background: var(--color-warning-dark);
        }

        .loading-spinner {
          width: 14px;
          height: 14px;
          border: 2px solid transparent;
          border-top: 2px solid currentColor;
          border-radius: 50%;
          animation: spin 1s linear infinite;
        }

        @keyframes spin {
          to {
            transform: rotate(360deg);
          }
        }
      `}</style>
    </Modal>
  );
};

export default AdjustFineModal;