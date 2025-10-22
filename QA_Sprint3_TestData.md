# 🗄️ Sprint 3 Test Data Setup

## 📚 **Test Books**

### **Available Books (Multiple Copies)**
```sql
INSERT INTO books (Title, Author, Isbn, Category, PublishedYear, IsActive, CreatedBy, UpdatedBy) VALUES
('The Great Gatsby', 'F. Scott Fitzgerald', '9780743273565', 'Fiction', 1925, 1, 1, 1),
('To Kill a Mockingbird', 'Harper Lee', '9780061120084', 'Fiction', 1960, 1, 1, 1),
('Pride and Prejudice', 'Jane Austen', '9780141439518', 'Romance', 1813, 1, 1, 1);

INSERT INTO book_inventory (BookId, TotalCopies, AvailableCopies) VALUES
(1, 3, 3),  -- The Great Gatsby: 3 available
(2, 2, 2),  -- To Kill a Mockingbird: 2 available  
(3, 4, 4);  -- Pride and Prejudice: 4 available
```

### **Books with Limited Copies**
```sql
INSERT INTO books (Title, Author, Isbn, Category, PublishedYear, IsActive, CreatedBy, UpdatedBy) VALUES
('1984', 'George Orwell', '9780451524935', 'Dystopian', 1949, 1, 1, 1),
('Brave New World', 'Aldous Huxley', '9780060850524', 'Science Fiction', 1932, 1, 1, 1);

INSERT INTO book_inventory (BookId, TotalCopies, AvailableCopies) VALUES
(4, 1, 0),  -- 1984: 1 total, 0 available (on loan)
(5, 2, 1);  -- Brave New World: 2 total, 1 available
```

---

## 👥 **Test Users**

### **Members**
```sql
INSERT INTO users (Username, Email, PasswordHash, Role, IsActive) VALUES
('john_doe', 'john@example.com', '$hashedPassword1', 'Member', 1),
('jane_smith', 'jane@example.com', '$hashedPassword2', 'Member', 1),
('bob_wilson', 'bob@example.com', '$hashedPassword3', 'Member', 1),
('alice_brown', 'alice@example.com', '$hashedPassword4', 'Member', 0),  -- Inactive
('charlie_davis', 'charlie@example.com', '$hashedPassword5', 'Member', 1);
```

### **Staff**
```sql
INSERT INTO users (Username, Email, PasswordHash, Role, IsActive) VALUES
('librarian_1', 'lib1@library.com', '$hashedPassword6', 'Librarian', 1),
('admin_user', 'admin@library.com', '$hashedPassword7', 'Administrator', 1);
```

---

## 📋 **Test Loans**

### **Active Loans**
```sql
-- Assuming we have a loans table structure like:
-- CREATE TABLE loans (
--     Id INT AUTO_INCREMENT PRIMARY KEY,
--     BookId INT NOT NULL,
--     MemberId INT NOT NULL,
--     IssueDate DATE NOT NULL,
--     DueDate DATE NOT NULL,
--     ReturnDate DATE NULL,
--     IsReturned BOOLEAN DEFAULT FALSE,
--     FineAmount DECIMAL(10,2) DEFAULT 0.00,
--     CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
--     FOREIGN KEY (BookId) REFERENCES books(Id),
--     FOREIGN KEY (MemberId) REFERENCES users(Id)
-- );

INSERT INTO loans (BookId, MemberId, IssueDate, DueDate, IsReturned) VALUES
-- Current loans (not overdue)
(4, 2, DATE_SUB(CURDATE(), INTERVAL 5 DAY), DATE_ADD(CURDATE(), INTERVAL 9 DAY), FALSE),  -- 1984 to john_doe
(5, 3, DATE_SUB(CURDATE(), INTERVAL 3 DAY), DATE_ADD(CURDATE(), INTERVAL 11 DAY), FALSE), -- Brave New World to jane_smith

-- Overdue loans
(1, 4, DATE_SUB(CURDATE(), INTERVAL 20 DAY), DATE_SUB(CURDATE(), INTERVAL 6 DAY), FALSE), -- Great Gatsby to bob_wilson (6 days overdue)
(2, 5, DATE_SUB(CURDATE(), INTERVAL 18 DAY), DATE_SUB(CURDATE(), INTERVAL 4 DAY), FALSE); -- To Kill a Mockingbird to charlie_davis (4 days overdue)
```

### **Returned Loans (History)**
```sql
INSERT INTO loans (BookId, MemberId, IssueDate, DueDate, ReturnDate, IsReturned, FineAmount) VALUES
-- On-time returns
(1, 2, DATE_SUB(CURDATE(), INTERVAL 30 DAY), DATE_SUB(CURDATE(), INTERVAL 16 DAY), DATE_SUB(CURDATE(), INTERVAL 16 DAY), TRUE, 0.00),
(3, 3, DATE_SUB(CURDATE(), INTERVAL 25 DAY), DATE_SUB(CURDATE(), INTERVAL 11 DAY), DATE_SUB(CURDATE(), INTERVAL 12 DAY), TRUE, 0.00),

-- Late returns with fines
(2, 4, DATE_SUB(CURDATE(), INTERVAL 35 DAY), DATE_SUB(CURDATE(), INTERVAL 21 DAY), DATE_SUB(CURDATE(), INTERVAL 18 DAY), TRUE, 1.50); -- 3 days late
```

---

## 🔖 **Test Reservations**

### **Active Reservations**
```sql
-- Assuming we have a reservations table structure like:
-- CREATE TABLE reservations (
--     Id INT AUTO_INCREMENT PRIMARY KEY,
--     BookId INT NOT NULL,
--     MemberId INT NOT NULL,
--     ReservationDate DATE NOT NULL,
--     Status ENUM('Active', 'Ready', 'Fulfilled', 'Cancelled', 'Expired') DEFAULT 'Active',
--     QueuePosition INT NOT NULL,
--     ExpiryDate DATE NULL,
--     CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
--     FOREIGN KEY (BookId) REFERENCES books(Id),
--     FOREIGN KEY (MemberId) REFERENCES users(Id)
-- );

INSERT INTO reservations (BookId, MemberId, ReservationDate, Status, QueuePosition) VALUES
-- Queue for "1984" (currently on loan)
(4, 3, DATE_SUB(CURDATE(), INTERVAL 2 DAY), 'Active', 1),  -- jane_smith - position 1
(4, 4, DATE_SUB(CURDATE(), INTERVAL 1 DAY), 'Active', 2),  -- bob_wilson - position 2
(4, 5, CURDATE(), 'Active', 3),                            -- charlie_davis - position 3

-- Queue for "Brave New World" (1 copy available, 1 on loan)
(5, 2, CURDATE(), 'Active', 1);                            -- john_doe - position 1
```

---

## 💰 **Test Fines**

### **Outstanding Fines**
```sql
-- Assuming we have a fines table structure like:
-- CREATE TABLE fines (
--     Id INT AUTO_INCREMENT PRIMARY KEY,
--     LoanId INT NOT NULL,
--     MemberId INT NOT NULL,
--     Amount DECIMAL(10,2) NOT NULL,
--     Description TEXT,
--     IsPaid BOOLEAN DEFAULT FALSE,
--     PaidDate DATE NULL,
--     CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
--     FOREIGN KEY (LoanId) REFERENCES loans(Id),
--     FOREIGN KEY (MemberId) REFERENCES users(Id)
-- );

INSERT INTO fines (LoanId, MemberId, Amount, Description, IsPaid) VALUES
-- Current overdue fines (calculated from active overdue loans)
(3, 4, 3.00, 'Late return fee: 6 days overdue', FALSE),    -- bob_wilson owes $3.00
(4, 5, 2.00, 'Late return fee: 4 days overdue', FALSE),    -- charlie_davis owes $2.00

-- Historical paid fines
(7, 4, 1.50, 'Late return fee: 3 days overdue', TRUE);     -- bob_wilson paid $1.50 previously
```

---

## 🔧 **System Configuration**

### **Lending Rules**
```json
{
  "maxLoansPerMember": 5,
  "maxReservationsPerMember": 3,
  "loanPeriodDays": 14,
  "reservationHoldDays": 2,
  "dailyFineRate": 0.50,
  "maxFineAmount": 10.00,
  "graceePeriodDays": 1
}
```

### **User Roles & Permissions**
```json
{
  "Member": {
    "canBorrow": true,
    "canReserve": true,
    "canViewOwnLoans": true,
    "canViewOwnReservations": true,
    "canPayFines": true
  },
  "Librarian": {
    "canIssueBooks": true,
    "canReturnBooks": true,
    "canViewAllLoans": true,
    "canViewAllReservations": true,
    "canManageFines": true,
    "canOverrideDueDates": true
  },
  "Administrator": {
    "inherits": "Librarian",
    "canManageUsers": true,
    "canConfigureSystem": true,
    "canViewReports": true
  }
}
```

---

## 🧪 **Test Environment Setup**

### **Database Reset Script**
```sql
-- Clear existing test data
DELETE FROM fines;
DELETE FROM reservations;
DELETE FROM loans;
DELETE FROM book_inventory;
DELETE FROM books WHERE Id > 100; -- Keep seeded data
DELETE FROM users WHERE Id > 10;  -- Keep seeded users

-- Reset auto-increment
ALTER TABLE loans AUTO_INCREMENT = 1;
ALTER TABLE reservations AUTO_INCREMENT = 1;
ALTER TABLE fines AUTO_INCREMENT = 1;

-- Re-insert test data
-- (Run all the INSERT statements above)
```

### **API Test Configuration**
```json
{
  "testEnvironment": {
    "baseUrl": "http://localhost:5192",
    "testDatabase": "lector_library_test",
    "authTokens": {
      "member": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9...",
      "librarian": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9...",
      "admin": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9..."
    }
  },
  "testUsers": {
    "activeMember": {
      "username": "john_doe",
      "password": "password123",
      "role": "Member"
    },
    "memberWithLoans": {
      "username": "jane_smith", 
      "password": "password123",
      "role": "Member"
    },
    "librarian": {
      "username": "librarian_1",
      "password": "lib123",
      "role": "Librarian"
    }
  }
}
```

---

## 🔄 **Data Refresh Strategy**

### **Before Each Test Suite**
1. Reset database to known state
2. Insert fresh test data
3. Verify data integrity
4. Generate test authentication tokens

### **After Each Test**
1. Clean up test-specific data
2. Reset counters and sequences
3. Clear cache if applicable

### **Test Data Validation**
```sql
-- Verify test data setup
SELECT 'Books' as Table_Name, COUNT(*) as Count FROM books
UNION ALL
SELECT 'Users', COUNT(*) FROM users  
UNION ALL
SELECT 'Loans', COUNT(*) FROM loans
UNION ALL
SELECT 'Reservations', COUNT(*) FROM reservations
UNION ALL
SELECT 'Fines', COUNT(*) FROM fines;

-- Check business rule compliance
SELECT 
  u.Username,
  COUNT(l.Id) as ActiveLoans,
  COUNT(r.Id) as ActiveReservations,
  SUM(f.Amount) as OutstandingFines
FROM users u
LEFT JOIN loans l ON u.Id = l.MemberId AND l.IsReturned = FALSE
LEFT JOIN reservations r ON u.Id = r.MemberId AND r.Status = 'Active'
LEFT JOIN fines f ON u.Id = f.MemberId AND f.IsPaid = FALSE
WHERE u.Role = 'Member'
GROUP BY u.Id, u.Username;
```