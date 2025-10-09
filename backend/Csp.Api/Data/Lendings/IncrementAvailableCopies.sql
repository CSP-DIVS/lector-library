UPDATE book_inventory 
SET AvailableCopies = AvailableCopies + 1, UpdatedAt = @UpdatedAt
WHERE BookId = @BookId