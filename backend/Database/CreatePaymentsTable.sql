-- Create payments table for recording fine payments
-- This table stores all payment transactions for fine payments

CREATE TABLE IF NOT EXISTS payments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    LendingId INT NOT NULL,
    MemberId INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentMethod VARCHAR(50) NOT NULL DEFAULT 'Cash',
    PaymentDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    RecordedBy INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Foreign key constraints
    CONSTRAINT fk_payments_lending FOREIGN KEY (LendingId) REFERENCES lendings(Id) ON DELETE RESTRICT,
    CONSTRAINT fk_payments_member FOREIGN KEY (MemberId) REFERENCES users(Id) ON DELETE RESTRICT,
    CONSTRAINT fk_payments_recorded_by FOREIGN KEY (RecordedBy) REFERENCES users(Id) ON DELETE RESTRICT,
    
    -- Constraints
    CONSTRAINT chk_payment_amount_positive CHECK (Amount > 0),
    CONSTRAINT chk_payment_method_valid CHECK (PaymentMethod IN ('Cash', 'Credit Card', 'Debit Card', 'Bank Transfer', 'Online', 'Check')),
    
    -- Indexes
    INDEX idx_payments_lending (LendingId),
    INDEX idx_payments_member (MemberId),
    INDEX idx_payments_date (PaymentDate),
    INDEX idx_payments_recorded_by (RecordedBy)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;