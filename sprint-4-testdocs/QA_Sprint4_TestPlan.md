# 📋 Sprint 4 Test Plan: Fine & Payment Management

## 🎯 **Test Scope**
**Sprint**: 4  
**Feature**: Fine & Payment Management  
**QA Lead**: Subodha Munasinghe  
**Start Date**: October 23, 2025  

---

## 📖 **User Stories to Test**

### **Story 1: Fine Generation and Member Fines**
**Priority**: High  
**Complexity**: High  
- System auto-generates fines for overdue loans
- Members view their outstanding/paid/waived fines and summary

### **Story 2: Fine Payment by Members and Admins**
**Priority**: High  
**Complexity**: High  
- Member can pay fine (self)
- Librarian/Admin can process payment for member
- Payment history maintained per member

### **Story 3: Librarian/Admin Fine Management**
**Priority**: Medium  
**Complexity**: High  
- Librarian/Admin can waive fines (with reason)
- Librarian/Admin can adjust fine amount
- Fine export/reporting (download)

### **Story 4: Statistics and Reporting**
**Priority**: Medium  
**Complexity**: Medium  
- Member can view summary/statistics of their fines/payments
- Librarian can export fines/payment reports

---

## 🔧 **API Endpoints to Test**

- /api/Fines/my-fines
- /api/Fines
- /api/Fines/user/{userId}
- /api/Fines/my-payments
- /api/Fines/payments
- /api/Fines/payments/user/{userId}
- /api/Fines/pay
- /api/Fines/waive
- /api/Fines/adjust-amount
- /api/Fines/generate-overdue-fines
- /api/Fines/my-statistics
- /api/Fines/check-overdue

---

## 🧪 **Test Categories**

### **1. Unit Tests**
**Focus**: Business rules, DTOs, calculations, model/properties validation
- Fine and Payment model/DTOs initial and edge values
- Logic for status, amount, overdue rules
- Validation for payment amounts, fine waivers, adjustments

### **2. Integration Tests**
**Focus**: Endpoint contracts, DB, role auth
- Fines/payments CRUD and flow
- Process payment/waiver/adjustment via API
- Error/edge/permissions

### **3. End-to-End Tests**
**Focus**: User journeys
- Member pays fine
- Librarian waives/adjusts/processes
- Fine/report export
- Tab switching (outstanding/history)

---

## 📊 **Test Data Requirements**

### **Users**
- Members with overdue loans, fines (different statuses)
- Librarian/admin users

### **Loans & Fines**
- Overdue loans
- Fines at different dates/statuses (Outstanding/Paid/Waived)

### **Payments**
- Processed payments, various methods/amounts

---

## ⚠️ **Risk Assessment**

### **High Risk Areas**
1. Fine calculation/date logic
2. Payment correctness and idempotency
3. Role authorization for waiver/adjust/export
4. Data consistency after adjustments

### **Medium Risk Areas**
1. UI sync with backend
2. Export integrity
3. Cross-role history/statistics

---

## 📅 **Testing Timeline**

### **Phase 1: Preparation**
- Test plan, data, environment setup

### **Phase 2: Dev & Sprint (Current)**
- Unit, integration, E2E implementation and run
- Coverage and bug triage

### **Phase 3: System Testing**
- End-to-end negative/edge flows
- UAT, performance

### **Phase 4: Release**
- Regression and reports
- Sign-off

---

## 📈 **Success Criteria**

### **Coverage Targets**
- Unit: >90%
- Integration: >85%
- E2E: 100% critical

### **Quality Gates**
- No high/critical open bugs
- All main flows pass acceptance
---

## 📝 **Test Execution Tracking**

| Test Suite | Total | Pass | Fail | Skipped | Coverage |
|------------|-------|------|------|---------|----------|
| Unit Tests | (from results) | ✔️ |  |  | ~100% |
| Integration Tests | (from results) | ✔️ |  |  | ~100% |
| E2E Tests | (from results) | ✔️ |  |  | ~100% |

> See Test Results file for specifics.

---

## 🔗 **References**
- [Sprint 3 Test Docs](../sprint%203%20testdocs/QA_Sprint3_TestPlan.md)
- [API Docs](../technical-docs.md)
- [Fines E2E/Integration/Unit tests](../backend/Csp.E2E.Tests/FinesPaymentE2E.cs, ../backend/Csp.Integration.Tests/FineManagementTests.cs, etc.)
