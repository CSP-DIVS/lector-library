# 🧪 Quality Assurance (QA) Documentation

Welcome to the QA documentation for the Lector Library project. This directory contains all QA-related documentation, test data, scripts, and reports.

## 📁 Directory Structure

```
qa/
├── documentation/     # Test plans, scenarios, and checklists
├── test-data/        # SQL scripts and test data sets
├── mock-apis/        # Mock API servers for testing
├── config/           # QA configuration files
├── scripts/          # Setup and automation scripts
├── reports/          # Test execution reports
└── README.md         # This file
```

## 🚀 Quick Start

### 1. Setup Test Environment
```bash
# Linux/Mac
./qa/scripts/setup-test-environment.sh

# Windows
qa\scripts\setup-test-environment.bat
```

### 2. Start Mock API (for early testing)
```bash
cd qa/mock-apis
npm install
node Sprint3_MockAPI.js
```

### 3. Run Tests
```bash
# Unit tests
dotnet test backend/Csp.Api.Tests

# E2E tests
dotnet test backend/Csp.E2E.Tests
```

## 📋 Current Sprint Status

**Sprint 3: Book Lending & Reservation Management**
- ⏳ Status: Waiting for developer feature branch
- 📊 Progress: QA preparation complete
- 🧪 Tests Ready: Mock API, test data, scenarios

## 📚 Documentation

### Test Plans
- [Sprint 3 Test Plan](documentation/Sprint3_TestPlan.md)
- [Sprint 3 Test Scenarios](documentation/Sprint3_TestScenarios.md)
- [Sprint 3 Testing Checklist](documentation/Sprint3_TestingChecklist.md)

### Test Data
- [Sprint 3 Test Data Setup](test-data/Sprint3_TestData.sql)

### Configuration
- [QA Configuration](config/qa-config.json)

## 🛠️ Tools & Technologies

- **Unit Testing**: xUnit, .NET 8
- **Integration Testing**: ASP.NET Core TestHost
- **E2E Testing**: Selenium WebDriver
- **Mock APIs**: Express.js
- **Database**: MySQL
- **CI/CD**: GitHub Actions

## 📝 Info
 
**Project**: SE3022 Case Study - Lector Library  
**Sprint**: 3 (Book Lending & Reservation Management)

---
*Last Updated: October 6, 2025*