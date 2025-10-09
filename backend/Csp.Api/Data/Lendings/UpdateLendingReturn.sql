UPDATE lendings 
SET ReturnDate = @ReturnDate, Status = 'Returned', FineAmount = @FineAmount, UpdatedAt = @UpdatedAt
WHERE Id = @LendingId