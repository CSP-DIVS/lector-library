-- ========================================
-- Library Management Test Data
-- For testing User Management Fine Adjustment Feature
-- ========================================

-- Note: Adjust table names and structure if needed based on your actual schema
-- This script assumes the tables: users, books, book_inventory, lendings

-- ========================================
-- 1. TEST USERS
-- ========================================

-- Create some test users (password is 'password123' hashed)
-- You may need to adjust the password hash format based on your implementation
INSERT INTO users (Username, Email, PasswordHash, Role, IsActive) VALUES
('admin_user', 'admin@library.com', '$2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Administrator', 1),
('librarian_user', 'librarian@library.com', '$2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Librarian', 1),
('john_doe', 'john.doe@email.com', '$2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Member', 1),
('jane_smith', 'jane.smith@email.com', '$2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Member', 1),
('mike_wilson', 'mike.wilson@email.com', '$2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Member', 1),
('sarah_davis', 'sarah.davis@email.com', '$2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'Member', 1);

-- ========================================
-- 2. TEST BOOKS
-- ========================================

INSERT INTO books (Title, Author, Isbn, Category, PublishedYear, IsActive, CreatedBy, UpdatedBy) VALUES
('The Great Gatsby', 'F. Scott Fitzgerald', '9780743273565', 'Fiction', 1925, 1, 1, 1),
('To Kill a Mockingbird', 'Harper Lee', '9780061120084', 'Fiction', 1960, 1, 1, 1),
('1984', 'George Orwell', '9780451524935', 'Dystopian Fiction', 1949, 1, 1, 1),
('Pride and Prejudice', 'Jane Austen', '9780141439518', 'Romance', 1813, 1, 1, 1),
('The Catcher in the Rye', 'J.D. Salinger', '9780316769174', 'Fiction', 1951, 1, 1, 1),
('Lord of the Flies', 'William Golding', '9780571056866', 'Fiction', 1954, 1, 1, 1),
('The Hobbit', 'J.R.R. Tolkien', '9780547928227', 'Fantasy', 1937, 1, 1, 1),
('Brave New World', 'Aldous Huxley', '9780060850524', 'Science Fiction', 1932, 1, 1, 1),
('Animal Farm', 'George Orwell', '9780451526342', 'Political Satire', 1945, 1, 1, 1),
('Jane Eyre', 'Charlotte Brontë', '9780141441146', 'Gothic Fiction', 1847, 1, 1, 1);

-- ========================================
-- 3. BOOK INVENTORY
-- ========================================

-- Add inventory for books (if you have a separate inventory table)
INSERT INTO book_inventory (BookId, AvailableCopies, TotalCopies, CreatedBy, UpdatedBy) VALUES
(1, 3, 5, 1, 1),
(2, 2, 4, 1, 1),
(3, 1, 3, 1, 1),
(4, 4, 6, 1, 1),
(5, 2, 3, 1, 1),
(6, 1, 2, 1, 1),
(7, 5, 8, 1, 1),
(8, 3, 4, 1, 1),
(9, 2, 3, 1, 1),
(10, 1, 2, 1, 1);

-- ========================================
-- 4. ACTIVE LOANS WITH FINES
-- ========================================

-- Create some overdue loans with fines for testing fine adjustment feature
-- john_doe (UserId 3) - Has multiple fines
INSERT INTO lendings (BookId, UserId, BorrowDate, DueDate, ReturnDate, Status, FineAmount, FinePaid, RenewalCount, CreatedBy, UpdatedBy) VALUES
(1, 3, '2024-08-15', '2024-09-01', NULL, 'Overdue', 12.50, 0, 0, 1, 1),
(2, 3, '2024-08-20', '2024-09-05', NULL, 'Overdue', 8.75, 0, 1, 1, 1),
(3, 3, '2024-09-01', '2024-09-15', '2024-09-18', 'Returned', 5.00, 0, 0, 1, 1);

-- jane_smith (UserId 4) - Has one significant fine
INSERT INTO lendings (BookId, UserId, BorrowDate, DueDate, ReturnDate, Status, FineAmount, FinePaid, RenewalCount, CreatedBy, UpdatedBy) VALUES
(4, 4, '2024-07-10', '2024-07-25', NULL, 'Overdue', 25.00, 0, 2, 1, 1),
(5, 4, '2024-09-10', '2024-09-25', '2024-09-22', 'Returned', 0.00, 1, 0, 1, 1);

-- mike_wilson (UserId 5) - Has mixed fines (some paid, some unpaid)
INSERT INTO lendings (BookId, UserId, BorrowDate, DueDate, ReturnDate, Status, FineAmount, FinePaid, RenewalCount, CreatedBy, UpdatedBy) VALUES
(6, 5, '2024-08-01', '2024-08-15', '2024-08-20', 'Returned', 7.50, 1, 0, 1, 1),
(7, 5, '2024-08-25', '2024-09-10', NULL, 'Overdue', 15.25, 0, 1, 1, 1),
(8, 5, '2024-09-05', '2024-09-20', NULL, 'Active', 0.00, 0, 0, 1, 1);

-- sarah_davis (UserId 6) - Has one unpaid fine
INSERT INTO lendings (BookId, UserId, BorrowDate, DueDate, ReturnDate, Status, FineAmount, FinePaid, RenewalCount, CreatedBy, UpdatedBy) VALUES
(9, 6, '2024-08-05', '2024-08-20', NULL, 'Overdue', 18.00, 0, 0, 1, 1);

-- ========================================
-- 5. ADDITIONAL CURRENT LOANS (NO FINES)
-- ========================================

-- Some current active loans without fines
INSERT INTO lendings (BookId, UserId, BorrowDate, DueDate, ReturnDate, Status, FineAmount, FinePaid, RenewalCount, CreatedBy, UpdatedBy) VALUES
(10, 3, '2024-10-01', '2024-10-15', NULL, 'Active', 0.00, 0, 0, 1, 1),
(1, 4, '2024-10-05', '2024-10-19', NULL, 'Active', 0.00, 0, 0, 1, 1),
(2, 5, '2024-10-10', '2024-10-24', NULL, 'Active', 0.00, 0, 0, 1, 1);

-- ========================================
-- 6. AUDIT LOG ENTRIES (for fine adjustments)
-- ========================================

-- If you have an audit log table, add some sample adjustment records
-- INSERT INTO audit_log (TableName, RecordId, Action, OldValues, NewValues, UserId, Timestamp) VALUES
-- ('lendings', 1, 'FINE_ADJUSTED', '{"FineAmount": 15.00}', '{"FineAmount": 12.50, "Reason": "Student hardship"}', 1, NOW()),
-- ('lendings', 4, 'FINE_WAIVED', '{"FineAmount": 30.00}', '{"FineAmount": 0.00, "Reason": "System error"}', 1, NOW());

-- ========================================
-- 7. VERIFICATION QUERIES
-- ========================================

-- Check users with fines
SELECT 
    u.Id,
    u.Username,
    u.Email,
    u.Role,
    COUNT(l.Id) as TotalLoans,
    SUM(CASE WHEN l.FineAmount > 0 AND l.FinePaid = 0 THEN l.FineAmount ELSE 0 END) as TotalUnpaidFines,
    SUM(CASE WHEN l.FineAmount > 0 AND l.FinePaid = 1 THEN l.FineAmount ELSE 0 END) as TotalPaidFines
FROM users u
LEFT JOIN lendings l ON u.Id = l.UserId
WHERE u.Role = 'Member'
GROUP BY u.Id, u.Username, u.Email, u.Role
ORDER BY TotalUnpaidFines DESC;

-- Check detailed fine information
SELECT 
    l.Id as LendingId,
    u.Username,
    b.Title,
    b.Author,
    l.BorrowDate,
    l.DueDate,
    l.ReturnDate,
    l.Status,
    l.FineAmount,
    l.FinePaid,
    CASE 
        WHEN l.Status = 'Overdue' AND l.DueDate < NOW() THEN DATEDIFF(NOW(), l.DueDate)
        WHEN l.Status = 'Returned' AND l.ReturnDate > l.DueDate THEN DATEDIFF(l.ReturnDate, l.DueDate)
        ELSE 0 
    END as DaysOverdue
FROM lendings l
JOIN users u ON l.UserId = u.Id
JOIN books b ON l.BookId = b.Id
WHERE l.FineAmount > 0
ORDER BY u.Username, l.DueDate;