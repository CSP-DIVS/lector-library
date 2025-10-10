SELECT COUNT(*) FROM reservations 
WHERE BookId = @BookId AND UserId = @UserId AND Status IN ('Pending', 'Available')