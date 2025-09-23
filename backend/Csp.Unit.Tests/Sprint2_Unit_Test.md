# Book Management Unit Testing Documentation

## Overview

This document outlines the comprehensive testing approach for the Book Management functionality in the CSP (Library Management System). The testing suite ensures reliability, maintainability, and quality of all book-related operations.


##  Testing Strategy

### Objectives
- Ensure all book management functionalities work correctly
- Validate business rules and constraints
- Test both positive and negative scenarios
- Maintain high code coverage (>80%)
- Provide fast feedback for developers
- Enable safe refactoring and feature additions

### Scope
Our testing covers the following components:
- **Controllers**: API endpoint behavior and HTTP responses
- **Services**: Business logic and data processing
- **Models**: Entity validation and constraints
- **DTOs**: Data transfer and mapping validation

##  Test Coverage

### Functional Areas Tested

#### 1. Book CRUD Operations
- ✅ Create Book (`POST /api/books`)
- ✅ Update Book (`PUT /api/books/{id}`)
- ✅ Get Book by ID (`GET /api/books/{id}`)
- ✅ Get Books with Pagination (`GET /api/books`)
- ✅ Delete Book (`DELETE /api/books/{id}`)

#### 2. Book Status Management
- ✅ Activate/Deactivate Book (`PUT /api/books/{id}/status`)
- ✅ Status-based filtering for different user roles

#### 3. Search and Filtering
- ✅ Search by title, author, ISBN
- ✅ Filter by category
- ✅ Pagination controls
- ✅ Role-based book visibility

#### 4. Inventory Management
- ✅ Total copies tracking
- ✅ Available copies calculation
- ✅ Copy count validation

#### 5. Validation Rules
- ✅ Required field validation
- ✅ ISBN format validation
- ✅ Published year constraints
- ✅ Copy count constraints
- ✅ Duplicate ISBN prevention

##  Testing Methodologies

### 1. Unit Testing
**Framework**: xUnit with Moq for mocking

**Approach**:
- Isolated testing of individual components
- Mock external dependencies (database, services)
- Fast execution with no external dependencies
- Focus on business logic validation



### 2. Positive Testing
**Objective**: Verify system behaves correctly with valid inputs

**Test Cases**:
- Valid book creation with all required fields
- Successful book updates with valid data
- Proper pagination and search functionality
- Correct role-based access control

### 3. Negative Testing
**Objective**: Ensure system handles invalid inputs gracefully

**Test Cases**:
- Missing required fields
- Invalid data formats (ISBN, dates)
- Boundary value violations
- Unauthorized access attempts
- Duplicate ISBN submissions

### 4. Boundary Testing
**Objective**: Test limits and edge cases

**Test Cases**:
- ISBN length limits (10-20 characters)
- Published year range (1000 - current year + 1)
- Minimum copy count (≥ 1)
- Pagination limits (page size, maximum results)

### 5. Error Handling Testing
**Objective**: Validate error responses and exception handling

**Test Cases**:
- Database connection failures
- Invalid user tokens
- Resource not found scenarios
- Constraint violations



### Test Naming Convention
- **Class**: `{ComponentName}Tests`
- **Method**: `{MethodName}_{Scenario}_{ExpectedResult}`

**Examples**:
- `CreateBook_ValidRequest_ReturnsCreatedResult`
- `UpdateBook_BookNotFound_ReturnsBadRequest`
- `GetBooks_MemberRole_FiltersActiveBooks`

### Test Categories

#### Controller Tests (`BooksControllerTests.cs`)
- **Focus**: HTTP request/response handling
- **Mock**: IBookService interface
- **Coverage**: All API endpoints with success/failure scenarios

#### Service Tests (`BookServiceTests.cs`)
- **Focus**: Business logic validation
- **Mock**: Database connections (for full integration, use separate test class)
- **Coverage**: CRUD operations, validation rules, business calculations

#### Model Tests (`BookModelValidationTests.cs`)
- **Focus**: Entity properties, constraints, DTOs
- **Mock**: None (pure model testing)
- **Coverage**: Default values, property assignment, validation rules


### Quick Test Run
```bash
# Run all tests with automatic report generation
dotnet test

# Run specific test class
dotnet test --filter "BooksControllerTests"

# Run with detailed console output
dotnet test --logger "console;verbosity=detailed"
```

### Automatic Report Generation

**Every time you run `dotnet test`, reports are automatically generated!**

#### Generated Reports:
1. **HTML Test Report**
   - Beautiful interactive dashboard
   - Pass/fail summary with visual indicators
   - Detailed test results grouped by class
   - Test execution times and error details
   - Direct links to coverage reports

2. **TRX Report**
   - Microsoft Test Results format
   - Compatible with Visual Studio and Azure DevOps
   - Machine-readable for CI/CD integration


