# Fine Adjustment Feature Documentation

## Overview
Administrators can waive or adjust fines to manage exceptional cases and resolve member accounts with discretionary authority.

## API Endpoint

### PUT `/api/fines/{lendingId}/adjust`

**Authorization:** Administrator role required (`[Authorize(Policy = "RequireAdmin")]`)

**Request Body:**
```json
{
  "newAmount": 0.00,
  "reason": "Waived due to exceptional circumstances"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Fine waived successfully",
  "lendingId": 123,
  "originalAmount": 10.00,
  "newAmount": 0.00,
  "finePaid": true,
  "lending": { /* Updated lending object */ }
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "A reason is required for all fine adjustments."
}
```

## Acceptance Criteria Coverage

### ✅ Scenario 1: Successfully Adjust a Fine to a Lower Amount
- **Given:** Member has an outstanding fine of $10.00
- **When:** Administrator adjusts fine to $2.00 with reason
- **Then:** System displays "Fine updated successfully"
- **And:** Member's outstanding fine is now $2.00
- **And:** Detailed record created in audit log

**Implementation:**
- Backend: `AdjustFineAsync` method in `LendingService`
- Frontend: `AdjustFineModal` component with validation
- Database: Updates `lendings.FineAmount` and inserts `audit_log` entry

### ✅ Scenario 2: Successfully Waive a Fine Completely
- **Given:** Member has an outstanding fine
- **When:** Administrator adjusts fine to $0.00 with reason
- **Then:** System displays "Fine waived successfully"
- **And:** Member's fine is marked as "Paid" or cleared
- **And:** Detailed record created in audit log

**Implementation:**
- Backend: Sets `FinePaid = true` when `newAmount = 0`
- Frontend: Shows "Waive Fine" button and waiver confirmation
- Database: Updates both amount and paid status

### ✅ Scenario 3: Attempt to Adjust a Fine Without a Reason
- **Given:** Administrator is on fine adjustment form
- **When:** Admin enters new amount but leaves "Reason" field blank
- **Then:** System displays "A reason is required for all fine adjustments."
- **And:** Fine is not changed

**Implementation:**
- Backend: Server-side validation in `AdjustFineAsync`
- Frontend: Client-side validation with real-time feedback
- Both prevent submission without reason

### ✅ Scenario 4: Non-Admin Attempt to Adjust a Fine
- **Given:** User with "Librarian" role is logged in
- **Then:** "Adjust Fine" button should not be visible or disabled
- **And:** Direct API access returns "403 Forbidden" error

**Implementation:**
- Backend: `[Authorize(Policy = "RequireAdmin")]` on controller
- Frontend: `user.role === 'Administrator'` condition for button visibility
- Policy defined in `Program.cs`: `RequireAdmin` policy

## Technical Implementation

### Backend Components

1. **FinesController.cs**
   ```csharp
   [HttpPut("{lendingId}/adjust")]
   [Authorize(Policy = "RequireAdmin")]
   public async Task<ActionResult<AdjustFineResponse>> AdjustFine(int lendingId, AdjustFineRequest request)
   ```

2. **LendingService.cs**
   ```csharp
   public async Task<AdjustFineResponse> AdjustFineAsync(int lendingId, AdjustFineRequest request, int adminUserId)
   ```

3. **DTOs (LendingDtos.cs)**
   ```csharp
   public class AdjustFineRequest
   {
       public decimal NewAmount { get; set; }
       public string Reason { get; set; } = string.Empty;
   }
   
   public class AdjustFineResponse
   {
       public bool Success { get; set; }
       public string Message { get; set; } = string.Empty;
       // ... additional properties
   }
   ```

4. **SQL Queries**
   - `GetFineInfoForAdjustment.sql`: Retrieves current fine and user info
   - `AdjustFineAmount.sql`: Updates fine amount and paid status
   - `InsertAuditLog.sql`: Records adjustment in audit trail

### Frontend Components

1. **AdjustFineModal.jsx**
   - Form with amount input and reason textarea
   - Real-time validation and error handling
   - Loading states and success feedback

2. **FinesPayment.jsx**
   - Admin-only "Adjust Fine" button
   - Modal state management
   - Success toast notifications
   - Local state updates after API success

3. **API Integration (api.js)**
   ```javascript
   export const finesApi = {
     adjustFine: (lendingId, adjustmentData) => 
       api.put(`/fines/${lendingId}/adjust`, adjustmentData)
   };
   ```

### Database Schema

**Table: `lendings`**
- `FineAmount` (decimal): Updated with new amount
- `FinePaid` (boolean): Set to `true` when amount is 0
- `UpdatedAt` (datetime): Timestamp of adjustment

**Table: `audit_log`**
- `ActorUserId` (int): Administrator performing action
- `Action` (varchar): "AdjustFine" or "WaiveFine"
- `TargetUserId` (int): Member whose fine was adjusted
- `Details` (text): JSON with lendingId, original amount, new amount, reason

## Security Features

1. **Authorization**
   - Backend: Policy-based authorization requiring Administrator role
   - Frontend: UI elements hidden for non-admin users
   - JWT token validation on all requests

2. **Audit Trail**
   - All adjustments logged with administrator ID
   - Original and new amounts recorded
   - Reason required and stored
   - Timestamp of all changes

3. **Input Validation**
   - Amount must be >= 0 (no negative fines)
   - Reason required and non-empty
   - Lending ID must exist and be valid

## Testing Coverage

### Backend Tests
- **Unit Tests:** Controller authorization, service validation
- **Files:** `FinesControllerTests.cs`, `LendingServiceFineValidationTests.cs`
- **Coverage:** Invalid inputs, authorization policies, business logic

### Frontend Tests
- **Unit Tests:** Component behavior, API integration, role-based UI
- **Files:** `AdjustFineModal.test.jsx`, `FinesPayment.test.jsx`, `fineAdjustment.test.js`
- **Coverage:** Form validation, user interactions, error handling

## Usage Instructions

### For Administrators:
1. Navigate to "Fines & Payment Management"
2. Find member with outstanding fine
3. Click "Adjust Fine" button
4. Enter new amount (0 for full waiver)
5. Provide mandatory reason
6. Submit adjustment
7. Verify success message and updated amount

### For Developers:
```bash
# Run backend tests
dotnet test backend/Csp.Unit.Tests/

# Install frontend dependencies
cd frontend/csp-web
npm install --legacy-peer-deps

# Run frontend tests
npm test

# Start development servers
dotnet run --project backend/Csp.Api  # Backend on :5192
npm run dev                          # Frontend on :5173
```

## Error Handling

### Common Errors:
- **400 Bad Request:** Invalid amount or missing reason
- **401 Unauthorized:** Missing or invalid JWT token
- **403 Forbidden:** Non-administrator attempting access
- **404 Not Found:** Lending record doesn't exist
- **500 Internal Server Error:** Database connection or query issues

### Frontend Error Display:
- Real-time validation messages
- API error responses shown to user
- Graceful handling of network issues
- Form remains open for correction

## Future Enhancements

1. **Bulk Adjustments:** Select multiple fines for batch processing
2. **Approval Workflow:** Multi-step approval for large adjustments
3. **Adjustment Limits:** Maximum adjustment amounts per administrator
4. **Notification System:** Email alerts for fine adjustments
5. **Reporting:** Fine adjustment reports and analytics

## Related Features

- **Member Management:** View member fine history
- **Audit Trail:** Review all administrative actions
- **Payment Processing:** Handle fine payments
- **Lending System:** Book return and fine calculation