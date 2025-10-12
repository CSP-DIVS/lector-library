# Manual Testing Setup Guide
## Fine Adjustment Feature in User Management

This guide helps you set up the database with test data to manually test the fine adjustment feature in the MemberManagement component.

## Prerequisites

1. MySQL database running and accessible
2. Backend API (`Csp.Api`) configured with correct connection string
3. Frontend development server running
4. Admin/Librarian user account for testing

## Step 1: Database Setup

### 1.1 Apply the Test Data

Run the SQL script `test_data_for_fines.sql` in your MySQL database:

```bash
mysql -u your_username -p your_database < test_data_for_fines.sql
```

Or execute it through your preferred MySQL client (MySQL Workbench, phpMyAdmin, etc.).

### 1.2 Verify Data Installation

Run these verification queries to confirm the test data was installed:

```sql
-- Check users with fines
SELECT 
    u.Id, u.Username, u.Email, u.Role,
    COUNT(l.Id) as TotalLoans,
    SUM(CASE WHEN l.FineAmount > 0 AND l.FinePaid = 0 THEN l.FineAmount ELSE 0 END) as TotalUnpaidFines
FROM users u
LEFT JOIN lendings l ON u.Id = l.UserId
WHERE u.Role = 'Member'
GROUP BY u.Id, u.Username, u.Email, u.Role
ORDER BY TotalUnpaidFines DESC;

-- Check detailed fine information
SELECT 
    l.Id as LendingId, u.Username, b.Title, l.FineAmount, l.FinePaid, l.Status
FROM lendings l
JOIN users u ON l.UserId = u.Id
JOIN books b ON l.BookId = b.Id
WHERE l.FineAmount > 0
ORDER BY u.Username;
```

## Step 2: Test User Accounts

The test data includes these users for testing:

| Username | Email | Role | Password | Has Fines |
|----------|-------|------|----------|-----------|
| `admin_user` | admin@library.com | Administrator | password123 | No |
| `librarian_user` | librarian@library.com | Librarian | password123 | No |
| `john_doe` | john.doe@email.com | Member | password123 | **Yes** (multiple fines) |
| `jane_smith` | jane.smith@email.com | Member | password123 | **Yes** (one large fine) |
| `mike_wilson` | mike.wilson@email.com | Member | password123 | **Yes** (mixed paid/unpaid) |
| `sarah_davis` | sarah.davis@email.com | Member | password123 | **Yes** (one unpaid fine) |

*Note: Password hashes are for 'password123' - adjust if your system uses different hashing.*

## Step 3: Testing Scenarios

### 3.1 Access User Management

1. **Login as admin or librarian**
   - Use `admin_user` or `librarian_user` credentials
   - Navigate to User Management page

2. **Verify role-based access**
   - Admin/Librarian users should see "View Fines" buttons
   - Regular members should NOT see fine-related controls

### 3.2 Test Fine Viewing

1. **Find users with fines**
   - Look for `john_doe`, `jane_smith`, `mike_wilson`, `sarah_davis`
   - Click their "View Fines" buttons

2. **Verify fine data display**
   - Should show real book titles and authors
   - Fine amounts should match database values
   - Status should be "Outstanding" for unpaid fines

### 3.3 Test Fine Adjustment

1. **Select a fine to adjust**
   - Click "Adjust Fine" button on any outstanding fine
   - Modal should open with fine details

2. **Test adjustment scenarios**
   - **Reduce fine**: Enter lower amount (e.g., $5.00 → $2.50)
   - **Waive fine**: Enter $0.00
   - **Add reason**: Provide explanation for adjustment

3. **Verify adjustment results**
   - Fine amount should update in the UI
   - Success message should appear
   - Database should reflect the change

### 3.4 Test Error Handling

1. **Invalid inputs**
   - Try negative amounts
   - Submit without reason
   - Test very large amounts

2. **Network errors**
   - Stop backend server mid-adjustment
   - Check error messages display properly

## Step 4: Database Verification

After making adjustments, verify changes in the database:

```sql
-- Check adjustment audit trail (if audit logging is implemented)
SELECT * FROM audit_log 
WHERE TableName = 'lendings' AND Action LIKE '%FINE%'
ORDER BY Timestamp DESC;

-- Verify fine amounts were updated
SELECT 
    l.Id as LendingId,
    u.Username,
    b.Title,
    l.FineAmount,
    l.FinePaid,
    l.UpdatedAt
FROM lendings l
JOIN users u ON l.UserId = u.Id  
JOIN books b ON l.BookId = b.Id
WHERE l.FineAmount > 0
ORDER BY l.UpdatedAt DESC;
```

## Step 5: Test Data Reset

To reset test data for fresh testing:

```sql
-- Remove test lendings
DELETE FROM lendings WHERE UserId IN (
    SELECT Id FROM users WHERE Username IN ('john_doe', 'jane_smith', 'mike_wilson', 'sarah_davis')
);

-- Remove test users (except admin)
DELETE FROM users WHERE Username IN ('john_doe', 'jane_smith', 'mike_wilson', 'sarah_davis');

-- Remove test books
DELETE FROM book_inventory WHERE BookId IN (1,2,3,4,5,6,7,8,9,10);
DELETE FROM books WHERE Id IN (1,2,3,4,5,6,7,8,9,10);
```

Then re-run the test data script.

## Expected API Endpoints

The manual testing relies on these API endpoints:

- `GET /api/fines/user/{userId}` - Get user's fines
- `PUT /api/fines/{lendingId}/adjust` - Adjust fine amount
- `GET /api/users` - Get users list (for member management)

## Troubleshooting

### Backend Issues
- **500 errors**: Check database connection and table structure
- **404 errors**: Verify API routes are registered
- **401 errors**: Ensure proper admin authentication

### Frontend Issues  
- **No fines showing**: Check browser network tab for API calls
- **Mock data still showing**: Clear browser cache and reload
- **Buttons not visible**: Verify user role and authentication

### Database Issues
- **Foreign key errors**: Ensure users and books tables exist first
- **Permission errors**: Verify MySQL user has INSERT/UPDATE privileges
- **Connection errors**: Check connection string configuration

## Success Criteria

Manual testing is successful when:

1. ✅ Admin/Librarian can view all user fines
2. ✅ Fine adjustment modal opens with correct data  
3. ✅ Fine amounts can be reduced or waived
4. ✅ Adjustments persist in database
5. ✅ UI updates reflect changes immediately
6. ✅ Error handling works for invalid inputs
7. ✅ Regular members cannot access fine controls