# E2E Test Suite - Book Management System

End-to-End tests for the Book Management functionality using Selenium WebDriver.

## Quick Start

```bash
# Run all tests
dotnet test

# Run with visible Chrome browser
set CSP_E2E_HEADLESS=false
dotnet test

# Run in headless mode (faster)
set CSP_E2E_HEADLESS=true
dotnet test
```

## Test Coverage

### Book Management Tests (8 tests)
- **RoleBasedAccess_LibrarianHasFullAccess** - Librarian permissions validation
- **BookSearch_FindsExistingBooks** - Search functionality testing
- **BookValidation_RequiredFieldsAndFormats** - Form validation testing
- **BookStatusManagement_ActivateDeactivate** - Book status management
- **AdvancedSearch_CategoryAndAuthorFiltering** - Advanced search features
- **DuplicateISBN_PreventionValidation** - ISBN uniqueness validation
- **BookCopiesManagement_InventoryTracking** - Inventory management

### User Management Tests (4 tests)
- **Admin_Login_And_ManageUsers** - Admin functionality
- **Librarian_Login_And_Access** - Librarian authentication
- **Member_Login_And_ProfileManagement** - Member capabilities
- **Invalid_Login_Shows_Error** - Error handling
- **User_Deactivate_And_Login_Block** - User lifecycle management

## Features Tested
- ✅ CRUD operations (Create, Read, Update, Delete books)
- ✅ Modal form interactions
- ✅ Search and filtering
- ✅ Role-based access control
- ✅ Form validation and error handling
- ✅ Authentication and authorization

## Test Results
- **Total Tests**: 12
- **Pass Rate**: 100%
- **Execution Time**: ~82 seconds
- **Auto HTML Reports**: Generated in TestResults folder


