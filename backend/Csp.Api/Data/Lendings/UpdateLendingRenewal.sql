UPDATE lendings 
SET DueDate = @NewDueDate, RenewalCount = RenewalCount + 1, UpdatedAt = @UpdatedAt
WHERE Id = @LendingId