SELECT BookId, UserId, DueDate, Status 
FROM lendings 
WHERE Id = @LendingId