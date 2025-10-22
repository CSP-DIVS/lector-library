UPDATE reservations 
SET Status = 'Cancelled', UpdatedAt = @UpdatedAt
WHERE Id = @ReservationId