UPDATE book_inventory 
SET TotalCopies = @TotalCopies, AvailableCopies = @AvailableCopies, UpdatedAt = CURRENT_TIMESTAMP
WHERE BookId = @BookId