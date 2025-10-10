SELECT l.*, b.Title, b.Author, b.Isbn, u.Username, u.Email,
       CASE WHEN l.DueDate < UTC_TIMESTAMP() THEN 1 ELSE 0 END as IsOverdue,
       CASE WHEN l.DueDate < UTC_TIMESTAMP() THEN DATEDIFF(UTC_TIMESTAMP(), l.DueDate) ELSE 0 END as OverdueDays
FROM lendings l
INNER JOIN books b ON l.BookId = b.Id
INNER JOIN users u ON l.UserId = u.Id
WHERE l.Status IN ('Active', 'Overdue') AND l.UserId = @UserId
ORDER BY l.DueDate ASC
LIMIT @PageSize OFFSET @Offset