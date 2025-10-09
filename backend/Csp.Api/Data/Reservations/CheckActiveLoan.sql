SELECT COUNT(*) FROM lendings 
WHERE BookId = @BookId AND UserId = @UserId AND Status = 'Active'