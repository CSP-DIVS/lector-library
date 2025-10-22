SELECT b.Id, b.IsActive, bi.AvailableCopies 
FROM books b
LEFT JOIN book_inventory bi ON b.Id = bi.BookId
WHERE b.Id = @BookId