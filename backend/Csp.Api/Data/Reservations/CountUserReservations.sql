SELECT COUNT(*) FROM reservations 
WHERE UserId = @UserId AND Status IN ('Pending', 'Available')