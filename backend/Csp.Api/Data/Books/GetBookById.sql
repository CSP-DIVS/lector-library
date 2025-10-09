SELECT b.Id, b.Title, b.Author, b.Isbn, b.Category, b.PublishedYear, 
       b.IsActive, b.CreatedAt, b.UpdatedAt,
       COALESCE(bi.TotalCopies, 0) as TotalCopies,
       COALESCE(bi.AvailableCopies, 0) as AvailableCopies,
       CASE 
           WHEN COALESCE(bi.AvailableCopies, 0) > 0 THEN 'Available'
           ELSE 'Unavailable'
       END as Status
FROM books b 
LEFT JOIN book_inventory bi ON b.Id = bi.BookId 
WHERE b.Id = @Id