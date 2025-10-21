CREATE TABLE IF NOT EXISTS Fines (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    LendingId INT NULL,
    UserId INT NOT NULL,
    BookId INT NOT NULL,
    Reason VARCHAR(255) NOT NULL,
    Amount DECIMAL(10, 2) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Outstanding',
    DueDate DATETIME NOT NULL,
    OverdueDate DATETIME NOT NULL,
    DaysOverdue INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (BookId) REFERENCES Books(Id),
    FOREIGN KEY (LendingId) REFERENCES Lendings(Id),
    INDEX idx_fines_userid (UserId),
    INDEX idx_fines_status (Status),
    INDEX idx_fines_bookid (BookId)
);

CREATE TABLE IF NOT EXISTS Payments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    FineId INT NOT NULL,
    UserId INT NOT NULL,
    Amount DECIMAL(10, 2) NOT NULL,
    PaymentMethod VARCHAR(50) NOT NULL,
    TransactionId VARCHAR(100) NOT NULL,
    Description VARCHAR(500) NOT NULL,
    PaymentDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(50) NOT NULL DEFAULT 'Completed',
    FOREIGN KEY (FineId) REFERENCES Fines(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    INDEX idx_payments_fineid (FineId),
    INDEX idx_payments_userid (UserId)
);
