SELECT BookId, UserId, Status, QueuePosition 
FROM reservations 
WHERE Id = @ReservationId