Test Summary
Unit Testing: The backend authorization logic that restricts this feature to administrators will be unit tested.

Integration Testing: The API endpoint will be tested to ensure it correctly updates the fine amount in the database and creates a detailed audit log entry.

End-to-End Testing: A Selenium script will automate the administrator's workflow:

Log in as an administrator.

Find a member with an outstanding fine.

Click "Adjust Fine."

Enter a new, lower amount and a reason, then submit.

Verify the fine amount is updated on the member's profile.

Navigate to the "Audit Trail" page and assert that a log entry for the fine adjustment exists with all the correct details.

Repeat the process for a full waiver (adjusting the fine to zero).

 
Unit Testing: The backend logic for calculating the number of overdue days and the corresponding fine amount will be unit tested with various date ranges.

Integration Testing: A test will be created to trigger the scheduled job manually. After it runs, the Fines table in the database will be inspected to verify that new fines were created and existing ones were updated correctly.

End-to-End Testing:

Log in as a librarian and check out a book to a member with a due date set to yesterday.

Manually trigger the fine calculation background job.

Log in as that member (or have the librarian view their profile).

Assert that a fine for one overdue day is now visible on the member's account.

Advance the system clock by another day, re-run the job, and assert that the fine has doubled.


Unit Testing: The backend logic for calculating the remaining balance after a partial payment will be unit tested.

Integration Testing: The API endpoint will be tested to ensure it correctly creates a payment record and updates the fine record in the database in a single transaction.

End-to-End Testing: A Selenium script will automate the workflow:

Log in as a librarian.

Find a member who has an outstanding fine.

Click "Record Payment."

Enter a partial payment amount and submit.

Verify that the outstanding balance is correctly updated on the screen.

Record another payment for the remaining amount.

Verify the fine is now marked as "Paid" or the balance is zero.