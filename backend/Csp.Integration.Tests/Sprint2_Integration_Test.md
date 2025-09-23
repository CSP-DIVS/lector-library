# Book Management Integration Tests

## Overview

This document outlines the comprehensive integration testing approach for the Book Management functionality in the CSP Library Management System. The tests are designed to validate the complete book management workflow from HTTP requests through the service layer to the database.

## Test Coverage

### Functionalities Tested

1. **Book Creation** (`POST /api/Books`)
   - Create books with valid data
   - Validate required fields (Title, Author, ISBN, Category)
   - ISBN format validation (10-20 characters)
   - Published year validation (1000 to current year + 1)
   - Total copies validation (minimum 1)
   - Duplicate ISBN prevention
   - Authorization requirements (Librarian role only)

2. **Book Retrieval** (`GET /api/Books`)
   - Retrieve paginated book lists
   - Search functionality by title/author/category
   - Filter by category and author
   - Public endpoint accessibility
   - Pagination parameter handling

3. **Single Book Retrieval** (`GET /api/Books/{id}`)
   - Retrieve specific book by ID
   - Handle non-existent book requests
   - Public endpoint accessibility

4. **Book Updates** (`PUT /api/Books/{id}`)
   - Update book information with valid data
   - Validate updated fields
   - Handle non-existent book updates
   - Authorization requirements (Librarian role only)

5. **Book Status Updates** (`PUT /api/Books/{id}/status`)
   - Activate/deactivate books
   - Authorization requirements (Librarian role only)
   - Status change validation

6. **Book Deletion** (`DELETE /api/Books/{id}`)
   - Soft delete books
   - Handle non-existent book deletions
   - Authorization requirements (Librarian role only)
   - Verify deletion through subsequent retrieval

## Test Strategy

### Test Categories

#### 1. Positive Test Scenarios
Tests that verify expected functionality with valid inputs:

- `CreateBook_ValidData_ReturnsCreated`
- `GetBooks_NoFilters_ReturnsPagedResults`
- `GetBooks_WithSearch_ReturnsFilteredResults`
- `GetBookById_ExistingBook_ReturnsBook`
- `UpdateBook_ValidData_ReturnsUpdated`
- `UpdateBookStatus_ValidData_ReturnsUpdated`
- `DeleteBook_ExistingBook_ReturnsNoContent`

#### 2. Negative Test Scenarios
Tests that verify error handling and validation:

- `CreateBook_EmptyTitle_ReturnsBadRequest`
- `CreateBook_InvalidIsbn_ReturnsBadRequest`
- `CreateBook_InvalidPublishedYear_ReturnsBadRequest`
- `CreateBook_DuplicateIsbn_ReturnsBadRequest`
- `GetBookById_NonExistentBook_ReturnsNotFound`
- `UpdateBook_NonExistentBook_ReturnsBadRequest`
- `DeleteBook_NonExistentBook_ReturnsNotFound`
- `GetBooks_InvalidPagination_HandlesGracefully`

#### 3. Security Test Scenarios
Tests that verify authorization and authentication:

- `CreateBook_WithoutAuth_ReturnsUnauthorized`
- `CreateBook_MemberRole_ReturnsForbidden`
- `UpdateBook_WithoutAuth_ReturnsUnauthorized`
- `DeleteBook_WithoutAuth_ReturnsUnauthorized`

## Validation Testing

### Input Validation Tests

1. **Required Fields**: Title, Author, ISBN, Category
2. **ISBN Format**: 10-20 character length validation
3. **Published Year**: Range validation (1000 to current year + 1)
4. **Total Copies**: Minimum value validation (≥ 1)
5. **Duplicate Prevention**: ISBN uniqueness validation

### Business Rule Testing

1. **Authorization**: Role-based access control
2. **Book Status**: Active/inactive state management
3. **Search Functionality**: Text-based search across multiple fields
4. **Pagination**: Proper handling of page and pageSize parameters

##  Test Reporting

### Automated Reports

The test suite generates comprehensive reports automatically:

1. **Test Results Report** (`TestSummary.html`)
   - Pass/fail statistics
   - Test execution duration
   - Coverage areas overview
   - Direct links to detailed reports


2. **Detailed Test Results** (`.trx` format)
   - Individual test outcomes
   - Error messages and stack traces
   - Test timing information
