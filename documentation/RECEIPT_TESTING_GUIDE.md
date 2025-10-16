# Testing Receipt Generation - Setup Guide

## Overview
This guide will help you create test payment data so you can test the PDF receipt generation feature.

## Prerequisites
1. Backend running (`dotnet run` in `backend/Csp.Api`)
2. MySQL database initialized with tables
3. MySQL client or tool to run SQL scripts (e.g., MySQL Workbench, DBeaver, or mysql CLI)

## Steps to Create Test Data

### Step 1: Run the Overdue Fines Seed Script
This creates members, books, and overdue lending records.

**File**: `backend/Csp.Api/Data/Tests/seed-overdue-fines-demo.sql`

**What it does**:
- Creates 3 test members: `seedmember1`, `seedmember2`, `seedmember3` (password: `Member@123!`)
- Creates 3 test books
- Creates active lending records with past due dates
- Sets up overdue fines (after fine calculation runs)

**How to run**:
```bash
# Option 1: Using mysql CLI
mysql -u root -p lector-library < "backend/Csp.Api/Data/Tests/seed-overdue-fines-demo.sql"

# Option 2: Using MySQL Workbench or DBeaver
# - Open the SQL file
# - Select your database connection
# - Execute the script
```

### Step 2: Trigger Fine Calculation (Optional)
If fines aren't calculated yet, you can trigger the calculation manually.

**Method 1: Wait for background service** (default every 24 hours)
- The `FineCalculationHostedService` runs automatically
- Configure interval in `appsettings.json`: `"Fines": { "IntervalHours": 1 }`

**Method 2: Trigger via API endpoint**
```bash
# Login as admin first to get token
curl -X POST http://localhost:5192/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123!"}'

# Use the token from response
curl -X POST http://localhost:5192/api/maintenance/trigger-fine-calculation \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Method 3: Restart the backend**
- The fine calculation runs on startup

### Step 3: Run the Payment History Seed Script
This creates payment records for testing receipt generation.

**File**: `backend/Csp.Api/Data/Tests/seed-payment-history.sql`

**What it does**:
- Updates lending records to set fine amounts
- Creates 4 payment records:
  - Payment 1: Full payment ($2.50) by seedmember1 - recorded by librarian
  - Payment 2: Partial payment ($0.50) by seedmember2 - recorded by admin
  - Payment 3: Partial payment ($0.75) by seedmember2 - recorded by librarian
  - Payment 4: Full payment ($0.50) by seedmember3 - recorded by librarian
- Marks lendings as paid where appropriate
- Shows verification query results

**How to run**:
```bash
# Option 1: Using mysql CLI
mysql -u root -p lector-library < "backend/Csp.Api/Data/Tests/seed-payment-history.sql"

# Option 2: Using MySQL Workbench or DBeaver
# - Open the SQL file
# - Select your database connection
# - Execute the script
```

### Step 4: Verify Payment Records
After running the script, you should see output like:
```
+------------+------------------+------------------+--------+----------------+---------------------+-------------+------------+-------------+
| PaymentId  | TransactionId    | MemberUsername   | Amount | PaymentMethod  | PaymentDate         | RecordedBy  | LendingId  | BookTitle   |
+------------+------------------+------------------+--------+----------------+---------------------+-------------+------------+-------------+
| 4          | TXN-123456       | seedmember3      | 0.50   | Cash           | 2025-10-16 10:30:00 | librarian   | 3          | Test Book 3 |
| 3          | TXN-789012       | seedmember2      | 0.75   | Debit Card     | 2025-10-15 10:30:00 | librarian   | 2          | Test Book 2 |
| 2          | TXN-456789       | seedmember2      | 0.50   | Credit Card    | 2025-10-14 10:30:00 | admin       | 2          | Test Book 2 |
| 1          | TXN-234567       | seedmember1      | 2.50   | Cash           | 2025-10-13 10:30:00 | librarian   | 1          | Test Book 1 |
+------------+------------------+------------------+--------+----------------+---------------------+-------------+------------+-------------+

+------------------+-------+-------------+
| Metric           | Count | TotalAmount |
+------------------+-------+-------------+
| Total Payments   | 4     | 4.25        |
+------------------+-------+-------------+
```

## Testing Receipt Generation

### Test 1: Download Receipt as Librarian
1. Navigate to `http://localhost:5192`
2. Login as librarian:
   - Username: `librarian`
   - Password: `lib123!`
3. Go to "Fines & Payments" → "Payment History" tab
4. Click "Download Receipt" on any payment
5. **Expected**: PDF file downloads with receipt details

### Test 2: Download Receipt as Admin
1. Login as admin:
   - Username: `admin`
   - Password: `admin123!`
2. Go to "Fines & Payments" → "Payment History" tab
3. Click "Download Receipt" on any payment
4. **Expected**: PDF file downloads with receipt details

### Test 3: Verify Receipt Contents
Open the downloaded PDF and verify it contains:
- ✅ Receipt header with library info
- ✅ Transaction ID (e.g., TXN-123456)
- ✅ Payment date
- ✅ Member information (name, email)
- ✅ Book information (title, author)
- ✅ Fine details (days overdue, borrow date, due date)
- ✅ Payment amount
- ✅ Payment method
- ✅ Recorded by (staff name)
- ✅ Footer with thank you message

### Test 4: Test Receipt API Directly
```bash
# Get payment ID from database or frontend
# Login to get token
TOKEN=$(curl -s -X POST http://localhost:5192/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"librarian","password":"lib123!"}' \
  | jq -r '.token // .Token')

# Download receipt
curl -X GET "http://localhost:5192/api/payments/1/receipt" \
  -H "Authorization: Bearer $TOKEN" \
  --output receipt-1.pdf

# Verify PDF file
file receipt-1.pdf
# Should output: receipt-1.pdf: PDF document, version 1.4
```

## Troubleshooting

### Issue: No payment records created
**Solution**: 
- Verify lendings exist: `SELECT * FROM lendings WHERE ReturnDate IS NULL;`
- Check user IDs: `SELECT * FROM users WHERE Username IN ('admin', 'librarian', 'seedmember1', 'seedmember2', 'seedmember3');`
- Re-run `seed-overdue-fines-demo.sql` first

### Issue: Receipt download returns 404
**Solution**:
- Verify payment ID exists: `SELECT * FROM payments;`
- Check the payment ID in the URL matches an existing payment

### Issue: Receipt download returns 401
**Solution**:
- Ensure you're logged in as Librarian or Administrator
- Members cannot download receipts (endpoint requires `RequireLibrarian` policy)
- Check JWT token is valid and not expired

### Issue: Receipt PDF is blank or corrupted
**Solution**:
- Check backend logs for QuestPDF errors
- Verify payment data is complete: 
  ```sql
  SELECT p.*, l.*, b.*, u.*, recorder.*
  FROM payments p
  JOIN lendings l ON p.LendingId = l.Id
  JOIN books b ON l.BookId = b.Id
  JOIN users u ON p.MemberId = u.Id
  JOIN users recorder ON p.RecordedBy = recorder.Id
  WHERE p.Id = 1;
  ```

### Issue: Payment history empty in frontend
**Solution**:
- Check browser console for errors
- Verify API endpoint: `GET /api/payments/history` (for librarian/admin)
- Verify API endpoint: `GET /api/payments/member/{memberId}` (for members)
- Check network tab for 401/403 errors

## Database Connection Info

If you need to connect directly to the database:
```bash
# Default local development connection
mysql -h localhost -P 3306 -u root -p lector-library

# Or use your configured connection string from appsettings.Development.json
```

## Quick Reference: Test Users

| Username     | Password      | Role          | Can Download Receipts |
|-------------|---------------|---------------|-----------------------|
| admin       | admin123!     | Administrator | ✅ Yes                |
| librarian   | lib123!       | Librarian     | ✅ Yes                |
| member      | member123!    | Member        | ❌ No                 |
| seedmember1 | Member@123!   | Member        | ❌ No                 |
| seedmember2 | Member@123!   | Member        | ❌ No                 |
| seedmember3 | Member@123!   | Member        | ❌ No                 |

## Next Steps After Testing

1. ✅ Verify receipt PDF formatting
2. ✅ Test with different payment methods (Cash, Credit Card, Debit Card)
3. ✅ Test with partial payments (multiple payments for one lending)
4. ⏭️ Optional: Add custom receipt templates
5. ⏭️ Optional: Email receipts to members automatically

---

**Created**: 2025-10-16  
**Purpose**: Receipt generation feature testing  
**Status**: Ready for testing
