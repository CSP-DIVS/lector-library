SELECT COUNT(*) FROM reservations 
WHERE BookId = @BookId AND Status = 'Pending'