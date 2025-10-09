UPDATE reservations 
SET QueuePosition = QueuePosition - 1, UpdatedAt = @UpdatedAt
WHERE BookId = @BookId AND Status = 'Pending' AND QueuePosition > @QueuePosition