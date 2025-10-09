UPDATE reservations 
SET Status = 'Fulfilled', FulfilledDate = @FulfilledDate, UpdatedAt = @UpdatedAt
WHERE Id = @ReservationId