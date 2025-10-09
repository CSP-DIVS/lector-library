SELECT BookId, UserId, DueDate, Status, RenewalCount, MaxRenewals 
FROM lendings 
WHERE Id = @LendingId