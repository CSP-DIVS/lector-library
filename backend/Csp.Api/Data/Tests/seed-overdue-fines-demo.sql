-- Demo data for overdue fine calculation testing (idempotent)
-- This script aligns with the current schema: users, books, book_inventory, lendings
-- Run after the app has created tables on startup (InitializeDatabaseAsync / InitializeLendingTablesAsync)
-- 0) Ensure admin and seed member users exist with valid hashes (SHA256(password + 'salt') as Base64)
INSERT INTO users (Username, Email, PasswordHash, Role, IsActive)
SELECT 'admin', 'admin@example.com', TO_BASE64(UNHEX(SHA2(CONCAT('admin123!','salt'), 256))), 'Administrator', 1
WHERE NOT EXISTS (SELECT 1 FROM users WHERE Username = 'admin');

INSERT INTO users (Username, Email, PasswordHash, Role, IsActive)
SELECT 'seedmember1', 'seedmember1@example.com', TO_BASE64(UNHEX(SHA2(CONCAT('Member@123!','salt'), 256))), 'Member', 1
WHERE NOT EXISTS (SELECT 1 FROM users WHERE Username = 'seedmember1');

INSERT INTO users (Username, Email, PasswordHash, Role, IsActive)
SELECT 'seedmember2', 'seedmember2@example.com', TO_BASE64(UNHEX(SHA2(CONCAT('Member@123!','salt'), 256))), 'Member', 1
WHERE NOT EXISTS (SELECT 1 FROM users WHERE Username = 'seedmember2');

INSERT INTO users (Username, Email, PasswordHash, Role, IsActive)
SELECT 'seedmember3', 'seedmember3@example.com', TO_BASE64(UNHEX(SHA2(CONCAT('Member@123!','salt'), 256))), 'Member', 1
WHERE NOT EXISTS (SELECT 1 FROM users WHERE Username = 'seedmember3');

-- Capture user IDs for references
SET @AdminId := (SELECT Id FROM users WHERE Username = 'admin' LIMIT 1);
SET @M1 := (SELECT Id FROM users WHERE Username = 'seedmember1' LIMIT 1);
SET @M2 := (SELECT Id FROM users WHERE Username = 'seedmember2' LIMIT 1);
SET @M3 := (SELECT Id FROM users WHERE Username = 'seedmember3' LIMIT 1);

-- 1) Insert demo books (books table does not have AvailableCopies; that lives in book_inventory)
INSERT INTO books (Title, Author, Isbn, Category, PublishedYear, IsActive, CreatedBy, UpdatedBy)
SELECT 'Test Book 1', 'Author A', 'SEED-ISBN-001', 'Fiction', 2010, 1, @AdminId, @AdminId
WHERE NOT EXISTS (SELECT 1 FROM books WHERE Isbn = 'SEED-ISBN-001');

INSERT INTO books (Title, Author, Isbn, Category, PublishedYear, IsActive, CreatedBy, UpdatedBy)
SELECT 'Test Book 2', 'Author B', 'SEED-ISBN-002', 'Non-Fiction', 2015, 1, @AdminId, @AdminId
WHERE NOT EXISTS (SELECT 1 FROM books WHERE Isbn = 'SEED-ISBN-002');

INSERT INTO books (Title, Author, Isbn, Category, PublishedYear, IsActive, CreatedBy, UpdatedBy)
SELECT 'Test Book 3', 'Author C', 'SEED-ISBN-003', 'Science', 2018, 1, @AdminId, @AdminId
WHERE NOT EXISTS (SELECT 1 FROM books WHERE Isbn = 'SEED-ISBN-003');

-- Capture book IDs
SET @B1 := (SELECT Id FROM books WHERE Isbn = 'SEED-ISBN-001' LIMIT 1);
SET @B2 := (SELECT Id FROM books WHERE Isbn = 'SEED-ISBN-002' LIMIT 1);
SET @B3 := (SELECT Id FROM books WHERE Isbn = 'SEED-ISBN-003' LIMIT 1);

-- 2) Ensure inventory rows exist for these books
INSERT INTO book_inventory (BookId, TotalCopies, AvailableCopies)
SELECT @B1, 3, 3 WHERE @B1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM book_inventory WHERE BookId = @B1);
INSERT INTO book_inventory (BookId, TotalCopies, AvailableCopies)
SELECT @B2, 2, 2 WHERE @B2 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM book_inventory WHERE BookId = @B2);
INSERT INTO book_inventory (BookId, TotalCopies, AvailableCopies)
SELECT @B3, 1, 1 WHERE @B3 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM book_inventory WHERE BookId = @B3);

-- 3) Insert lending records with past due dates (to trigger overdue fines after calculation)
-- Note: Using BorrowDate (not LendDate) and Status 'Active'; ReturnDate NULL
INSERT INTO lendings (BookId, UserId, BorrowDate, DueDate, ReturnDate, Status, FineAmount, FinePaid, RenewalCount, MaxRenewals)
SELECT @B1, @M1, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 20 DAY), DATE_SUB(UTC_TIMESTAMP(), INTERVAL 10 DAY), NULL, 'Active', NULL, 0, 0, 2
WHERE @B1 IS NOT NULL AND @M1 IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM lendings WHERE BookId = @B1 AND UserId = @M1 AND ReturnDate IS NULL);

INSERT INTO lendings (BookId, UserId, BorrowDate, DueDate, ReturnDate, Status, FineAmount, FinePaid, RenewalCount, MaxRenewals)
SELECT @B2, @M2, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 15 DAY), DATE_SUB(UTC_TIMESTAMP(), INTERVAL 5 DAY), NULL, 'Active', NULL, 0, 0, 2
WHERE @B2 IS NOT NULL AND @M2 IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM lendings WHERE BookId = @B2 AND UserId = @M2 AND ReturnDate IS NULL);

INSERT INTO lendings (BookId, UserId, BorrowDate, DueDate, ReturnDate, Status, FineAmount, FinePaid, RenewalCount, MaxRenewals)
SELECT @B3, @M3, DATE_SUB(UTC_TIMESTAMP(), INTERVAL 12 DAY), DATE_SUB(UTC_TIMESTAMP(), INTERVAL 2 DAY), NULL, 'Active', NULL, 0, 0, 2
WHERE @B3 IS NOT NULL AND @M3 IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM lendings WHERE BookId = @B3 AND UserId = @M3 AND ReturnDate IS NULL);

-- 4) Recompute AvailableCopies based on active (unreturned) loans
UPDATE book_inventory bi
LEFT JOIN (
  SELECT BookId, COUNT(*) AS ActiveLoans
  FROM lendings
  WHERE ReturnDate IS NULL
  GROUP BY BookId
) l ON l.BookId = bi.BookId
SET bi.AvailableCopies = GREATEST(bi.TotalCopies - IFNULL(l.ActiveLoans, 0), 0),
    bi.UpdatedAt = UTC_TIMESTAMP()
WHERE bi.BookId IN (@B1, @B2, @B3);

-- 5) Optional: You can trigger fine calculation immediately via the admin endpoint after running this seed
-- POST /api/maintenance/trigger-fine-calculation (requires Administrator auth)
-- Or wait for the FineCalculationHostedService to run based on Fines:IntervalHours
