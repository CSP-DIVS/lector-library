# Backend Unit Tests for Fines and Lending Feature

## Test Coverage Summary

### 🧪 Test Projects & Files

Project: `backend/Csp.Unit.Tests`

Key test files related to fines, lending, and profiles:
- `FinesControllerTests.cs` — Admin-only fine adjustment API tests
- `LendingServiceFineValidationTests.cs` — Fine calculation/validation logic
- `UsersControllerStatusTests.cs` — Role/status changes impact
- `UsersControllerTests.cs` — General users API validations
- `BooksControllerTests.cs`, `BookServiceTests.cs` — Supporting domain stability
- `UserProfileValidationTests.cs` — Profile validation ensures consistent auth context

### ✅ Test Categories Covered

#### 1. API Contracts (ASP.NET Core)
- Admin-only endpoint authorization (`[Authorize(Policy = "RequireAdmin")]`)
- `PUT /api/fines/{lendingId}/adjust` happy path and failure modes
- Validation errors → 400 with message
- Not found or forbidden → 404/403

#### 2. Business Rules
- New amount must be >= 0
- Waiver (set to 0) marks fine as paid and logs audit
- Amount must not increase via adjustment
- Reason is required and trimmed; min length enforced

#### 3. Data Access Layer (ADO.NET)
- Parameterized SQL calls
- Transaction boundaries for update + audit
- Retry/exception mapping for MySQL transient errors

#### 4. Cross-Cutting Concerns
- JWT auth propagation and failure handling
- Audit logging on adjustments and waivers
- Idempotency checks on already-paid fines

### 🎯 Representative Scenarios

- Adjust fine from 12.50 → 5.00 with reason "Correction"
  - Expected: 200 OK, persisted change, audit entry
- Waive fine from 7.00 → 0.00 with reason "Waiver approved"
  - Expected: 200 OK, status to paid, audit entry
- Invalid: negative amount or missing reason
  - Expected: 400 Bad Request with validation details
- Forbidden: non-admin attempts adjustment
  - Expected: 403 Forbidden
- Not found: invalid lendingId
  - Expected: 404 Not Found

### 🔧 Test Harness & Utilities

- Framework: xUnit
- Test server: Minimal WebApplicationFactory-style setup (see `TestServer.cs` in integration tests) when needed
- Mocks: Repository/service fakes; SQL loader stubbed for unit scope
- Assertions: FluentAssertions where appropriate
- Configs: `appsettings.*.json` bound for in-memory/test DB

### 🚀 How to Run

From repo root or the test project folder:

```bash
# Run unit tests (Debug)
dotnet test backend/Csp.Unit.Tests/Csp.Unit.Tests.csproj -c Debug --nologo

# Run all backend tests (unit + integration + e2e if applicable)
dotnet test

# Generate minimal TRX and HTML reports (example scripts exist in TestReport folder)
powershell -File backend/Csp.Unit.Tests/MinimalTestReport.ps1
```

### 📊 Quality Gates

- Build: PASS (net8.0)
- Lint/Style: Follows project conventions; analyzers can be added via `.editorconfig`
- Tests: All unit tests passing per latest runs (see `TestReport` folder)

### 📝 Notes

- Keep DTO/validation logic mirrored between controller and service to preserve API error shapes
- Prefer parameterized SQL via ADO.NET and central SQL loader to avoid duplicates
- When changing endpoint behavior (e.g., success payload shape), update both unit and integration tests consistently
- For larger refactors, add contract tests to pin API shape before changes
