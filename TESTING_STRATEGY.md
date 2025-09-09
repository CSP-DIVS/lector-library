# Project Testing Strategy

This document outlines the testing strategy for our project, defining how developers and QA will collaborate to ensure high-quality software. The core philosophy is to structure our tests by their *type*, not by the person who writes them.

## Core Test Projects

We have two primary test projects, each with a distinct purpose:

1.  **`Csp.Api.Tests`**: For **Unit & Integration Tests**. This project tests the backend API and its internal logic.
2.  **`Csp.E2E.Tests`**: For **End-to-End Tests**. This project tests the full application from a user's perspective by automating a real web browser.

---

## 1. `Csp.Api.Tests` (Unit & Integration)

This is a shared project for both developers and QA. We will separate tests by type within this project to ensure clarity.

### A. Integration Tests (For API Controllers)

Integration tests verify that different parts of the API work together (e.g., a request hitting a Controller, which uses a Service). We primarily use these to test our API controllers.

*   **File Location:** The root of the `Csp.Api.Tests` project.
*   **File Naming:** Name the test file after the **Controller** it tests, with an `IntegrationTests` suffix.
*   **Examples:**
    *   `AuthController.cs` -> `AuthIntegrationTests.cs`
    *   `UsersController.cs` -> `UsersIntegrationTests.cs`

### B. Unit Tests (For Services, Helpers, etc.)

Unit tests verify a single class (a "unit") in complete isolation, mocking all external dependencies. They are perfect for testing the detailed business logic within services or helper classes.

*   **File Location:** Create subfolders that **mirror the main application's structure**.
*   **File Naming:** Name the test file after the **Class** it tests, with a `Tests` suffix.
*   **Examples:**
    *   Source: `Csp.Api/Services/UserService.cs` -> Test: `Csp.Api.Tests/Services/UserServiceTests.cs`
    *   Source: `Csp.Api/Services/JwtTokenService.cs` -> Test: `Csp.Api.Tests/Services/JwtTokenServiceTests.cs`

### Test Method Naming Convention

For both test types, use the `MethodName_Scenario_ExpectedResult` pattern for clear, descriptive test names.

```csharp
// Example in UsersIntegrationTests.cs
[Fact]
public async Task ChangeMyPassword_WithIncorrectOldPassword_ReturnsBadRequest() { /* ... */ }

// Example in UserServiceTests.cs
[Fact]
public void HashPassword_WithValidInput_ReturnsHashedString() { /* ... */ }
```

### Roles & Responsibilities

*   **Developer:** Writes initial "happy path" integration tests for new controllers and unit tests for new services to prove the basic functionality works.
*   **QA:** Writes the comprehensive suite of integration and unit tests covering all business rules, validation, permissions, and edge cases to ensure the feature is robust and secure.

---

## 2. `Csp.E2E.Tests` (End-to-End)

This project is primarily owned by QA to validate complete user stories and workflows.

### File Organization

*   **Rule:** Create one test file for each major feature, user story, or page.
*   **Examples:**
    *   `Story01_LoginTests.cs`
    *   `Story02_MemberManagementTests.cs`
    *   `Story05_BookManagementTests.cs`

### Test Method Naming Convention

Use a user-centric `Action_Scenario_ExpectedResult` pattern.

```csharp
// Example in MemberManagementTests.cs

[Fact]
public void Search_ForExistingUser_ShouldDisplayUserInList() { /* ... */ }
```

---

## Action Plan

1.  **Align:** All team members should read and agree on this strategy.
2.  **Organize `Csp.Api.Tests`:**
    *   Organize integration tests for controllers in the root.
    *   Create subfolders for unit tests (e.g., `/Services`) to mirror the application structure.
    *   Delete placeholder files like `UnitTest1.cs`.
3.  **Organize `Csp.E2E.Tests`:**
    *   Continue using the `StoryXX_FeatureNameTests.cs` naming convention for all new E2E test files.
