# 📋 Sprint 3 Test Plan: Book Lending & Reservation Management

## 🎯 **Test Scope**
**Sprint**: 3  
**Feature**: Book Lending & Reservation Management  
**QA Lead**: Subodha Munasinghe  
**Start Date**: October 6, 2025  

---

## 📖 **User Stories to Test**

### **Story 1: Book Lending Management**
**Priority**: High  
**Complexity**: High  

**Test Scenarios:**
- ✅ Issue book to member (positive flow)
- ✅ Issue book when none available (negative flow)
- ✅ Return book on time
- ✅ Return overdue book
- ✅ Attempt to issue to inactive member
- ✅ Issue multiple books to same member
- ✅ Loan limit validation

### **Story 2: Member Book Tracking**
**Priority**: High  
**Complexity**: Medium  

**Test Scenarios:**
- ✅ View personal active loans
- ✅ See accurate due dates
- ✅ Identify overdue books
- ✅ Empty state when no loans
- ✅ Pagination for many loans

### **Story 3: Book Reservation System**
**Priority**: High  
**Complexity**: High  

**Test Scenarios:**
- ✅ Reserve unavailable book
- ✅ Join reservation queue
- ✅ View queue position accurately
- ✅ Cancel reservation
- ✅ Auto-fulfill when book available
- ✅ Multiple reservations per user
- ✅ Reservation expiry

### **Story 4: Lending Administration**
**Priority**: Medium  
**Complexity**: Medium  

**Test Scenarios:**
- ✅ View all system loans
- ✅ Search loans by member/book
- ✅ Process returns
- ✅ Override due dates
- ✅ Force return books

### **Story 5: Fine Management**
**Priority**: Medium  
**Complexity**: Medium  

**Test Scenarios:**
- ✅ Calculate fines correctly
- ✅ Display outstanding amounts
- ✅ Process payments
- ✅ Fine exemptions

---

## 🔧 **API Endpoints to Test**

Based on expected backend implementation:

### **Loan Management**
```
POST   /api/loans              - Issue book to member
GET    /api/loans              - Get loans (with filters)
GET    /api/loans/member/{id}  - Get member's loans
PUT    /api/loans/{id}/return  - Return book
DELETE /api/loans/{id}         - Cancel loan (admin)
```

### **Reservation Management**
```
POST   /api/reservations           - Create reservation
GET    /api/reservations           - Get reservations
GET    /api/reservations/member/{id} - Get member's reservations
PUT    /api/reservations/{id}/fulfill - Fulfill reservation
DELETE /api/reservations/{id}       - Cancel reservation
```

### **Fine Management**
```
GET    /api/fines              - Get fines
GET    /api/fines/member/{id}  - Get member's fines
POST   /api/fines/{id}/pay     - Process payment
```

---

## 🧪 **Test Categories**

### **1. Unit Tests** 
**Focus**: Business logic validation
- Loan calculation logic
- Fine calculation algorithms  
- Reservation queue management
- Date validations

### **2. Integration Tests**
**Focus**: API endpoint testing
- CRUD operations for loans/reservations
- Authentication & authorization
- Database transactions
- Error handling

### **3. End-to-End Tests**
**Focus**: Complete user workflows
- Complete lending workflow
- Reservation process
- Fine payment flow
- Admin management tasks

---

## 📊 **Test Data Requirements**

### **Books**
- Available books (multiple copies)
- Books with single copy
- Books with no available copies
- Different categories

### **Users**
- Active members
- Inactive members
- Members with existing loans
- Members with fines
- Librarians and admins

### **Existing Loans**
- Current loans (various due dates)
- Overdue loans
- Recently returned loans

### **Reservations**
- Active reservations
- Expired reservations
- Queue with multiple members

---

## ⚠️ **Risk Assessment**

### **High Risk Areas**
1. **Concurrent Operations**: Multiple users reserving same book
2. **Data Consistency**: Loan/reservation state synchronization
3. **Date Calculations**: Due dates, fines, overdue detection
4. **Inventory Management**: Available copy tracking

### **Medium Risk Areas**
1. **Role Permissions**: Member vs Librarian access
2. **Notification System**: Reservation alerts
3. **Payment Processing**: Fine calculations
4. **Queue Management**: Reservation order

---

## 📅 **Testing Timeline**

### **Phase 1: Pre-Development (Current)**
- ✅ Test plan creation
- ✅ Test data preparation
- ✅ Environment setup
- ✅ Mock API creation

### **Phase 2: Development Testing**
- Unit test execution
- Integration test development
- API testing
- Bug reporting

### **Phase 3: System Testing**
- End-to-end testing
- User acceptance testing
- Performance testing
- Security testing

### **Phase 4: Release Preparation**
- Regression testing
- Documentation updates
- Test report generation
- Sign-off

---

## 📈 **Success Criteria**

### **Coverage Targets**
- Unit Tests: >90%
- Integration Tests: >85%
- E2E Tests: 100% of critical paths

### **Quality Gates**
- Zero critical/high severity bugs
- All user stories pass acceptance criteria
- Performance meets requirements
- Security vulnerabilities addressed

---

## 📝 **Test Execution Tracking**

| Test Suite | Total | Pass | Fail | Blocked | Coverage |
|------------|-------|------|------|---------|----------|
| Unit Tests | TBD | 0 | 0 | 0 | 0% |
| Integration Tests | TBD | 0 | 0 | 0 | 0% |
| E2E Tests | TBD | 0 | 0 | 0 | 0% |

**Status**: ⏳ Waiting for feature branch

---

## 🔗 **References**
- [Sprint 2 Test Report](backend/Csp.Unit.Tests/TestReport/)
- [API Documentation](technical-docs.md)
- [User Stories](README.md)
- [Architecture Docs](technical-docs.md)