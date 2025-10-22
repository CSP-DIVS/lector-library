# 🧪 Sprint 3 Test Scenarios: Book Lending & Reservation Management

## 📚 **STORY 1: Book Lending Management**

### **Scenario 1.1: Issue Book to Member (Happy Path)**
```gherkin
Given I am logged in as a Librarian
And there is an available book "1984" by George Orwell
And member "john_doe" is active
When I issue the book to "john_doe"
Then the book should be marked as on loan
And the loan record should show today's date as issue date
And the due date should be 14 days from today
And the available copies should decrease by 1
```

### **Scenario 1.2: Issue Book - No Copies Available**
```gherkin
Given I am logged in as a Librarian
And book "1984" has 0 available copies
And member "john_doe" is active
When I attempt to issue the book to "john_doe"
Then I should see error "No copies available"
And no loan record should be created
And available copies should remain 0
```

### **Scenario 1.3: Return Book On Time**
```gherkin
Given I am logged in as a Librarian
And "john_doe" has borrowed "1984" due today
When I process the return of "1984" from "john_doe"
Then the loan should be marked as returned
And the return date should be today
And available copies should increase by 1
And no fine should be calculated
```

### **Scenario 1.4: Return Overdue Book**
```gherkin
Given I am logged in as a Librarian
And "john_doe" has borrowed "1984" due 3 days ago
When I process the return of "1984" from "john_doe"
Then the loan should be marked as returned
And the return date should be today
And available copies should increase by 1
And a fine of $1.50 should be calculated (3 days × $0.50)
```

---

## 🔖 **STORY 2: Member Book Tracking**

### **Scenario 2.1: View My Active Loans**
```gherkin
Given I am logged in as member "john_doe"
And I have 2 books on loan
When I navigate to "My Books & Reservations"
And I select the "My Loans" tab
Then I should see 2 loan records
And each record should show book title, author, due date
And overdue books should be highlighted in red
```

### **Scenario 2.2: No Active Loans**
```gherkin
Given I am logged in as member "jane_doe"
And I have no books on loan
When I navigate to "My Books & Reservations"
And I select the "My Loans" tab
Then I should see "No active loans" message
And I should see a link to "Browse Books"
```

---

## 📋 **STORY 3: Book Reservation System**

### **Scenario 3.1: Reserve Unavailable Book**
```gherkin
Given I am logged in as member "john_doe"
And book "1984" has 0 available copies
And the book is not on my active loans
When I click "Reserve" on the book details page
Then a reservation should be created
And I should see "Book reserved successfully"
And my queue position should be shown
```

### **Scenario 3.2: View Reservation Queue Position**
```gherkin
Given I am logged in as member "john_doe"
And I have reserved "1984"
And there are 2 people ahead of me in queue
When I view my reservations
Then I should see queue position "#3"
And estimated availability date should be shown
```

### **Scenario 3.3: Cancel Reservation**
```gherkin
Given I am logged in as member "john_doe"
And I have an active reservation for "1984"
When I click "Cancel Reservation"
And confirm the cancellation
Then the reservation should be removed
And people behind me should move up in queue
```

### **Scenario 3.4: Auto-Fulfill Reservation**
```gherkin
Given member "john_doe" has reserved "1984"
And "john_doe" is first in the reservation queue
When another member returns "1984"
Then "john_doe" should be notified
And the book should be held for 48 hours
And reservation status should change to "Ready for pickup"
```

---

## 🛡️ **AUTHORIZATION & SECURITY TESTS**

### **Scenario S1: Member Access Control**
```gherkin
Given I am logged in as a member
When I attempt to access loan management functions
Then I should see "Access Denied"
And I should only see my own loans and reservations
```

### **Scenario S2: Librarian Permissions**
```gherkin
Given I am logged in as a Librarian
When I access the lending management section
Then I should see all loans and reservations
And I should be able to issue and return books
```

---

## 📊 **DATA VALIDATION TESTS**

### **Scenario D1: Loan Limit Validation**
```gherkin
Given member "john_doe" already has 5 books on loan
And the system limit is 5 books per member
When I attempt to issue another book
Then I should see error "Member has reached loan limit"
```

### **Scenario D2: Reservation Limit Validation**
```gherkin
Given member "john_doe" already has 3 active reservations
And the system limit is 3 reservations per member
When I attempt to reserve another book
Then I should see error "Maximum reservations reached"
```

---

## 💰 **FINE MANAGEMENT TESTS**

### **Scenario F1: Calculate Daily Fines**
```gherkin
Given member "john_doe" has an overdue book
And the book was due 5 days ago
And the daily fine rate is $0.50
When the system calculates fines
Then "john_doe" should have a fine of $2.50
```

### **Scenario F2: View Outstanding Fines**
```gherkin
Given I am logged in as member "john_doe"
And I have outstanding fines of $5.00
When I view my account
Then I should see "Outstanding Fines: $5.00"
And I should see option to "Pay Fines"
```

---

## 🔄 **INTEGRATION TESTS**

### **Scenario I1: Book Return Updates Reservation Queue**
```gherkin
Given "1984" has 0 available copies
And members A, B, C have reservations in that order
When a copy of "1984" is returned
Then member A should be notified
And the book should be held for member A
And members B, C should see updated queue positions
```

### **Scenario I2: Member Deactivation Cancels Loans**
```gherkin
Given member "john_doe" has active loans and reservations
When admin deactivates "john_doe"
Then all reservations should be cancelled
And return notices should be sent for active loans
```

---

## 🧪 **EDGE CASES & ERROR HANDLING**

### **Scenario E1: Concurrent Reservation**
```gherkin
Given book "1984" becomes available
And members A and B simultaneously click "Reserve"
When both requests are processed
Then only one reservation should succeed
And the other should see "Book no longer available"
```

### **Scenario E2: Database Connection Failure**
```gherkin
Given the database connection is lost
When I attempt to process a loan
Then I should see "System temporarily unavailable"
And the operation should be retried automatically
```

---

## 📱 **UI/UX TESTS**

### **Scenario U1: Responsive Design**
```gherkin
Given I am on the lending management page
When I view on mobile device
Then all functions should be accessible
And tables should scroll horizontally
```

### **Scenario U2: Loading States**
```gherkin
Given I click "Issue Book"
When the system is processing
Then I should see a loading spinner
And the button should be disabled
```

---

## 🚀 **PERFORMANCE TESTS**

### **Scenario P1: Large Data Sets**
```gherkin
Given there are 10,000 loan records
When I load the loans page
Then the page should load within 3 seconds
And pagination should work smoothly
```

### **Scenario P2: Concurrent Users**
```gherkin
Given 50 users are accessing the system simultaneously
When they perform various lending operations
Then response times should remain under 2 seconds
And no data corruption should occur
```