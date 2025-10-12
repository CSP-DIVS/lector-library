SELECT 
    l.Id as LendingId,
    l.BookId,
    b.Title as BookTitle,
    b.Author as BookAuthor,
    l.BorrowDate,
    l.DueDate,
    l.ReturnDate,
    l.Status,
    l.FineAmount,
    l.FinePaid,
    CASE 
        WHEN l.Status = 'Overdue' AND l.DueDate < NOW() THEN DATEDIFF(NOW(), l.DueDate)
        WHEN l.Status = 'Returned' AND l.ReturnDate > l.DueDate THEN DATEDIFF(l.ReturnDate, l.DueDate)
        ELSE 0 
    END as OverdueDays
FROM lendings l
JOIN books b ON l.BookId = b.Id
WHERE l.UserId = @UserId 
    AND l.FineAmount > 0
ORDER BY l.DueDate DESC;