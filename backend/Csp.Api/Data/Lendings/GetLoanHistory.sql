SELECT l.*, b.Title, b.Author, b.Isbn, u.Username, u.Email,
       0 as IsOverdue, 0 as OverdueDays
FROM lendings l
INNER JOIN books b ON l.BookId = b.Id
INNER JOIN users u ON l.UserId = u.Id
WHERE l.Status = 'Returned'
ORDER BY l.ReturnDate DESC
LIMIT @PageSize OFFSET @Offset