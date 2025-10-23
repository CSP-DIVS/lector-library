# 🧪 Sprint 4 Test Scenarios: Fine & Payment Management

## 📚 **STORY 1: Fine Management**

### **Scenario 1.1: System Generates Fine for Overdue Loan**
```gherkin
Given a loan is overdue for member "member"
When the system checks for overdue loans
Then a fine should be generated for the loan
And the fine should show the correct overdue amount and status
```

### **Scenario 1.2: Member Views Outstanding Fines Summary**
```gherkin
Given I am logged in as a member
When I navigate to "Fines & Payment"
Then I should see an outstanding summary and list of fines
And the balance reflects all active fines
```

### **Scenario 1.3: Member Views All Fine Details**
```gherkin
Given I am logged in as a member
And I have active and paid fines
When I view my fines
Then each fine card shows book, amount, reason, status and due date
```

### **Scenario 1.4: Member Switches Between Outstanding and History Tabs**
```gherkin
Given I am a member with paid and outstanding fines
When I click between the "Outstanding" and "History" tabs
Then the respective fines/payment histories are shown
And the active tab is visually highlighted
```

## 💰 **STORY 2: Fine Payment and Processing**

### **Scenario 2.1: Member Pays Outstanding Fine**
```gherkin
Given I am logged in as a member
And I have an outstanding fine
When I click "Pay Fine" and confirm payment
Then the fine status changes to "Paid"
And my payment is reflected in history
```

### **Scenario 2.2: Payment Failure for Nonexistent Fine**
```gherkin
Given I am logged in as a member
When I attempt to pay a fine that does not exist
Then I receive an error indicating the fine is not found
```

### **Scenario 2.3: Librarian Processes Payment for Member**
```gherkin
Given I am logged in as a Librarian
When I process payment for a member's outstanding fine
Then the payment is applied and visible in reports/history
```

## 🛡️ **STORY 3: Fine Waiving and Adjustment**

### **Scenario 3.1: Librarian Waives Fine**
```gherkin
Given I am logged in as a Librarian
When I select an outstanding fine and click "Waive"
And provide a reason
Then the fine status updates to "Waived"
And action is reflected in member and admin views
```

### **Scenario 3.2: Waiving Nonexistent Fine**
```gherkin
Given I am logged in as a Librarian
When I attempt to waive a fine that does not exist
Then I receive an error message
```

### **Scenario 3.3: Member Cannot Waive Fine**
```gherkin
Given I am logged in as a Member
When I attempt to waive a fine
Then I receive an authorization error
```

### **Scenario 3.4: Librarian Adjusts Fine Amount**
```gherkin
Given I am logged in as a Librarian
When I select an outstanding fine and click "Adjust"
And enter a valid new amount and reason
Then the fine amount is updated
And all records reflect the adjustment
```

### **Scenario 3.5: Adjustment Validation & Edge Cases**
```gherkin
Given I am logged in as a Librarian
When I try to adjust a fine to zero/negative or provide no reason
Then I receive a validation error and no changes are made
```

## 📊 **STORY 4: Statistics, History and Reporting**

### **Scenario 4.1: Member Views Fine and Payment Statistics**
```gherkin
Given I am logged in as a member
When I open the statistics page/section
Then I see totals for outstanding, paid, and waived fines
```

### **Scenario 4.2: Librarian Exports Fines Report**
```gherkin
Given I am logged in as a Librarian
When I click "Export Report"
Then a report file (PDF/CSV) is generated and downloaded
And no error is shown
```

### **Scenario 4.3: Permissions Validation for Fines/Payments**
```gherkin
Given I am logged in as a Member
When I try to access admin-only endpoints or adjust/waive fines
Then access is denied; actions not permitted
```

## 🧪 **Other & Negative Cases**

### **Scenario 5.1: Unauthorized Fine/Payment Actions**
```gherkin
Given I am not logged in
When I attempt to access fines, payments, or make changes
Then I receive an unauthorized error
```

### **Scenario 5.2: System Edge - No Fines Exist**
```gherkin
Given I am a member with no fines
When I open the "Fines" page
Then I see a no-data/empty state message and actions are disabled
```

---

All scenarios map directly to E2E, integration, and unit tests implemented in:
- FinesPaymentE2E.cs
- FineManagementTests.cs
- FineModelValidationTests.cs
- FinesControllerTests.cs
- FineServiceTests.cs
